using NextNet.DevTools.Panels;
using Xunit;

namespace NextNet.DevTools.Tests;

public class DevToolsServerExtendedTests
{
    [Fact]
    public void DevToolsOptions_Should_HaveDefaults_When_NewInstance()
    {
        var options = new DevToolsOptions();
        Assert.Equal(DevToolsMode.Tui, options.Mode);
        Assert.Equal(3001, options.Port);
        Assert.True(options.EnableProfiler);
        Assert.True(options.EnableRouteInspector);
        Assert.True(options.EnableComponentGraph);
        Assert.True(options.EnableNetworkInspector);
        Assert.True(options.EnableConsolePanel);
    }

    [Fact]
    public void RenderTui_Should_NotThrow_When_PanelsExist()
    {
        var options = new DevToolsOptions { Mode = DevToolsMode.Tui };
        using var server = new DevToolsServer(options);

        var output = CaptureConsoleOutput(() => server.RenderTui());
        Assert.Contains("NextNet DevTools", output);
    }

    [Fact]
    public void ActivePanel_Should_ReturnCurrentPanel_When_Accessed()
    {
        var options = new DevToolsOptions { Mode = DevToolsMode.Tui };
        using var server = new DevToolsServer(options);

        var panel = server.ActivePanel;
        Assert.NotNull(panel);
    }

    [Fact]
    public void DataStore_Should_OverwriteRoutes_When_SetRoutesCalledTwice()
    {
        var store = new DevToolsDataStore();
        store.SetRoutes(new[]
        {
            new RouteInspectorPanel.RouteInfo { Path = "/first", Type = "static", File = "first.cs" }
        });
        store.SetRoutes(new[]
        {
            new RouteInspectorPanel.RouteInfo { Path = "/second", Type = "static", File = "second.cs" }
        });

        var routes = store.GetRoutes();
        Assert.Single(routes);
        Assert.Equal("/second", routes[0].Path);
    }

    [Fact]
    public void DataStore_Should_StoreManyMetrics_When_Added()
    {
        var store = new DevToolsDataStore();
        for (int i = 0; i < 100; i++)
        {
            store.AddMetric(new PerformanceProfilerPanel.PerformanceMetric
            {
                Name = $"Metric {i}",
                DurationMs = i * 10
            });
        }

        Assert.Equal(100, store.GetMetrics().Count);
    }

    [Fact]
    public void DataStore_Should_MaintainOrder_When_AddingNetworkRequests()
    {
        var store = new DevToolsDataStore();
        store.AddNetworkRequest(new NetworkInspectorPanel.NetworkRequest { Url = "/first", Method = "GET" });
        store.AddNetworkRequest(new NetworkInspectorPanel.NetworkRequest { Url = "/second", Method = "POST" });

        var requests = store.GetNetworkRequests();
        Assert.Equal(2, requests.Count);
        Assert.Equal("/first", requests[0].Url);
        Assert.Equal("/second", requests[1].Url);
    }

    [Fact]
    public void DataStore_Should_MaintainOrder_When_AddingConsoleLogs()
    {
        var store = new DevToolsDataStore();
        store.AddConsoleLog("first");
        store.AddConsoleLog("second");

        var logs = store.GetConsoleLogs();
        Assert.Equal(2, logs.Count);
        Assert.Equal("first", logs[0]);
        Assert.Equal("second", logs[1]);
    }

    [Fact]
    public void DevToolsMode_Should_HaveCorrectValues_When_Enumerated()
    {
        Assert.Equal(0, (int)DevToolsMode.Tui);
        Assert.Equal(1, (int)DevToolsMode.Headless);
        Assert.Equal(2, (int)DevToolsMode.Off);
    }

    [Fact]
    public void EventBus_Should_NotThrow_When_PublishingWithoutSubscribers()
    {
        var bus = new DevToolsEventBus();
        // Should not throw
        bus.Publish(new RouteDiscoveredEvent { Path = "/test", Type = "static", File = "test.cs" });
    }

    [Fact]
    public void EventBus_Should_NotThrow_When_DisposingSubscriptionTwice()
    {
        var bus = new DevToolsEventBus();
        var called = 0;
        var sub = bus.Subscribe<RouteDiscoveredEvent>(_ => called++);

        sub.Dispose();
        sub.Dispose(); // should not throw

        bus.Publish(new RouteDiscoveredEvent { Path = "/test", Type = "static", File = "test.cs" });
        Assert.Equal(0, called);
    }

    [Fact]
    public void TuiRenderContext_Should_HaveWidth_When_Created()
    {
        var ctx = new TuiRenderContext(120);
        Assert.Equal(120, ctx.Width);
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
