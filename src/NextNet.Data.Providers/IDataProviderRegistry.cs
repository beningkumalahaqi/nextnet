namespace NextNet.Data;

/// <summary>
/// Runtime registry of all registered data providers.
/// Enables dynamic provider resolution and enumeration across the application.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="IDataProviderRegistry"/> is registered as a singleton in the DI container
/// and populated during <see cref="NextNetDataBuilder.Build"/>. It provides runtime
/// introspection for health checks, CLI diagnostics, and multi-provider routing.
/// </para>
/// <example>
/// <code>
/// public class HealthDashboardService
/// {
///     private readonly IDataProviderRegistry _registry;
///
///     public HealthDashboardService(IDataProviderRegistry registry)
///     {
///         _registry = registry;
///     }
///
///     public async Task ShowHealthAsync()
///     {
///         foreach (var provider in _registry.GetAll())
///         {
///             var result = await provider.IsHealthyAsync();
///             Console.WriteLine($"{provider.Name}: {result.IsHealthy}");
///         }
///     }
/// }
/// </code>
/// </example>
/// </remarks>
public interface IDataProviderRegistry
{
    /// <summary>
    /// Gets all registered data providers.
    /// </summary>
    /// <returns>A read-only list of all registered providers.</returns>
    IReadOnlyList<IDataProvider> GetAll();

    /// <summary>
    /// Gets a provider by its <see cref="IDataProvider.Name"/>.
    /// </summary>
    /// <param name="name">The provider name to look up.</param>
    /// <returns>The matching provider, or <c>null</c> if no provider with the given name is registered.</returns>
    IDataProvider? GetByName(string name);

    /// <summary>
    /// Gets the default provider (first registered).
    /// </summary>
    /// <returns>The first registered provider, or <c>null</c> if no providers are registered.</returns>
    IDataProvider? GetDefault();
}
