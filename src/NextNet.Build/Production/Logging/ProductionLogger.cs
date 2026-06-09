using Microsoft.Extensions.Logging;

namespace NextNet.Build.Production.Logging;

/// <summary>
/// Structured logger for production diagnostics.
/// Extends the standard ILogger with NextNet-specific production logging concerns.
/// </summary>
public sealed class ProductionLogger
{
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="ProductionLogger"/>.
    /// </summary>
    public ProductionLogger(ILogger<ProductionLogger> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Logs application startup with timing information.
    /// </summary>
    public void LogStartup(long startupTimeMs, string version)
    {
        _logger.LogInformation(
            "NextNet started in {StartupTimeMs}ms (version: {Version})",
            startupTimeMs, version);
    }

    /// <summary>
    /// Logs a request summary with timing and status.
    /// </summary>
    public void LogRequest(string method, string path, int statusCode, long durationMs, long? size = null)
    {
        if (statusCode >= 500)
        {
            _logger.LogError(
                "HTTP {Method} {Path} responded {StatusCode} in {DurationMs}ms",
                method, path, statusCode, durationMs);
        }
        else if (statusCode >= 400)
        {
            _logger.LogWarning(
                "HTTP {Method} {Path} responded {StatusCode} in {DurationMs}ms",
                method, path, statusCode, durationMs);
        }
        else
        {
            _logger.LogInformation(
                "HTTP {Method} {Path} responded {StatusCode} in {DurationMs}ms [size: {Size}]",
                method, path, statusCode, durationMs, size);
        }
    }

    /// <summary>
    /// Logs a performance budget violation.
    /// </summary>
    public void LogBudgetViolation(string metric, string expected, string actual)
    {
        _logger.LogWarning(
            "Performance budget violation: {Metric} = {Actual} (budget: {Expected})",
            metric, actual, expected);
    }

    /// <summary>
    /// Logs a build completion event.
    /// </summary>
    public void LogBuildComplete(long durationMs, bool success, long bytesSaved, long totalSize)
    {
        if (success)
        {
            _logger.LogInformation(
                "Build completed in {DurationMs}ms. Saved {BytesSaved} bytes. Total output: {TotalSize} bytes.",
                durationMs, bytesSaved, totalSize);
        }
        else
        {
            _logger.LogError(
                "Build FAILED after {DurationMs}ms. Saved {BytesSaved} bytes before failure.",
                durationMs, bytesSaved);
        }
    }

    /// <summary>
    /// Logs a security event.
    /// </summary>
    public void LogSecurityEvent(string eventType, string message)
    {
        _logger.LogWarning("Security event: {EventType} — {Message}", eventType, message);
    }

    /// <summary>
    /// Logs cache-related events.
    /// </summary>
    public void LogCacheEvent(string eventType, string key, string? detail = null)
    {
        if (detail != null)
        {
            _logger.LogDebug("Cache {EventType}: {Key} ({Detail})", eventType, key, detail);
        }
        else
        {
            _logger.LogDebug("Cache {EventType}: {Key}", eventType, key);
        }
    }
}
