using NextNet.Data.Abstractions.Models;

namespace NextNet.Data.Abstractions.Abstractions;

/// <summary>
/// Defines the generic CRUD contract for entity persistence.
/// Implementations are provider-specific (EF Core DbSet, Dapper SqlMapper, MongoDB collection, etc.).
/// </summary>
/// <typeparam name="T">The entity type. Must be a reference type.</typeparam>
/// <remarks>
/// <para>
/// The <see cref="IRepository{T}"/> interface provides a uniform CRUD API across all
/// NextNet data providers. It supports find by ID, paginated queries, insert, update,
/// and delete operations.
/// </para>
/// <para>
/// Providers register concrete implementations of this interface via the
/// <see cref="Registration.NextNetDataBuilder.AddRepository{TEntity}"/> method.
/// </para>
/// <example>
/// <code>
/// public class UserService
/// {
///     private readonly IRepository&lt;User&gt; _userRepo;
///
///     public UserService(IRepository&lt;User&gt; userRepo)
///     {
///         _userRepo = userRepo;
///     }
///
///     public async Task&lt;User?&gt; GetUserAsync(int id) =>
///         await _userRepo.FindAsync(id);
///
///     public async Task&lt;PagedResult&lt;User&gt;&gt; GetUsersAsync(int page) =>
///         await _userRepo.GetAllAsync(new RepositoryQueryOptions(Page: page));
/// }
/// </code>
/// </example>
/// </remarks>
public interface IRepository<T> where T : class
{
    /// <summary>
    /// Finds a single entity by its primary key value(s).
    /// </summary>
    /// <param name="id">The primary key value.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The entity if found; otherwise <c>null</c>.</returns>
    Task<T?> FindAsync(object id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all entities of type <typeparamref name="T"/>, optionally filtered and paginated.
    /// </summary>
    /// <param name="options">Optional query options for filtering, sorting, and pagination.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A paged result containing the matching entities.</returns>
    Task<PagedResult<T>> GetAllAsync(RepositoryQueryOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts a new entity into the data store.
    /// </summary>
    /// <param name="entity">The entity to insert.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous insert operation.</returns>
    Task InsertAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing entity in the data store.
    /// </summary>
    /// <param name="entity">The entity with updated values.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous update operation.</returns>
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an entity from the data store by its primary key value(s).
    /// </summary>
    /// <param name="id">The primary key value of the entity to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous delete operation.</returns>
    Task DeleteAsync(object id, CancellationToken cancellationToken = default);
}
