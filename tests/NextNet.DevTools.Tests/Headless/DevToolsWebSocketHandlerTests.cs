using NextNet.DevTools.Headless;
using Xunit;

namespace NextNet.DevTools.Tests.Headless;

public class DevToolsWebSocketHandlerTests
{
    [Fact]
    public void Constructor_NullWebSocket_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new DevToolsWebSocketHandler(null!, new DevToolsDataStore()));
    }

    [Fact]
    public void Constructor_NullDataStore_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
        {
            // We need a real WebSocket, but we can't easily create one in tests.
            // This test verifies the null guard for data store.
            var ws = new System.Net.WebSockets.ClientWebSocket();
            try
            {
                new DevToolsWebSocketHandler(ws, null!);
            }
            finally
            {
                ws.Dispose();
            }
        });
    }

    [Fact]
    public void Dispose_Multiple_NoThrow()
    {
        // Create a handler with a disposed websocket
        using var ws = new System.Net.WebSockets.ClientWebSocket();
        var handler = new DevToolsWebSocketHandler(ws, new DevToolsDataStore());
        handler.Dispose();
        handler.Dispose(); // should not throw
    }

    [Fact]
    public void DevToolsWebSocketManager_Constructor()
    {
        using var manager = new DevToolsWebSocketManager();
        Assert.NotNull(manager);
        Assert.Equal(0, manager.ConnectionCount);
    }

    [Fact]
    public void DevToolsWebSocketManager_DisposeMultiple()
    {
        var manager = new DevToolsWebSocketManager();
        manager.Dispose();
        manager.Dispose();
    }
}
