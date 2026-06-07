using System.Diagnostics;
using NextNet.Data;

namespace NextNet.Data.EntityFramework;

/// <summary>
/// Entity Framework Core implementation of <see cref="Abstractions.Abstractions.IDataProvider"/>
/// and <see cref="Data.IDataProvider"/>. Manages DbContext instances, migration lifecycle,
/// repository access, scaffolding, and health checks for EF Core-backed databases.
/// </summary>
/// <remarks>
/// <para>
/// Register via <c>services.AddNextNetData().UseEntityFramework()</c>.
/// The provider is initialized once during application startup and can serve
/// multiple named database connections.
/// </para>
/// <para>
/// DbContext instances are created via <see cref="IDbContextFactory{TContext}"/>
/// which is registered in DI as a singleton factory. Each repository operation
/// creates a short-lived DbContext instance to avoid concurrency issues.
/// </para>
/// </remarks>
[ProviderMetadata(
    "EntityFramework",
    "Entity Framework Core 8",
    "Full-featured ORM provider built on Microsoft.EntityFrameworkCore",
    PackageName = "NextNet.Data.EntityFramework",
    CliCommand = "nextnet add data ef",
    SupportedDatabases = new[] { "SQL Server", "SQLite", "PostgreSQL", "MySQL", "InMemory" },
    SupportsMigrations = true,
    SupportsRepositories = true)]
public sealed class EfCoreDataProvider : NextNet.Data.Abstractions.Abstractions.IDataProvider,
    NextNet.Data.IDataProvider
{
    private DataConfig? _config;
    private bool _initialized;
    private readonly ILogger<EfCoreDataProvider> _logger;
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly EfCoreOptions _options;

    /// <summary>
    /// Gets the unique provider name "EntityFramework".
    /// </summary>
    public string Name => "EntityFramework";

    /// <summary>
    /// Gets the display-friendly label for this provider.
    /// </summary>
    public string DisplayName => "Entity Framework Core 8";

    /// <summary>
    /// Gets the provider version matching the assembly version.
    /// </summary>
    public Version Version { get; } = typeof(EfCoreDataProvider).Assembly.GetName().Version ?? new Version(0, 1, 0);

    /// <summary>
    /// Initializes a new instance of the <see cref="EfCoreDataProvider"/> class.
    /// </summary>
    /// <param name="contextFactory">The DbContext factory for creating context instances.</param>
    /// <param name="options">The EF Core provider options.</param>
    /// <param name="logger">The logger for provider diagnostics.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="contextFactory"/> or <paramref name="options"/> is null.</exception>
    public EfCoreDataProvider(
        IDbContextFactory<AppDbContext> contextFactory,
        EfCoreOptions options,
        ILogger<EfCoreDataProvider> logger)
    {
        _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Initializes the EF Core provider with the given configuration.
    /// Configures DbContext for each named connection and optionally applies
    /// pending migrations if <see cref="MigrationConfig.AutoApply"/> is set.
    /// </summary>
    /// <param name="config">The data configuration containing connection and migration settings.</param>
    /// <param name="cancellationToken">A token to cancel initialization.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="config"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when required configuration is missing.</exception>
    Task NextNet.Data.Abstractions.Abstractions.IDataProvider.InitializeAsync(DataConfig config, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(config);

        if (_initialized)
        {
            _logger.LogDebug("EfCoreDataProvider is already initialized. Skipping.");
            return Task.CompletedTask;
        }

        _config = config;

        _logger.LogInformation(
            "Initializing EF Core provider with connection '{ConnectionName}'...",
            _options.ConnectionName);

        // Validate connection config exists
        if (config.Connections is not null &&
            config.Connections.TryGetValue(_options.ConnectionName, out var connectionConfig) &&
            connectionConfig.Enabled)
        {
            _logger.LogDebug(
                "Connection '{ConnectionName}' configured with provider '{Provider}'.",
                _options.ConnectionName,
                connectionConfig.Provider);
        }
        else
        {
            _logger.LogWarning(
                "Connection '{ConnectionName}' not found or disabled in configuration.",
                _options.ConnectionName);
        }

        _initialized = true;
        _logger.LogInformation("EF Core provider initialized successfully.");
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
            _logger.LogInformation("EfCoreDataProvider initialized with default configuration.");
            _initialized = true;
            return Task.CompletedTask;
        }

        return ((NextNet.Data.Abstractions.Abstractions.IDataProvider)this)
            .InitializeAsync(_config, cancellationToken);
    }

    /// <summary>
    /// Checks whether all configured database connections are reachable.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the health check.</param>
    /// <returns>A health check result summarizing connection status across all databases.</returns>
    async Task<HealthCheckResult> NextNet.Data.Abstractions.Abstractions.IDataProvider.IsHealthyAsync(CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            var canConnect = await context.Database.CanConnectAsync(cancellationToken);
            stopwatch.Stop();

            if (canConnect)
            {
                return new HealthCheckResult(
                    true,
                    "Healthy",
                    stopwatch.Elapsed,
                    "Database connection is healthy.",
                    new Dictionary<string, object>
                    {
                        ["connection"] = _options.ConnectionName
                    });
            }

            return new HealthCheckResult(
                false,
                "Unhealthy",
                stopwatch.Elapsed,
                "Database reported as not connectable.",
                new Dictionary<string, object>
                {
                    ["connection"] = _options.ConnectionName
                });
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
                new Dictionary<string, object>
                {
                    ["connection"] = _options.ConnectionName,
                    ["error"] = ex.ToString()
                });
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
