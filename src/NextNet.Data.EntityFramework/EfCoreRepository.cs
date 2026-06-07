using System.Linq.Expressions;

namespace NextNet.Data.EntityFramework;

/// <summary>
/// Generic EF Core implementation of <see cref="IRepository{T}"/>.
/// Uses <see cref="IDbContextFactory{TContext}"/> to create short-lived DbContext instances
/// for each operation, avoiding concurrency and tracking issues.
/// </summary>
/// <typeparam name="T">The entity type. Must be a class registered as a DbSet in <see cref="AppDbContext"/>.</typeparam>
/// <remarks>
/// <para>
/// Each method creates a new DbContext via the factory pattern, performs the operation,
/// and disposes the context. This ensures thread safety and avoids stale tracking state.
/// </para>
/// <para>
/// For query operations (<see cref="FindAsync"/>, <see cref="GetAllAsync"/>), entities are
/// queried with <c>AsNoTracking()</c> by default to improve performance.
/// </para>
/// </remarks>
public sealed class EfCoreRepository<T> : IRepository<T> where T : class
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    /// <summary>
    /// Initializes a new instance of <see cref="EfCoreRepository{T}"/>.
    /// </summary>
    /// <param name="contextFactory">The DbContext factory for creating context instances.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="contextFactory"/> is null.</exception>
    public EfCoreRepository(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
    }

    /// <summary>
    /// Finds an entity by its primary key. The context is disposed after the operation.
    /// </summary>
    /// <param name="id">The primary key value.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The entity if found; otherwise <c>null</c>.</returns>
    public async Task<T?> FindAsync(object id, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Set<T>().FindAsync(new[] { id }, cancellationToken);
    }

    /// <summary>
    /// Retrieves all entities with optional sorting and pagination.
    /// Supports eager loading via <see cref="RepositoryQueryOptions.Includes"/>.
    /// </summary>
    /// <param name="options">Optional query options (filter, sort, pagination, includes).</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A paged result of matching entities.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when page or page size are invalid.</exception>
    public async Task<PagedResult<T>> GetAllAsync(RepositoryQueryOptions? options = null, CancellationToken cancellationToken = default)
    {
        options ??= new RepositoryQueryOptions();

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        IQueryable<T> query = context.Set<T>().AsNoTracking();

        // Apply eager loading for included navigation properties
        if (options.Includes is not null)
        {
            foreach (var include in options.Includes)
            {
                if (!string.IsNullOrWhiteSpace(include))
                {
                    query = query.Include(include);
                }
            }
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        if (!string.IsNullOrWhiteSpace(options.SortBy))
        {
            query = ApplyOrdering(query, options.SortBy, options.SortDescending);
        }

        // Apply pagination
        var page = options.EffectivePage;
        var pageSize = options.EffectivePageSize;
        var skip = (page - 1) * pageSize;

        var items = await query
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<T>(items, totalCount, page, pageSize);
    }

    /// <summary>
    /// Inserts a new entity. The entity is tracked and saved within a new DbContext scope.
    /// </summary>
    /// <param name="entity">The entity to insert.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    public async Task InsertAsync(T entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        context.Set<T>().Add(entity);
        await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Updates an existing entity. Attaches the entity as modified and saves changes.
    /// </summary>
    /// <param name="entity">The entity with updated values.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    public async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        context.Set<T>().Update(entity);
        await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Deletes an entity by primary key. Fetches the entity first, then removes and saves.
    /// </summary>
    /// <param name="id">The primary key value of the entity to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="KeyNotFoundException">Thrown when no entity with the given id exists.</exception>
    public async Task DeleteAsync(object id, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var entity = await context.Set<T>().FindAsync(new[] { id }, cancellationToken);

        if (entity is null)
        {
            throw new KeyNotFoundException($"No entity of type '{typeof(T).Name}' with id '{id}' was found.");
        }

        context.Set<T>().Remove(entity);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static IQueryable<T> ApplyOrdering(IQueryable<T> query, string sortBy, bool sortDescending)
    {
        var param = Expression.Parameter(typeof(T), "e");
        var property = Expression.PropertyOrField(param, sortBy);
        var lambda = Expression.Lambda(property, param);

        var methodName = sortDescending ? "OrderByDescending" : "OrderBy";
        var resultExpression = Expression.Call(
            typeof(Queryable),
            methodName,
            new[] { typeof(T), property.Type },
            query.Expression,
            Expression.Quote(lambda));

        return query.Provider.CreateQuery<T>(resultExpression);
    }
}
