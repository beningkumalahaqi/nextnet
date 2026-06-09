using NextNet.DevTools.Panels;
using Xunit;

namespace NextNet.DevTools.Tests;

public class DevToolsServerTests
{
    [Fact]
    public void Constructor_Should_CreatePanels_When_TuiMode()
    {
        var options = new DevToolsOptions { Mode = DevToolsMode.Tui };
        using var server = new DevToolsServer(options);

        Assert.NotNull(server.Panels);
        Assert.NotEmpty(server.Panels);
    }

    [Fact]
    public void Constructor_Should_CreateNoPanels_When_OffMode()
    {
        var options = new DevToolsOptions
        {
            Mode = DevToolsMode.Off,
            EnableRouteInspector = false,
            EnableComponentGraph = false,
            EnableProfiler = false,
            EnableNetworkInspector = false,
            EnableConsolePanel = false
        };
        using var server = new DevToolsServer(options);

        Assert.Empty(server.Panels);
    }

    [Fact]
    public void Constructor_Should_ReturnEmpty_When_AllPanelsDisabled()
    {
        var options = new DevToolsOptions
        {
            EnableRouteInspector = false,
            EnableComponentGraph = false,
            EnableProfiler = false,
            EnableNetworkInspector = false,
            EnableConsolePanel = false
        };
        using var server = new DevToolsServer(options);

        Assert.Empty(server.Panels);
    }

    [Fact]
    public void ActivePanelIndex_Should_SwitchPanel_When_SetToValidIndex()
    {
        var options = new DevToolsOptions { Mode = DevToolsMode.Tui };
        using var server = new DevToolsServer(options);

        var initial = server.ActivePanelIndex;
        server.ActivePanelIndex = 1;

        Assert.Equal(1, server.ActivePanelIndex);
    }

    [Fact]
    public void ActivePanelIndex_Should_Clamp_When_IndexOutOfRange()
    {
        var options = new DevToolsOptions { Mode = DevToolsMode.Tui };
        using var server = new DevToolsServer(options);

        server.ActivePanelIndex = 999;
        Assert.True(server.ActivePanelIndex < server.Panels.Count);

        server.ActivePanelIndex = -1;
        Assert.True(server.ActivePanelIndex >= 0);
    }

    [Fact]
    public void IsRunning_Should_BeFalse_When_InitialState()
    {
        var options = new DevToolsOptions { Mode = DevToolsMode.Tui };
        using var server = new DevToolsServer(options);

        Assert.False(server.IsRunning);
    }

    [Fact]
    public void EventBus_Should_ReceiveEvents_When_Published()
    {
        var options = new DevToolsOptions();
        using var server = new DevToolsServer(options);

        var received = false;
        using var sub = server.EventBus.Subscribe<RouteDiscoveredEvent>(_ => received = true);

        server.EventBus.Publish(new RouteDiscoveredEvent
        {
            Path = "/test",
            Type = "static",
            File = "app/test/page.cs"
        });

        Assert.True(received);
    }

    [Fact]
    public void EventBus_Should_DeliverToAllSubscribers_When_Multiple()
    {
        var options = new DevToolsOptions();
        using var server = new DevToolsServer(options);

        var count = 0;
        using var sub1 = server.EventBus.Subscribe<RouteDiscoveredEvent>(_ => count++);
        using var sub2 = server.EventBus.Subscribe<RouteDiscoveredEvent>(_ => count++);

        server.EventBus.Publish(new RouteDiscoveredEvent
        {
            Path = "/test",
            Type = "static",
            File = "app/test/page.cs"
        });

        Assert.Equal(2, count);
    }

    [Fact]
    public void EventBus_Should_NotReceive_When_Unsubscribed()
    {
        var options = new DevToolsOptions();
        using var server = new DevToolsServer(options);

        var count = 0;
        var sub = server.EventBus.Subscribe<RouteDiscoveredEvent>(_ => count++);

        server.EventBus.Publish(new RouteDiscoveredEvent
        {
            Path = "/first",
            Type = "static",
            File = "app/first/page.cs"
        });

        sub.Dispose();

        server.EventBus.Publish(new RouteDiscoveredEvent
        {
            Path = "/second",
            Type = "static",
            File = "app/second/page.cs"
        });

        Assert.Equal(1, count);
    }

