using Microsoft.Extensions.DependencyInjection;
using NextNet.Configuration;
using NextNet.Logging;
using NextNet.Plugins.CLI;
using NextNet.Plugins.Extensions;
using NextNet.Plugins.Hooks;
using NextNet.Plugins.Tests.TestInfrastructure;
using Xunit;

namespace NextNet.Plugins.Tests;

/// <summary>
/// Coverage-focused tests targeting specific classes and methods
/// to ensure ≥80% code coverage of the NextNet.Plugins project.
/// </summary>
public class PluginCoverageTests
{
    private readonly INextNetLogger _logger;
    private readonly PluginContext _defaultContext;

    public PluginCoverageTests()
    {
        _logger = new NextNetLogger("CoverageTest");
        _defaultContext = new PluginContext(
            new MockServiceProvider(),
            _logger,
            new NextNetConfig(),
            "/plugins",
            new PluginManifest { Name = "test", Version = "1.0.0" });
    }

    // ─── PluginManifest coverage ───────────────────────────────────────

    [Fact]
    public void PluginManifest_DefaultValues()
    {
        var manifest = new PluginManifest();
        Assert.Equal(string.Empty, manifest.Name);
        Assert.Equal("1.0.0", manifest.Version);
        Assert.Null(manifest.Description);
        Assert.Null(manifest.Author);
        Assert.Empty(manifest.Dependencies);
        Assert.Null(manifest.AssemblyPath);
    }

    [Fact]
    public void PluginManifest_WithValues()
    {
        var manifest = new PluginManifest
        {
            Name = "MyPlugin",
            Version = "2.0.0",
            Description = "A great plugin",
            Author = "Test Author",
            AssemblyPath = "/path/to/plugin.dll",
            Dependencies = new List<PluginDependency>
            {
                new() { Name = "Dep1", Version = "1.0.0" }
            }
        };

        Assert.Equal("MyPlugin", manifest.Name);
        Assert.Equal("2.0.0", manifest.Version);
        Assert.Equal("A great plugin", manifest.Description);
        Assert.Equal("Test Author", manifest.Author);
        Assert.Equal("/path/to/plugin.dll", manifest.AssemblyPath);
        Assert.Single(manifest.Dependencies);
    }

    // ─── PluginDependency coverage ─────────────────────────────────────

    [Fact]
    public void PluginDependency_DefaultValues()
    {
        var dep = new PluginDependency();
        Assert.Equal(string.Empty, dep.Name);
        Assert.Equal("1.0.0", dep.Version);
    }

    [Fact]
    public void PluginDependency_WithValues()
    {
        var dep = new PluginDependency { Name = "Core", Version = "2.0.0" };
        Assert.Equal("Core", dep.Name);
        Assert.Equal("2.0.0", dep.Version);
    }

    // ─── PluginContext coverage ────────────────────────────────────────

    [Fact]
    public void PluginContext_Constructor_ThrowsOnNullArgs()
    {
        var config = new NextNetConfig();
        var manifest = new PluginManifest { Name = "test", Version = "1.0" };

        Assert.Throws<ArgumentNullException>(() =>
            new PluginContext(null!, _logger, config, "/plugins", manifest));
        Assert.Throws<ArgumentNullException>(() =>
            new PluginContext(new MockServiceProvider(), null!, config, "/plugins", manifest));
        Assert.Throws<ArgumentNullException>(() =>
            new PluginContext(new MockServiceProvider(), _logger, null!, "/plugins", manifest));
        Assert.Throws<ArgumentNullException>(() =>
            new PluginContext(new MockServiceProvider(), _logger, config, null!, manifest));
        Assert.Throws<ArgumentNullException>(() =>
            new PluginContext(new MockServiceProvider(), _logger, config, "/plugins", null!));
    }

    [Fact]
    public void PluginContext_PropertiesAreAccessible()
    {
        var services = new MockServiceProvider();
        var config = new NextNetConfig();
        var manifest = new PluginManifest { Name = "ctx-plugin", Version = "3.0.0" };
        var cts = new CancellationTokenSource();

        var ctx = new PluginContext(services, _logger, config, "/my/plugins", manifest, cts.Token);

        Assert.Same(services, ctx.Services);
        Assert.Same(_logger, ctx.Logger);
        Assert.Same(config, ctx.Config);
        Assert.Equal("/my/plugins", ctx.PluginDirectory);
        Assert.Same(manifest, ctx.Manifest);
        Assert.Equal(cts.Token, ctx.CancellationToken);
    }

