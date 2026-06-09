using System.Collections.Concurrent;
using NextNet.Data.Dapper.Internal;

namespace NextNet.Data.Dapper;

/// <summary>
/// Generic Dapper implementation of <see cref="IRepository{T}"/>.
/// Executes raw parameterized SQL queries against the database via
/// <see cref="DapperConnectionManager"/>.
/// </summary>
/// <typeparam name="T">The entity type. Must be a class with a parameterless constructor (POCO).</typeparam>
/// <remarks>
/// <para>
/// All queries use Dapper's parameterized syntax (<c>@paramName</c>) to prevent SQL injection.
/// Entity property names are mapped to column names by convention (identical names).
/// To customize column mapping, decorate properties with <c>[Column("name")]</c> or
/// use a custom <c>ITypeMap</c> via <c>SqlMapper.SetTypeMap()</c>.
/// </para>
/// <para>
/// Each repository operation:
/// <list type="number">
///   <item>Leases a connection from <see cref="DapperConnectionManager"/></item>
///   <item>Creates a <see cref="CommandDefinition"/> with the SQL and parameters</item>
///   <item>Executes via <c>SqlMapper</c> (Dapper's extension methods)</item>
///   <item>Returns the result and yields the connection back to the pool</item>
/// </list>
/// </para>
/// <para>
/// Null entity checks are performed on insert and update operations,
/// throwing <see cref="ArgumentNullException"/> if the entity is null.
/// </para>
/// </remarks>
public sealed class DapperRepository<T> : IRepository<T> where T : class
{
    private static readonly ConcurrentDictionary<Type, HashSet<string>> ValidSortColumnsCache = new();

    private readonly DapperConnectionManager _connectionManager;
    private readonly DapperRepositoryOptions _options;
    private readonly EntityMetadata _metadata;
    private readonly ILogger<DapperRepository<T>> _logger;
    private readonly string _connectionName;

