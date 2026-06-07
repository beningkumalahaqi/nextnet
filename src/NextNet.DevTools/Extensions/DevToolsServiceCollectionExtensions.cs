using NextNet.DevTools;
using NextNet.DevTools.Headless;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for registering NextNet DevTools services.
/// </summary>
public static class DevToolsServiceCollectionExtensions
{
    /// <summary>
    /// Adds DevTools services (event bus, data store, WebSocket manager) to the DI container.
    /// </summary>
    public static IServiceCollection AddNextNetDevTools(this IServiceCollection services, DevToolsOptions? options = null)
    {
        options ??= new DevToolsOptions();

        services.AddSingleton(options);
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
    /// </summary>
    public static IServiceCollection AddNextNetDevToolsServer(this IServiceCollection services, DevToolsOptions? options = null)
    {
        options ??= new DevToolsOptions();
        services.AddNextNetDevTools(options);
        services.AddSingleton(new DevToolsServer(options));
        return services;
    }
}
