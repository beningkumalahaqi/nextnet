namespace NextNet.Data.Abstractions.Models;

/// <summary>
/// Represents the result of a health check operation against a data provider or database.
/// </summary>
/// <remarks>
/// <para>
/// A standardized result type returned by <see cref="Abstractions.IHealthCheckProvider.GetHealthCheckAsync"/>
/// and <see cref="Abstractions.IDataProvider.IsHealthyAsync"/>. It captures health status,
/// operation duration, and optional diagnostic data.
/// </para>
/// <example>
/// <code>
/// var result = new HealthCheckResult(
///     true,
///     "Healthy",
///     TimeSpan.FromMilliseconds(42),
///     "All systems operational",
///     new Dictionary&lt;string, object&gt; { { "version", "1.0.0" } });
/// </code>
/// </example>
/// </remarks>
/// <param name="IsHealthy">Whether the health check passed.</param>
/// <param name="Status">A human-readable status string (e.g., "Healthy", "Degraded", "Unhealthy").</param>
/// <param name="Duration">The duration of the health check operation.</param>
/// <param name="Message">Optional diagnostic message or error details.</param>
/// <param name="Data">Optional key-value pairs with additional diagnostic information.</param>
public sealed record HealthCheckResult(
    bool IsHealthy,
    string Status,
    TimeSpan Duration,
    string? Message = null,
    IReadOnlyDictionary<string, object>? Data = null
);
