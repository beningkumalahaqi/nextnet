using Microsoft.AspNetCore.Http;
using NextNet.Plugins.Hooks;

namespace NextNet.Plugins.Tests.TestInfrastructure;

/// <summary>
/// A simple test plugin with no hook implementations.
/// </summary>
public class TestPlugin : NextNetPlugin
{
    public override string Name => "TestPlugin";
    public override string Description => "A test plugin for unit testing";
    public override Version Version => new(1, 0, 0);

    public bool WasInitialized { get; private set; }

    public override Task OnInitializeAsync(PluginContext context)
    {
        WasInitialized = true;
        return Task.CompletedTask;
    }
}

/// <summary>
/// Another test plugin with different name/version.
/// </summary>
public class AnotherTestPlugin : NextNetPlugin
{
    public override string Name => "AnotherTestPlugin";
    public override string Description => "Another test plugin";
    public override Version Version => new(2, 0, 0);

    public bool WasInitialized { get; private set; }

    public override Task OnInitializeAsync(PluginContext context)
    {
        WasInitialized = true;
        return Task.CompletedTask;
    }
}

/// <summary>
/// A plugin that implements IBuildHook.
/// </summary>
public class PluginWithBuildHook : NextNetPlugin, IBuildHook
{
    public override string Name => "PluginWithBuildHook";
    public override string Description => "Plugin that implements IBuildHook";

    public bool BuildStartCalled { get; private set; }
    public bool BuildEndCalled { get; private set; }

    public Task OnBuildStart(PluginContext ctx)
    {
        BuildStartCalled = true;
        return Task.CompletedTask;
    }

    public Task OnBuildEnd(PluginContext ctx)
    {
        BuildEndCalled = true;
        return Task.CompletedTask;
    }
}

/// <summary>
/// A plugin that implements IRouteScannerHook.
/// </summary>
public class PluginWithRouteScannerHook : NextNetPlugin, IRouteScannerHook
{
    public override string Name => "PluginWithRouteScannerHook";
    public bool RoutesDiscoveredCalled { get; private set; }
    public Routing.RouteManifest? ReceivedManifest { get; private set; }

    public Task OnRoutesDiscovered(PluginContext ctx, Routing.RouteManifest manifest)
    {
        RoutesDiscoveredCalled = true;
        ReceivedManifest = manifest;
        return Task.CompletedTask;
    }
}

/// <summary>
/// A plugin that implements IRequestHook.
/// </summary>
public class PluginWithRequestHook : NextNetPlugin, IRequestHook
{
    public override string Name => "PluginWithRequestHook";
    public bool RequestCalled { get; private set; }
    public HttpContext? ReceivedHttpContext { get; private set; }

    public Task OnRequestAsync(PluginContext ctx, HttpContext httpContext)
    {
        RequestCalled = true;
        ReceivedHttpContext = httpContext;
        return Task.CompletedTask;
    }
}

/// <summary>
/// A plugin that implements IErrorHook.
/// </summary>
public class PluginWithErrorHook : NextNetPlugin, IErrorHook
{
    public override string Name => "PluginWithErrorHook";
    public bool ErrorCalled { get; private set; }
    public Exception? ReceivedException { get; private set; }

    public Task OnError(PluginContext ctx, Exception exception)
    {
        ErrorCalled = true;
        ReceivedException = exception;
        return Task.CompletedTask;
    }
}

/// <summary>
/// A plugin that implements IRenderHook.
/// </summary>
public class PluginWithRenderHook : NextNetPlugin, IRenderHook
{
    public override string Name => "PluginWithRenderHook";
    public bool PreRenderCalled { get; private set; }
    public bool PostRenderCalled { get; private set; }

    public Task OnPreRender(PluginContext ctx, HttpContext httpContext)
    {
        PreRenderCalled = true;
        return Task.CompletedTask;
    }

    public Task OnPostRender(PluginContext ctx, Microsoft.AspNetCore.Html.IHtmlContent content)
    {
        PostRenderCalled = true;
        return Task.CompletedTask;
    }
}

/// <summary>
/// A plugin that implements multiple hooks (IBuildHook + IStartupHook).
/// </summary>
public class PluginWithMultipleHooks : NextNetPlugin, IBuildHook, IStartupHook
{
    public override string Name => "PluginWithMultipleHooks";
    public bool BuildStartCalled { get; private set; }
    public bool BuildEndCalled { get; private set; }
    public bool StartupCalled { get; private set; }

    public Task OnBuildStart(PluginContext ctx)
    {
        BuildStartCalled = true;
        return Task.CompletedTask;
    }

    public Task OnBuildEnd(PluginContext ctx)
    {
        BuildEndCalled = true;
        return Task.CompletedTask;
    }

    public Task OnStartup(PluginContext ctx)
    {
        StartupCalled = true;
        return Task.CompletedTask;
    }
}

/// <summary>
/// A plugin that throws during initialization to test error isolation.
/// </summary>
public class PluginThatFailsOnInit : NextNetPlugin
{
    public override string Name => "FailingPlugin";
    public bool InitializeWasCalled { get; private set; }

    public override Task OnInitializeAsync(PluginContext context)
    {
        InitializeWasCalled = true;
        throw new InvalidOperationException("Plugin initialization failed");
    }
}

/// <summary>
/// A plugin with a build hook that throws during execution.
/// </summary>
public class PluginWithFailingBuildHook : NextNetPlugin, IBuildHook
{
    public override string Name => "FailingBuildHookPlugin";

    public Task OnBuildStart(PluginContext ctx)
    {
        throw new InvalidOperationException("Build hook failed");
    }

    public Task OnBuildEnd(PluginContext ctx)
    {
        throw new InvalidOperationException("Build hook failed");
    }
}
