using System.Diagnostics;
using NextNet.Data.Exceptions;
using NextNet.Data.MongoDB.Internal;

namespace NextNet.Data.MongoDB;

/// <summary>
/// MongoDB implementation of <see cref="Abstractions.Abstractions.IDataProvider"/>
/// and <see cref="Data.IDataProvider"/>. Manages <see cref="MongoClient"/> instances,
/// database resolution, collection access, migration lifecycle, and health checks for
/// MongoDB-backed document stores.
/// </summary>
/// <remarks>
/// <para>
/// Register via <c>services.AddNextNetData().UseMongoDB()</c>.
/// The provider is initialized once during application startup and manages
/// a pool of <see cref="MongoClient"/> instances per named connection.
/// </para>
/// <para>
/// MongoDB connections use a connection pool managed by the driver
/// (<c>MongoClientSettings.MaxConnectionPoolSize</c>). Each named connection
/// gets its own <c>MongoClient</c> instance, which internally manages
/// a pool of TCP sockets to the MongoDB server(s).
/// </para>
/// <para>
/// BSON serialization is configured via a convention pack that applies
/// camelCase element names, ignores extra elements, and enables
/// string representation for <c>ObjectId</c> properties by default.
/// </para>
/// </remarks>
[ProviderMetadata(
    "MongoDB",
    "MongoDB 7.0+",
    "Document database provider built on MongoDB.Driver — schema-less, BSON-based document persistence",
    PackageName = "NextNet.Data.MongoDB",
    CliCommand = "nextnet add data mongo",
    SupportedDatabases = new[] { "MongoDB 7.0+", "MongoDB Atlas", "Azure CosmosDB for MongoDB", "AWS DocumentDB" },
    SupportsMigrations = false,
    SupportsRepositories = true)]
