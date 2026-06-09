using NextNet.Configuration;
using NextNet.Logging;
using NextNet.Plugins.Hooks;
using NextNet.Plugins.Tests.TestInfrastructure;
using NextNet.Routing;
using NextNet.Routing.Models;
using Xunit;

namespace NextNet.Plugins.Tests.Integration;

/// <summary>
/// Integration tests that exercise the full plugin lifecycle: discovery, loading,
/// registration, initialization, and hook execution.
/// </summary>
public class PluginIntegrationTests
{
    private readonly INextNetLogger _logger;

    public PluginIntegrationTests()
    {
        _logger = new NextNetLogger("IntegrationTest");
    }

    [Fact]
    public void FullLifecycle_Should_DiscoverRegisterAndQueryPlugins_When_UsingAssemblyMetadata()
    {
        // Arrange: simulate discovering plugins from assembly metadata
        var loader = new PluginLoader(_logger);
        var registry = new PluginRegistry(_logger);

        // Using typeof(TestPlugin) for both since it's proven to work with dynamic assemblies.
        // Name comes from the class, not the attribute parameter.
        var assembly1 = TestPluginBuilder.BuildPluginAssembly(
            "PluginAlpha", "1.0.0", typeof(TestPlugin));
        var assembly2 = TestPluginBuilder.BuildPluginAssembly(
            "PluginBeta", "2.0.0", typeof(AnotherTestPlugin));

        var plugin1 = loader.LoadFromAssemblyMetadata(assembly1);
        var plugin2 = loader.LoadFromAssemblyMetadata(assembly2);

        Assert.NotNull(plugin1);
        Assert.NotNull(plugin2);

        // Act: register all discovered plugins
        registry.Register(plugin1!);
        registry.Register(plugin2!);

        // Assert: query all plugins - names come from the class, not attribute
        Assert.Equal(2, registry.Count);
        Assert.NotNull(registry.GetPlugin("TestPlugin"));
        Assert.NotNull(registry.GetPlugin("AnotherTestPlugin"));

        // Assert: hook filtering works (neither implements IBuildHook)
        var buildHooks = registry.GetHooks<IBuildHook>();
        Assert.Empty(buildHooks);

        var emptyHooks = registry.GetHooks<IErrorHook>();
        Assert.Empty(emptyHooks);
    }

    [Fact]
    public async Task FullLifecycle_Should_InitializeAllPlugins_When_CalledWithContextFactory()
    {
        // Arrange
        var registry = new PluginRegistry(_logger);
        registry.Register(new TestPlugin());
        registry.Register(new PluginWithBuildHook());

        var config = new NextNetConfig();

        // Act: initialize all plugins
        await registry.InitializeAllAsync(p =>
        {
            return new PluginContext
            {
                Services = new MockServiceProvider(),
                Logger = _logger,
                Config = config,
                PluginDirectory = "/plugins",
                Manifest = new PluginManifest { Name = p.Name, Version = p.Version.ToString() }
            };
        });

        // Assert: all plugins were initialized
        var testPlugin = (TestPlugin)registry.GetPlugin("TestPlugin")!;
        Assert.True(testPlugin.WasInitialized);
    }

    [Fact]
    public async Task BuildHooks_Should_ExecuteInOrder_When_SimulatingBuildPipeline()
    {
        // Arrange
        var registry = new PluginRegistry(_logger);
        registry.Register(new PluginWithBuildHook());
        registry.Register(new PluginWithMultipleHooks());

        var ctx = new PluginContext
        {
            Services = new MockServiceProvider(),
            Logger = _logger,
            Config = new NextNetConfig(),
            PluginDirectory = "/plugins",
            Manifest = new PluginManifest { Name = "test", Version = "1.0.0" }
        };

        // Act: simulate build pipeline
        var buildHooks = registry.GetHooks<IBuildHook>();

        // Phase 1: Build start
        foreach (var hook in buildHooks)
        {
            await hook.OnBuildStart(ctx);
        }

        // Phase 2: (build would execute here)

        // Phase 3: Build end
        foreach (var hook in buildHooks)
        {
            await hook.OnBuildEnd(ctx);
        }

        // Assert
        var plugin1 = (PluginWithBuildHook)registry.GetPlugin("PluginWithBuildHook")!;
        var plugin2 = (PluginWithMultipleHooks)registry.GetPlugin("PluginWithMultipleHooks")!;

        Assert.True(plugin1.BuildStartCalled);
        Assert.True(plugin1.BuildEndCalled);
        Assert.True(plugin2.BuildStartCalled);
        Assert.True(plugin2.BuildEndCalled);
    }

