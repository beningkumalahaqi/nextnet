namespace NextNet.Data;

/// <summary>
/// Defines the contract for a NextNet data provider.
/// Each provider (EF Core, Dapper, MongoDB, etc.) implements this interface
/// to participate in the NextNet data lifecycle.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="IDataProvider"/> interface is the primary extension point for
/// integrating database technologies with NextNet. Providers are registered via
/// the <see cref="NextNetDataBuilder"/> fluent API and initialized once per
/// application lifetime through the <see cref="Internal.ProviderInitializationHostedService"/>.
/// </para>
/// <para>
/// Each provider exposes identity information (<see cref="Name"/>, <see cref="DisplayName"/>,
/// <see cref="Version"/>), a startup initialization hook (<see cref="InitializeAsync"/>),
/// and a runtime health check (<see cref="IsHealthyAsync"/>).
/// </para>
/// <example>
/// <code>
/// public class MyCustomProvider : IDataProvider
/// {
///     public string Name => "MyCustomProvider";
///     public string DisplayName => "My Custom Provider 1.0";
///     public Version Version => new(1, 0, 0);
///
///     public Task InitializeAsync(CancellationToken ct = default)
///     {
///         // Open connections, configure pools, etc.
///         return Task.CompletedTask;
///     }
///
///     public Task&lt;DataProviderHealthResult&gt; IsHealthyAsync(CancellationToken ct = default)
///     {
///         // Ping database, check connectivity
///         return Task.FromResult(DataProviderHealthResult.Healthy("All systems operational"));
///     }
/// }
/// </code>
/// </example>
/// </remarks>
public interface IDataProvider
{
    /// <summary>
    /// Gets the unique name of this provider (e.g., "EntityFramework", "Dapper", "MongoDB").
    /// Used for registration lookup, CLI display, and multi-provider resolution.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the display-friendly label for this provider (e.g., "Entity Framework Core 10").
    /// Used for CLI output, admin dashboards, and diagnostic messages.
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Gets the version of this provider implementation.
    /// Used for compatibility checking and diagnostic reporting.
    /// </summary>
    Version Version { get; }

    /// <summary>
    /// Initializes the provider asynchronously.
    /// Called once during application startup after the DI container is ready.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the initialization operation.</param>
    /// <returns>A task that represents the asynchronous initialization operation.</returns>
    Task InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether the provider is in a healthy state.
    /// Called by health-check middleware, CLI diagnostics, and the admin dashboard.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the health check operation.</param>
    /// <returns>A <see cref="DataProviderHealthResult"/> indicating the health status.</returns>
    Task<DataProviderHealthResult> IsHealthyAsync(CancellationToken cancellationToken = default);
}
