using NextNet.Logging;
using NextNet.Plugins.Hooks;
using NextNet.Plugins.Tests.TestInfrastructure;
using Xunit;

namespace NextNet.Plugins.Tests;

public class PluginRegistryTests
{
    private readonly INextNetLogger _logger;

    public PluginRegistryTests()
    {
        _logger = new NextNetLogger("Test");
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new PluginRegistry(null!));
    }

    [Fact]
    public void GetPlugins_WhenEmpty_ReturnsEmptyList()
    {
        var registry = new PluginRegistry(_logger);
        var plugins = registry.GetPlugins();
        Assert.Empty(plugins);
    }

    [Fact]
    public void Register_WithValidPlugin_AddsToRegistry()
    {
        var registry = new PluginRegistry(_logger);
        var plugin = new TestPlugin();

        registry.Register(plugin);

        Assert.Single(registry.GetPlugins());
        Assert.Equal("TestPlugin", registry.GetPlugins()[0].Name);
    }

    [Fact]
    public void Register_WithNull_ThrowsArgumentNullException()
    {
        var registry = new PluginRegistry(_logger);
        Assert.Throws<ArgumentNullException>(() => registry.Register(null!));
    }

    [Fact]
    public void Register_DuplicatePluginName_DoesNotAddDuplicate()
    {
        var registry = new PluginRegistry(_logger);
        registry.Register(new TestPlugin());
        registry.Register(new TestPlugin()); // Same name

        Assert.Single(registry.GetPlugins());
    }

    [Fact]
    public void RegisterAll_AddsMultiplePlugins()
    {
        var registry = new PluginRegistry(_logger);
        registry.RegisterAll(new INextNetPlugin[]
        {
            new TestPlugin(),
            new AnotherTestPlugin()
        });

        Assert.Equal(2, registry.Count);
    }

    [Fact]
    public void Unregister_ExistingPlugin_RemovesIt()
    {
        var registry = new PluginRegistry(_logger);
        registry.Register(new TestPlugin());

        var result = registry.Unregister("TestPlugin");

        Assert.True(result);
        Assert.Empty(registry.GetPlugins());
    }

    [Fact]
    public void Unregister_NonExistentPlugin_ReturnsFalse()
    {
        var registry = new PluginRegistry(_logger);
        var result = registry.Unregister("NonExistent");
        Assert.False(result);
    }

    [Fact]
    public void GetPlugin_ByName_ReturnsCorrectPlugin()
    {
        var registry = new PluginRegistry(_logger);
        registry.Register(new TestPlugin());
        registry.Register(new AnotherTestPlugin());

        var plugin = registry.GetPlugin("AnotherTestPlugin");

        Assert.NotNull(plugin);
        Assert.IsType<AnotherTestPlugin>(plugin);
    }

    [Fact]
    public void GetPlugin_NonExistent_ReturnsNull()
    {
        var registry = new PluginRegistry(_logger);
        var plugin = registry.GetPlugin("NonExistent");
        Assert.Null(plugin);
    }

    [Fact]
    public void GetHooks_WithNoMatchingHooks_ReturnsEmptyList()
    {
        var registry = new PluginRegistry(_logger);
        registry.Register(new TestPlugin()); // Does not implement any hook

        var hooks = registry.GetHooks<IBuildHook>();

        Assert.Empty(hooks);
    }

    [Fact]
    public void GetHooks_WithBuildHook_ReturnsPlugin()
    {
        var registry = new PluginRegistry(_logger);
        registry.Register(new PluginWithBuildHook());

        var hooks = registry.GetHooks<IBuildHook>();

        Assert.Single(hooks);
    }

    [Fact]
    public void GetHooks_WithMultipleHookTypes_ReturnsCorrectly()
    {
        var registry = new PluginRegistry(_logger);
        registry.Register(new PluginWithBuildHook());        // IBuildHook
        registry.Register(new PluginWithMultipleHooks());     // IBuildHook + IStartupHook
        registry.Register(new TestPlugin());                  // No hooks

        var buildHooks = registry.GetHooks<IBuildHook>();
        var startupHooks = registry.GetHooks<IStartupHook>();
        var errorHooks = registry.GetHooks<IErrorHook>();

        Assert.Equal(2, buildHooks.Count);  // PluginWithBuildHook + PluginWithMultipleHooks
        Assert.Single(startupHooks);        // PluginWithMultipleHooks only
        Assert.Empty(errorHooks);           // No error hook implementations
    }

    [Fact]
    public void GetHooks_IsCached_AndUpdatedOnRegister()
    {
        var registry = new PluginRegistry(_logger);
        registry.Register(new PluginWithBuildHook());

        var hooks1 = registry.GetHooks<IBuildHook>();

        registry.Register(new PluginWithMultipleHooks());

        var hooks2 = registry.GetHooks<IBuildHook>();

        Assert.Single(hooks1);
        Assert.Equal(2, hooks2.Count); // Cache was invalidated
    }

    [Fact]
    public async Task InitializeAllAsync_WithoutPlugins_DoesNotThrow()
    {
        var registry = new PluginRegistry(_logger);

        await registry.InitializeAllAsync(p =>
        {
            Assert.Fail("Should not be called");
            return null!;
        });
    }

    [Fact]
    public async Task InitializeAllAsync_CallsOnInitializeForEachPlugin()
    {
        var registry = new PluginRegistry(_logger);
        var plugin1 = new TestPlugin();
        var plugin2 = new AnotherTestPlugin();

        registry.Register(plugin1);
        registry.Register(plugin2);

        var initCalls = 0;
        await registry.InitializeAllAsync(p =>
        {
            initCalls++;
            return new PluginContext(
                new MockServiceProvider(),
                _logger,
                new NextNet.Configuration.NextNetConfig(),
                "/plugins",
                new PluginManifest { Name = p.Name, Version = p.Version.ToString() });
        });

        Assert.Equal(2, initCalls);
        Assert.True(plugin1.WasInitialized);
        Assert.True(plugin2.WasInitialized);
    }

    [Fact]
    public async Task InitializeAllAsync_PluginException_DoesNotPreventOtherPlugins()
    {
        var registry = new PluginRegistry(_logger);
        var failingPlugin = new PluginThatFailsOnInit();
        var goodPlugin = new TestPlugin();

        registry.Register(failingPlugin);
        registry.Register(goodPlugin);

        await registry.InitializeAllAsync(p =>
        {
            return new PluginContext(
                new MockServiceProvider(),
                _logger,
                new NextNet.Configuration.NextNetConfig(),
                "/plugins",
                new PluginManifest { Name = p.Name, Version = p.Version.ToString() });
        });

        Assert.True(failingPlugin.InitializeWasCalled);
        Assert.True(goodPlugin.WasInitialized);
    }

    [Fact]
    public void Count_ReturnsCorrectNumberOfPlugins()
    {
        var registry = new PluginRegistry(_logger);
        Assert.Equal(0, registry.Count);

        registry.Register(new TestPlugin());
        Assert.Equal(1, registry.Count);

        registry.Register(new AnotherTestPlugin());
        Assert.Equal(2, registry.Count);

        registry.Unregister("TestPlugin");
        Assert.Equal(1, registry.Count);
    }
}
