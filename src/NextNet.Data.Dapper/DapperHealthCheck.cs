using System.Diagnostics;

namespace NextNet.Data.Dapper;

/// <summary>
/// Dapper implementation of <see cref="IHealthCheckProvider"/>.
/// Verifies database connectivity by executing a <c>SELECT 1</c> query
/// against each configured connection.
/// </summary>
/// <remarks>
/// <para>
/// The health check opens a new connection via <see cref="DapperConnectionManager"/>,
/// executes <c>SELECT 1</c> using Dapper's <c>ExecuteScalarAsync</c>, measures
/// latency, and reports per-connection status.
/// </para>
/// <para>
/// If multiple connections are configured, an unhealthy connection degrades but
/// does not fail the entire health check (aggregate reporting).
/// </para>
/// </remarks>
public sealed class DapperHealthCheck : IHealthCheckProvider
{
    private readonly DapperConnectionManager _connectionManager;
    private readonly IReadOnlyList<string> _connectionNames;
    private readonly ILogger<DapperHealthCheck> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="DapperHealthCheck"/>.
    /// </summary>
    /// <param name="connectionManager">The connection manager for database access.</param>
    /// <param name="connectionNames">The names of connections to check.</param>
    /// <param name="logger">Optional logger for health check diagnostics.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="connectionManager"/> or <paramref name="connectionNames"/> is null.</exception>
    public DapperHealthCheck(
        DapperConnectionManager connectionManager,
        IReadOnlyList<string> connectionNames,
        ILogger<DapperHealthCheck>? logger = null)
    {
        _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
        _connectionNames = connectionNames ?? throw new ArgumentNullException(nameof(connectionNames));
        _logger = logger ?? new Microsoft.Extensions.Logging.Abstractions.NullLogger<DapperHealthCheck>();
    }

    /// <summary>
    /// Executes a health check against all configured databases.
    /// Each connection is tested by opening and executing <c>SELECT 1</c>.
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

            try
            {
                using var conn = await _connectionManager.OpenConnectionAsync(connectionName, cancellationToken);
                var result = await conn.ExecuteScalarAsync<int>(
                    new CommandDefinition("SELECT 1", commandTimeout: 5, cancellationToken: cancellationToken));
                connectionStopwatch.Stop();

                if (result == 1)
                {
                    healthyCount++;
                    connectionResults[connectionName] = new
                    {
                        status = "Healthy",
                        latencyMs = connectionStopwatch.ElapsedMilliseconds,
                        checkedAt = DateTimeOffset.UtcNow
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
                        error = "SELECT 1 returned unexpected result.",
                        checkedAt = DateTimeOffset.UtcNow
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
                    checkedAt = DateTimeOffset.UtcNow
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
