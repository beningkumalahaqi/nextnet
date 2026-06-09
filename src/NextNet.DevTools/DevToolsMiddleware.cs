using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using NextNet.DevTools.Errors;
using NextNet.DevTools.Headless;
using NextNet.DevTools.Panels;

namespace NextNet.DevTools;

/// <summary>
/// ASP.NET Core middleware that exposes DevTools REST API endpoints and WebSocket handler.
/// Endpoints are served under the <c>/__devtools/</c> path prefix.
/// </summary>
/// <example>
/// <code>
/// // In Program.cs:
/// app.UseWebSockets();
/// app.UseMiddleware&lt;DevToolsMiddleware&gt;(dataStore, wsManager);
/// </code>
/// </example>
public sealed class DevToolsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly DevToolsDataStore _dataStore;
    private readonly DevToolsWebSocketManager _wsManager;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Creates a new DevTools middleware instance.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="dataStore">The DevTools data store instance.</param>
    /// <param name="wsManager">The WebSocket connection manager.</param>
    public DevToolsMiddleware(
        RequestDelegate next,
        DevToolsDataStore dataStore,
        DevToolsWebSocketManager wsManager)
    {
        _next = next;
        _dataStore = dataStore;
        _wsManager = wsManager;
    }

    /// <summary>
    /// Invokes the middleware, handling DevTools requests or passing through to the next middleware.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        var request = context.Request;
        var path = request.Path.Value ?? string.Empty;

        if (!path.StartsWith("/__devtools/", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        // WebSocket upgrade
        if (path.Equals("/__devtools/ws", StringComparison.OrdinalIgnoreCase) && request.HttpContext.WebSockets.IsWebSocketRequest)
        {
            var ws = await request.HttpContext.WebSockets.AcceptWebSocketAsync();
            var handler = new DevToolsWebSocketHandler(ws, _dataStore);
            await _wsManager.RegisterAsync(handler);
            await handler.ReceiveLoopAsync();
            return;
        }

        // Handle API routes
        var response = context.Response;
        response.ContentType = "application/json; charset=utf-8";

        try
        {
            switch (path.ToLowerInvariant())
            {
                case "/__devtools/status":
                    await WriteJson(response, new
                    {
                        running = true,
                        mode = "headless",
                        uptime = Environment.TickCount64
                    });
                    break;

                case "/__devtools/routes":
                    var routes = _dataStore.GetRoutes();
                    await WriteJson(response, new
                    {
                        routes = routes.Select(r => new
                        {
                            path = r.Path,
                            type = r.Type,
                            file = r.File,
                            layout = r.Layout,
                            ssr = r.Ssr,
                            ssg = r.Ssg,
                            renderCount = r.RenderCount,
                            avgRenderTime = r.AverageRenderTimeMs
                        }),
                        meta = new
                        {
                            total = routes.Count,
                            staticCount = routes.Count(r => r.Type == "static"),
                            dynamicCount = routes.Count(r => r.Type == "dynamic"),
                            apiCount = routes.Count(r => r.Type == "api")
                        }
                    });
                    break;

                case var p when p.StartsWith("/__devtools/routes/", StringComparison.OrdinalIgnoreCase):
                    var routePath = WebUtility.UrlDecode(p.Replace("/__devtools/routes/", ""));
                    // Ensure the path starts with "/" for matching
                    if (!routePath.StartsWith('/'))
                        routePath = "/" + routePath;
                    var route = _dataStore.GetRoutes().FirstOrDefault(r =>
                        r.Path.Equals(routePath, StringComparison.OrdinalIgnoreCase));
                    if (route is null)
                    {
                        response.StatusCode = 404;
                        await WriteJson(response, new
                        {
                            error = $"{DevToolsErrorCodes.RouteNotFound}: Route not found",
                            path = routePath
                        });
                    }
                    else
                    {
                        await WriteJson(response, new
                        {
                            path = route.Path,
                            type = route.Type,
                            file = route.File,
                            layout = route.Layout,
                            ssr = route.Ssr,
                            ssg = route.Ssg,
                            renderCount = route.RenderCount,
                            avgRenderTime = route.AverageRenderTimeMs
                        });
                    }
                    break;

                case "/__devtools/components":
                    var components = _dataStore.GetComponents();
                    await WriteJson(response, new
                    {
                        components = components.Select(c => new
                        {
                            name = c.Name,
                            type = c.Type,
                            file = c.File,
                            renderCount = c.RenderCount,
                            averageRenderTime = c.AverageRenderTimeMs,
                            children = c.Children
                        }),
                        meta = new { total = components.Count }
                    });
                    break;

                case "/__devtools/performance":
                    var metrics = _dataStore.GetMetrics();
                    await WriteJson(response, new
                    {
                        metrics = metrics.Select(m => new
                        {
                            name = m.Name,
                            durationMs = m.DurationMs,
                            timestamp = m.Timestamp,
                            category = m.Category
                        }),
                        meta = new { total = metrics.Count }
                    });
                    break;

                case "/__devtools/network":
                    var networkRequests = _dataStore.GetNetworkRequests();
                    await WriteJson(response, new
                    {
                        requests = networkRequests.Select(n => new
                        {
                            method = n.Method,
                            url = n.Url,
                            statusCode = n.StatusCode,
                            durationMs = n.DurationMs,
                            timestamp = n.Timestamp,
                            contentType = n.ContentType
                        }),
                        meta = new { total = networkRequests.Count }
                    });
                    break;

                case "/__devtools/console":
                    var logs = _dataStore.GetConsoleLogs();
                    await WriteJson(response, new
                    {
                        logs = logs.Select(l => new { message = l }),
                        meta = new { total = logs.Count }
                    });
                    break;

                case "/__devtools/profiler/start":
                    var startSuccess = true;
                    await WriteJson(response, new { success = startSuccess, message = "Profiler started" });
                    break;

                case "/__devtools/profiler/stop":
                    var stopSuccess = true;
                    await WriteJson(response, new { success = stopSuccess, message = "Profiler stopped" });
                    break;

                default:
                    response.StatusCode = 404;
                    await WriteJson(response, new
                    {
                        error = $"{DevToolsErrorCodes.UnknownEndpoint}: Unknown DevTools endpoint",
                        path
                    });
                    break;
            }
        }
        catch (Exception ex)
        {
            response.StatusCode = 500;
            await WriteJson(response, new { error = "Internal error", details = ex.Message });
        }
    }

    private static async Task WriteJson(HttpResponse response, object data)
    {
        var json = JsonSerializer.Serialize(data, JsonOptions);
        response.ContentLength = Encoding.UTF8.GetByteCount(json);
        await response.WriteAsync(json, Encoding.UTF8);
    }
}
