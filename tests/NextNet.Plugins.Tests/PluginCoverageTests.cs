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
        _defaultContext = new PluginContext
        {
            Services = new MockServiceProvider(),
            Logger = _logger,
            Config = new NextNetConfig(),
            PluginDirectory = "/plugins",
            Manifest = new PluginManifest { Name = "test", Version = "1.0.0" }
        };
    }

    // ─── PluginManifest coverage ───────────────────────────────────────

    [Fact]
    public void PluginManifest_Should_HaveDefaultValues_When_ConstructedWithNoArgs()
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
    public void PluginManifest_Should_StoreValues_When_ConstructedWithArgs()
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
                new PluginDependency { Name = "Dep1", Version = "1.0.0" }
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
    public void PluginDependency_Should_HaveDefaultValues_When_ConstructedWithNoArgs()
    {
        var dep = new PluginDependency();
        Assert.Equal(string.Empty, dep.Name);
        Assert.Equal("1.0.0", dep.Version);
    }

    [Fact]
    public void PluginDependency_Should_StoreValues_When_ConstructedWithArgs()
    {
        var dep = new PluginDependency { Name = "Core", Version = "2.0.0" };
        Assert.Equal("Core", dep.Name);
        Assert.Equal("2.0.0", dep.Version);
    }

    // ─── PluginContext coverage ────────────────────────────────────────

    [Fact]
    public void PluginContext_Should_ThrowArgumentNullException_When_RequiredPropertyIsNull()
    {
        var config = new NextNetConfig();
        var manifest = new PluginManifest { Name = "test", Version = "1.0" };

        Assert.Throws<ArgumentNullException>(() =>
        {
            var ctx = new PluginContext { Services = null!, Logger = _logger, Config = config, PluginDirectory = "/plugins", Manifest = manifest };
            ctx.Validate();
        });
        Assert.Throws<ArgumentNullException>(() =>
        {
            var ctx = new PluginContext { Services = new MockServiceProvider(), Logger = null!, Config = config, PluginDirectory = "/plugins", Manifest = manifest };
            ctx.Validate();
        });
        Assert.Throws<ArgumentNullException>(() =>
        {
            var ctx = new PluginContext { Services = new MockServiceProvider(), Logger = _logger, Config = null!, PluginDirectory = "/plugins", Manifest = manifest };
            ctx.Validate();
        });
        Assert.Throws<ArgumentNullException>(() =>
        {
            var ctx = new PluginContext { Services = new MockServiceProvider(), Logger = _logger, Config = config, PluginDirectory = null!, Manifest = manifest };
            ctx.Validate();
        });
        Assert.Throws<ArgumentNullException>(() =>
        {
            var ctx = new PluginContext { Services = new MockServiceProvider(), Logger = _logger, Config = config, PluginDirectory = "/plugins", Manifest = null! };
            ctx.Validate();
        });
    }

    [Fact]
    public void PluginContext_Should_HaveAccessibleProperties_When_Initialized()
    {
        var services = new MockServiceProvider();
        var config = new NextNetConfig();
        var manifest = new PluginManifest { Name = "ctx-plugin", Version = "3.0.0" };
        var cts = new CancellationTokenSource();

        var ctx = new PluginContext
        {
            Services = services,
            Logger = _logger,
            Config = config,
            PluginDirectory = "/my/plugins",
            Manifest = manifest,
            CancellationToken = cts.Token
        };

        Assert.Same(services, ctx.Services);
        Assert.Same(_logger, ctx.Logger);
        Assert.Same(config, ctx.Config);
        Assert.Equal("/my/plugins", ctx.PluginDirectory);
        Assert.Same(manifest, ctx.Manifest);
        Assert.Equal(cts.Token, ctx.CancellationToken);
    }

    // ─── NextNetPlugin coverage ───────────────────────────────────────

    [Fact]
    public async Task NextNetPlugin_Should_UseDefaultImplementations_When_OnlyNameIsOverridden()
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
    public void LoadAll_Should_ReturnEmptyList_When_DefaultPluginDirectoryDoesNotExist()
    {
        var loader = new PluginLoader(_logger);
        var plugins = loader.LoadAll(Path.GetTempPath());
        // Should not throw and return empty (no plugins directory)
        Assert.NotNull(plugins);
    }

    [Fact]
    public void LoadFromAssemblyMetadata_Should_ReturnPlugin_When_AssemblyHasAttribute()
    {
        var assembly = TestPluginBuilder.BuildPluginAssembly(
            "CoveredPlugin", "1.0.0", typeof(TestPlugin));

        var loader = new PluginLoader(_logger);
        var plugin = loader.LoadFromAssemblyMetadata(assembly);

        Assert.NotNull(plugin);
        Assert.IsType<TestPlugin>(plugin);
    }

    [Fact]
    public void LoadFromAssembly_Should_ReturnNull_When_AssemblyFileDoesNotExist()
    {
        // Test that the LoadFromAssembly code path doesn't crash on non-existent file
        var loader = new PluginLoader(_logger);
        var result = loader.LoadFromAssembly("/tmp/nonexistent_plugin_assembly.dll");
        Assert.Null(result);
    }

    // ─── PluginAssemblyLoadContext coverage ────────────────────────────

    [Fact]
    public void Constructor_Should_CreateCollectibleContext_When_GivenValidAssemblyPath()
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
    public void AddNextNetPlugins_Should_RegisterPluginServices_When_Called()
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
    public void AddNextNetPlugin_Should_RegisterPluginInRegistry_When_ResolvedFromDI()
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
    public void Execute_Should_ReturnZero_When_NoPluginsRegistered()
    {
        var registry = new PluginRegistry(_logger);
        var loader = new PluginLoader(_logger);
        var command = new PluginsCommand(registry, loader, _logger);

        var exitCode = command.Execute();
        Assert.Equal(0, exitCode);
    }

    [Fact]
    public void Execute_Should_ReturnZero_When_PluginsAreRegistered()
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
    public void Execute_Should_ReturnZero_When_ScanOptionIsUsed()
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
    public void GetHooks_Should_ReturnEmpty_When_NoPluginsRegistered()
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
    public void LoadAllFromDirectory_Should_ReturnEmptyList_When_NoMatchingDllFiles()
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
    public void NextNetPluginAttribute_Should_StoreValues_When_ConstructedWithArgs()
    {
        var attr = new NextNetPluginAttribute(typeof(TestPlugin), "AttrPlugin", "4.0.0");

        Assert.Equal(typeof(TestPlugin), attr.PluginType);
        Assert.Equal("AttrPlugin", attr.Name);
        Assert.Equal("4.0.0", attr.Version);
    }

    [Fact]
    public void NextNetPluginAttribute_Should_ThrowArgumentNullException_When_AnyArgumentIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new NextNetPluginAttribute(null!, "name", "1.0"));
        Assert.Throws<ArgumentNullException>(() =>
            new NextNetPluginAttribute(typeof(TestPlugin), null!, "1.0"));
        Assert.Throws<ArgumentNullException>(() =>
            new NextNetPluginAttribute(typeof(TestPlugin), "name", null!));
    }

    [Fact]
    public async Task InitializeAllAsync_Should_NotThrow_When_CalledTwice()
    {
        var registry = new PluginRegistry(_logger);
        registry.Register(new TestPlugin());

        await registry.InitializeAllAsync(p =>
            new PluginContext
            {
                Services = new MockServiceProvider(),
                Logger = _logger,
                Config = new NextNetConfig(),
                PluginDirectory = "/plugins",
                Manifest = new PluginManifest { Name = p.Name, Version = p.Version.ToString() }
            });

        // Second call should not throw but log a warning
        await registry.InitializeAllAsync(p =>
            new PluginContext
            {
                Services = new MockServiceProvider(),
                Logger = _logger,
                Config = new NextNetConfig(),
                PluginDirectory = "/plugins",
                Manifest = new PluginManifest { Name = p.Name, Version = p.Version.ToString() }
            });
    }
}
