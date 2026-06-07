using NextNet.Routing;

namespace NextNet.Plugins.Hooks;

/// <summary>
/// Hook that fires when the route scanner has finished discovering routes.
/// Plugins can inspect or modify the route manifest.
/// </summary>
public interface IRouteScannerHook
{
    /// <summary>
    /// Called after all routes have been discovered.
    /// </summary>
    /// <param name="ctx">The plugin context.</param>
    /// <param name="manifest">The complete route manifest with all discovered pages, layouts, and API routes.</param>
    Task OnRoutesDiscovered(PluginContext ctx, RouteManifest manifest);
}