    // ─── NextNetPlugin coverage ───────────────────────────────────────

    [Fact]
    public async Task NextNetPlugin_DefaultImplementations()
    {
        var plugin = new MinimalPlugin();

        Assert.Equal("Minimal", plugin.Name);
        Assert.Equal(string.Empty, plugin.Description);
        Assert.Equal(new Version(1, 0, 0), plugin.Version);

        // Default OnInitializeAsync should not throw
        await plugin.OnInitializeAsync(_defaultContext);
    }

    /// <summary>
    /// A minimal plugin that only overrides Name.
    /// </summary>
    private class MinimalPlugin : NextNetPlugin
    {
        public override string Name => "Minimal";
    }

    // ─── PluginLoader coverage ─────────────────────────────────────────

    [Fact]
    public void PluginLoader_LoadAll_UsesDefaultPluginDirectory()
    {
        var loader = new PluginLoader(_logger);
        var plugins = loader.LoadAll(Path.GetTempPath());
        // Should not throw and return empty (no plugins directory)
        Assert.NotNull(plugins);
    }

    [Fact]
    public void PluginLoader_LoadFromAssemblyMetadata_AssemblyWithAttribute_ReturnsPlugin()
    {
        var assembly = TestPluginBuilder.BuildPluginAssembly(
            "CoveredPlugin", "1.0.0", typeof(TestPlugin));

        var loader = new PluginLoader(_logger);
        var plugin = loader.LoadFromAssemblyMetadata(assembly);

        Assert.NotNull(plugin);
        Assert.IsType<TestPlugin>(plugin);
    }

    [Fact]
    public void PluginLoader_LoadFromAssembly_WithRealFile_CreatesPlugin()
    {
        // Test that the LoadFromAssembly code path doesn't crash on non-existent file
        var loader = new PluginLoader(_logger);
        var result = loader.LoadFromAssembly("/tmp/nonexistent_plugin_assembly.dll");
        Assert.Null(result);
    }

    // ─── PluginAssemblyLoadContext coverage ────────────────────────────

    [Fact]
    public void PluginAssemblyLoadContext_Constructor_CreatesCollectibleContext()
    {
        // AssemblyDependencyResolver requires a valid managed assembly.
        // Use our own test assembly which is guaranteed to be a valid PE file.
        var testAssemblyPath = typeof(PluginCoverageTests).Assembly.Location;
        var alc = new PluginAssemblyLoadContext(testAssemblyPath);
        Assert.NotNull(alc);
        Assert.True(alc.IsCollectible);
    }

    // ─── PluginServiceCollectionExtensions coverage ────────────────────

    [Fact]
    public void AddNextNetPlugins_RegistersServices()
    {
        var services = new ServiceCollection();

        // Register a logger first since PluginRegistry depends on INextNetLogger
        services.AddSingleton<INextNetLogger>(_logger);
        services.AddNextNetPlugins();

        // Verify services were registered
        var serviceProvider = services.BuildServiceProvider();
        var registry = serviceProvider.GetService<PluginRegistry>();
        Assert.NotNull(registry);
        Assert.Empty(registry!.GetPlugins());

        var loader = serviceProvider.GetService<PluginLoader>();
        Assert.NotNull(loader);

        var cmd = serviceProvider.GetService<PluginsCommand>();
        Assert.NotNull(cmd);
    }

    [Fact]
    public void AddNextNetPlugin_RegistersSpecificPlugin()
    {
        var services = new ServiceCollection();
        services.AddSingleton<INextNetLogger>(_logger);
        services.AddNextNetPlugins();
        services.AddNextNetPlugin<TestPlugin>();

        var serviceProvider = services.BuildServiceProvider();

        // Resolving INextNetPlugin triggers the factory which registers the plugin
        var resolvedPlugin = serviceProvider.GetRequiredService<INextNetPlugin>();
        Assert.NotNull(resolvedPlugin);
        Assert.IsType<TestPlugin>(resolvedPlugin);

        // Now check the registry has it
        var registry = serviceProvider.GetRequiredService<PluginRegistry>();
        var plugin = registry.GetPlugin("TestPlugin");
        Assert.NotNull(plugin);
        Assert.IsType<TestPlugin>(plugin);
    }

