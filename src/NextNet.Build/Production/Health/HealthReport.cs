namespace NextNet.Build.Production.Health;

/// <summary>
/// A report returned by the NextNet health check endpoint.
/// </summary>
/// <param name="Status">Overall status: Healthy, Degraded, or Unhealthy.</param>
/// <param name="Timestamp">The time at which this report was generated (UTC).</param>
/// <param name="Version">The application version.</param>
/// <param name="Uptime">The application's uptime.</param>
/// <param name="Checks">Individual check results.</param>
public sealed record HealthReport(
    string Status,
    DateTime Timestamp,
    string? Version,
    string? Uptime,
    List<HealthCheckResult> Checks)
{
    /// <summary>
    /// Creates a new health report with default values.
    /// </summary>
    public HealthReport()
        : this("Healthy", DateTime.UtcNow, null, null, new()) { }
}

/// <summary>
/// Result of an individual health check.
/// </summary>
/// <param name="Name">Display name of the check.</param>
/// <param name="Status">Status: Healthy, Degraded, or Unhealthy.</param>
/// <param name="Description">Optional description or error message.</param>
/// <param name="DurationMs">Duration of the check in milliseconds.</param>
/// <param name="Data">Optional data associated with the check.</param>
public sealed record HealthCheckResult(
    string Name,
    string Status,
    string? Description,
    long DurationMs,
    Dictionary<string, object>? Data)
{
    /// <summary>
    /// Creates a new health check result with default values.
    /// </summary>
    public HealthCheckResult()
        : this(string.Empty, "Healthy", null, 0, null) { }
}
