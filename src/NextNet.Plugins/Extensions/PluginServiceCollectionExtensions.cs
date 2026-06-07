using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NextNet.Logging;
using NextNet.Plugins.CLI;

namespace NextNet.Plugins.Extensions;

/// <summary>
/// Extension methods for registering the NextNet plugin system with the DI container.
/// </summary>
public static class PluginServiceCollectionExtensions
{
    /// <summary>
    /// Adds the NextNet plugin system to the service collection.
    /// Registers <see cref="PluginRegistry"/> as a singleton and <see cref="PluginLoader"/> as transient.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddNextNetPlugins(this IServiceCollection services)
    {
        services.TryAddSingleton<PluginRegistry>(sp =>
        {
            var logger = sp.GetRequiredService<INextNetLogger>();
            return new PluginRegistry(logger);
        });

        services.TryAddTransient<PluginLoader>(sp =>
        {
            var logger = sp.GetRequiredService<INextNetLogger>();
            return new PluginLoader(logger);
        });

        services.TryAddTransient<PluginsCommand>();

        return services;
    }

    /// <summary>
    /// Registers a specific plugin type with the DI container and the <see cref="PluginRegistry"/>.
    /// The plugin will be created via DI and registered on initialization.
    /// </summary>
    /// <typeparam name="T">The plugin type implementing <see cref="INextNetPlugin"/>.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddNextNetPlugin<T>(this IServiceCollection services)
        where T : class, INextNetPlugin
    {
        services.AddTransient<T>();
        services.AddSingleton<INextNetPlugin>(sp =>
        {
            var logger = sp.GetRequiredService<INextNetLogger>();
            var registry = sp.GetRequiredService<PluginRegistry>();

            var plugin = ActivatorUtilities.CreateInstance<T>(sp);
            registry.Register(plugin);
            logger.Info("Registered plugin via DI: {0} v{1}", plugin.Name, plugin.Version);

            return plugin;
        });

        return services;
    }
}
