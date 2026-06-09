namespace NextNet.Plugins;

/// <summary>
/// Assembly-level attribute that marks an assembly as a NextNet plugin
/// and identifies the plugin type to instantiate.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
public sealed class NextNetPluginAttribute : Attribute
{
    /// <summary>
    /// Gets the <see cref="Type"/> that implements <see cref="INextNetPlugin"/>.
    /// </summary>
    public Type PluginType { get; }

    /// <summary>
    /// Gets the plugin name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the plugin version string.
    /// </summary>
    public string Version { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="NextNetPluginAttribute"/> class.
    /// </summary>
    /// <param name="pluginType">The type implementing <see cref="INextNetPlugin"/>.</param>
    /// <param name="name">The plugin name.</param>
    /// <param name="version">The plugin version.</param>
    public NextNetPluginAttribute(Type pluginType, string name, string version)
    {
        PluginType = pluginType ?? throw new ArgumentNullException(nameof(pluginType));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Version = version ?? throw new ArgumentNullException(nameof(version));
    }
}
