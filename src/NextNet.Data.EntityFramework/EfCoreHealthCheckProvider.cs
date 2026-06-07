using System.Diagnostics;

namespace NextNet.Data.EntityFramework;

/// <summary>
/// EF Core implementation of <see cref="IHealthCheckProvider"/>.
/// Verifies database connectivity by executing <c>Database.CanConnectAsync()</c>
/// against each configured connection.
/// </summary>
/// <remarks>
/// <para>
/// The health check measures connection latency and reports per-database status.
/// If multiple connections are configured, an unhealthy connection degrades but
/// does not fail the entire health check (aggregate reporting).
/// </para>
/// </remarks>
public sealed class EfCoreHealthCheckProvider : IHealthCheckProvider
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly IReadOnlyList<string> _connectionNames;
    private readonly ILogger<EfCoreHealthCheckProvider> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="EfCoreHealthCheckProvider"/>.
    /// </summary>
    /// <param name="contextFactory">The DbContext factory for creating context instances.</param>
    /// <param name="connectionNames">The names of connections to check.</param>
    /// <param name="logger">Optional logger for health check diagnostics.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="contextFactory"/> or <paramref name="connectionNames"/> is null.</exception>
    public EfCoreHealthCheckProvider(
        IDbContextFactory<AppDbContext> contextFactory,
        IReadOnlyList<string> connectionNames,
        ILogger<EfCoreHealthCheckProvider>? logger = null)
    {
        _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        _connectionNames = connectionNames ?? throw new ArgumentNullException(nameof(connectionNames));
        _logger = logger ?? new Microsoft.Extensions.Logging.Abstractions.NullLogger<EfCoreHealthCheckProvider>();
    }

    /// <summary>
    /// Executes a health check against all configured databases.
    /// Each connection is tested with <c>Database.CanConnectAsync()</c>.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="HealthCheckResult"/> with aggregate status and per-connection diagnostics.</returns>
    public async Task<HealthCheckResult> GetHealthCheckAsync(CancellationToken cancellationToken = default)
    {
        var overallStopwatch = Stopwatch.StartNew();
        var connectionResults = new Dictionary<string, object>();
        var healthyCount = 0;
        var totalCount = _connectionNames.Count;

        _logger.LogDebug("Running health check for {ConnectionCount} connection(s)...", totalCount);

        foreach (var connectionName in _connectionNames)
        {
            var connectionStopwatch = Stopwatch.StartNew();
            var checkedAt = DateTimeOffset.UtcNow;

            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
                var canConnect = await context.Database.CanConnectAsync(cancellationToken);
                connectionStopwatch.Stop();

                if (canConnect)
                {
                    healthyCount++;
                    connectionResults[connectionName] = new
                    {
                        status = "Healthy",
                        latencyMs = connectionStopwatch.ElapsedMilliseconds,
                        checkedAt
                    };
                    _logger.LogDebug("Connection '{ConnectionName}' is healthy ({Latency}ms).",
                        connectionName, connectionStopwatch.ElapsedMilliseconds);
                }
                else
                {
                    connectionResults[connectionName] = new
                    {
                        status = "Unhealthy",
                        latencyMs = connectionStopwatch.ElapsedMilliseconds,
                        error = "Database reported as not connectable.",
                        checkedAt
                    };
                    _logger.LogWarning("Connection '{ConnectionName}' is unhealthy.", connectionName);
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                connectionStopwatch.Stop();
                connectionResults[connectionName] = new
                {
                    status = "Unhealthy",
                    latencyMs = connectionStopwatch.ElapsedMilliseconds,
                    error = ex.Message,
                    checkedAt
                };
                _logger.LogWarning(ex, "Health check failed for connection '{ConnectionName}'.", connectionName);
            }
        }

        overallStopwatch.Stop();

        // Determine aggregate status
        string overallStatus;
        bool isHealthy;

        if (healthyCount == totalCount)
        {
            overallStatus = "Healthy";
            isHealthy = true;
        }
        else if (healthyCount > 0)
        {
            overallStatus = "Degraded";
            isHealthy = false;
        }
        else
        {
            overallStatus = "Unhealthy";
            isHealthy = false;
        }

        _logger.LogInformation(
            "Health check completed: {Status} ({HealthyCount}/{TotalCount} connections healthy, {Duration}ms)",
            overallStatus, healthyCount, totalCount, overallStopwatch.ElapsedMilliseconds);

        return new HealthCheckResult(
            isHealthy,
            overallStatus,
            overallStopwatch.Elapsed,
            $"{healthyCount} of {totalCount} connection(s) are healthy.",
            connectionResults);
    }
}