public sealed class MongoDbProvider : NextNet.Data.Abstractions.Abstractions.IDataProvider,
    NextNet.Data.IDataProvider
{
    private MongoClientManager? _clientManager;
    private MongoDbOptions? _options;
    private MongoDbConventionOptions? _conventionOptions;
    private DataConfig? _config;
    private bool _initialized;
    private readonly ILogger<MongoDbProvider> _logger;

    /// <summary>
    /// Gets the unique provider name "MongoDB".
    /// </summary>
    public string Name => "MongoDB";

    /// <summary>
    /// Gets the display-friendly label for this provider.
    /// </summary>
    public string DisplayName => "MongoDB 7.0+";

    /// <summary>
    /// Gets the provider version matching the assembly version.
    /// </summary>
    public Version Version { get; } = typeof(MongoDbProvider).Assembly.GetName().Version ?? new Version(0, 1, 0);

    /// <summary>
    /// Gets the client manager instance, if initialized.
    /// </summary>
    internal MongoClientManager? ClientManager => _clientManager;

    /// <summary>
    /// Gets the provider options.
    /// </summary>
    internal MongoDbOptions? Options => _options;

    /// <summary>
    /// Gets the BSON convention options.
    /// </summary>
    internal MongoDbConventionOptions? ConventionOptions => _conventionOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="MongoDbProvider"/> class.
    /// </summary>
    /// <param name="options">The MongoDB provider options.</param>
    /// <param name="logger">The logger for provider diagnostics.</param>
    /// <param name="conventionOptions">Optional BSON convention options.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
    public MongoDbProvider(
        MongoDbOptions options,
        ILogger<MongoDbProvider> logger,
        MongoDbConventionOptions? conventionOptions = null)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _conventionOptions = conventionOptions;
    }

    /// <summary>
    /// Initializes the MongoDB provider with the given configuration.
    /// </summary>
    /// <param name="config">The data configuration containing connection and migration settings.</param>
    /// <param name="cancellationToken">A token to cancel initialization.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="config"/> is null.</exception>
    /// <exception cref="ProviderConfigurationException">Thrown when required configuration is missing or invalid.</exception>
    Task NextNet.Data.Abstractions.Abstractions.IDataProvider.InitializeAsync(DataConfig config, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(config);

        if (_initialized)
        {
            _logger.LogDebug("MongoDbProvider is already initialized. Skipping.");
            return Task.CompletedTask;
        }

        _config = config;

        _logger.LogInformation(
            "Initializing MongoDB provider with connection '{ConnectionName}'...",
            _options!.ConnectionName);

        // Register BSON conventions
        BsonConventionRegistrar.Register(_conventionOptions);

        // Create client manager from config
        var connections = config.Connections;
        if (connections is null || connections.Count == 0)
        {
            _logger.LogWarning("No connections configured. MongoDB provider initialized without connections.");
        }
        else
        {
            // Validate connections
            foreach (var (name, connectionConfig) in connections)
            {
                if (connectionConfig.Enabled)
                {
                    DefaultConnectionStrings.Validate(
                        connectionConfig.ConnectionString,
                        _options.DefaultDatabaseName,
                        name);
                }
            }

            _clientManager = new MongoClientManager(
                connections,
                _options,
                _conventionOptions,
                _logger as ILogger<MongoClientManager>);
        }

        _initialized = true;
        _logger.LogInformation("MongoDB provider initialized successfully.");
        return Task.CompletedTask;
    }

    /// <summary>
    /// Initializes the provider without explicit config (uses stored options).
    /// Called by the provider initialization hosted service.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel initialization.</param>
    Task NextNet.Data.IDataProvider.InitializeAsync(CancellationToken cancellationToken)
    {
        if (_config is null)
        {
            _logger.LogInformation("MongoDbProvider initialized with stored options.");
            _initialized = true;
            return Task.CompletedTask;
        }

        return ((NextNet.Data.Abstractions.Abstractions.IDataProvider)this)
            .InitializeAsync(_config, cancellationToken);
    }

    /// <summary>
    /// Checks whether all configured MongoDB connections are reachable
    /// by executing the <c>{ ping: 1 }</c> admin command on each.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the health check.</param>
    /// <returns>A health check result summarizing connection status across all databases.</returns>
    async Task<HealthCheckResult> NextNet.Data.Abstractions.Abstractions.IDataProvider.IsHealthyAsync(CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        if (_clientManager is null)
        {
            stopwatch.Stop();
            return new HealthCheckResult(
                false,
                "Unhealthy",
                stopwatch.Elapsed,
                "Client manager not initialized.",
                new Dictionary<string, object> { ["error"] = "ClientManager is null" });
        }

        try
        {
            var results = await _clientManager.PingAllAsync(cancellationToken);
            stopwatch.Stop();

            var healthyCount = results.Count(r => r.Value.IsHealthy);
            var totalCount = results.Count;

            var diagnostics = results.ToDictionary(
                r => r.Key,
                r => (object)new { healthy = r.Value.IsHealthy, latencyMs = r.Value.Latency.TotalMilliseconds, error = r.Value.Error });

            var isHealthy = healthyCount == totalCount;
            var status = isHealthy ? "Healthy" : healthyCount > 0 ? "Degraded" : "Unhealthy";

            return new HealthCheckResult(
                isHealthy,
                status,
                stopwatch.Elapsed,
                $"{healthyCount} of {totalCount} connection(s) are healthy.",
                diagnostics);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return new HealthCheckResult(
                false,
                "Unhealthy",
                stopwatch.Elapsed,
                ex.Message,
                new Dictionary<string, object> { ["error"] = ex.ToString() });
        }
    }

    /// <summary>
    /// Checks whether the provider is in a healthy state.
    /// Called by health-check middleware, CLI diagnostics, and the admin dashboard.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the health check operation.</param>
    /// <returns>A <see cref="DataProviderHealthResult"/> indicating the health status.</returns>
    async Task<DataProviderHealthResult> NextNet.Data.IDataProvider.IsHealthyAsync(CancellationToken cancellationToken)
    {
        var result = await ((NextNet.Data.Abstractions.Abstractions.IDataProvider)this)
            .IsHealthyAsync(cancellationToken);

        if (result.IsHealthy)
        {
            return DataProviderHealthResult.Healthy(result.Message);
        }

        return DataProviderHealthResult.Unhealthy(result.Message ?? "Health check failed");
    }
}
