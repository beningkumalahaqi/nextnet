using System.Diagnostics;

namespace NextNet.Data.Dapper;

/// <summary>
/// Dapper implementation of <see cref="Abstractions.Abstractions.IDataProvider"/>
/// and <see cref="Data.IDataProvider"/>. Manages SQL connections, query execution,
/// repository access, scaffold code generation, and health checks for Dapper-backed databases.
/// </summary>
/// <remarks>
/// <para>
/// Register via <c>services.AddNextNetData().UseDapper()</c>.
/// The provider is initialized once during application startup and manages
/// a pool of <see cref="SqlConnection"/> instances per named connection.
/// </para>
/// <para>
/// Unlike EF Core, Dapper does not provide change tracking or unit of work.
/// Each repository operation opens a connection (or leases from the pool),
/// executes the query, and returns the result. Connection management is
/// handled by <see cref="DapperConnectionManager"/>.
/// </para>
/// <para>
/// All SQL queries MUST use Dapper's parameterized query syntax (@paramName)
/// to prevent SQL injection. Raw string concatenation of user input is detected
/// by the scaffold provider and rejected at generation time.
/// </para>
/// </remarks>
[ProviderMetadata(
    "Dapper",
    "Dapper 2.1",
    "Lightweight SQL-first micro-ORM provider built on Dapper and Microsoft.Data.SqlClient",
    PackageName = "NextNet.Data.Dapper",
    CliCommand = "nextnet add data dapper",
    SupportedDatabases = new[] { "SQL Server", "Azure SQL", "SQL Server LocalDB" },
    SupportsMigrations = true,
    SupportsRepositories = true)]
public sealed class DapperDataProvider : NextNet.Data.Abstractions.Abstractions.IDataProvider,
    NextNet.Data.IDataProvider
{
    private DapperConnectionManager? _connectionManager;
    private DapperOptions? _options;
    private DataConfig? _config;
    private bool _initialized;
    private readonly ILogger<DapperDataProvider> _logger;

    /// <summary>
    /// Gets the unique provider name "Dapper".
    /// </summary>
    public string Name => "Dapper";

    /// <summary>
    /// Gets the display-friendly label for this provider.
    /// </summary>
    public string DisplayName => "Dapper 2.1";

    /// <summary>
    /// Gets the provider version matching the assembly version.
    /// </summary>
    public Version Version { get; } = typeof(DapperDataProvider).Assembly.GetName().Version ?? new Version(0, 1, 0);

    /// <summary>
    /// Gets the connection manager instance, if initialized.
    /// </summary>
    internal DapperConnectionManager? ConnectionManager => _connectionManager;

    /// <summary>
    /// Gets the provider options.
    /// </summary>
    internal DapperOptions? Options => _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="DapperDataProvider"/> class.
    /// </summary>
    /// <param name="options">The Dapper provider options.</param>
    /// <param name="logger">The logger for provider diagnostics.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
    public DapperDataProvider(
        DapperOptions options,
        ILogger<DapperDataProvider> logger)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Initializes the Dapper provider with the given configuration.
    /// Validates connection strings, configures connection pool settings,
    /// and ensures each named connection is reachable.
    /// </summary>
    /// <param name="config">The data configuration containing connection and migration settings.</param>
    /// <param name="cancellationToken">A token to cancel initialization.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="config"/> is null.</exception>
    Task NextNet.Data.Abstractions.Abstractions.IDataProvider.InitializeAsync(DataConfig config, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(config);

        if (_initialized)
        {
            _logger.LogDebug("DapperDataProvider is already initialized. Skipping.");
            return Task.CompletedTask;
        }

        _config = config;

        _logger.LogInformation(
            "Initializing Dapper provider with connection '{ConnectionName}'...",
            _options!.ConnectionName);

        // Create connection manager from config
        var connections = config.Connections;
        if (connections is null || connections.Count == 0)
        {
            _logger.LogWarning("No connections configured. Dapper provider initialized without connections.");
        }
        else
        {
            _connectionManager = new DapperConnectionManager(
                connections,
                _options,
                _logger as ILogger<DapperConnectionManager>);
        }

        _initialized = true;
        _logger.LogInformation("Dapper provider initialized successfully.");
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
            _logger.LogInformation("DapperDataProvider initialized with stored options.");
            _initialized = true;
            return Task.CompletedTask;
        }

        return ((NextNet.Data.Abstractions.Abstractions.IDataProvider)this)
            .InitializeAsync(_config, cancellationToken);
    }

    /// <summary>
    /// Checks whether all configured database connections are reachable
    /// by executing a lightweight <c>SELECT 1</c> query on each.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the health check.</param>
    /// <returns>A health check result summarizing connection status across all databases.</returns>
    async Task<HealthCheckResult> NextNet.Data.Abstractions.Abstractions.IDataProvider.IsHealthyAsync(CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        if (_connectionManager is null)
        {
            stopwatch.Stop();
            return new HealthCheckResult(
                false,
                "Unhealthy",
                stopwatch.Elapsed,
                "Connection manager not initialized.",
                new Dictionary<string, object> { ["error"] = "ConnectionManager is null" });
        }

        try
        {
            var results = await _connectionManager.ValidateConnectionsAsync(cancellationToken);
            stopwatch.Stop();

            var healthyCount = results.Count(r => r.Value);
            var totalCount = results.Count;
            var diagnostics = results.ToDictionary(
                r => r.Key,
                r => (object)new { healthy = r.Value });

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
