#if NET8_0_OR_GREATER
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NextNet.Data.Abstractions.Abstractions;
using NextNet.Data.Abstractions.Models;
using Microsoft.Extensions.Logging;

namespace NextNet.Data.Sdk.Base;

/// <summary>
/// Base class for NextNet data repositories. Implements <see cref="IRepository{T}"/>
/// with common error handling, logging, and abstract methods for provider-specific CRUD.
/// </summary>
/// <typeparam name="T">The entity type. Must be a class.</typeparam>
/// <remarks>
/// <para>
/// Provider authors override the four <c>CoreAsync</c> methods to implement
/// the actual data access using their provider's technology (ADO.NET, REST API, etc.).
/// The base class handles argument validation, exception wrapping, logging, and
/// <see cref="PagedResult{T}"/> computation.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class MyCustomRepository&lt;T&gt; : RepositoryBase&lt;T&gt;
/// {
///     private readonly MyCustomConnection _connection;
///
///     public MyCustomRepository(MyCustomConnection connection, ILogger logger)
///         : base(logger) => _connection = connection;
///
///     protected override async Task&lt;T?&gt; FindCoreAsync(object id, CancellationToken ct)
///         => await _connection.FindAsync&lt;T&gt;(id, ct);
///
///     protected override async Task&lt;IReadOnlyList&lt;T&gt;&gt; GetAllCoreAsync(
///         RepositoryQueryOptions? options, CancellationToken ct)
///         => await _connection.QueryAsync&lt;T&gt;("SELECT * FROM " + typeof(T).Name, ct);
///
///     protected override async Task InsertCoreAsync(T entity, CancellationToken ct)
///         => await _connection.InsertAsync(entity, ct);
///
///     protected override async Task UpdateCoreAsync(T entity, CancellationToken ct)
///         => await _connection.UpdateAsync(entity, ct);
///
///     protected override async Task DeleteCoreAsync(object id, CancellationToken ct)
///         => await _connection.DeleteAsync&lt;T&gt;(id, ct);
/// }
/// </code>
/// </example>
public abstract class RepositoryBase<T> : IRepository<T>
    where T : class
{
    private readonly ILogger? _logger;

    /// <summary>
    /// Gets the logger used by this repository for diagnostic output.
    /// </summary>
    protected ILogger? Logger => _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RepositoryBase{T}"/> class.
    /// </summary>
    /// <param name="logger">An optional logger for diagnostic output.</param>
    protected RepositoryBase(ILogger? logger = null)
    {
        _logger = logger;
    }

    /// <summary>
    /// Finds an entity by its primary key.
    /// Logs the operation and wraps exceptions in <see cref="RepositoryException"/>.
    /// </summary>
    /// <param name="id">The primary key value.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The entity if found; otherwise <c>null</c>.</returns>
    /// <exception cref="RepositoryException">Thrown when the operation fails.</exception>
    public async Task<T?> FindAsync(object id, CancellationToken cancellationToken = default)
    {
        if (id is null)
            throw new ArgumentNullException(nameof(id));

        try
        {
            _logger?.LogDebug("Finding entity of type '{EntityType}' with id '{Id}'.", typeof(T).Name, id);
            return await FindCoreAsync(id, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to find entity of type '{EntityType}' with id '{Id}'.", typeof(T).Name, id);
            throw new RepositoryException($"Failed to find entity of type '{typeof(T).Name}'.", id, "Find", ex);
        }
    }

    /// <summary>
    /// Retrieves all entities with optional filtering, sorting, and pagination.
    /// If <paramref name="options"/> has pagination, the base class computes
    /// <see cref="PagedResult{T}"/> metadata.
    /// </summary>
    /// <param name="options">Optional query options for filtering, sorting, and pagination.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A paged result containing the matching entities.</returns>
    /// <exception cref="RepositoryException">Thrown when the operation fails.</exception>
    public async Task<PagedResult<T>> GetAllAsync(RepositoryQueryOptions? options = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger?.LogDebug("Getting all entities of type '{EntityType}'.", typeof(T).Name);
            var items = await GetAllCoreAsync(options, cancellationToken).ConfigureAwait(false);

            var effectivePage = options?.EffectivePage ?? 1;
            var effectivePageSize = options?.EffectivePageSize ?? 20;
            var totalCount = items.Count;

            return new PagedResult<T>(items, totalCount, effectivePage, effectivePageSize);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to get entities of type '{EntityType}'.", typeof(T).Name);
            throw new RepositoryException($"Failed to get entities of type '{typeof(T).Name}'.", entityId: null, operation: "GetAll", inner: ex);
        }
    }

    /// <summary>
    /// Inserts a new entity. Validates the entity is not null before delegating.
    /// </summary>
    /// <param name="entity">The entity to insert.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entity"/> is null.</exception>
    /// <exception cref="RepositoryException">Thrown when the operation fails.</exception>
    public async Task InsertAsync(T entity, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            throw new ArgumentNullException(nameof(entity));

        try
        {
            _logger?.LogDebug("Inserting entity of type '{EntityType}'.", typeof(T).Name);
            await InsertCoreAsync(entity, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to insert entity of type '{EntityType}'.", typeof(T).Name);
            throw new RepositoryException($"Failed to insert entity of type '{typeof(T).Name}'.", entityId: null, operation: "Insert", inner: ex);
        }
    }

    /// <summary>
    /// Updates an existing entity. Validates the entity is not null before delegating.
    /// </summary>
    /// <param name="entity">The entity with updated values.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entity"/> is null.</exception>
    /// <exception cref="RepositoryException">Thrown when the operation fails.</exception>
    public async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            throw new ArgumentNullException(nameof(entity));

        try
        {
            _logger?.LogDebug("Updating entity of type '{EntityType}'.", typeof(T).Name);
            await UpdateCoreAsync(entity, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to update entity of type '{EntityType}'.", typeof(T).Name);
            throw new RepositoryException($"Failed to update entity of type '{typeof(T).Name}'.", entityId: null, operation: "Update", inner: ex);
        }
    }

    /// <summary>
    /// Deletes an entity by primary key. Logs a warning if the entity is not found.
    /// </summary>
    /// <param name="id">The primary key value of the entity to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="id"/> is null.</exception>
    /// <exception cref="RepositoryException">Thrown when the operation fails.</exception>
    public async Task DeleteAsync(object id, CancellationToken cancellationToken = default)
    {
        if (id is null)
            throw new ArgumentNullException(nameof(id));

        try
        {
            _logger?.LogDebug("Deleting entity of type '{EntityType}' with id '{Id}'.", typeof(T).Name, id);
            await DeleteCoreAsync(id, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to delete entity of type '{EntityType}' with id '{Id}'.", typeof(T).Name, id);
            throw new RepositoryException($"Failed to delete entity of type '{typeof(T).Name}'.", id, "Delete", ex);
        }
    }

    /// <summary>
    /// Provider-specific find implementation.
    /// </summary>
    /// <param name="id">The primary key value.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The entity if found; otherwise <c>null</c>.</returns>
    protected abstract Task<T?> FindCoreAsync(object id, CancellationToken cancellationToken);

    /// <summary>
    /// Provider-specific get-all implementation.
    /// The returned list should include all matching entities; the base class handles pagination slicing.
    /// </summary>
    /// <param name="options">Optional query options for filtering, sorting, and pagination.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A list of matching entities.</returns>
    protected abstract Task<IReadOnlyList<T>> GetAllCoreAsync(RepositoryQueryOptions? options, CancellationToken cancellationToken);

    /// <summary>
    /// Provider-specific insert implementation.
    /// </summary>
    /// <param name="entity">The entity to insert.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    protected abstract Task InsertCoreAsync(T entity, CancellationToken cancellationToken);

    /// <summary>
    /// Provider-specific update implementation.
    /// </summary>
    /// <param name="entity">The entity with updated values.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    protected abstract Task UpdateCoreAsync(T entity, CancellationToken cancellationToken);

    /// <summary>
    /// Provider-specific delete implementation.
    /// </summary>
    /// <param name="id">The primary key value of the entity to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    protected abstract Task DeleteCoreAsync(object id, CancellationToken cancellationToken);
}
#endif
