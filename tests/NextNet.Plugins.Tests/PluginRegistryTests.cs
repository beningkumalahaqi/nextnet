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
    public void Constructor_Should_ThrowArgumentNullException_When_LoggerIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new PluginRegistry(null!));
    }

    [Fact]
    public void GetPlugins_Should_ReturnEmptyList_When_NoPluginsRegistered()
    {
        var registry = new PluginRegistry(_logger);
        var plugins = registry.GetPlugins();
        Assert.Empty(plugins);
    }

    [Fact]
    public void Register_Should_AddPluginToRegistry_When_PluginIsValid()
    {
        var registry = new PluginRegistry(_logger);
        var plugin = new TestPlugin();

        registry.Register(plugin);

        Assert.Single(registry.GetPlugins());
        Assert.Equal("TestPlugin", registry.GetPlugins()[0].Name);
    }

    [Fact]
    public void Register_Should_ThrowArgumentNullException_When_PluginIsNull()
    {
        var registry = new PluginRegistry(_logger);
        Assert.Throws<ArgumentNullException>(() => registry.Register(null!));
    }

    [Fact]
    public void Register_Should_NotAddDuplicate_When_PluginNameAlreadyExists()
    {
        var registry = new PluginRegistry(_logger);
        registry.Register(new TestPlugin());
        registry.Register(new TestPlugin()); // Same name

        Assert.Single(registry.GetPlugins());
    }

    [Fact]
    public void RegisterAll_Should_AddMultiplePlugins_When_GivenList()
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
    public void Unregister_Should_RemoveExistingPlugin_When_NameMatches()
    {
        var registry = new PluginRegistry(_logger);
        registry.Register(new TestPlugin());

        var result = registry.Unregister("TestPlugin");

        Assert.True(result);
        Assert.Empty(registry.GetPlugins());
    }

    [Fact]
    public void Unregister_Should_ReturnFalse_When_PluginNotFound()
    {
        var registry = new PluginRegistry(_logger);
        var result = registry.Unregister("NonExistent");
        Assert.False(result);
    }

    [Fact]
    public void GetPlugin_Should_ReturnCorrectPlugin_When_NameMatches()
    {
        var registry = new PluginRegistry(_logger);
        registry.Register(new TestPlugin());
        registry.Register(new AnotherTestPlugin());

        var plugin = registry.GetPlugin("AnotherTestPlugin");

        Assert.NotNull(plugin);
        Assert.IsType<AnotherTestPlugin>(plugin);
    }

    [Fact]
    public void GetPlugin_Should_ReturnNull_When_NameDoesNotExist()
    {
        var registry = new PluginRegistry(_logger);
        var plugin = registry.GetPlugin("NonExistent");
        Assert.Null(plugin);
    }

    [Fact]
    public void GetHooks_Should_ReturnEmptyList_When_NoMatchingHooks()
    {
        var registry = new PluginRegistry(_logger);
        registry.Register(new TestPlugin()); // Does not implement any hook

        var hooks = registry.GetHooks<IBuildHook>();

        Assert.Empty(hooks);
    }

    [Fact]
    public void GetHooks_Should_ReturnPlugin_When_PluginImplementsBuildHook()
    {
        var registry = new PluginRegistry(_logger);
        registry.Register(new PluginWithBuildHook());

        var hooks = registry.GetHooks<IBuildHook>();

        Assert.Single(hooks);
    }

    [Fact]
    public void GetHooks_Should_FilterCorrectly_When_MultipleHookTypesExist()
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
    public void GetHooks_Should_InvalidateCache_When_NewPluginRegistered()
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
    public async Task InitializeAllAsync_Should_NotThrow_When_NoPlugins()
    {
        var registry = new PluginRegistry(_logger);

        await registry.InitializeAllAsync(p =>
        {
            Assert.Fail("Should not be called");
            return null!;
        });
    }

    [Fact]
    public async Task InitializeAllAsync_Should_CallOnInitializeForEachPlugin()
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
            return new PluginContext
            {
                Services = new MockServiceProvider(),
                Logger = _logger,
                Config = new NextNet.Configuration.NextNetConfig(),
                PluginDirectory = "/plugins",
                Manifest = new PluginManifest { Name = p.Name, Version = p.Version.ToString() }
            };
        });

        Assert.Equal(2, initCalls);
        Assert.True(plugin1.WasInitialized);
        Assert.True(plugin2.WasInitialized);
    }

    [Fact]
    public async Task InitializeAllAsync_Should_NotPreventOtherPlugins_When_OnePluginThrows()
    {
        var registry = new PluginRegistry(_logger);
        var failingPlugin = new PluginThatFailsOnInit();
        var goodPlugin = new TestPlugin();

        registry.Register(failingPlugin);
        registry.Register(goodPlugin);

        await registry.InitializeAllAsync(p =>
        {
            return new PluginContext
            {
                Services = new MockServiceProvider(),
                Logger = _logger,
                Config = new NextNet.Configuration.NextNetConfig(),
                PluginDirectory = "/plugins",
                Manifest = new PluginManifest { Name = p.Name, Version = p.Version.ToString() }
            };
        });

        Assert.True(failingPlugin.InitializeWasCalled);
        Assert.True(goodPlugin.WasInitialized);
    }

    [Fact]
    public void Count_Should_ReturnCorrectNumberOfPlugins_After_RegisterAndUnregister()
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
