using NextNet.Data.Abstractions.Configuration;
using NextNet.Data.Abstractions.Models;

namespace NextNet.Data.Abstractions.Abstractions;

/// <summary>
/// Defines the lifecycle contract for a NextNet data provider.
/// Each provider (EF Core, Dapper, MongoDB, etc.) implements this interface
/// to integrate with the NextNet data pipeline.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="IDataProvider"/> interface is the primary extension point for
/// integrating database technologies with NextNet. Providers handle initialization,
/// health monitoring, and serve as factories for <see cref="IRepository{T}"/> instances.
/// </para>
/// <para>
/// Providers are registered via the <see cref="Registration.NextNetDataBuilder"/>
/// fluent API during application startup and initialized once per application lifetime.
/// </para>
/// <example>
/// <code>
/// public class MyCustomProvider : IDataProvider
/// {
///     public string Name => "MyCustomProvider";
///
///     public async Task InitializeAsync(DataConfig config, CancellationToken ct = default)
///     {
///         // Open connections, configure pools, etc.
///     }
///
///     public async Task&lt;HealthCheckResult&gt; IsHealthyAsync(CancellationToken ct = default)
///     {
///         // Ping database, check connectivity
///         return new HealthCheckResult(true, "Healthy", TimeSpan.Zero);
///     }
/// }
/// </code>
/// </example>
/// </remarks>
public interface IDataProvider
{
    /// <summary>
    /// Gets the unique name of this provider (e.g., "EntityFramework", "Dapper", "MongoDB").
    /// Used for registration lookup and diagnostic reporting.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Initializes the provider with the given configuration.
    /// Called once during application startup after all services are registered.
    /// </summary>
    /// <param name="config">The data configuration for this provider instance.</param>
    /// <param name="cancellationToken">A token to cancel the initialization.</param>
    /// <returns>A task representing the asynchronous initialization operation.</returns>
    Task InitializeAsync(DataConfig config, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether the provider and its underlying database are healthy and reachable.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the health check.</param>
    /// <returns>A task containing the health check result.</returns>
    Task<HealthCheckResult> IsHealthyAsync(CancellationToken cancellationToken = default);
}
