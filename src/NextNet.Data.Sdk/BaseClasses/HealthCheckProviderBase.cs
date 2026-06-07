#if NET8_0_OR_GREATER
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NextNet.Data.Abstractions.Abstractions;
using NextNet.Data.Abstractions.Configuration;
using NextNet.Data.Abstractions.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace NextNet.Data.Sdk.Base;

/// <summary>
/// Base class for provider-specific health checks. Implements <see cref="IHealthCheckProvider"/>
/// with per-connection dispatch, timeout handling, and aggregate result reporting.
/// </summary>
/// <remarks>
/// <para>
/// Provider authors override <see cref="CheckConnectionAsync"/> to implement the
/// actual connectivity test for their database technology. The base class handles
/// iterating over all configured connections, measuring latency, and producing the
/// aggregate <see cref="HealthCheckResult"/>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class MyCustomHealthCheckProvider : HealthCheckProviderBase
/// {
///     public MyCustomHealthCheckProvider(
///         ConnectionManagerBase connectionManager,
///         IOptions&lt;DataConfig&gt; config,
///         ILogger logger)
///         : base(connectionManager, config, logger) { }
///
///     protected override async Task&lt;HealthCheckResult&gt; CheckConnectionAsync(
///         string connectionName, IDataConnection connection, CancellationToken ct)
///     {
///         var sw = Stopwatch.StartNew();
///         try
///         {
///             var conn = (MyCustomConnection)connection;
///             await conn.PingAsync(ct);
///             sw.Stop();
///             return HealthyResult(connectionName, sw.Elapsed);
///         }
///         catch (Exception ex)
///         {
///             sw.Stop();
///             return UnhealthyResult(connectionName, sw.Elapsed, ex.Message);
///         }
///     }
/// }
/// </code>
/// </example>
public abstract class HealthCheckProviderBase : IHealthCheckProvider
{
    private readonly ConnectionManagerBase _connectionManager;
    private readonly IOptions<DataConfig> _config;
    private readonly ILogger? _logger;

    /// <summary>
    /// Gets the connection manager used for database access.
    /// </summary>
    protected ConnectionManagerBase ConnectionManager => _connectionManager;

    /// <summary>
    /// Gets the data configuration.
    /// </summary>
    protected IOptions<DataConfig> Config => _config;

    /// <summary>
    /// Initializes a new instance of the <see cref="HealthCheckProviderBase"/> class.
    /// </summary>
    /// <param name="connectionManager">The connection manager for database access.</param>
    /// <param name="config">The data configuration (used to enumerate connections).</param>
    /// <param name="logger">An optional logger.</param>
    protected HealthCheckProviderBase(
        ConnectionManagerBase connectionManager,
        IOptions<DataConfig> config,
        ILogger? logger = null)
    {
        _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = logger;
    }

    /// <summary>
    /// Executes health checks against all configured connections and returns an aggregate result.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="HealthCheckResult"/> with aggregate status.</returns>
    public async Task<HealthCheckResult> GetHealthCheckAsync(CancellationToken cancellationToken = default)
    {
        var connections = _config.Value.Connections;
        if (connections == null || connections.Count == 0)
        {
            return new HealthCheckResult(
                IsHealthy: false,
                Status: "Unhealthy",
                Duration: TimeSpan.Zero,
                Message: "No connections configured for health check.");
        }

        var results = new List<HealthCheckResult>();
        var totalSw = Stopwatch.StartNew();

        foreach (var kvp in connections)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var connection = _connectionManager.GetConnection(kvp.Key);
                var result = await CheckConnectionAsync(kvp.Key, connection, cancellationToken).ConfigureAwait(false);
                results.Add(result);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Health check failed for connection '{ConnectionName}'.", kvp.Key);
                results.Add(new HealthCheckResult(
                    IsHealthy: false,
                    Status: "Unhealthy",
                    Duration: TimeSpan.Zero,
                    Message: ex.Message));
            }
        }

        totalSw.Stop();

        var allHealthy = results.All(r => r.IsHealthy);
        var anyHealthy = results.Any(r => r.IsHealthy);
        var status = allHealthy ? "Healthy" : anyHealthy ? "Degraded" : "Unhealthy";

        var data = new Dictionary<string, object>
        {
            ["connections"] = results.Select((r, i) => new { r.IsHealthy, r.Duration, r.Message, Index = i })
                .ToDictionary(
                    r => $"connection_{r.Index}",
                    r => (object)new { r.IsHealthy, r.Duration, r.Message })
        };

        return new HealthCheckResult(
            IsHealthy: allHealthy,
            Status: status,
            Duration: totalSw.Elapsed,
            Message: allHealthy
                ? "All connections are healthy."
                : $"{results.Count(r => !r.IsHealthy)} of {results.Count} connection(s) are unhealthy.",
            Data: data);
    }

    /// <summary>
    /// Checks the health of a single connection. Provider authors implement this
    /// to execute their database-specific connectivity test.
    /// </summary>
    /// <param name="connectionName">The logical name of the connection.</param>
    /// <param name="connection">The connection to test.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="HealthCheckResult"/> for this single connection.</returns>
    protected abstract Task<HealthCheckResult> CheckConnectionAsync(
        string connectionName,
        IDataConnection connection,
        CancellationToken cancellationToken);

    /// <summary>
    /// Creates a healthy result for a single connection.
    /// </summary>
    /// <param name="connectionName">The name of the connection.</param>
    /// <param name="latency">The measured latency.</param>
    /// <returns>A healthy <see cref="HealthCheckResult"/>.</returns>
    protected HealthCheckResult HealthyResult(string connectionName, TimeSpan latency)
    {
        return new HealthCheckResult(
            IsHealthy: true,
            Status: "Healthy",
            Duration: latency,
            Message: $"Connection '{connectionName}' is healthy.",
            Data: new Dictionary<string, object> { ["connection"] = connectionName });
    }

    /// <summary>
    /// Creates an unhealthy result for a single connection.
    /// </summary>
    /// <param name="connectionName">The name of the connection.</param>
    /// <param name="latency">The measured latency.</param>
    /// <param name="error">The error message.</param>
    /// <returns>An unhealthy <see cref="HealthCheckResult"/>.</returns>
    protected HealthCheckResult UnhealthyResult(string connectionName, TimeSpan latency, string error)
    {
        return new HealthCheckResult(
            IsHealthy: false,
            Status: "Unhealthy",
            Duration: latency,
            Message: $"Connection '{connectionName}' is unhealthy: {error}",
            Data: new Dictionary<string, object> { ["connection"] = connectionName, ["error"] = error });
    }
}
#endif
