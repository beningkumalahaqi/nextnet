using System.Net.WebSockets;
using NextNet.DevTools.Headless;
using Xunit;

namespace NextNet.DevTools.Tests.Headless;

public class WebSocketManagerTests
{
    [Fact]
    public void Constructor_InitialState_NoConnections()
    {
        using var manager = new DevToolsWebSocketManager();
        Assert.Equal(0, manager.ConnectionCount);
    }

    [Fact]
    public void Dispose_MultipleTimes_NoThrow()
    {
        var manager = new DevToolsWebSocketManager();
        manager.Dispose();
        manager.Dispose(); // should not throw
    }

    [Fact]
    public async Task BroadcastAsync_WithNoConnections_NoThrow()
    {
        using var manager = new DevToolsWebSocketManager();

        // Should not throw when there are no connections
        await manager.BroadcastAsync("test.event", new { message = "hello" });
    }

    [Fact]
    public async Task BroadcastRouteUpdateAsync_NoThrow()
    {
        using var manager = new DevToolsWebSocketManager();
        await manager.BroadcastRouteUpdateAsync("/test", "static", "app/test/page.cs");
    }

    [Fact]
    public async Task BroadcastHmrUpdateAsync_NoThrow()
    {
        using var manager = new DevToolsWebSocketManager();
        await manager.BroadcastHmrUpdateAsync(new[] { "app/page.cs" }, 420, true);
    }

    [Fact]
    public async Task BroadcastRenderCompletedAsync_NoThrow()
    {
        using var manager = new DevToolsWebSocketManager();
        await manager.BroadcastRenderCompletedAsync("/test", 23, 4);
    }
}
