using NextNet.Data.Abstractions.Models;

namespace NextNet.Data.Abstractions.Abstractions;

/// <summary>
/// Defines the contract for provider-level health checking.
/// Registered providers expose health status for integration with ASP.NET Core health checks
/// and the NextNet admin dashboard.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="IHealthCheckProvider"/> interface enables standardized health monitoring
/// across all data providers. Implementations should perform lightweight checks such as
/// database connectivity pings and report the results via <see cref="HealthCheckResult"/>.
/// </para>
/// <example>
/// <code>
/// public class MyHealthCheckService
/// {
///     private readonly IHealthCheckProvider _health;
///
///     public MyHealthCheckService(IHealthCheckProvider health)
///     {
///         _health = health;
///     }
///
///     public async Task&lt;HealthCheckResult&gt; CheckDatabaseAsync() =>
///         await _health.GetHealthCheckAsync();
/// }
/// </code>
/// </example>
/// </remarks>
public interface IHealthCheckProvider
{
    /// <summary>
    /// Executes a health check against the database and returns the current status.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A health check result with status, duration, and optional diagnostic data.</returns>
    Task<HealthCheckResult> GetHealthCheckAsync(CancellationToken cancellationToken = default);
}
