namespace NextNet.Plugins.Hooks;

/// <summary>
/// Hook that fires when the NextNet application starts up.
/// Plugins can use this to register services, middleware, or perform startup tasks.
/// </summary>
/// <example>
/// <code>
/// public class MyStartupPlugin : NextNetPlugin, IStartupHook
/// {
///     public override string Name => "StartupConfigurator";
///
///     public Task OnStartup(PluginContext ctx)
///     {
///         ctx.Logger.Info("Application starting up.");
///         // Register custom services or middleware here
///         return Task.CompletedTask;
///     }
/// }
/// </code>
/// </example>
public interface IStartupHook
{
    /// <summary>
    /// Called during application startup.
    /// </summary>
    /// <param name="ctx">The plugin context.</param>
    Task OnStartup(PluginContext ctx);
}
