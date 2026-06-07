using Microsoft.AspNetCore.Http;
using NextNet.Logging;
using NextNet.Plugins.Hooks;
using NextNet.Plugins.Tests.TestInfrastructure;
using NextNet.Routing;
using Xunit;

namespace NextNet.Plugins.Tests;

public class HookExecutionTests
{
    private readonly INextNetLogger _logger;
    private readonly PluginContext _defaultContext;

    public HookExecutionTests()
    {
        _logger = new NextNetLogger("Test");
        _defaultContext = new PluginContext(
            new MockServiceProvider(),
            _logger,
            new NextNet.Configuration.NextNetConfig(),
            "/plugins",
            new PluginManifest { Name = "Test", Version = "1.0.0" });
    }

    [Fact]
    public void BuildHook_PluginsImplementingInterface_AreDiscoverable()
    {
        var registry = new PluginRegistry(_logger);
        registry.Register(new PluginWithBuildHook());
        registry.Register(new TestPlugin()); // no hooks

        var hooks = registry.GetHooks<IBuildHook>();

        Assert.Single(hooks);
        Assert.IsAssignableFrom<IBuildHook>(hooks[0]);
    }

    [Fact]
    public async Task BuildHook_OnBuildStart_CalledCorrectly()
    {
        var plugin = new PluginWithBuildHook();
        var registry = new PluginRegistry(_logger);
        registry.Register(plugin);

        var hooks = registry.GetHooks<IBuildHook>();
        foreach (var hook in hooks)
        {
            await hook.OnBuildStart(_defaultContext);
        }

        Assert.True(plugin.BuildStartCalled);
        Assert.False(plugin.BuildEndCalled);
    }

    [Fact]
    public async Task BuildHook_OnBuildEnd_CalledCorrectly()
    {
        var plugin = new PluginWithBuildHook();
        var registry = new PluginRegistry(_logger);
        registry.Register(plugin);

        var hooks = registry.GetHooks<IBuildHook>();
        foreach (var hook in hooks)
        {
            await hook.OnBuildEnd(_defaultContext);
        }

        Assert.True(plugin.BuildEndCalled);
        Assert.False(plugin.BuildStartCalled);
    }

    [Fact]
    public async Task RouteScannerHook_OnRoutesDiscovered_CalledWithManifest()
    {
        var plugin = new PluginWithRouteScannerHook();
        var registry = new PluginRegistry(_logger);
        registry.Register(plugin);

        var manifest = RouteManifest.Empty;
        var hooks = registry.GetHooks<IRouteScannerHook>();
        foreach (var hook in hooks)
        {
            await hook.OnRoutesDiscovered(_defaultContext, manifest);
        }

        Assert.True(plugin.RoutesDiscoveredCalled);
        Assert.Same(manifest, plugin.ReceivedManifest);
    }

    [Fact]
    public async Task StartupHook_OnStartup_CalledCorrectly()
    {
        var plugin = new PluginWithMultipleHooks();
        var registry = new PluginRegistry(_logger);
        registry.Register(plugin);

        var hooks = registry.GetHooks<IStartupHook>();
        foreach (var hook in hooks)
        {
            await hook.OnStartup(_defaultContext);
        }

        Assert.True(plugin.StartupCalled);
    }

    [Fact]
    public async Task RequestHook_OnRequestAsync_CalledWithHttpContext()
    {
        var plugin = new PluginWithRequestHook();
        var registry = new PluginRegistry(_logger);
        registry.Register(plugin);

        var httpContext = new DefaultHttpContext();
        var hooks = registry.GetHooks<IRequestHook>();
        foreach (var hook in hooks)
        {
            await hook.OnRequestAsync(_defaultContext, httpContext);
        }

        Assert.True(plugin.RequestCalled);
        Assert.Same(httpContext, plugin.ReceivedHttpContext);
    }

    [Fact]
    public async Task ErrorHook_OnError_CalledWithException()
    {
        var plugin = new PluginWithErrorHook();
        var registry = new PluginRegistry(_logger);
        registry.Register(plugin);

        var exception = new InvalidOperationException("Test error");
        var hooks = registry.GetHooks<IErrorHook>();
        foreach (var hook in hooks)
        {
            await hook.OnError(_defaultContext, exception);
        }

        Assert.True(plugin.ErrorCalled);
        Assert.Same(exception, plugin.ReceivedException);
    }

    [Fact]
    public async Task RenderHook_OnPreRender_CalledCorrectly()
    {
        var plugin = new PluginWithRenderHook();
        var registry = new PluginRegistry(_logger);
        registry.Register(plugin);

        var httpContext = new DefaultHttpContext();
        var hooks = registry.GetHooks<IRenderHook>();
        foreach (var hook in hooks)
        {
            await hook.OnPreRender(_defaultContext, httpContext);
        }

        Assert.True(plugin.PreRenderCalled);
    }

    [Fact]
    public async Task RenderHook_OnPostRender_CalledWithContent()
    {
        var plugin = new PluginWithRenderHook();
        var registry = new PluginRegistry(_logger);
        registry.Register(plugin);

        var content = new Microsoft.AspNetCore.Html.HtmlString("<div>test</div>");
        var hooks = registry.GetHooks<IRenderHook>();
        foreach (var hook in hooks)
        {
            await hook.OnPostRender(_defaultContext, content);
        }

        Assert.True(plugin.PostRenderCalled);
    }

    [Fact]
    public async Task HookException_DoesNotThrowInRegistry()
    {
        var registry = new PluginRegistry(_logger);
        registry.Register(new PluginWithFailingBuildHook());

        var hooks = registry.GetHooks<IBuildHook>();

        // The hook itself will throw when called, but the registry should not throw
        // when enumerating or retrieving hooks
        Assert.Single(hooks);

        // The exception should come from the hook execution, not the registry
        var hook = hooks[0];
        await Assert.ThrowsAsync<InvalidOperationException>(() => hook.OnBuildStart(_defaultContext));
    }

    [Fact]
    public void MultipleHooks_OnSamePlugin_AllDiscovered()
    {
        var plugin = new PluginWithMultipleHooks();
        var type = plugin.GetType();

        Assert.True(typeof(IBuildHook).IsAssignableFrom(type));
        Assert.True(typeof(IStartupHook).IsAssignableFrom(type));

        var registry = new PluginRegistry(_logger);
        registry.Register(plugin);

        Assert.Single(registry.GetHooks<IBuildHook>());
        Assert.Single(registry.GetHooks<IStartupHook>());
    }
}
