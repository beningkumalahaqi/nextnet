using System.Reflection;
using System.Runtime.Loader;

namespace NextNet.Plugins;

/// <summary>
/// Provides an isolated <see cref="AssemblyLoadContext"/> for each plugin,
/// enabling plugin assemblies to be loaded in isolation and optionally collected
/// (unloaded) when no longer needed.
/// </summary>
internal class PluginAssemblyLoadContext : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver _resolver;

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginAssemblyLoadContext"/> class.
    /// </summary>
    /// <param name="pluginPath">The path to the plugin assembly.</param>
    public PluginAssemblyLoadContext(string pluginPath)
        : base(isCollectible: true)
    {
        _resolver = new AssemblyDependencyResolver(pluginPath);
    }

    /// <inheritdoc />
    protected override Assembly? Load(AssemblyName assemblyName)
    {
        // Try to resolve from the plugin directory first
        var path = _resolver.ResolveAssemblyToPath(assemblyName);
        if (path != null)
        {
            return LoadFromAssemblyPath(path);
        }

        // Fall back to the default context (shared framework / host assemblies)
        try
        {
            return Default.LoadFromAssemblyName(assemblyName);
        }
        catch
        {
            return null;
        }
    }
}
