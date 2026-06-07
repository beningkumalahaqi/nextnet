using System.Reflection;
using NextNet.Logging;

namespace NextNet.Plugins;

/// <summary>
/// Discovers and loads NextNet plugins from specified directories or assemblies.
/// Supports loading from <c>plugins/</c> directories with isolated
/// <see cref="PluginAssemblyLoadContext"/> per assembly.
/// </summary>
public class PluginLoader
{
    private readonly INextNetLogger _logger;

    /// <summary>
    /// The default directory name where plugin assemblies are stored.
    /// </summary>
    public const string DefaultPluginDirectory = "plugins";

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginLoader"/> class.
    /// </summary>
    /// <param name="logger">The logger for diagnostic output.</param>
    public PluginLoader(INextNetLogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Loads all plugins from the default <c>plugins/</c> directory relative to the given base path.
    /// </summary>
    /// <param name="basePath">The base path containing the <c>plugins/</c> directory.</param>
    /// <returns>A list of discovered plugin instances.</returns>
    public IReadOnlyList<INextNetPlugin> LoadAll(string basePath)
    {
        var pluginDir = Path.Combine(basePath, DefaultPluginDirectory);
        return LoadAllFromDirectory(pluginDir);
    }

    /// <summary>
    /// Loads all plugins from the specified directory.
    /// </summary>
    /// <param name="pluginDirectory">The directory containing plugin assemblies (*.dll).</param>
    /// <returns>A list of discovered plugin instances.</returns>
    public IReadOnlyList<INextNetPlugin> LoadAllFromDirectory(string pluginDirectory)
    {
        var plugins = new List<INextNetPlugin>();

        if (!Directory.Exists(pluginDirectory))
        {
            _logger.Debug("Plugin directory does not exist: {0}", pluginDirectory);
            return plugins;
        }

        var assemblyFiles = Directory.GetFiles(pluginDirectory, "*.dll");
        _logger.Info("Scanning {0} assemblies in {1} for plugins...", assemblyFiles.Length, pluginDirectory);

        foreach (var assemblyPath in assemblyFiles)
        {
            try
            {
                var plugin = LoadFromAssembly(assemblyPath);
                if (plugin != null)
                {
                    plugins.Add(plugin);
                    _logger.Info("Loaded plugin: {0} v{1} from {2}", plugin.Name, plugin.Version, assemblyPath);
                }
            }
            catch (Exception ex)
            {
                _logger.Warn("Failed to load plugin from {0}: {1}", assemblyPath, ex.Message);
            }
        }

        return plugins;
    }

    /// <summary>
    /// Loads a plugin from a specific assembly file path.
    /// Uses an isolated <see cref="PluginAssemblyLoadContext"/> for assembly isolation.
    /// </summary>
    /// <param name="assemblyPath">The full path to the plugin assembly DLL.</param>
    /// <returns>The plugin instance, or <c>null</c> if the assembly does not contain a NextNet plugin.</returns>
    public INextNetPlugin? LoadFromAssembly(string assemblyPath)
    {
        if (!File.Exists(assemblyPath))
        {
            _logger.Warn("Assembly not found: {0}", assemblyPath);
            return null;
        }

        // Load in isolated ALC
        var alc = new PluginAssemblyLoadContext(assemblyPath);
        Assembly assembly;

        try
        {
            assembly = alc.LoadFromAssemblyPath(assemblyPath);
        }
        catch (Exception ex)
        {
            _logger.Warn("Could not load assembly {0}: {1}", assemblyPath, ex.Message);
            return null;
        }

        // Scan for the NextNetPlugin attribute
        var attribute = assembly.GetCustomAttribute<NextNetPluginAttribute>();
        if (attribute == null)
        {
            _logger.Debug("Assembly {0} has no [NextNetPlugin] attribute, skipping.", assemblyPath);
            return null;
        }

        if (!typeof(INextNetPlugin).IsAssignableFrom(attribute.PluginType))
        {
            _logger.Warn("Plugin type {0} in {1} does not implement INextNetPlugin.", attribute.PluginType.FullName, assemblyPath);
            return null;
        }

        // Create an instance
        try
        {
            var plugin = (INextNetPlugin)Activator.CreateInstance(attribute.PluginType)!;
            return plugin;
        }
        catch (Exception ex)
        {
            _logger.Warn("Failed to create plugin instance of {0}: {1}", attribute.PluginType.FullName, ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Loads a plugin from a pre-loaded assembly that has the <see cref="NextNetPluginAttribute"/>.
    /// </summary>
    /// <param name="assembly">The assembly to scan.</param>
    /// <returns>The plugin instance, or <c>null</c> if the assembly is not a plugin.</returns>
    public INextNetPlugin? LoadFromAssemblyMetadata(Assembly assembly)
    {
        var attribute = assembly.GetCustomAttribute<NextNetPluginAttribute>();
        if (attribute == null)
        {
            return null;
        }

        if (!typeof(INextNetPlugin).IsAssignableFrom(attribute.PluginType))
        {
            _logger.Warn("Plugin type {0} does not implement INextNetPlugin.", attribute.PluginType.FullName);
            return null;
        }

        try
        {
            return (INextNetPlugin)Activator.CreateInstance(attribute.PluginType)!;
        }
        catch (Exception ex)
        {
            _logger.Warn("Failed to create plugin instance of {0}: {1}", attribute.PluginType.FullName, ex.Message);
            return null;
        }
    }
}