    [Fact]
    public void DataStore_Should_StoreAndRetrieveRoutes_When_Set()
    {
        var options = new DevToolsOptions();
        using var server = new DevToolsServer(options);

        var routes = new[]
        {
            new RouteInspectorPanel.RouteInfo
            {
                Path = "/",
                Type = "static",
                File = "app/page.cs",
                Ssr = true
            },
            new RouteInspectorPanel.RouteInfo
            {
                Path = "/about",
                Type = "static",
                File = "app/about/page.cs",
                Ssr = true
            }
        };

        server.DataStore.SetRoutes(routes);
        var retrieved = server.DataStore.GetRoutes();

        Assert.Equal(2, retrieved.Count);
        Assert.Contains(retrieved, r => r.Path == "/");
        Assert.Contains(retrieved, r => r.Path == "/about");
    }

    [Fact]
    public void DataStore_Should_StoreAndClearMetrics_When_AddAndClear()
    {
        var options = new DevToolsOptions();
        using var server = new DevToolsServer(options);

        server.DataStore.AddMetric(new PerformanceProfilerPanel.PerformanceMetric
        {
            Name = "Build",
            DurationMs = 100,
            Category = "build"
        });

        Assert.Single(server.DataStore.GetMetrics());

        server.DataStore.ClearMetrics();
        Assert.Empty(server.DataStore.GetMetrics());
    }

    [Fact]
    public void DataStore_Should_StoreAndClearNetworkRequests_When_AddAndClear()
    {
        var options = new DevToolsOptions();
        using var server = new DevToolsServer(options);

        server.DataStore.AddNetworkRequest(new NetworkInspectorPanel.NetworkRequest
        {
            Method = "GET",
            Url = "/test",
            StatusCode = 200,
            DurationMs = 50
        });

        Assert.Single(server.DataStore.GetNetworkRequests());

        server.DataStore.ClearNetworkRequests();
        Assert.Empty(server.DataStore.GetNetworkRequests());
    }

    [Fact]
    public void DataStore_Should_StoreAndClearConsoleLogs_When_AddAndClear()
    {
        var options = new DevToolsOptions();
        using var server = new DevToolsServer(options);

        server.DataStore.AddConsoleLog("[INFO] Server started");
        server.DataStore.AddConsoleLog("[INFO] Route scanned: /about");

        Assert.Equal(2, server.DataStore.GetConsoleLogs().Count);

        server.DataStore.ClearConsoleLogs();
        Assert.Empty(server.DataStore.GetConsoleLogs());
    }

    [Fact]
    public void DataStore_Should_StoreAndRetrieveComponents_When_Set()
    {
        var options = new DevToolsOptions();
        using var server = new DevToolsServer(options);

        var components = new[]
        {
            new ComponentTreePanel.ComponentInfo
            {
                Name = "Layout",
                Type = "layout",
                File = "app/layout.cs"
            },
            new ComponentTreePanel.ComponentInfo
            {
                Name = "Header",
                Type = "component",
                File = "app/components/Header.cs",
                Parent = "Layout"
            }
        };

        server.DataStore.SetComponents(components);
        var retrieved = server.DataStore.GetComponents();

        Assert.Equal(2, retrieved.Count);
        Assert.Contains(retrieved, c => c.Name == "Header");
    }

    [Fact]
    public void DataStore_Should_RespectMaxNetworkEntries_When_ExceedingLimit()
    {
        var options = new DevToolsOptions();
        using var server = new DevToolsServer(options);

        server.DataStore.MaxNetworkEntries = 3;

        for (int i = 0; i < 10; i++)
        {
            server.DataStore.AddNetworkRequest(new NetworkInspectorPanel.NetworkRequest
            {
                Method = "GET",
                Url = $"/test/{i}",
                StatusCode = 200
            });
        }

        Assert.Equal(3, server.DataStore.GetNetworkRequests().Count);
    }

    [Fact]
    public void DataStore_Should_RespectMaxConsoleEntries_When_ExceedingLimit()
    {
        var options = new DevToolsOptions();
        using var server = new DevToolsServer(options);

        server.DataStore.MaxConsoleEntries = 5;

        for (int i = 0; i < 20; i++)
        {
            server.DataStore.AddConsoleLog($"Log entry {i}");
        }

        Assert.Equal(5, server.DataStore.GetConsoleLogs().Count);
    }

    [Fact]
    public async Task StartStop_Should_NotThrow_When_OffMode()
    {
        var options = new DevToolsOptions
        {
            Mode = DevToolsMode.Off
        };
        using var server = new DevToolsServer(options);

        // Should not throw even with Off mode
        await server.StartAsync();
        await server.StopAsync();
    }
}
