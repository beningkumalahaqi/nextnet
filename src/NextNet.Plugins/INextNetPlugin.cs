namespace NextNet.Plugins;

/// <summary>
/// Defines the contract for a NextNet plugin.
/// Plugins implement this interface to participate in the NextNet lifecycle
/// and can additionally implement hook interfaces (e.g., <see cref="Hooks.IBuildHook"/>)
/// to extend specific pipeline stages.
/// </summary>
public interface INextNetPlugin
{
    /// <summary>
    /// Gets the human-readable name of the plugin.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets a description of what the plugin does.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets the plugin version.
    /// </summary>
    Version Version { get; }

    /// <summary>
    /// Called once when the plugin is first loaded and initialized.
    /// Use this for one-time setup (registering routes, reading config, etc.).
    /// </summary>
    /// <param name="context">The plugin context providing services, logging, and configuration.</param>
    Task OnInitializeAsync(PluginContext context);
}
