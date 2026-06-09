using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using NextNet.DevTools.Errors;

namespace NextNet.DevTools.Headless;

/// <summary>
/// Handles a single WebSocket connection for DevTools real-time communication.
/// Manages the receive/send loop and provides helpers for sending text, JSON, and raw data.
/// </summary>
/// <example>
/// <code>
/// var ws = await context.WebSockets.AcceptWebSocketAsync();
/// var handler = new DevToolsWebSocketHandler(ws, dataStore);
/// await handler.ReceiveLoopAsync();
/// </code>
/// </example>
public sealed class DevToolsWebSocketHandler : IDisposable
{
    private readonly WebSocket _webSocket;
    private readonly DevToolsDataStore _dataStore;
    private readonly CancellationTokenSource _cts = new();
    private bool _disposed;

    /// <summary>
    /// Creates a new WebSocket handler for the given connection.
    /// </summary>
    /// <param name="webSocket">The WebSocket connection.</param>
    /// <param name="dataStore">The DevTools data store for reading/writing runtime data.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="webSocket"/> or <paramref name="dataStore"/> is null.</exception>
    public DevToolsWebSocketHandler(WebSocket webSocket, DevToolsDataStore dataStore)
    {
        _webSocket = webSocket ?? throw new ArgumentNullException(nameof(webSocket));
        _dataStore = dataStore ?? throw new ArgumentNullException(nameof(dataStore));
    }

    /// <summary>
    /// The receive loop for this WebSocket connection. Blocks until the socket closes.
    /// </summary>
    public async Task ReceiveLoopAsync()
    {
        var buffer = new byte[4096];

        try
        {
            while (_webSocket.State == WebSocketState.Open && !_cts.Token.IsCancellationRequested)
            {
                var result = await _webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer),
                    _cts.Token);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await _webSocket.CloseAsync(
                        WebSocketCloseStatus.NormalClosure,
                        "Closing",
                        CancellationToken.None);
                    break;
                }

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                    // Echo back for now — future: handle client commands
                    await SendTextAsync($"Received: {message}");
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Normal cancellation
        }
        catch (WebSocketException ex)
        {
            // DS-902: WebSocket connection failed
            Debug.WriteLine($"{DevToolsErrorCodes.WebSocketConnectionFailed}: WebSocket error: {ex.Message}");
        }
        finally
        {
            if (_webSocket.State == WebSocketState.Open ||
                _webSocket.State == WebSocketState.CloseReceived)
            {
                try
                {
                    await _webSocket.CloseAsync(
                        WebSocketCloseStatus.NormalClosure,
                        "Closing",
                        CancellationToken.None);
                }
                catch { }
            }
        }
    }

    /// <summary>
    /// Send a text message over the WebSocket.
    /// </summary>
    public async Task SendTextAsync(string message)
    {
        if (_webSocket.State != WebSocketState.Open) return;

        var bytes = Encoding.UTF8.GetBytes(message);
        await _webSocket.SendAsync(
            new ArraySegment<byte>(bytes),
            WebSocketMessageType.Text,
            true,
            _cts.Token);
    }

    /// <summary>
    /// Send a JSON message over the WebSocket.
    /// </summary>
    public async Task SendJsonAsync(object data)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(data, new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
        });
        await SendTextAsync(json);
    }

    /// <summary>
    /// Send raw bytes over the WebSocket.
    /// </summary>
    public async Task SendRawAsync(ArraySegment<byte> data)
    {
        if (_webSocket.State != WebSocketState.Open) return;

        await _webSocket.SendAsync(
            data,
            WebSocketMessageType.Text,
            true,
            _cts.Token);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _cts.Cancel();
        _cts.Dispose();
        _webSocket.Dispose();
    }
}
