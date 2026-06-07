using NextNet.Logging;
using NextNet.Plugins.Hooks;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace NextNet.Plugins.CLI;

/// <summary>
/// Implements the <c>nextnet plugins</c> command — lists installed and discovered plugins,
/// their versions, descriptions, and the hook interfaces they implement.
/// </summary>
public class PluginsCommand
{
    private readonly PluginRegistry _registry;
    private readonly PluginLoader _loader;
    private readonly INextNetLogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginsCommand"/> class.
    /// </summary>
    /// <param name="registry">The plugin registry.</param>
    /// <param name="loader">The plugin loader.</param>
    /// <param name="logger">The logger.</param>
    public PluginsCommand(PluginRegistry registry, PluginLoader loader, INextNetLogger logger)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _loader = loader ?? throw new ArgumentNullException(nameof(loader));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates the <c>plugins</c> subcommand.
    /// </summary>
    public Command Create()
    {
        var command = new Command("plugins", "List installed NextNet plugins and their hooks");

        var scanOption = new Option<bool>("--scan", "Scan the plugins/ directory for additional assemblies");
        command.AddOption(scanOption);

        Handler.SetHandler(command, (InvocationContext context) =>
        {
            var scan = context.ParseResult.GetValueForOption(scanOption);
            var exitCode = Execute(scan);
            context.ExitCode = exitCode;
        });

        return command;
    }

    /// <summary>
    /// Executes the plugins command.
    /// </summary>
    /// <param name="scan">Whether to scan the plugins/ directory.</param>
    /// <returns>Exit code (0 = success).</returns>
    public int Execute(bool scan = false)
    {
        try
        {
            var plugins = _registry.GetPlugins();

            if (scan)
            {
                var discovered = _loader.LoadAll(Environment.CurrentDirectory);
                foreach (var plugin in discovered)
                {
                    if (plugins.All(p => p.Name != plugin.Name))
                    {
                        _registry.Register(plugin);
                    }
                }
                plugins = _registry.GetPlugins();
            }

            Console.WriteLine("╔══════════════════════════════════════════════════╗");
            Console.WriteLine("║           NextNet Plugin Registry              ║");
            Console.WriteLine("╚══════════════════════════════════════════════════╝");
            Console.WriteLine();

            if (plugins.Count == 0)
            {
                Console.WriteLine("  No plugins are currently loaded.");
                Console.WriteLine("  Use --scan to scan the plugins/ directory.");
                Console.WriteLine();
                return 0;
            }

            Console.WriteLine($"  Loaded plugins: {plugins.Count}");
            Console.WriteLine();

            foreach (var plugin in plugins)
            {
                Console.WriteLine($"  ── {plugin.Name} v{plugin.Version}");
                if (!string.IsNullOrEmpty(plugin.Description))
                    Console.WriteLine($"     {plugin.Description}");

                // List hook interfaces implemented
                var hooks = GetImplementedHooks(plugin);
                if (hooks.Count > 0)
                {
                    Console.WriteLine($"     Hooks: {string.Join(", ", hooks)}");
                }

                Console.WriteLine();
            }

            return 0;
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to list plugins: {0}", ex.Message);
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    private static List<string> GetImplementedHooks(INextNetPlugin plugin)
    {
        var hooks = new List<string>();
        var type = plugin.GetType();

        if (typeof(IBuildHook).IsAssignableFrom(type))
            hooks.Add("build");
        if (typeof(IRouteScannerHook).IsAssignableFrom(type))
            hooks.Add("routeScanner");
        if (typeof(IStartupHook).IsAssignableFrom(type))
            hooks.Add("startup");
        if (typeof(IRequestHook).IsAssignableFrom(type))
            hooks.Add("request");
        if (typeof(IRenderHook).IsAssignableFrom(type))
            hooks.Add("render");
        if (typeof(IErrorHook).IsAssignableFrom(type))
            hooks.Add("error");

        return hooks;
    }
}
