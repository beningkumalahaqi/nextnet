using NextNet.Data.Abstractions.Abstractions;
using NextNet.Data.Abstractions.Models;

namespace NextNet.Data.MultiDb.Extensions;

/// <summary>
/// Extension methods for performing aggregate health checks across all
/// named connections in a multi-database setup.
/// </summary>
/// <remarks>
/// <para>
/// These extensions enable checking the health of all registered connections
/// at once, returning a consolidated result. Useful for health check endpoints
/// and monitoring dashboards.
/// </para>
/// <example>
/// <code>
/// var results = await selector.CheckAllConnectionsAsync();
/// foreach (var (name, result) in results)
/// {
///     Console.WriteLine($"{name}: {(result.IsHealthy ? "Healthy" : "Unhealthy")}");
/// }
/// </code>
/// </example>
/// </remarks>
public static class MultiDbHealthCheckExtensions
{
    /// <summary>
    /// Runs health checks on all registered named connections.
    /// </summary>
    /// <param name="selector">The database selector.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A dictionary mapping connection names to their health check results.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <c>null</c>.</exception>
    public static async Task<IReadOnlyDictionary<string, HealthCheckResult>> CheckAllConnectionsAsync(
        this IDatabaseSelector selector,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(selector);

        var results = new Dictionary<string, HealthCheckResult>(StringComparer.OrdinalIgnoreCase);

        foreach (var name in selector.ConnectionNames)
        {
            try
            {
                using var context = selector.For(name);
                var result = await context.Provider.IsHealthyAsync(cancellationToken);
                results[name] = result;
            }
            catch (Exception ex)
            {
                results[name] = new HealthCheckResult(
                    false,
                    $"Health check failed: {ex.Message}",
                    TimeSpan.Zero);
            }
        }

        return results.AsReadOnly();
    }

    /// <summary>
    /// Checks whether all registered connections are healthy.
    /// </summary>
    /// <param name="selector">The database selector.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><c>true</c> if all connections are healthy; otherwise <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <c>null</c>.</exception>
    public static async Task<bool> AllConnectionsHealthyAsync(
        this IDatabaseSelector selector,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(selector);

        var results = await CheckAllConnectionsAsync(selector, cancellationToken);
        return results.Values.All(r => r.IsHealthy);
    }
}
