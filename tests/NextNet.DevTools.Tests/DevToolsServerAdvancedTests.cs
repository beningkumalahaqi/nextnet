using NextNet.DevTools.Panels;
using Xunit;

namespace NextNet.DevTools.Tests;

public class DevToolsServerAdvancedTests
{
    [Fact]
    public void Server_WithCustomOptions_RespectsSettings()
    {
        var options = new DevToolsOptions
        {
            Mode = DevToolsMode.Headless,
            Port = 8080,
            EnableProfiler = false,
            EnableRouteInspector = false,
            EnableComponentGraph = true,
            EnableNetworkInspector = false,
            EnableConsolePanel = false
        };

        using var server = new DevToolsServer(options);
        Assert.Single(server.Panels);
        Assert.Equal("Component Tree", server.Panels[0].Name);
    }

    [Fact]
    public void Server_SwitchPanel_AllIndices()
    {
        using var server = new DevToolsServer(new DevToolsOptions { Mode = DevToolsMode.Tui });

        for (int i = 0; i < server.Panels.Count; i++)
        {
            server.ActivePanelIndex = i;
            Assert.Equal(i, server.ActivePanelIndex);
        }
    }

    [Fact]
    public void Server_WsManager_NullInTuiMode()
    {
        using var server = new DevToolsServer(new DevToolsOptions { Mode = DevToolsMode.Tui });
        Assert.Null(server.WsManager);
    }

