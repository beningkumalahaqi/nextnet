using System.Collections.Concurrent;
using NextNet.Logging;
using NextNet.Plugins.Errors;

namespace NextNet.Plugins;

/// <summary>
/// Central registry for all loaded plugins and their hook implementations.
/// Provides typed access to plugins implementing specific hook interfaces.
/// </summary>
public sealed class PluginRegistry
{
    private readonly List<INextNetPlugin> _plugins = new();
    private readonly ConcurrentDictionary<Type, List<object>> _hookCache = new();
    private readonly INextNetLogger _logger;
    private bool _initialized;

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginRegistry"/> class.
    /// </summary>
    /// <param name="logger">The logger for diagnostic output.</param>
    public PluginRegistry(INextNetLogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all loaded plugins.
    /// </summary>
    /// <returns>A read-only list of all registered plugins.</returns>
    public IReadOnlyList<INextNetPlugin> GetPlugins() => _plugins.AsReadOnly();

    /// <summary>
    /// Gets all plugins that implement the specified hook interface <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The hook interface type (e.g., <see cref="Hooks.IBuildHook"/>).</typeparam>
    /// <returns>A list of hook implementations.</returns>
    public IReadOnlyList<T> GetHooks<T>() where T : class
    {
        var hookType = typeof(T);

        if (_hookCache.TryGetValue(hookType, out var cached))
        {
            return cached.Cast<T>().ToList().AsReadOnly();
        }

        var hooks = _plugins
            .Where(p => p is T)
            .Select(p => (T)(object)p)
            .ToList();

        _hookCache[hookType] = hooks.Cast<object>().ToList();
        return hooks.AsReadOnly();
    }

    /// <summary>
    /// Registers a plugin instance with the registry.
    /// </summary>
    /// <param name="plugin">The plugin instance to register.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="plugin"/> is null.</exception>
    public void Register(INextNetPlugin plugin)
    {
        if (plugin == null) throw new ArgumentNullException(nameof(plugin));

        if (_plugins.Any(p => p.Name == plugin.Name))
        {
            _logger.Warn("[{0}] Plugin '{1}' is already registered. Skipping duplicate.", PluginErrorCodes.AlreadyRegistered, plugin.Name);
            return;
        }

        _plugins.Add(plugin);
        _hookCache.Clear(); // Invalidate cache
        _logger.Info("Registered plugin: {0} v{1}", plugin.Name, plugin.Version);
    }

    /// <summary>
    /// Registers multiple plugin instances with the registry.
    /// </summary>
    /// <param name="plugins">The plugin instances to register.</param>
    public void RegisterAll(IEnumerable<INextNetPlugin> plugins)
    {
        foreach (var plugin in plugins)
        {
            Register(plugin);
        }
    }

    /// <summary>
    /// Removes a plugin from the registry.
    /// </summary>
    /// <param name="name">The name of the plugin to remove.</param>
    /// <returns><c>true</c> if the plugin was found and removed; otherwise <c>false</c>.</returns>
    public bool Unregister(string name)
    {
        var plugin = _plugins.FirstOrDefault(p => p.Name == name);
        if (plugin == null) return false;

        _plugins.Remove(plugin);
        _hookCache.Clear();
        _logger.Info("Unregistered plugin: {0}", name);
        return true;
    }

    /// <summary>
    /// Gets a loaded plugin by name.
    /// </summary>
    /// <param name="name">The plugin name.</param>
    /// <returns>The plugin instance, or <c>null</c> if not found.</returns>
    public INextNetPlugin? GetPlugin(string name)
    {
        return _plugins.FirstOrDefault(p => p.Name == name);
    }

    /// <summary>
    /// Gets the number of registered plugins.
    /// </summary>
    public int Count => _plugins.Count;

    /// <summary>
    /// Initializes all registered plugins by calling <see cref="INextNetPlugin.OnInitializeAsync"/>.
    /// Plugins are initialized in dependency order (if dependencies are declared in their manifests).
    /// </summary>
    /// <param name="contextFactory">A factory function that creates a <see cref="PluginContext"/> for a given plugin.</param>
    public async Task InitializeAllAsync(Func<INextNetPlugin, PluginContext> contextFactory)
    {
        if (_initialized)
        {
            _logger.Warn("Plugins are already initialized.");
            return;
        }

        // Sort by dependencies (topological order)
        var ordered = SortByDependencies(_plugins);

        foreach (var plugin in ordered)
        {
            try
            {
                var context = contextFactory(plugin);
                await plugin.OnInitializeAsync(context);
                _logger.Info("Initialized plugin: {0}", plugin.Name);
            }
            catch (Exception ex)
            {
                _logger.Error("[{0}] Plugin '{1}' failed during initialization: {2}", PluginErrorCodes.InitializationFailed, plugin.Name, ex.Message);
            }
        }

        _initialized = true;
    }

    /// <summary>
    /// Topological sort of plugins based on their <see cref="PluginManifest.Dependencies"/>.
    /// Uses a depth-first traversal with cycle detection.
    /// </summary>
    private static IReadOnlyList<INextNetPlugin> SortByDependencies(IReadOnlyList<INextNetPlugin> plugins)
    {
        if (plugins.Count == 0)
            return Array.Empty<INextNetPlugin>();

        // Build a name-to-plugin map
        var pluginMap = plugins.ToDictionary(p => p.Name, p => p);

        // Resolve manifest per plugin (convention: look for a manifest property or build from metadata)
        // Since INextNetPlugin doesn't expose PluginManifest directly, we check conventions:
        // plugins that extend NextNetPlugin or that provide a Manifest via property.
        var manifests = new Dictionary<string, PluginManifest>(StringComparer.Ordinal);
        foreach (var plugin in plugins)
        {
            // Use reflection to find a "Manifest" property of type PluginManifest
            var manifestProp = plugin.GetType().GetProperty("Manifest",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (manifestProp != null && manifestProp.PropertyType == typeof(PluginManifest))
            {
                if (manifestProp.GetValue(plugin) is PluginManifest m)
                {
                    manifests[plugin.Name] = m;
                }
            }
        }

        // Build dependency graph: adjacency list of plugin names
        var graph = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        foreach (var plugin in plugins)
        {
            var deps = new List<string>();
            if (manifests.TryGetValue(plugin.Name, out var manifest))
            {
                foreach (var dep in manifest.Dependencies)
                {
                    // Only include dependencies that are actually loaded
                    if (pluginMap.ContainsKey(dep.Name))
                    {
                        deps.Add(dep.Name);
                    }
                }
            }
            graph[plugin.Name] = deps;
        }

        // Topological sort via DFS with cycle detection
        var sorted = new List<INextNetPlugin>();
        var visited = new HashSet<string>(StringComparer.Ordinal);
        var visiting = new HashSet<string>(StringComparer.Ordinal);

        void Visit(string name)
        {
            if (visited.Contains(name))
                return;

            if (visiting.Contains(name))
                throw new InvalidOperationException(
                    $"[{PluginErrorCodes.CircularDependency}] Circular dependency detected involving plugin '{name}'.");

            visiting.Add(name);

            if (graph.TryGetValue(name, out var deps))
            {
                foreach (var dep in deps)
                {
                    Visit(dep);
                }
            }

            visiting.Remove(name);
            visited.Add(name);

            if (pluginMap.TryGetValue(name, out var plugin))
            {
                sorted.Add(plugin);
            }
        }

        foreach (var plugin in plugins)
        {
            Visit(plugin.Name);
        }

        return sorted;
    }
}
