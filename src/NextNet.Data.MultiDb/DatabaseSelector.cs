using NextNet.Data.MultiDb.Exceptions;
using NextNet.Data.MultiDb.Internal;

namespace NextNet.Data.MultiDb;

/// <summary>
/// Default implementation of <see cref="IDatabaseSelector"/>.
/// Resolves named connections using the <see cref="ConnectionNameRegistry"/>
/// and <see cref="ConnectionPoolRegistry"/>, with optional caching.
/// </summary>
/// <remarks>
/// <para>
/// This is the core multi-database resolver. It is registered as a singleton
/// in the DI container and provides connection resolution via the
/// <c>For("name")</c> pattern. Resolution is thread-safe and supports
/// optional context caching for performance.
/// </para>
/// <para>
/// The selector uses two internal registries:
/// <list type="bullet">
///   <item><description><see cref="ConnectionNameRegistry"/> — maps logical names to provider descriptors</description></item>
///   <item><description><see cref="ConnectionPoolRegistry"/> — manages per-connection pool entries</description></item>
/// </list>
/// </para>
/// <example>
/// <code>
/// public class DashboardController
/// {
///     private readonly IDatabaseSelector _db;
///
///     public DashboardController(IDatabaseSelector db) => _db = db;
///
///     public async Task&lt;DashboardViewModel&gt; GetDashboardAsync()
///     {
///         var primary = _db.For("Primary");
///         var analytics = _db.For("Analytics");
///         // ...
///     }
/// }
/// </code>
/// </example>
/// </remarks>
internal sealed class DatabaseSelector : IDatabaseSelector, IDisposable
{
    private readonly ConnectionNameRegistry _nameRegistry;
    private readonly ConnectionPoolRegistry _poolRegistry;
    private readonly DatabaseContextFactory _contextFactory;
    private readonly MultiDbOptions _options;
    private readonly ILogger<DatabaseSelector> _logger;
    private readonly ConcurrentCache<string, IDatabaseContext>? _cache;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseSelector"/> class.
    /// </summary>
    /// <param name="nameRegistry">The connection name registry.</param>
    /// <param name="poolRegistry">The connection pool registry.</param>
    /// <param name="contextFactory">The factory for creating database contexts.</param>
    /// <param name="options">The multi-database options.</param>
    /// <param name="logger">The logger instance.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when any required parameter is <c>null</c>.
    /// </exception>
    public DatabaseSelector(
        ConnectionNameRegistry nameRegistry,
        ConnectionPoolRegistry poolRegistry,
        DatabaseContextFactory contextFactory,
        IOptions<MultiDbOptions> options,
        ILogger<DatabaseSelector> logger)
    {
        _nameRegistry = nameRegistry ?? throw new ArgumentNullException(nameof(nameRegistry));
        _poolRegistry = poolRegistry ?? throw new ArgumentNullException(nameof(poolRegistry));
        _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (_options.CacheContexts)
        {
            _cache = new ConcurrentCache<string, IDatabaseContext>(StringComparer.OrdinalIgnoreCase);
        }
    }

    /// <inheritdoc />
    public IDatabaseContext For(string name)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Connection name must not be null or empty.", nameof(name));

        // Check cache first
        if (_cache is not null && _cache.TryGet(name, out var cachedContext))
        {
            _logger.LogDebug("Returning cached database context for connection '{ConnectionName}'.", name);
            return cachedContext;
        }

        // Look up registration
        if (!_nameRegistry.TryGet(name, out var registration))
        {
            if (_options.FallbackToDefault)
            {
                _logger.LogWarning(
                    "Connection '{ConnectionName}' not found. Falling back to default connection.",
                    name);

                var defaultName = _options.DefaultConnectionName ?? "Default";
                // Avoid infinite recursion: if we're already trying the default, throw
                if (string.Equals(name, defaultName, StringComparison.OrdinalIgnoreCase))
                {
                    throw new MissingConnectionException(name);
                }
                return For(defaultName);
            }

            throw new MissingConnectionException(name);
        }

        // Get pool entry
        ConnectionPoolEntry poolEntry;
        try
        {
            poolEntry = _poolRegistry.Get(name);
        }
        catch (KeyNotFoundException)
        {
            throw new ConnectionUnavailableException(
                name,
                registration.ProviderName,
                "Pool entry not found in connection pool registry.");
        }

        if (!poolEntry.IsEnabled)
        {
            throw new ConnectionUnavailableException(
                name,
                registration.ProviderName,
                "Connection is disabled.");
        }

        // Create context
        var context = _contextFactory.Create(poolEntry);

        // Cache if enabled
        if (_cache is not null)
        {
            _cache.Set(name, context);
            _logger.LogDebug("Cached database context for connection '{ConnectionName}'.", name);
        }

        return context;
    }

    /// <inheritdoc />
    public Task<IDatabaseContext> ForAsync(string name, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(For(name));
    }

    /// <inheritdoc />
    public IDatabaseContext Default
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            var defaultName = _options.DefaultConnectionName;
            return For(defaultName ?? "Default");
        }
    }

    /// <inheritdoc />
    public IReadOnlyCollection<string> ConnectionNames
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return _nameRegistry.Names;
        }
    }

    /// <inheritdoc />
    public bool HasConnection(string name)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return _nameRegistry.Exists(name);
    }

    /// <inheritdoc />
    public IDataConnection GetConnection(string name)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (!_nameRegistry.TryGet(name, out var registration))
        {
            throw new MissingConnectionException(name);
        }

        return new DataConnection(
            registration.ConnectionName,
            registration.ConnectionString,
            registration.ProviderName);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            _poolRegistry.DisposeAll();
            _cache?.Clear();
        }
    }

    /// <summary>
    /// Internal connection record that implements <see cref="IDataConnection"/>.
    /// </summary>
    private sealed record DataConnection : IDataConnection
    {
        public DataConnection(string name, string connectionString, string providerName)
        {
            Name = name;
            ConnectionString = connectionString;
            ProviderName = providerName;
        }

        public string Name { get; }
        public string ConnectionString { get; }
        public string ProviderName { get; }
    }

    /// <summary>
    /// Simple concurrent cache for database contexts.
    /// </summary>
    private sealed class ConcurrentCache<TKey, TValue> where TKey : notnull
    {
        private readonly ConcurrentDictionary<TKey, TValue> _cache;

        public ConcurrentCache(IEqualityComparer<TKey>? comparer = null)
        {
            _cache = new ConcurrentDictionary<TKey, TValue>(comparer ?? EqualityComparer<TKey>.Default);
        }

        public bool TryGet(TKey key, out TValue value)
        {
            return _cache.TryGetValue(key, out value!);
        }

        public void Set(TKey key, TValue value)
        {
            _cache[key] = value;
        }

        public void Clear()
        {
            _cache.Clear();
        }
    }
}
