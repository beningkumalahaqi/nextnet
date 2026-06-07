using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;

namespace NextNet.DevTools.Headless;

/// <summary>
/// Manages DevTools WebSocket connections for real-time updates.
/// </summary>
public sealed class DevToolsWebSocketManager : IDisposable
{
    private readonly ConcurrentDictionary<Guid, DevToolsWebSocketHandler> _connections = new();
    private readonly object _lock = new();
    private bool _disposed;

    /// <summary>
    /// Gets the number of active WebSocket connections.
    /// </summary>
    public int ConnectionCount => _connections.Count;

    /// <summary>
    /// Register a new WebSocket handler connection.
    /// </summary>
    public async Task RegisterAsync(DevToolsWebSocketHandler handler)
    {
        var id = Guid.NewGuid();
        _connections.TryAdd(id, handler);

        try
        {
            // Send initial status
            await handler.SendJsonAsync(new
            {
                type = "connection.established",
                data = new { connectionId = id.ToString() }
            });
        }
        finally
        {
            // Remove connection when handler completes
            _connections.TryRemove(id, out _);
        }
    }

    /// <summary>
    /// Broadcast a JSON message to all connected WebSocket clients.
    /// </summary>
    public async Task BroadcastAsync(string type, object data)
    {
        var payload = new
        {
            type,
            data
        };

        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var bytes = Encoding.UTF8.GetBytes(json);
        var segment = new ArraySegment<byte>(bytes);

        var deadConnections = new List<Guid>();

        foreach (var (id, handler) in _connections)
        {
            try
            {
                await handler.SendRawAsync(segment);
            }
            catch
            {
                deadConnections.Add(id);
            }
        }

        // Clean up dead connections
        foreach (var id in deadConnections)
        {
            _connections.TryRemove(id, out _);
        }
    }

    /// <summary>
    /// Broadcast a route update to all connected clients.
    /// </summary>
    public Task BroadcastRouteUpdateAsync(string path, string type, string file)
        => BroadcastAsync("route.discovered", new { path, type, file });

    /// <summary>
    /// Broadcast an HMR update event.
    /// </summary>
    public Task BroadcastHmrUpdateAsync(string[] files, long duration, bool success)
        => BroadcastAsync("hmr.updated", new { files, duration, success });

    /// <summary>
    /// Broadcast a render completion event.
    /// </summary>
    public Task BroadcastRenderCompletedAsync(string route, long duration, int components)
        => BroadcastAsync("render.completed", new { route, duration, components });

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        foreach (var (_, handler) in _connections)
        {
            try { handler.Dispose(); } catch { }
        }

        _connections.Clear();
    }
}