    [Fact]
    public async Task ErrorHook_Should_ReceiveException_When_SimulatingErrorPipeline()
    {
        // Arrange
        var registry = new PluginRegistry(_logger);
        registry.Register(new PluginWithErrorHook());

        var ctx = new PluginContext
        {
            Services = new MockServiceProvider(),
            Logger = _logger,
            Config = new NextNetConfig(),
            PluginDirectory = "/plugins",
            Manifest = new PluginManifest { Name = "test", Version = "1.0.0" }
        };

        var simulatedException = new InvalidOperationException("Something went wrong");

        // Act: simulate error pipeline
        var errorHooks = registry.GetHooks<IErrorHook>();
        foreach (var hook in errorHooks)
        {
            await hook.OnError(ctx, simulatedException);
        }

        // Assert
        var errorPlugin = (PluginWithErrorHook)registry.GetPlugin("PluginWithErrorHook")!;
        Assert.True(errorPlugin.ErrorCalled);
        Assert.Same(simulatedException, errorPlugin.ReceivedException);
    }

    [Fact]
    public void PluginIsolation_Should_NotInterfere_When_RegisteringAndUnregistering()
    {
        // Arrange: simulate two independent registrations
        var registry = new PluginRegistry(_logger);

        // Act: register and unregister in sequence
        registry.Register(new TestPlugin());
        registry.Register(new AnotherTestPlugin());
        Assert.Equal(2, registry.Count);

        registry.Unregister("TestPlugin");
        Assert.Single(registry.GetPlugins());
        Assert.Equal("AnotherTestPlugin", registry.GetPlugins()[0].Name);

        registry.Register(new TestPlugin());
        Assert.Equal(2, registry.Count);
    }

    [Fact]
    public async Task PluginException_Should_NotCrashHost_When_FailingPluginIsInitialized()
    {
        // Arrange
        var registry = new PluginRegistry(_logger);
        registry.Register(new PluginThatFailsOnInit());
        registry.Register(new TestPlugin());

        var config = new NextNetConfig();

        // Act & Assert: initialization of the failing plugin is caught,
        // and the good plugin still initializes
        var exception = await Record.ExceptionAsync(() =>
            registry.InitializeAllAsync(p =>
            {
                return new PluginContext
                {
                    Services = new MockServiceProvider(),
                    Logger = _logger,
                    Config = config,
                    PluginDirectory = "/plugins",
                    Manifest = new PluginManifest { Name = p.Name, Version = p.Version.ToString() }
                };
            }));

        Assert.Null(exception); // The registry should not throw

        var goodPlugin = (TestPlugin)registry.GetPlugin("TestPlugin")!;
        Assert.True(goodPlugin.WasInitialized);
    }

    [Fact]
    public async Task RouteScannerHook_Should_ReceiveManifest_When_InvokedWithRouteManifest()
    {
        // Arrange
        var registry = new PluginRegistry(_logger);
        registry.Register(new PluginWithRouteScannerHook());

        // Create a real RouteManifest
        var manifest = new RouteManifest(
            new List<RouteEntry>(),
            new List<RouteEntry>(),
            new List<RouteEntry>(),
            new List<RouteEntry>(),
            null,
            new List<RouteConflict>());

        var ctx = new PluginContext
        {
            Services = new MockServiceProvider(),
            Logger = _logger,
            Config = new NextNetConfig(),
            PluginDirectory = "/plugins",
            Manifest = new PluginManifest { Name = "test", Version = "1.0.0" }
        };

        // Act
        var hooks = registry.GetHooks<IRouteScannerHook>();
        foreach (var hook in hooks)
        {
            await hook.OnRoutesDiscovered(ctx, manifest);
        }

        // Assert
        var routePlugin = (PluginWithRouteScannerHook)registry.GetPlugin("PluginWithRouteScannerHook")!;
        Assert.True(routePlugin.RoutesDiscoveredCalled);
        Assert.Same(manifest, routePlugin.ReceivedManifest);
    }
}
