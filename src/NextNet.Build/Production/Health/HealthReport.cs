namespace NextNet.Build.Production.Health;

/// <summary>
/// A report returned by the NextNet health check endpoint.
/// </summary>
public class HealthReport
{
    /// <summary>
    /// Overall status: Healthy, Degraded, or Unhealthy.
    /// </summary>
    public string Status { get; set; } = "Healthy";

    /// <summary>
    /// The time at which this report was generated (UTC).
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// The application version.
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// The application's uptime.
    /// </summary>
    public string? Uptime { get; set; }

    /// <summary>
    /// Individual check results.
    /// </summary>
    public List<HealthCheckResult> Checks { get; set; } = new();
}

/// <summary>
/// Result of an individual health check.
/// </summary>
public class HealthCheckResult
{
    /// <summary>
    /// Display name of the check.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Status: Healthy, Degraded, or Unhealthy.
    /// </summary>
    public string Status { get; set; } = "Healthy";

    /// <summary>
    /// Optional description or error message.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Duration of the check in milliseconds.
    /// </summary>
    public long DurationMs { get; set; }

    /// <summary>
    /// Optional data associated with the check.
    /// </summary>
    public Dictionary<string, object>? Data { get; set; }
}
