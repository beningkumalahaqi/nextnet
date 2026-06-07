using NextNet.Configuration;
using NextNet.Logging;

namespace NextNet.Plugins;

/// <summary>
/// Provides contextual information and services to a plugin during initialization and execution.
/// </summary>
public class PluginContext
{
    /// <summary>
    /// Gets the service provider for resolving application services.
    /// </summary>
    public IServiceProvider Services { get; }

    /// <summary>
    /// Gets the NextNet logger for the plugin.
    /// </summary>
    public INextNetLogger Logger { get; }

    /// <summary>
    /// Gets the current NextNet configuration.
    /// </summary>
    public NextNetConfig Config { get; }

    /// <summary>
    /// Gets the directory from which the plugin was loaded.
    /// </summary>
    public string PluginDirectory { get; }

    /// <summary>
    /// Gets the optional cancellation token for cooperative cancellation.
    /// </summary>
    public CancellationToken CancellationToken { get; }

    /// <summary>
    /// Gets the manifest describing this plugin's metadata.
    /// </summary>
    public PluginManifest Manifest { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginContext"/> class.
    /// </summary>
    public PluginContext(
        IServiceProvider services,
        INextNetLogger logger,
        NextNetConfig config,
        string pluginDirectory,
        PluginManifest manifest,
        CancellationToken cancellationToken = default)
    {
        Services = services ?? throw new ArgumentNullException(nameof(services));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        Config = config ?? throw new ArgumentNullException(nameof(config));
        PluginDirectory = pluginDirectory ?? throw new ArgumentNullException(nameof(pluginDirectory));
        Manifest = manifest ?? throw new ArgumentNullException(nameof(manifest));
        CancellationToken = cancellationToken;
    }
}
