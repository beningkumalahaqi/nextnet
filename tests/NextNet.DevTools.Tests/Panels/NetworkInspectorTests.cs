using NextNet.DevTools.Panels;
using NextNet.DevTools.UI;
using Xunit;

namespace NextNet.DevTools.Tests.Panels;

public class NetworkInspectorTests
{
    [Fact]
    public void Panel_HasCorrectName()
    {
        var store = new DevToolsDataStore();
        var panel = new NetworkInspectorPanel(store);

        Assert.Equal("Network", panel.Name);
    }

    [Fact]
    public void Panel_HasIcon()
    {
        var store = new DevToolsDataStore();
        var panel = new NetworkInspectorPanel(store);
        Assert.NotNull(panel.Icon);
        Assert.NotEmpty(panel.Icon);
    }

    [Fact]
    public void Render_WithEmptyStore_NoCrash()
    {
        var store = new DevToolsDataStore();
        var panel = new NetworkInspectorPanel(store);
        var context = new TuiRenderContext(80);

        var output = CaptureConsoleOutput(() => panel.Render(context));
        Assert.Contains("Network Inspector", output);
        Assert.Contains("No network requests recorded yet", output);
    }

    [Fact]
    public void Render_WithRequests_ShowsData()
    {
        var store = new DevToolsDataStore();
        store.AddNetworkRequest(new NetworkInspectorPanel.NetworkRequest
        {
            Method = "GET",
            Url = "/api/health",
            StatusCode = 200,
            DurationMs = 15,
            ContentType = "application/json"
        });
        store.AddNetworkRequest(new NetworkInspectorPanel.NetworkRequest
        {
            Method = "POST",
            Url = "/api/data",
            StatusCode = 201,
            DurationMs = 45,
            ContentType = "text/plain"
        });

        var panel = new NetworkInspectorPanel(store);
        var context = new TuiRenderContext(80);

        var output = CaptureConsoleOutput(() => panel.Render(context));
        Assert.Contains("Network Inspector", output);
        Assert.Contains("2 requests", output);
    }

    [Fact]
    public void HandleInput_C_ClearsRequests()
    {
        var store = new DevToolsDataStore();
        store.AddNetworkRequest(new NetworkInspectorPanel.NetworkRequest
        {
            Method = "GET",
            Url = "/test",
            StatusCode = 200
        });

        var panel = new NetworkInspectorPanel(store);
        Assert.Single(store.GetNetworkRequests());

        panel.HandleInput(ConsoleKey.C);
        Assert.Empty(store.GetNetworkRequests());
    }

    [Fact]
    public void NetworkRequest_DefaultValues()
    {
        var req = new NetworkInspectorPanel.NetworkRequest
        {
            Url = "/test"
        };

        Assert.Equal("/test", req.Url);
        Assert.Equal("GET", req.Method); // default
    }

    private static string CaptureConsoleOutput(Action action)
    {
        var original = System.Console.Out;
        try
        {
            using var writer = new StringWriter();
            System.Console.SetOut(writer);
            action();
            return writer.ToString();
        }
        finally
        {
            System.Console.SetOut(original);
        }
    }
}
