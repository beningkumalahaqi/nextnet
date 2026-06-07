using System.Diagnostics;

namespace NextNet.Data.MongoDB;

/// <summary>
/// MongoDB implementation of <see cref="IHealthCheckProvider"/>.
/// Verifies database connectivity by executing the <c>{ ping: 1 }</c>
/// administrative command against each configured database.
/// </summary>
/// <remarks>
/// <para>
/// The health check obtains the <c>IMongoDatabase</c> from <see cref="MongoClientManager"/>,
/// sends a <c>{ ping: 1 }</c> command to the server, measures latency,
/// and reports per-connection status.
/// </para>
/// <para>
/// If multiple connections are configured, an unhealthy connection degrades but
/// does not fail the entire health check (aggregate reporting).
/// </para>
/// <para>
/// Diagnostics include the following tags: <c>["nextnet", "data", "mongodb"]</c>.
/// </para>
/// </remarks>
public sealed class MongoDbHealthCheck : IHealthCheckProvider
{
    private readonly MongoClientManager _clientManager;
    private readonly IReadOnlyList<string> _connectionNames;
    private readonly ILogger<MongoDbHealthCheck> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="MongoDbHealthCheck"/>.
    /// </summary>
    /// <param name="clientManager">The MongoDB client manager.</param>
    /// <param name="connectionNames">The names of connections to check.</param>
    /// <param name="logger">Optional logger for health check diagnostics.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="clientManager"/> or <paramref name="connectionNames"/> is null.</exception>
    public MongoDbHealthCheck(
        MongoClientManager clientManager,
        IReadOnlyList<string> connectionNames,
        ILogger<MongoDbHealthCheck>? logger = null)
    {
        _clientManager = clientManager ?? throw new ArgumentNullException(nameof(clientManager));
        _connectionNames = connectionNames ?? throw new ArgumentNullException(nameof(connectionNames));
        _logger = logger ?? new NullLogger<MongoDbHealthCheck>();
    }

    /// <summary>
    /// Gets the display name "MongoDB" for this health check provider.
    /// </summary>
    public string Name => "MongoDB";

    /// <summary>
    /// Executes a health check against all configured MongoDB databases.
    /// Each connection is tested by running the <c>{ ping: 1 }</c> command.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="HealthCheckResult"/> with aggregate status and per-connection diagnostics.</returns>
    public async Task<HealthCheckResult> GetHealthCheckAsync(CancellationToken cancellationToken = default)
    {
        var overallStopwatch = Stopwatch.StartNew();
        var connectionResults = new Dictionary<string, object>();
        var healthyCount = 0;
        var totalCount = _connectionNames.Count;

        // No connections configured means unhealthy (nothing to check)
        if (totalCount == 0)
        {
            overallStopwatch.Stop();
            _logger.LogWarning("MongoDB health check: no connections configured.");
            return new HealthCheckResult(
                false,
                "Unhealthy",
                overallStopwatch.Elapsed,
                "No connections configured to check.",
                new Dictionary<string, object> { { "tags", new[] { "nextnet", "data", "mongodb" } } });
        }

        _logger.LogDebug("Running MongoDB health check for {ConnectionCount} connection(s)...", totalCount);

        foreach (var connectionName in _connectionNames)
        {
            var connectionStopwatch = Stopwatch.StartNew();

            try
            {
                var database = await _clientManager.GetDatabaseAsync(connectionName, cancellationToken);
                var command = new BsonDocument("ping", 1);
                var result = await database.RunCommandAsync<BsonDocument>(command, cancellationToken: cancellationToken);
                connectionStopwatch.Stop();

                var ok = result.GetValue("ok", 0.0).ToDouble();

                if (Math.Abs(ok - 1.0) < 0.001)
                {
                    healthyCount++;
                    connectionResults[connectionName] = new
                    {
                        status = "Healthy",
                        latencyMs = connectionStopwatch.ElapsedMilliseconds,
                        ping = ok,
                        tags = new[] { "nextnet", "data", "mongodb" },
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
                        error = $"Ping returned ok={ok}, expected 1.0.",
                        tags = new[] { "nextnet", "data", "mongodb" },
                        checkedAt = DateTimeOffset.UtcNow
                    };
                    _logger.LogWarning("Connection '{ConnectionName}' is unhealthy (ok={Ok}).", connectionName, ok);
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
                    tags = new[] { "nextnet", "data", "mongodb" },
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
            "MongoDB health check completed: {Status} ({HealthyCount}/{TotalCount} connections healthy, {Duration}ms)",
            overallStatus, healthyCount, totalCount, overallStopwatch.ElapsedMilliseconds);

        return new HealthCheckResult(
            isHealthy,
            overallStatus,
            overallStopwatch.Elapsed,
            $"{healthyCount} of {totalCount} connection(s) are healthy.",
            connectionResults);
    }
}