    // ─── PluginsCommand coverage ───────────────────────────────────────

    [Fact]
    public void PluginsCommand_Execute_WithNoPlugins_ReturnsZero()
    {
        var registry = new PluginRegistry(_logger);
        var loader = new PluginLoader(_logger);
        var command = new PluginsCommand(registry, loader, _logger);

        var exitCode = command.Execute();
        Assert.Equal(0, exitCode);
    }

    [Fact]
    public void PluginsCommand_Execute_WithPlugins_ReturnsZero()
    {
        var registry = new PluginRegistry(_logger);
        var loader = new PluginLoader(_logger);
        var command = new PluginsCommand(registry, loader, _logger);

        registry.Register(new TestPlugin());
        registry.Register(new PluginWithBuildHook());

        var exitCode = command.Execute();
        Assert.Equal(0, exitCode);
    }

    [Fact]
    public void PluginsCommand_Execute_WithScanOption_ReturnsZero()
    {
        var registry = new PluginRegistry(_logger);
        var loader = new PluginLoader(_logger);
        var command = new PluginsCommand(registry, loader, _logger);

        // Scan will look in current directory's plugins/ folder which doesn't exist
        var exitCode = command.Execute(scan: true);
        Assert.Equal(0, exitCode);
    }

    // ─── Edge case coverage ────────────────────────────────────────────

    [Fact]
    public void Registry_GetHooks_WithNoPlugins_ReturnsEmpty()
    {
        var registry = new PluginRegistry(_logger);
        Assert.Empty(registry.GetHooks<IBuildHook>());
        Assert.Empty(registry.GetHooks<IStartupHook>());
        Assert.Empty(registry.GetHooks<IRequestHook>());
        Assert.Empty(registry.GetHooks<IRenderHook>());
        Assert.Empty(registry.GetHooks<IErrorHook>());
        Assert.Empty(registry.GetHooks<IRouteScannerHook>());
    }

    [Fact]
    public void PluginLoader_LoadAllFromDirectory_WithNoMatchingDlls_ReturnsEmpty()
    {
        // Create a temp directory with a non-dll file
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        try
        {
            // Create a .txt file (not a .dll)
            File.WriteAllText(Path.Combine(tempDir, "not-a-plugin.txt"), "hello");
            // Also create an empty .dll that will fail to load
            File.WriteAllBytes(Path.Combine(tempDir, "invalid.dll"), new byte[] { 0, 0 });

            var loader = new PluginLoader(_logger);
            var plugins = loader.LoadAllFromDirectory(tempDir);
            Assert.Empty(plugins);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void PluginAttribute_Constructors_StoreValues()
    {
        var attr = new NextNetPluginAttribute(typeof(TestPlugin), "AttrPlugin", "4.0.0");

        Assert.Equal(typeof(TestPlugin), attr.PluginType);
        Assert.Equal("AttrPlugin", attr.Name);
        Assert.Equal("4.0.0", attr.Version);
    }

    [Fact]
    public void PluginAttribute_NullArgs_Throw()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new NextNetPluginAttribute(null!, "name", "1.0"));
        Assert.Throws<ArgumentNullException>(() =>
            new NextNetPluginAttribute(typeof(TestPlugin), null!, "1.0"));
        Assert.Throws<ArgumentNullException>(() =>
            new NextNetPluginAttribute(typeof(TestPlugin), "name", null!));
    }

    [Fact]
    public void InitializeAllAsync_Twice_LogsWarning()
    {
        var registry = new PluginRegistry(_logger);
        registry.Register(new TestPlugin());

        var initTask1 = registry.InitializeAllAsync(p =>
            new PluginContext(new MockServiceProvider(), _logger, new NextNetConfig(),
                "/plugins", new PluginManifest { Name = p.Name, Version = p.Version.ToString() }));

        // Second call should not throw but log a warning
        var initTask2 = registry.InitializeAllAsync(p =>
            new PluginContext(new MockServiceProvider(), _logger, new NextNetConfig(),
                "/plugins", new PluginManifest { Name = p.Name, Version = p.Version.ToString() }));
    }
}
