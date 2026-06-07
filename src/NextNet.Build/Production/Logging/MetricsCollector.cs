using System.Collections.Concurrent;

namespace NextNet.Build.Production.Logging;

/// <summary>
/// Collects runtime metrics about HTTP requests and system performance.
/// Provides in-memory aggregation suitable for diagnostics dashboards.
/// </summary>
public class MetricsCollector
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
        return new MetricsSnapshot
        {
            TotalRequests = _totalRequests,
            TotalErrors = _totalErrors,
            ErrorRate = _totalRequests > 0
                ? (double)_totalErrors / _totalRequests * 100
                : 0,
            Endpoints = _endpointMetrics.Values
                .Select(m => new EndpointMetric
                {
                    Method = m.Method,
                    Path = m.Path,
                    Count = m.Count,
                    AvgDurationMs = m.Count > 0 ? m.TotalDurationMs / m.Count : 0,
                    MaxDurationMs = m.MaxDurationMs,
                    MinDurationMs = m.MinDurationMs,
                    LastStatusCode = m.LastStatusCode,
                })
                .OrderByDescending(e => e.Count)
                .ToList(),
            CollectedAt = DateTime.UtcNow,
        };
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

    private class MethodMetrics
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
public class MetricsSnapshot
{
    /// <summary>
    /// Total number of requests recorded.
    /// </summary>
    public long TotalRequests { get; set; }

    /// <summary>
    /// Total number of 5xx errors recorded.
    /// </summary>
    public long TotalErrors { get; set; }

    /// <summary>
    /// Error rate as a percentage.
    /// </summary>
    public double ErrorRate { get; set; }

    /// <summary>
    /// Per-endpoint metrics.
    /// </summary>
    public List<EndpointMetric> Endpoints { get; set; } = new();

    /// <summary>
    /// When this snapshot was taken.
    /// </summary>
    public DateTime CollectedAt { get; set; }
}

/// <summary>
/// Metrics for a single endpoint.
/// </summary>
public class EndpointMetric
{
    /// <summary>
    /// HTTP method.
    /// </summary>
    public string Method { get; set; } = string.Empty;

    /// <summary>
    /// Request path.
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Number of requests to this endpoint.
    /// </summary>
    public long Count { get; set; }

    /// <summary>
    /// Average response duration in milliseconds.
    /// </summary>
    public double AvgDurationMs { get; set; }

    /// <summary>
    /// Maximum response duration in milliseconds.
    /// </summary>
    public long MaxDurationMs { get; set; }

    /// <summary>
    /// Minimum response duration in milliseconds.
    /// </summary>
    public long MinDurationMs { get; set; }

    /// <summary>
    /// The last HTTP status code returned.
    /// </summary>
    public int LastStatusCode { get; set; }
}
