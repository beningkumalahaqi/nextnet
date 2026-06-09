using System.Collections.Concurrent;

namespace NextNet.Build.Production.Logging;

/// <summary>
/// Collects runtime metrics about HTTP requests and system performance.
/// Provides in-memory aggregation suitable for diagnostics dashboards.
/// </summary>
public sealed class MetricsCollector
{
    private readonly ConcurrentDictionary<string, MethodMetrics> _endpointMetrics = new();
    private long _totalRequests;
    private long _totalErrors;

    /// <summary>
    /// Records a request metric.
    /// </summary>
    public void RecordRequest(string method, string path, int statusCode, long durationMs, long? size = null)
    {
        Interlocked.Increment(ref _totalRequests);

        if (statusCode >= 500)
            Interlocked.Increment(ref _totalErrors);

        var key = $"{method}:{path}";

        _endpointMetrics.AddOrUpdate(key,
            _ => new MethodMetrics
            {
                Method = method,
                Path = path,
                Count = 1,
                TotalDurationMs = durationMs,
                MaxDurationMs = durationMs,
                MinDurationMs = durationMs,
                LastStatusCode = statusCode,
            },
            (_, existing) =>
            {
                existing.Count++;
                existing.TotalDurationMs += durationMs;
                if (durationMs > existing.MaxDurationMs) existing.MaxDurationMs = durationMs;
                if (durationMs < existing.MinDurationMs) existing.MinDurationMs = durationMs;
                existing.LastStatusCode = statusCode;
                return existing;
            });
    }

    /// <summary>
    /// Gets a snapshot of all collected metrics.
    /// </summary>
    public MetricsSnapshot GetSnapshot()
    {
        return new MetricsSnapshot(
            TotalRequests: _totalRequests,
            TotalErrors: _totalErrors,
            ErrorRate: _totalRequests > 0
                ? (double)_totalErrors / _totalRequests * 100
                : 0,
            Endpoints: _endpointMetrics.Values
                .Select(m => new EndpointMetric(
                    Method: m.Method,
                    Path: m.Path,
                    Count: m.Count,
                    AvgDurationMs: m.Count > 0 ? m.TotalDurationMs / m.Count : 0,
                    MaxDurationMs: m.MaxDurationMs,
                    MinDurationMs: m.MinDurationMs,
                    LastStatusCode: m.LastStatusCode))
                .OrderByDescending(e => e.Count)
                .ToList(),
            CollectedAt: DateTime.UtcNow);
    }

    /// <summary>
    /// Resets all collected metrics.
    /// </summary>
    public void Reset()
    {
        _endpointMetrics.Clear();
        _totalRequests = 0;
        _totalErrors = 0;
    }

    private sealed class MethodMetrics
    {
        public string Method { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public long Count { get; set; }
        public long TotalDurationMs { get; set; }
        public long MaxDurationMs { get; set; }
        public long MinDurationMs { get; set; }
        public int LastStatusCode { get; set; }
    }
}

/// <summary>
/// A snapshot of runtime metrics.
/// </summary>
/// <param name="TotalRequests">Total number of requests recorded.</param>
/// <param name="TotalErrors">Total number of 5xx errors recorded.</param>
/// <param name="ErrorRate">Error rate as a percentage.</param>
/// <param name="Endpoints">Per-endpoint metrics.</param>
/// <param name="CollectedAt">When this snapshot was taken.</param>
public sealed record MetricsSnapshot(
    long TotalRequests,
    long TotalErrors,
    double ErrorRate,
    List<EndpointMetric> Endpoints,
    DateTime CollectedAt);

/// <summary>
/// Metrics for a single endpoint.
/// </summary>
/// <param name="Method">HTTP method.</param>
/// <param name="Path">Request path.</param>
/// <param name="Count">Number of requests to this endpoint.</param>
/// <param name="AvgDurationMs">Average response duration in milliseconds.</param>
/// <param name="MaxDurationMs">Maximum response duration in milliseconds.</param>
/// <param name="MinDurationMs">Minimum response duration in milliseconds.</param>
/// <param name="LastStatusCode">The last HTTP status code returned.</param>
public sealed record EndpointMetric(
    string Method,
    string Path,
    long Count,
    double AvgDurationMs,
    long MaxDurationMs,
    long MinDurationMs,
    int LastStatusCode);
