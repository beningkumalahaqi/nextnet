using System.Collections.Concurrent;
using NextNet.Data.Exceptions;
using NextNet.Data.MongoDB.Internal;

namespace NextNet.Data.MongoDB;

/// <summary>
/// Manages named <see cref="MongoClient"/> instances for the MongoDB provider.
/// Each named connection string maps to a dedicated <c>MongoClient</c> with
/// its own connection pool and settings.
/// </summary>
/// <remarks>
/// <para>
/// <c>MongoClient</c> instances are created once and reused for the application
/// lifetime. The MongoDB driver manages an internal pool of TCP connections
/// per client. Clients are thread-safe and designed for singleton usage.
/// </para>
/// <para>
/// This manager resolves the correct <see cref="IMongoDatabase"/> for a given
/// named connection and database name pair. Database names are resolved from
/// the MongoDB connection URI or from <see cref="MongoDbOptions.DefaultDatabaseName"/>.
/// </para>
/// <para>
/// Thread-safe: all operations are synchronised via <see cref="ConcurrentDictionary{TKey, TValue}"/>
/// and lazy initialization patterns. Designed for singleton lifetime.
/// </para>
/// <example>
/// <code>
/// var database = await clientManager.GetDatabaseAsync("Default");
/// var collection = database.GetCollection&lt;User&gt;("users");
/// </code>
/// </example>
/// </remarks>
public sealed class MongoClientManager : IDisposable
{
    private readonly IReadOnlyDictionary<string, ConnectionConfig> _connections;
    private readonly MongoDbOptions _options;
    private readonly MongoDbConventionOptions? _conventionOptions;
    private readonly ILogger<MongoClientManager> _logger;
    private readonly ConcurrentDictionary<string, MongoClient> _clients = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, IMongoDatabase> _databases = new(StringComparer.OrdinalIgnoreCase);
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of <see cref="MongoClientManager"/>.
    /// </summary>
    /// <param name="connections">The named connection configurations.</param>
    /// <param name="options">Provider options controlling client settings and BSON conventions.</param>
    /// <param name="conventionOptions">Optional BSON convention options.</param>
    /// <param name="logger">Optional logger for connection events.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="connections"/> is null.</exception>
    public MongoClientManager(
        IReadOnlyDictionary<string, ConnectionConfig> connections,
        MongoDbOptions? options = null,
        MongoDbConventionOptions? conventionOptions = null,
        ILogger<MongoClientManager>? logger = null)
    {
        _connections = connections ?? throw new ArgumentNullException(nameof(connections));
        _options = options ?? new MongoDbOptions();
        _conventionOptions = conventionOptions;
        _logger = logger ?? new NullLogger<MongoClientManager>();

        // Register BSON conventions on first use
        BsonConventionRegistrar.Register(_conventionOptions);
    }

    /// <summary>
    /// Gets the <see cref="IMongoDatabase"/> for the specified named connection.
    /// The database name is resolved from the connection URI or
    /// <see cref="MongoDbOptions.DefaultDatabaseName"/>.
    /// </summary>
    /// <param name="connectionName">The logical connection name (e.g., "Default", "Analytics").</param>
    /// <param name="cancellationToken">A token to cancel the operation (used for DNS resolution).</param>
    /// <returns>The MongoDB database instance.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when no connection with the given name is registered.</exception>
    public Task<IMongoDatabase> GetDatabaseAsync(string connectionName, CancellationToken cancellationToken = default)
    {
        return GetDatabaseAsync(connectionName, databaseName: null, cancellationToken);
    }

    /// <summary>
    /// Gets the <see cref="IMongoDatabase"/> for the specified named connection and database name.
    /// Overrides the database name from the connection URI or default.
    /// </summary>
    /// <param name="connectionName">The logical connection name.</param>
    /// <param name="databaseName">The explicit database name to use. If null, resolves from connection URI or default.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The MongoDB database instance.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when no connection with the given name is registered.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the manager has been disposed.</exception>
    public Task<IMongoDatabase> GetDatabaseAsync(string connectionName, string? databaseName, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var cacheKey = databaseName is not null
            ? $"{connectionName}|{databaseName}"
            : connectionName;

        // Use GetOrAdd for thread-safe lazy initialization
        var database = _databases.GetOrAdd(cacheKey, _ =>
        {
            var client = GetOrCreateClient(connectionName);
            var resolvedDbName = databaseName ?? ResolveDatabaseName(connectionName);
            return client.GetDatabase(resolvedDbName);
        });

        return Task.FromResult(database);
    }

