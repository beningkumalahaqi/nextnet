using NextNet.Configuration;
using NextNet.Logging;

namespace NextNet.Plugins;

/// <summary>
/// Provides contextual information and services to a plugin during initialization and execution.
/// </summary>
public sealed class PluginContext
{
    /// <summary>
    /// Gets the service provider for resolving application services.
    /// </summary>
    public IServiceProvider Services { get; init; }

    /// <summary>
    /// Gets the NextNet logger for the plugin.
    /// </summary>
    public INextNetLogger Logger { get; init; }

    /// <summary>
    /// Gets the current NextNet configuration.
    /// </summary>
    public NextNetConfig Config { get; init; }

    /// <summary>
    /// Gets the directory from which the plugin was loaded.
    /// </summary>
    public string PluginDirectory { get; init; }

    /// <summary>
    /// Gets the optional cancellation token for cooperative cancellation.
    /// </summary>
    public CancellationToken CancellationToken { get; init; }

    /// <summary>
    /// Gets the manifest describing this plugin's metadata.
    /// </summary>
    public PluginManifest Manifest { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginContext"/> class.
    /// </summary>
    public PluginContext()
    {
        Services = default!;
        Logger = default!;
        Config = default!;
        PluginDirectory = default!;
        Manifest = default!;
    }

    /// <summary>
    /// Validates that all required properties are set. Throws if any is null.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown if any required property is null.</exception>
    internal void Validate()
    {
        ArgumentNullException.ThrowIfNull(Services);
        ArgumentNullException.ThrowIfNull(Logger);
        ArgumentNullException.ThrowIfNull(Config);
        ArgumentNullException.ThrowIfNull(PluginDirectory);
        ArgumentNullException.ThrowIfNull(Manifest);
    }
}
