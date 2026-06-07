namespace NextNet.Plugins.Hooks;

/// <summary>
/// Hook that fires when the NextNet application starts up.
/// Plugins can use this to register services, middleware, or perform startup tasks.
/// </summary>
public interface IStartupHook
{
    /// <summary>
    /// Called during application startup.
    /// </summary>
    /// <param name="ctx">The plugin context.</param>
    Task OnStartup(PluginContext ctx);
}
