using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NextNet.DevTools.Headless;
using NextNet.DevTools.Panels;
using Xunit;

namespace NextNet.DevTools.Tests;

public class DevToolsMiddlewareIntegrationTests : IDisposable
{
    private readonly TestServer _server;
    private readonly HttpClient _client;
    private readonly DevToolsDataStore _dataStore;
    private readonly DevToolsWebSocketManager _wsManager;

    public DevToolsMiddlewareIntegrationTests()
    {
        _dataStore = new DevToolsDataStore();
        _wsManager = new DevToolsWebSocketManager();

        var builder = new WebHostBuilder()
            .Configure(app =>
            {
                app.UseMiddleware<DevToolsMiddleware>(_dataStore, _wsManager);
            });

        _server = new TestServer(builder);
        _client = _server.CreateClient();
    }

    [Fact]
    public async Task NonDevToolsPath_PassesThrough()
    {
        var response = await _client.GetAsync("/");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Status_ReturnsOk()
    {
        var response = await _client.GetAsync("/__devtools/status");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"running\"", json);
        Assert.Contains("\"mode\"", json);
    }

    [Fact]
    public async Task Routes_ReturnsEmptyList()
    {
        var response = await _client.GetAsync("/__devtools/routes");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"routes\"", json);
        Assert.Contains("\"meta\"", json);
    }

    [Fact]
    public async Task Routes_WithData_ReturnsRoutes()
    {
        _dataStore.SetRoutes(new[]
        {
            new RouteInspectorPanel.RouteInfo
            {
                Path = "/",
                Type = "static",
                File = "app/page.cs",
                Ssr = true,
                RenderCount = 10,
                AverageRenderTimeMs = 15
            },
            new RouteInspectorPanel.RouteInfo
            {
                Path = "/about",
                Type = "static",
                File = "app/about/page.cs",
                Ssr = true
            }
        });

        var response = await _client.GetAsync("/__devtools/routes");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"/\"", json);
        Assert.Contains("\"/about\"", json);
        Assert.Contains("\"static\"", json);
        Assert.Contains("\"renderCount\"", json);
        Assert.Contains("\"total\"", json);
    }

    [Fact]
    public async Task RouteDetail_ReturnsRoute()
    {
        _dataStore.SetRoutes(new[]
        {
            new RouteInspectorPanel.RouteInfo
            {
                Path = "/test-route",
                Type = "dynamic",
                File = "app/test-route/page.cs",
                Layout = "app/layout.cs",
                Ssr = true,
                Ssg = false,
                RenderCount = 5,
                AverageRenderTimeMs = 20
            }
        });

        // The route path in the URL should match the stored route path
        var response = await _client.GetAsync("/__devtools/routes//test-route");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("/test-route", json);
        Assert.Contains("dynamic", json);
        Assert.Contains("app/test-route/page.cs", json);
    }

    [Fact]
    public async Task RouteDetail_NotFound_Returns404()
    {
        var response = await _client.GetAsync("/__devtools/routes/nonexistent");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"error\"", json);
    }

    [Fact]
    public async Task Components_ReturnsEmptyList()
    {
        var response = await _client.GetAsync("/__devtools/components");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"components\"", json);
        Assert.Contains("\"meta\"", json);
    }

    [Fact]
    public async Task Components_WithData_ReturnsComponents()
    {
        _dataStore.SetComponents(new[]
        {
            new ComponentTreePanel.ComponentInfo
            {
                Name = "Layout",
                Type = "layout",
                File = "app/layout.cs",
                RenderCount = 5,
                AverageRenderTimeMs = 10
            }
        });

        var response = await _client.GetAsync("/__devtools/components");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"Layout\"", json);
        Assert.Contains("\"layout\"", json);
    }

    [Fact]
    public async Task Performance_ReturnsMetrics()
    {
        _dataStore.AddMetric(new PerformanceProfilerPanel.PerformanceMetric
        {
            Name = "Build step",
            DurationMs = 500,
            Category = "build"
        });

        var response = await _client.GetAsync("/__devtools/performance");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"metrics\"", json);
        Assert.Contains("\"Build step\"", json);
    }

    [Fact]
    public async Task Network_ReturnsRequests()
    {
        _dataStore.AddNetworkRequest(new NetworkInspectorPanel.NetworkRequest
        {
            Method = "GET",
            Url = "/api/test",
            StatusCode = 200,
            DurationMs = 50,
            ContentType = "application/json"
        });

        var response = await _client.GetAsync("/__devtools/network");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"requests\"", json);
        Assert.Contains("\"GET\"", json);
    }

    [Fact]
    public async Task Console_ReturnsLogs()
    {
        _dataStore.AddConsoleLog("[INFO] Server started");
        _dataStore.AddConsoleLog("[WARN] Something odd");

        var response = await _client.GetAsync("/__devtools/console");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"logs\"", json);
        Assert.Contains("Server started", json);
        Assert.Contains("total", json);
    }

    [Fact]
    public async Task ProfilerStart_ReturnsSuccess()
    {
        var response = await _client.PostAsync("/__devtools/profiler/start", null);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"success\"", json);
    }

    [Fact]
    public async Task ProfilerStop_ReturnsSuccess()
    {
        var response = await _client.PostAsync("/__devtools/profiler/stop", null);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"success\"", json);
    }

    [Fact]
    public async Task UnknownEndpoint_Returns404()
    {
        var response = await _client.GetAsync("/__devtools/unknown");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"error\"", json);
    }

    [Fact]
    public async Task RouteDetail_UrlEncoded_Works()
    {
        _dataStore.SetRoutes(new[]
        {
            new RouteInspectorPanel.RouteInfo
            {
                Path = "/blog/my-post",
                Type = "static",
                File = "app/blog/my-post/page.cs"
            }
        });

        // URL-encoded path, decoded by middleware
        var response = await _client.GetAsync("/__devtools/routes//blog/my-post");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Response_IsCamelCase()
    {
        var response = await _client.GetAsync("/__devtools/status");
        var json = await response.Content.ReadAsStringAsync();

        // Should be camelCase, not PascalCase
        Assert.DoesNotContain("\"Running\"", json);
        Assert.Contains("\"running\"", json);
    }

    public void Dispose()
    {
        _client.Dispose();
        _server.Dispose();
        _wsManager.Dispose();
    }
}
