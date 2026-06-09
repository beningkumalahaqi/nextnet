using NextNet.DevTools.UI;

namespace NextNet.DevTools.Panels;

/// <summary>
/// Network Inspector panel — displays HTTP request/response logs from the dev server.
/// Shows method, path, status code, duration, and content type in a table view.
/// </summary>
/// <example>
/// <code>
/// var panel = new NetworkInspectorPanel(dataStore);
/// panel.Render(new TuiRenderContext(120));
/// panel.HandleInput(ConsoleKey.C); // clear requests
/// </code>
/// </example>
public sealed class NetworkInspectorPanel : IDevToolsPanel
{
    private readonly DevToolsDataStore _dataStore;
    private int _scrollOffset;

    /// <inheritdoc />
    public string Name => "Network";

    /// <inheritdoc />
    public string Icon => "🌐";

    /// <summary>
    /// Creates a new NetworkInspectorPanel.
    /// </summary>
    /// <param name="dataStore">The DevTools data store providing network request data.</param>
    public NetworkInspectorPanel(DevToolsDataStore dataStore)
    {
        _dataStore = dataStore;
    }

    /// <inheritdoc />
    public void Render(TuiRenderContext context)
    {
        var requests = _dataStore.GetNetworkRequests();

        System.Console.WriteLine($" Network Inspector — {requests.Count} requests");
        System.Console.WriteLine();

        if (requests.Count == 0)
        {
            System.Console.WriteLine("  No network requests recorded yet.");
            System.Console.WriteLine();
            return;
        }

        var table = new DevToolsTable("Method", "Path", "Status", "Time", "Type");

        foreach (var req in requests.TakeLast(25))
        {
            var statusColor = req.StatusCode switch
            {
                >= 200 and < 300 => "✓",
                >= 300 and < 400 => "→",
                >= 400 and < 500 => "✗",
                >= 500 => "!!",
                _ => "?"
            };

            table.AddRow(
                req.Method,
                req.Url.Length > 40 ? req.Url[..37] + "..." : req.Url,
                $"{statusColor} {req.StatusCode}",
                $"{req.DurationMs}ms",
                req.ContentType ?? "-");
        }

        System.Console.Write(table.Render());
        System.Console.WriteLine();

        // Summary
        System.Console.ForegroundColor = ConsoleColor.DarkGray;
        System.Console.WriteLine($"  Showing last {Math.Min(requests.Count, 25)} of {requests.Count} requests");
        System.Console.ResetColor();
    }

    /// <inheritdoc />
    public void HandleInput(ConsoleKey key)
    {
        switch (key)
        {
            case ConsoleKey.UpArrow:
                _scrollOffset = Math.Max(0, _scrollOffset - 1);
                break;
            case ConsoleKey.DownArrow:
                _scrollOffset++;
                break;
            case ConsoleKey.C:
                _dataStore.ClearNetworkRequests();
                break;
        }
    }

    /// <summary>
    /// A single network request log entry.
    /// Captures HTTP method, URL, status code, duration, timestamp, and content type.
    /// </summary>
    /// <example>
    /// <code>
    /// var req = new NetworkInspectorPanel.NetworkRequest
    /// {
    ///     Method = "GET",
    ///     Url = "/api/users",
    ///     StatusCode = 200,
    ///     DurationMs = 45,
    ///     ContentType = "application/json"
    /// };
    /// </code>
    /// </example>
    public sealed record NetworkRequest
    {
        /// <summary>HTTP method (GET, POST, etc.).</summary>
        public string Method { get; init; } = "GET";

        /// <summary>Request URL path.</summary>
        public string Url { get; init; } = string.Empty;

        /// <summary>HTTP status code.</summary>
        public int StatusCode { get; init; }

        /// <summary>Request duration in milliseconds.</summary>
        public long DurationMs { get; init; }

        /// <summary>Timestamp of the request.</summary>
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;

        /// <summary>Response content type.</summary>
        public string? ContentType { get; init; }
    }
}