    /// <summary>
    /// Initializes a new instance of <see cref="DapperRepository{T}"/>.
    /// </summary>
    /// <param name="connectionManager">The connection manager for database access.</param>
    /// <param name="options">Optional repository configuration (table name, column mapping, timeout).</param>
    /// <param name="connectionName">The name of the connection to use. Defaults to "Default".</param>
    /// <param name="logger">Optional logger for query diagnostics.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="connectionManager"/> is null.</exception>
    public DapperRepository(
        DapperConnectionManager connectionManager,
        DapperRepositoryOptions? options = null,
        string? connectionName = null,
        ILogger<DapperRepository<T>>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(connectionManager);

        _connectionManager = connectionManager;
        _options = options ?? new DapperRepositoryOptions();
        _metadata = EntityMetadata.For<T>(_options);
        _connectionName = connectionName ?? "Default";
        _logger = logger ?? new Microsoft.Extensions.Logging.Abstractions.NullLogger<DapperRepository<T>>();

        // Cache valid sort columns for column whitelist enforcement
        ValidSortColumnsCache.GetOrAdd(typeof(T), _ => new HashSet<string>(
            _metadata.AllColumns, StringComparer.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets the table name for the entity.
    /// </summary>
    internal string TableName => _metadata.QualifiedTableName;

    /// <summary>
    /// Finds an entity by its primary key using a parameterized <c>SELECT</c> query.
    /// Default SQL: <c>SELECT * FROM {TableName} WHERE Id = @Id</c>
    /// </summary>
    /// <param name="id">The primary key value.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The entity if found; otherwise <c>null</c>.</returns>
    /// <example>
    /// <code>
    /// var user = await repository.FindAsync(42);
    /// if (user is not null) { /* use user */ }
    /// </code>
    /// </example>
    public async Task<T?> FindAsync(object id, CancellationToken cancellationToken = default)
    {
        var sql = SqlBuilder.BuildFind(_metadata);
        var parameters = new { Id = id };

        LogSql(sql, parameters);

        using var conn = await _connectionManager.OpenConnectionAsync(_connectionName, cancellationToken);
        return await conn.QueryFirstOrDefaultAsync<T>(
            new CommandDefinition(sql, parameters, commandTimeout: _options.CommandTimeout, cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Retrieves all entities with optional filtering, sorting, and pagination.
    /// Filter is applied as raw SQL appended after <c>WHERE</c> (caller must ensure
    /// it uses parameterized syntax).
    /// </summary>
    /// <param name="options">Optional query options (filter, sort, pagination).</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A paged result of matching entities.</returns>
    /// <exception cref="InvalidOperationException">Thrown when sort column is not in the whitelist.</exception>
    /// <remarks>
    /// <para>
    /// The <c>options.Filter</c> value is appended directly after <c>WHERE</c> in
    /// the generated SQL. It MUST use Dapper parameter syntax (<c>@paramName</c>)
    /// and MUST NOT contain raw user input concatenation.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var result = await repository.GetAllAsync(new RepositoryQueryOptions(
    ///     filter: "Age &gt; @minAge",
    ///     sortBy: "LastName",
    ///     page: 1,
    ///     pageSize: 20));
    /// </code>
    /// </example>
    public async Task<PagedResult<T>> GetAllAsync(RepositoryQueryOptions? options = null, CancellationToken cancellationToken = default)
    {
        options ??= new RepositoryQueryOptions();

        var sortBy = options.SortBy;
        var page = options.EffectivePage;
        var pageSize = options.EffectivePageSize;
        var offset = (page - 1) * pageSize;

        // Column whitelist enforcement for dynamic sorting
        if (!string.IsNullOrWhiteSpace(sortBy))
        {
            var validColumns = ValidSortColumnsCache.GetOrAdd(typeof(T), _ => new HashSet<string>(
                _metadata.AllColumns, StringComparer.OrdinalIgnoreCase));

            if (!validColumns.Contains(sortBy))
            {
                throw new InvalidOperationException(
                    $"[{DapperErrorCodes.ParameterMappingFailed}] Invalid sort column '{sortBy}'. Valid columns for '{typeof(T).Name}': " +
                    $"{string.Join(", ", validColumns)}");
            }
        }

        var countSql = SqlBuilder.BuildCount(_metadata, options.Filter);
        var querySql = SqlBuilder.BuildGetAll(_metadata, _options.PaginationStyle, sortBy, options.SortDescending, options.Filter);

        LogSql(countSql, new { });
        LogSql(querySql, new { Offset = offset, Limit = pageSize });

        using var conn = await _connectionManager.OpenConnectionAsync(_connectionName, cancellationToken);

        var totalCount = await conn.ExecuteScalarAsync<long>(
            new CommandDefinition(countSql, cancellationToken: cancellationToken, commandTimeout: _options.CommandTimeout));

        var items = await conn.QueryAsync<T>(
            new CommandDefinition(querySql, new { Offset = offset, Limit = pageSize },
                cancellationToken: cancellationToken, commandTimeout: _options.CommandTimeout));

        return new PagedResult<T>(items.ToList().AsReadOnly(), totalCount, page, pageSize);
    }

    /// <summary>
    /// Inserts a new entity using a parameterized <c>INSERT</c> query.
    /// Default SQL: <c>INSERT INTO {TableName} ({Columns}) VALUES ({ParameterizedValues})</c>
    /// </summary>
    /// <param name="entity">The entity to insert. Must not be null.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entity"/> is null.</exception>
    /// <remarks>
    /// <para>
    /// If the entity type has a primary key property of type <c>int</c> or <c>long</c>,
    /// and <see cref="DapperRepositoryOptions.KeyIsAutoGenerated"/> is <c>true</c>,
    /// <c>SELECT CAST(SCOPE_IDENTITY() AS INT)</c> is appended to retrieve the generated key,
    /// which is then set back on the entity instance.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var user = new User { Name = "Alice", Email = "alice@example.com" };
    /// await repository.InsertAsync(user);
    /// Console.WriteLine(user.Id); // Populated with generated key
    /// </code>
    /// </example>
    public async Task InsertAsync(T entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var sql = SqlBuilder.BuildInsert(_metadata, _options.KeyIsAutoGenerated);

        LogSql(sql, entity);

        using var conn = await _connectionManager.OpenConnectionAsync(_connectionName, cancellationToken);

        if (_options.KeyIsAutoGenerated)
        {
            var newId = await conn.ExecuteScalarAsync<int>(
                new CommandDefinition(sql, entity, commandTimeout: _options.CommandTimeout, cancellationToken: cancellationToken));

            // Set the generated key back on the entity
            var keyProp = typeof(T).GetProperty(_metadata.KeyColumn, BindingFlags.Public | BindingFlags.Instance);
            if (keyProp is not null && keyProp.CanWrite)
            {
                keyProp.SetValue(entity, Convert.ChangeType(newId, keyProp.PropertyType));
            }
        }
        else
        {
            await conn.ExecuteAsync(
                new CommandDefinition(sql, entity, commandTimeout: _options.CommandTimeout, cancellationToken: cancellationToken));
        }
    }

    /// <summary>
    /// Updates an existing entity using a parameterized <c>UPDATE</c> query.
    /// Default SQL: <c>UPDATE {TableName} SET {SetClause} WHERE Id = @Id</c>
    /// </summary>
    /// <param name="entity">The entity with updated values. Must not be null.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entity"/> is null.</exception>
    /// <example>
    /// <code>
    /// var user = await repository.FindAsync(42);
    /// user.Email = "newemail@example.com";
    /// await repository.UpdateAsync(user);
    /// </code>
    /// </example>
    public async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var sql = SqlBuilder.BuildUpdate(_metadata);

        LogSql(sql, entity);

        using var conn = await _connectionManager.OpenConnectionAsync(_connectionName, cancellationToken);
        await conn.ExecuteAsync(
            new CommandDefinition(sql, entity, commandTimeout: _options.CommandTimeout, cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Deletes an entity by primary key using a parameterized <c>DELETE</c> query.
    /// Default SQL: <c>DELETE FROM {TableName} WHERE Id = @Id</c>
    /// </summary>
    /// <param name="id">The primary key value of the entity to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="KeyNotFoundException">Thrown when no entity with the given id exists.</exception>
    /// <example>
    /// <code>
    /// await repository.DeleteAsync(42);
    /// </code>
    /// </example>
    public async Task DeleteAsync(object id, CancellationToken cancellationToken = default)
    {
        var sql = SqlBuilder.BuildDelete(_metadata);
        var parameters = new { Id = id };

        LogSql(sql, parameters);

        using var conn = await _connectionManager.OpenConnectionAsync(_connectionName, cancellationToken);
        var rowsAffected = await conn.ExecuteAsync(
            new CommandDefinition(sql, parameters, commandTimeout: _options.CommandTimeout, cancellationToken: cancellationToken));

        if (rowsAffected == 0)
        {
            throw new KeyNotFoundException($"[{DapperErrorCodes.EntityNotFound}] No entity of type '{typeof(T).Name}' with id '{id}' was found.");
        }
    }

    /// <summary>
    /// Gets the valid sort columns for column whitelist enforcement.
    /// </summary>
    internal static IReadOnlySet<string> GetValidSortColumns() =>
        ValidSortColumnsCache.TryGetValue(typeof(T), out var columns)
            ? columns
            : new HashSet<string>();

    private void LogSql(string sql, object? parameters)
    {
        if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
        {
            // Note: In production, parameters should not be logged as they may contain PII.
            // This is controlled by the EnableSqlLogging option.
            _logger.LogDebug("Executing SQL: {Sql}", sql);
        }
    }
}
