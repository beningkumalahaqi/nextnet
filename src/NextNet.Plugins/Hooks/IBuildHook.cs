namespace NextNet.Plugins.Hooks;

/// <summary>
/// Hook that fires during the build pipeline — before and after the build executes.
/// Plugins implementing this interface can modify build behaviour or inspect results.
/// </summary>
public interface IBuildHook
{
    /// <summary>
    /// Called before the build process begins.
    /// </summary>
    /// <param name="ctx">The plugin context.</param>
    Task OnBuildStart(PluginContext ctx);

    /// <summary>
    /// Called after the build process completes.
    /// </summary>
    /// <param name="ctx">The plugin context.</param>
    Task OnBuildEnd(PluginContext ctx);
}