    [Fact]
    public async Task DataStore_ConcurrentAccess_NoCrash()
    {
        var store = new DevToolsDataStore();
        var exceptions = new List<Exception>();

        var threads = Enumerable.Range(0, 10).Select(i => Task.Run(() =>
        {
            try
            {
                for (int j = 0; j < 100; j++)
                {
                    store.AddMetric(new PerformanceProfilerPanel.PerformanceMetric
                    {
                        Name = $"Thread-{i}-Metric-{j}",
                        DurationMs = j,
                        Category = "test"
                    });
                    store.GetMetrics();
                    store.AddConsoleLog($"Thread-{i} log {j}");
                    store.GetConsoleLogs();
                    store.AddNetworkRequest(new NetworkInspectorPanel.NetworkRequest
                    {
                        Method = "GET",
                        Url = $"/thread-{i}/req-{j}",
                        StatusCode = 200
                    });
                    store.GetNetworkRequests();
                }
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        })).ToArray();

        await Task.WhenAll(threads);

        Assert.Empty(exceptions);
        Assert.True(store.GetMetrics().Count >= 500);
    }

    [Fact]
    public void DataStore_ClearNetwork_WhileAdding()
    {
        var store = new DevToolsDataStore();

        for (int i = 0; i < 50; i++)
        {
            store.AddNetworkRequest(new NetworkInspectorPanel.NetworkRequest
            {
                Method = "GET",
                Url = $"/req/{i}",
                StatusCode = 200
            });
        }

        Assert.Equal(50, store.GetNetworkRequests().Count);
        store.ClearNetworkRequests();
        Assert.Empty(store.GetNetworkRequests());
    }

    [Fact]
    public void DataStore_ClearConsole_WhileAdding()
    {
        var store = new DevToolsDataStore();

        for (int i = 0; i < 50; i++)
        {
            store.AddConsoleLog($"Log {i}");
        }

        Assert.Equal(50, store.GetConsoleLogs().Count);
        store.ClearConsoleLogs();
        Assert.Empty(store.GetConsoleLogs());
    }

    [Fact]
    public void DataStore_SetComponents_Replaces()
    {
        var store = new DevToolsDataStore();

        store.SetComponents(new[]
        {
            new ComponentTreePanel.ComponentInfo { Name = "A", Type = "component" }
        });
        Assert.Single(store.GetComponents());

        store.SetComponents(new[]
        {
            new ComponentTreePanel.ComponentInfo { Name = "B", Type = "component" },
            new ComponentTreePanel.ComponentInfo { Name = "C", Type = "component" }
        });
        Assert.Equal(2, store.GetComponents().Count);
    }

    [Fact]
    public void DataStore_MaxNetwork_CapsEntries()
    {
        var store = new DevToolsDataStore();
        store.MaxNetworkEntries = 10;

        for (int i = 0; i < 25; i++)
        {
            store.AddNetworkRequest(new NetworkInspectorPanel.NetworkRequest
            {
                Method = "GET",
                Url = $"/req/{i}",
                StatusCode = 200
            });
        }

        Assert.Equal(10, store.GetNetworkRequests().Count);
    }

    [Fact]
    public void DataStore_MaxConsole_CapsEntries()
    {
        var store = new DevToolsDataStore();
        store.MaxConsoleEntries = 10;

        for (int i = 0; i < 25; i++)
        {
            store.AddConsoleLog($"Log {i}");
        }

        Assert.Equal(10, store.GetConsoleLogs().Count);
    }

    [Fact]
    public void DevToolsEventBus_SubscribeToDifferentTypes()
    {
        var bus = new DevToolsEventBus();
        var routeEvents = 0;
        var buildEvents = 0;

        using var sub1 = bus.Subscribe<RouteDiscoveredEvent>(_ => routeEvents++);
        using var sub2 = bus.Subscribe<BuildCompletedEvent>(_ => buildEvents++);

        bus.Publish(new RouteDiscoveredEvent { Path = "/test", Type = "static", File = "test.cs" });
        bus.Publish(new BuildCompletedEvent { TotalDurationMs = 100, Success = true });

        Assert.Equal(1, routeEvents);
        Assert.Equal(1, buildEvents);
    }

    [Fact]
    public void DevToolsEventBus_HandlerException_DoesNotCrash()
    {
        var bus = new DevToolsEventBus();
        var goodHandlerCalled = false;

        using var sub1 = bus.Subscribe<RouteDiscoveredEvent>(_ => throw new InvalidOperationException("bad handler"));
        using var sub2 = bus.Subscribe<RouteDiscoveredEvent>(_ => goodHandlerCalled = true);

        bus.Publish(new RouteDiscoveredEvent { Path = "/test", Type = "static", File = "test.cs" });

        Assert.True(goodHandlerCalled);
    }

    [Fact]
    public void RouteInfo_DefaultValues_Complete()
    {
        var info = new RouteInspectorPanel.RouteInfo
        {
            Path = "/blog/[slug]",
            Type = "dynamic",
            File = "app/blog/[slug]/page.cs",
            Layout = "app/blog/layout.cs",
            Ssr = true,
            Ssg = false,
            RenderCount = 42,
            AverageRenderTimeMs = 18
        };

        Assert.Equal("/blog/[slug]", info.Path);
        Assert.Equal("dynamic", info.Type);
        Assert.Equal("app/blog/[slug]/page.cs", info.File);
        Assert.Equal("app/blog/layout.cs", info.Layout);
        Assert.True(info.Ssr);
        Assert.False(info.Ssg);
        Assert.Equal(42, info.RenderCount);
        Assert.Equal(18, info.AverageRenderTimeMs);
    }

    [Fact]
    public void PerformanceMetric_DefaultValues_Complete()
    {
        var metric = new PerformanceProfilerPanel.PerformanceMetric
        {
            Name = "Build",
            DurationMs = 1500,
            Category = "build"
        };

        Assert.Equal("Build", metric.Name);
        Assert.Equal(1500, metric.DurationMs);
        Assert.Equal("build", metric.Category);
    }

    [Fact]
    public void NetworkRequest_DefaultValues_Complete()
    {
        var req = new NetworkInspectorPanel.NetworkRequest
        {
            Method = "POST",
            Url = "/api/submit",
            StatusCode = 201,
            DurationMs = 100,
            ContentType = "application/json"
        };

        Assert.Equal("POST", req.Method);
        Assert.Equal("/api/submit", req.Url);
        Assert.Equal(201, req.StatusCode);
        Assert.Equal(100, req.DurationMs);
        Assert.Equal("application/json", req.ContentType);
    }
}
