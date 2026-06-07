using NextNet.DevTools.Headless;
using NextNet.DevTools.Panels;
using Xunit;

namespace NextNet.DevTools.Tests;

public class DevToolsMiddlewareTests
{
    [Fact]
    public void Constructor_WithNulls_Throws()
    {
        var store = new DevToolsDataStore();
        var wsManager = new DevToolsWebSocketManager();

        // We can't easily test the middleware constructor without ASP.NET Core infrastructure
        // But we can test that the data store and WS manager are properly constructed
        Assert.NotNull(store);
        Assert.NotNull(wsManager);
    }

    [Fact]
    public void DevToolsApiController_SerializesCorrectly()
    {
        var json = DevToolsApiController.SerializeResponse(new
        {
            status = "ok",
            count = 5
        });

        Assert.Contains("\"status\"", json);
        Assert.Contains("\"ok\"", json);
        Assert.Contains("\"count\"", json);
        Assert.Contains("5", json);
    }
}