    /// <summary>
    /// Gets the <see cref="IMongoCollection{T}"/> for the specified entity type.
    /// Collection name is resolved from <see cref="CollectionNameAttribute"/> on <typeparamref name="T"/>,
    /// or from <see cref="MongoDbRepositoryOptions.CollectionName"/>, or defaults to the
    /// pluralized, camelCase version of the type name.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="connectionName">The logical connection name.</param>
    /// <param name="repositoryOptions">Optional per-repository options.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The MongoDB collection instance.</returns>
    public async Task<IMongoCollection<T>> GetCollectionAsync<T>(
        string connectionName,
        MongoDbRepositoryOptions? repositoryOptions = null,
        CancellationToken cancellationToken = default) where T : class
    {
        var effectiveConnectionName = repositoryOptions?.ConnectionName ?? connectionName;
        var database = await GetDatabaseAsync(effectiveConnectionName, cancellationToken);
        var collectionName = CollectionNameResolver.Resolve<T>(repositoryOptions);
        return database.GetCollection<T>(collectionName);
    }

    /// <summary>
    /// Pings all configured MongoDB servers by running the <c>{ ping: 1 }</c> command
    /// against the admin database.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A dictionary mapping connection name to health status with latency and optional error.</returns>
    public async Task<IReadOnlyDictionary<string, (bool IsHealthy, TimeSpan Latency, string? Error)>> PingAllAsync(
        CancellationToken cancellationToken = default)
    {
        var results = new Dictionary<string, (bool IsHealthy, TimeSpan Latency, string? Error)>(
            StringComparer.OrdinalIgnoreCase);

        foreach (var (name, config) in _connections)
        {
            if (!config.Enabled)
            {
                results[name] = (true, TimeSpan.Zero, "Connection is disabled, skipped.");
                continue;
            }

            var stopwatch = Stopwatch.StartNew();

            try
            {
                var client = GetOrCreateClient(name);
                var database = client.GetDatabase("admin");
                var command = new BsonDocument("ping", 1);
                var result = await database.RunCommandAsync<BsonDocument>(command, cancellationToken: cancellationToken);
                stopwatch.Stop();

                var ok = result.GetValue("ok", 0.0).ToDouble();
                var isHealthy = Math.Abs(ok - 1.0) < 0.001;

                results[name] = (isHealthy, stopwatch.Elapsed, isHealthy ? null : $"Ping returned ok={ok}");
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                results[name] = (false, stopwatch.Elapsed, ex.Message);
                _logger.LogWarning(ex, "Ping failed for connection '{ConnectionName}'.", name);
            }
        }

        return results;
    }

    /// <summary>
    /// Gets the connection string for the specified named connection.
    /// </summary>
    /// <param name="connectionName">The logical connection name.</param>
    /// <returns>The connection string.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when no connection with the given name is registered.</exception>
    public string GetConnectionString(string connectionName)
    {
        if (!_connections.TryGetValue(connectionName, out var config))
        {
            throw new KeyNotFoundException($"No connection with name '{connectionName}' is registered.");
        }

        return config.ConnectionString;
    }

    /// <summary>
    /// Gets all registered connection names.
    /// </summary>
    /// <returns>A read-only collection of connection names.</returns>
    public IReadOnlyCollection<string> GetConnectionNames() =>
        _connections.Keys.ToList().AsReadOnly();

    /// <summary>
    /// Disposes all managed <see cref="MongoClient"/> instances.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        foreach (var client in _clients.Values)
        {
            try
            {
                if (client is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error disposing MongoClient instance.");
            }
        }

        _clients.Clear();
        _databases.Clear();
    }

    private MongoClient GetOrCreateClient(string connectionName)
    {
        return _clients.GetOrAdd(connectionName, name =>
        {
            if (!_connections.TryGetValue(name, out var config))
            {
                throw new KeyNotFoundException($"No connection with name '{name}' is registered. " +
                    "Ensure the connection is defined in nextnet.config.json or added via WithConnection().");
            }

            _logger.LogDebug("Creating MongoClient for connection '{ConnectionName}'...", name);
            var settings = ClientSettingsBuilder.Build(config.ConnectionString, _options);
            return new MongoClient(settings);
        });
    }

    private string ResolveDatabaseName(string connectionName)
    {
        var connectionString = GetConnectionString(connectionName);
            return DefaultConnectionStrings.ResolveDatabaseName(connectionString, _options.DefaultDatabaseName)
            ?? throw new ProviderConfigurationException(
                connectionName,
                $"No database name specified for connection '{connectionName}'. " +
                "Include a database name in the connection URI or set MongoDbOptions.DefaultDatabaseName.");
    }
}
