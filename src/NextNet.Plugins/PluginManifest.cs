namespace NextNet.Plugins;

/// <summary>
/// Describes metadata about a loaded plugin assembly.
/// </summary>
public sealed record PluginManifest
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

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginManifest"/> record.
    /// </summary>
    /// <param name="name">The plugin name. If null, defaults to <see cref="string.Empty"/>.</param>
    /// <param name="version">The plugin version string. If null, defaults to "1.0.0".</param>
    /// <param name="description">The human-readable description.</param>
    /// <param name="author">The plugin author.</param>
    /// <param name="dependencies">The list of plugin dependencies.</param>
    /// <param name="assemblyPath">The full path to the plugin assembly file.</param>
    public PluginManifest(
        string? name = null,
        string? version = null,
        string? description = null,
        string? author = null,
        IReadOnlyList<PluginDependency>? dependencies = null,
        string? assemblyPath = null)
    {
        if (name != null) Name = name;
        if (version != null) Version = version;
        if (description != null) Description = description;
        if (author != null) Author = author;
        if (dependencies != null) Dependencies = dependencies;
        if (assemblyPath != null) AssemblyPath = assemblyPath;
    }
}

/// <summary>
/// Describes a dependency of a plugin on another plugin.
/// </summary>
public sealed record PluginDependency
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
