namespace NextNet.Plugins;

/// <summary>
/// Describes metadata about a loaded plugin assembly.
/// </summary>
public class PluginManifest
{
    /// <summary>
    /// Gets the plugin name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the plugin version string.
    /// </summary>
    public string Version { get; init; } = "1.0.0";

    /// <summary>
    /// Gets the human-readable description of the plugin.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets the plugin author.
    /// </summary>
    public string? Author { get; init; }

    /// <summary>
    /// Gets the list of plugin dependencies.
    /// </summary>
    public IReadOnlyList<PluginDependency> Dependencies { get; init; } = Array.Empty<PluginDependency>();

    /// <summary>
    /// Gets the full path to the plugin assembly file.
    /// </summary>
    public string? AssemblyPath { get; init; }
}

/// <summary>
/// Describes a dependency of a plugin on another plugin.
/// </summary>
public class PluginDependency
{
    /// <summary>
    /// Gets the name of the dependent plugin.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the required version of the dependent plugin.
    /// </summary>
    public string Version { get; init; } = "1.0.0";
}
