using NextNet.DevTools;
using NextNet.DevTools.Headless;
using NextNet.DevTools.UI;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for registering NextNet DevTools services in the DI container.
/// Provides overloads for adding core services, optional WebSocket manager, and the DevTools server.
/// </summary>
/// <example>
/// <code>
/// // Register DevTools with default TUI mode
/// services.AddNextNetDevTools();
///
/// // Register DevTools with custom headless configuration
/// services.AddNextNetDevTools(new DevToolsOptions
/// {
///     Mode = DevToolsMode.Headless,
///     Port = 9000
/// });
///
/// // Register DevTools server as a hosted service
/// services.AddNextNetDevToolsServer(new DevToolsOptions { Mode = DevToolsMode.Headless });
/// </code>
/// </example>
public static class DevToolsServiceCollectionExtensions
{
    /// <summary>
    /// Adds DevTools services (event bus, data store, WebSocket manager) to the DI container.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="options">Optional configuration. Uses defaults if null.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddNextNetDevTools(this IServiceCollection services, DevToolsOptions? options = null)
    {
        options ??= new DevToolsOptions();

        services.AddSingleton(options);
        services.AddSingleton(new TerminalColorPalette(options.IsDark));
        services.AddSingleton<DevToolsDataStore>();
        services.AddSingleton<DevToolsEventBus>();
        services.AddSingleton<IDevToolsEventBus>(sp => sp.GetRequiredService<DevToolsEventBus>());

        if (options.Mode == DevToolsMode.Headless)
        {
            services.AddSingleton<DevToolsWebSocketManager>();
        }

        return services;
    }

    /// <summary>
    /// Adds DevTools server as a hosted service for headless mode.
    /// Registers core DevTools services and a singleton <see cref="DevToolsServer"/> instance.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="options">Optional configuration. Uses defaults if null.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddNextNetDevToolsServer(this IServiceCollection services, DevToolsOptions? options = null)
    {
        options ??= new DevToolsOptions();
        services.AddNextNetDevTools(options);
        services.AddSingleton(new DevToolsServer(options));
        return services;
    }
}
