using Microsoft.Extensions.DependencyInjection;
using NextNet.DevTools;
using NextNet.DevTools.Headless;
using Xunit;

namespace NextNet.DevTools.Tests;

public class DevToolsServiceCollectionExtensionsTests
{
    [Fact]
    public void AddNextNetDevTools_RegistersServices()
    {
        var services = new ServiceCollection();
        services.AddNextNetDevTools();

        var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetService<DevToolsOptions>());
        Assert.NotNull(provider.GetService<DevToolsDataStore>());
        Assert.NotNull(provider.GetService<DevToolsEventBus>());
        Assert.NotNull(provider.GetService<IDevToolsEventBus>());
    }

    [Fact]
    public void AddNextNetDevTools_WithOptions_RegistersOptions()
    {
        var services = new ServiceCollection();
        var options = new DevToolsOptions
        {
            Mode = DevToolsMode.Headless,
            Port = 9999
        };
        services.AddNextNetDevTools(options);

        var provider = services.BuildServiceProvider();
        var resolved = provider.GetRequiredService<DevToolsOptions>();

        Assert.Equal(DevToolsMode.Headless, resolved.Mode);
        Assert.Equal(9999, resolved.Port);
    }

    [Fact]
    public void AddNextNetDevTools_HeadlessMode_RegistersWebSocketManager()
    {
        var services = new ServiceCollection();
        services.AddNextNetDevTools(new DevToolsOptions { Mode = DevToolsMode.Headless });

        var provider = services.BuildServiceProvider();
        var wsManager = provider.GetService<DevToolsWebSocketManager>();

        Assert.NotNull(wsManager);
    }

    [Fact]
    public void AddNextNetDevTools_TuiMode_DoesNotRegisterWebSocketManager()
    {
        var services = new ServiceCollection();
        services.AddNextNetDevTools(new DevToolsOptions { Mode = DevToolsMode.Tui });

        var provider = services.BuildServiceProvider();
        var wsManager = provider.GetService<DevToolsWebSocketManager>();

        Assert.Null(wsManager);
    }

    [Fact]
    public void AddNextNetDevToolsServer_RegistersServer()
    {
        var services = new ServiceCollection();
        services.AddNextNetDevToolsServer();

        var provider = services.BuildServiceProvider();
        var server = provider.GetService<DevToolsServer>();

        Assert.NotNull(server);
    }
}
