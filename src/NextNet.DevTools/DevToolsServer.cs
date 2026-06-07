using System.Diagnostics;
using System.Net.WebSockets;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NextNet.DevTools.Headless;
using NextNet.DevTools.Panels;
using NextNet.DevTools.UI;

namespace NextNet.DevTools;

/// <summary>
/// Defines the DevTools operating mode.
/// </summary>
public enum DevToolsMode
{
    /// <summary>Terminal UI mode (default in terminal).</summary>
    Tui,

    /// <summary>HTTP API + WebSocket mode for external consumers (VS Code extension).</summary>
    Headless,

    /// <summary>Disabled.</summary>
    Off
}

/// <summary>
/// Configuration options for the DevTools host.
/// </summary>
public class DevToolsOptions
{
    /// <summary>Operating mode: Tui, Headless, or Off.</summary>
    public DevToolsMode Mode { get; set; } = DevToolsMode.Tui;

    /// <summary>Port for the headless HTTP API (default 3001).</summary>
    public int Port { get; set; } = 3001;

    /// <summary>Initial active panel index.</summary>
    public int ActivePanel { get; set; }

    /// <summary>Enable the performance profiler.</summary>
    public bool EnableProfiler { get; set; } = true;

    /// <summary>Enable the route inspector.</summary>
    public bool EnableRouteInspector { get; set; } = true;

    /// <summary>Enable the component graph.</summary>
    public bool EnableComponentGraph { get; set; } = true;

    /// <summary>Enable the network inspector.</summary>
    public bool EnableNetworkInspector { get; set; } = true;

    /// <summary>Enable the console log panel.</summary>
    public bool EnableConsolePanel { get; set; } = true;
}

/// <summary>
/// Event bus interface for DevTools publish/subscribe.
/// </summary>
public interface IDevToolsEventBus
{
    /// <summary>Publish an event to all subscribers.</summary>
    void Publish<TEvent>(TEvent evt) where TEvent : IDevToolsEvent;

    /// <summary>Subscribe to an event type. Returns a disposable that unsubscribes.</summary>
    IDisposable Subscribe<TEvent>(Action<TEvent> handler) where TEvent : IDevToolsEvent;
}

/// <summary>
/// Marker interface for DevTools events.
/// </summary>
public interface IDevToolsEvent { }

/// <summary>
/// Event fired when a new route is discovered.
/// </summary>
public class RouteDiscoveredEvent : IDevToolsEvent
{
    public string Path { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string File { get; init; } = string.Empty;
}

/// <summary>
/// Event fired when a component renders.
/// </summary>
public class ComponentRenderedEvent : IDevToolsEvent
{
    public string Component { get; init; } = string.Empty;
    public long DurationMs { get; init; }
    public string Route { get; init; } = string.Empty;
}

/// <summary>
/// Event fired when a build completes.
/// </summary>
public class BuildCompletedEvent : IDevToolsEvent
{
    public long TotalDurationMs { get; init; }
    public bool Success { get; init; }
    public IReadOnlyList<BuildStepMetric> Steps { get; init; } = Array.Empty<BuildStepMetric>();
}

/// <summary>
/// A single build step metric.
/// </summary>
public class BuildStepMetric
{
    public string Name { get; init; } = string.Empty;
    public long DurationMs { get; init; }
}

/// <summary>
/// Event fired when an HMR update is applied.
/// </summary>
public class HmrUpdatedEvent : IDevToolsEvent
{
    public IReadOnlyList<string> Files { get; init; } = Array.Empty<string>();
    public long DurationMs { get; init; }
    public bool Success { get; init; }
}

/// <summary>
/// Event fired when an error occurs in the dev server.
/// </summary>
public class ErrorOccurredEvent : IDevToolsEvent
{
    public string Message { get; init; } = string.Empty;
    public string? File { get; init; }
    public int? Line { get; init; }
}

// ── Event bus implementation ────────────────────────────────────────────

/// <summary>
/// Simple in-memory event bus for DevTools publish/subscribe.
/// </summary>
public sealed class DevToolsEventBus : IDevToolsEventBus
{
    private readonly object _lock = new();
    private readonly Dictionary<Type, List<Delegate>> _handlers = new();

    /// <inheritdoc />
    public void Publish<TEvent>(TEvent evt) where TEvent : IDevToolsEvent
    {
        List<Delegate>? handlers;
        lock (_lock)
        {
            if (!_handlers.TryGetValue(typeof(TEvent), out handlers))
                return;
            // Snapshot under lock
            handlers = new List<Delegate>(handlers);
        }

        foreach (var handler in handlers)
        {
            try
            {
                ((Action<TEvent>)handler)(evt);
            }
            catch
            {
                // Swallow subscriber exceptions to keep bus alive
            }
        }
    }

    /// <inheritdoc />
    public IDisposable Subscribe<TEvent>(Action<TEvent> handler) where TEvent : IDevToolsEvent
    {
        lock (_lock)
        {
            var eventType = typeof(TEvent);
            if (!_handlers.ContainsKey(eventType))
                _handlers[eventType] = new List<Delegate>();
            _handlers[eventType].Add(handler);
        }

        return new Subscription<TEvent>(this, handler);
    }

    private sealed class Subscription<TEvent> : IDisposable where TEvent : IDevToolsEvent
    {
        private readonly DevToolsEventBus _bus;
        private readonly Action<TEvent> _handler;
        private bool _disposed;

        public Subscription(DevToolsEventBus bus, Action<TEvent> handler)
        {
            _bus = bus;
            _handler = handler;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            lock (_bus._lock)
            {
                if (_bus._handlers.TryGetValue(typeof(TEvent), out var handlers))
                {
                    handlers.Remove(_handler);
                }
            }
        }
    }
}

// ── In-memory data store for DevTools ───────────────────────────────────

/// <summary>
/// Thread-safe in-memory store for DevTools collected data.
/// </summary>
public sealed class DevToolsDataStore
{
    private readonly object _lock = new();
    private readonly List<RouteInspectorPanel.RouteInfo> _routes = new();
    private readonly List<ComponentTreePanel.ComponentInfo> _components = new();
    private readonly List<PerformanceProfilerPanel.PerformanceMetric> _metrics = new();
    private readonly List<NetworkInspectorPanel.NetworkRequest> _networkRequests = new();
    private readonly List<string> _consoleLogs = new();
    private int _maxNetworkEntries = 500;
    private int _maxConsoleEntries = 1000;

    /// <summary>Maximum number of network request entries to retain.</summary>
    public int MaxNetworkEntries { get => _maxNetworkEntries; set => _maxNetworkEntries = value; }

    /// <summary>Maximum number of console log entries to retain.</summary>
    public int MaxConsoleEntries { get => _maxConsoleEntries; set => _maxConsoleEntries = value; }

    // ── Routes ───────────────────────────────────────────────────────

    /// <summary>Set the current route list (replaces all).</summary>
    public void SetRoutes(IEnumerable<RouteInspectorPanel.RouteInfo> routes)
    {
        lock (_lock)
        {
            _routes.Clear();
            _routes.AddRange(routes);
        }
    }

    /// <summary>Get all routes.</summary>
    public IReadOnlyList<RouteInspectorPanel.RouteInfo> GetRoutes()
    {
        lock (_lock) return _routes.ToList();
    }

    // ── Components ───────────────────────────────────────────────────

    /// <summary>Set the current component list.</summary>
    public void SetComponents(IEnumerable<ComponentTreePanel.ComponentInfo> components)
    {
        lock (_lock)
        {
            _components.Clear();
            _components.AddRange(components);
        }
    }

    /// <summary>Get all components.</summary>
    public IReadOnlyList<ComponentTreePanel.ComponentInfo> GetComponents()
    {
        lock (_lock) return _components.ToList();
    }

    // ── Performance ──────────────────────────────────────────────────

    /// <summary>Add a performance metric.</summary>
    public void AddMetric(PerformanceProfilerPanel.PerformanceMetric metric)
    {
        lock (_lock) _metrics.Add(metric);
    }

    /// <summary>Get all performance metrics.</summary>
    public IReadOnlyList<PerformanceProfilerPanel.PerformanceMetric> GetMetrics()
    {
        lock (_lock) return _metrics.ToList();
    }

    /// <summary>Clear all performance metrics.</summary>
    public void ClearMetrics()
    {
        lock (_lock) _metrics.Clear();
    }

    // ── Network ──────────────────────────────────────────────────────

    /// <summary>Add a network request.</summary>
    public void AddNetworkRequest(NetworkInspectorPanel.NetworkRequest request)
    {
        lock (_lock)
        {
            _networkRequests.Add(request);
            if (_networkRequests.Count > _maxNetworkEntries)
                _networkRequests.RemoveAt(0);
        }
    }

    /// <summary>Get all network requests.</summary>
    public IReadOnlyList<NetworkInspectorPanel.NetworkRequest> GetNetworkRequests()
    {
        lock (_lock) return _networkRequests.ToList();
    }

    /// <summary>Clear all network requests.</summary>
    public void ClearNetworkRequests()
    {
        lock (_lock) _networkRequests.Clear();
    }

    // ── Console ──────────────────────────────────────────────────────

    /// <summary>Add a console log entry.</summary>
    public void AddConsoleLog(string entry)
    {
        lock (_lock)
        {
            _consoleLogs.Add(entry);
            if (_consoleLogs.Count > _maxConsoleEntries)
                _consoleLogs.RemoveAt(0);
        }
    }

    /// <summary>Get all console logs.</summary>
    public IReadOnlyList<string> GetConsoleLogs()
    {
        lock (_lock) return _consoleLogs.ToList();
    }

    /// <summary>Clear all console logs.</summary>
    public void ClearConsoleLogs()
    {
        lock (_lock) _consoleLogs.Clear();
    }
}

// ── DevTools Server ─────────────────────────────────────────────────────

/// <summary>
/// Main DevTools host that manages the TUI or headless server lifecycle.
/// </summary>
public sealed class DevToolsServer : IDisposable, IAsyncDisposable
{
    private readonly DevToolsOptions _options;
    private readonly DevToolsDataStore _dataStore;
    private readonly DevToolsEventBus _eventBus;
    private readonly IReadOnlyList<IDevToolsPanel> _panels;
    private CancellationTokenSource? _cts;
    private Task? _serverTask;
    private WebApplication? _webApp;
    private DevToolsWebSocketManager? _wsManager;
    private int _activePanelIndex;

    /// <summary>
    /// Creates a new DevTools server instance.
    /// </summary>
    public DevToolsServer(DevToolsOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _dataStore = new DevToolsDataStore();
        _eventBus = new DevToolsEventBus();
        _panels = CreatePanels();
        _activePanelIndex = options.ActivePanel;
    }

    /// <summary>
    /// Gets the event bus for publishing/subscribing to DevTools events.
    /// </summary>
    public IDevToolsEventBus EventBus => _eventBus;

    /// <summary>
    /// Gets the data store for DevTools collected data.
    /// </summary>
    public DevToolsDataStore DataStore => _dataStore;

    /// <summary>
    /// Gets the list of registered panels.
    /// </summary>
    public IReadOnlyList<IDevToolsPanel> Panels => _panels;

    /// <summary>
    /// Gets or sets the active panel index.
    /// </summary>
    public int ActivePanelIndex
    {
        get => _activePanelIndex;
        set
        {
            if (value >= 0 && value < _panels.Count)
                _activePanelIndex = value;
        }
    }

    /// <summary>
    /// Gets the active panel.
    /// </summary>
    public IDevToolsPanel ActivePanel => _panels[_activePanelIndex];

    /// <summary>
    /// Gets the WebSocket manager for headless mode.
    /// </summary>
    public DevToolsWebSocketManager? WsManager => _wsManager;

    /// <summary>
    /// Whether the server is currently running.
    /// </summary>
    public bool IsRunning => _serverTask is { IsCompleted: false };

    /// <summary>
    /// Start the DevTools server.
    /// </summary>
    public Task StartAsync(CancellationToken ct = default)
    {
        if (IsRunning) return Task.CompletedTask;

        _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);

        if (_options.Mode == DevToolsMode.Off)
            return Task.CompletedTask;

        if (_options.Mode == DevToolsMode.Headless)
        {
            StartHeadlessAsync(_cts.Token);
        }
        // TUI mode is started as a foreground live display loop
        return Task.CompletedTask;
    }

    /// <summary>
    /// Stop the DevTools server.
    /// </summary>
    public async Task StopAsync()
    {
        _cts?.Cancel();

        if (_webApp is not null)
        {
            try
            {
                await _webApp.StopAsync();
            }
            catch { /* ignore shutdown errors */ }
            await _webApp.DisposeAsync();
            _webApp = null;
        }

        _wsManager?.Dispose();
        _wsManager = null;

        if (_serverTask is not null)
        {
            try { await _serverTask; } catch { /* ignore */ }
            _serverTask = null;
        }
    }

    /// <summary>
    /// Runs the TUI live display loop. Blocks until the user quits.
    /// </summary>
    public void RunTuiLoop()
    {
        if (_options.Mode == DevToolsMode.Off)
            return;

        System.Console.CursorVisible = false;

        try
        {
            while (true)
            {
                RenderTui();

                var key = System.Console.ReadKey(true);

                if (key.Key == ConsoleKey.Q)
                    break;

                switch (key.Key)
                {
                    case ConsoleKey.Tab:
                        ActivePanelIndex = (_activePanelIndex + 1) % _panels.Count;
                        break;
                    case ConsoleKey.UpArrow:
                        ActivePanel.HandleInput(ConsoleKey.UpArrow);
                        break;
                    case ConsoleKey.DownArrow:
                        ActivePanel.HandleInput(ConsoleKey.DownArrow);
                        break;
                    case ConsoleKey.Enter:
                        ActivePanel.HandleInput(ConsoleKey.Enter);
                        break;
                    case ConsoleKey.LeftArrow:
                        ActivePanel.HandleInput(ConsoleKey.LeftArrow);
                        break;
                    case ConsoleKey.RightArrow:
                        ActivePanel.HandleInput(ConsoleKey.RightArrow);
                        break;
                    case ConsoleKey.D1:
                        if (_panels.Count > 0) ActivePanelIndex = 0;
                        break;
                    case ConsoleKey.D2:
                        if (_panels.Count > 1) ActivePanelIndex = 1;
                        break;
                    case ConsoleKey.D3:
                        if (_panels.Count > 2) ActivePanelIndex = 2;
                        break;
                    case ConsoleKey.D4:
                        if (_panels.Count > 3) ActivePanelIndex = 3;
                        break;
                    case ConsoleKey.D5:
                        if (_panels.Count > 4) ActivePanelIndex = 4;
                        break;
                    default:
                        ActivePanel.HandleInput(key.Key);
                        break;
                }
            }
        }
        finally
        {
            System.Console.CursorVisible = true;
        }
    }

    /// <summary>
    /// Renders the TUI to the console.
    /// </summary>
    public void RenderTui()
    {
        System.Console.Clear();

        var width = System.Console.WindowWidth;
        var panelWidth = (width - 2) / _panels.Count;

        // ── Header bar ───────────────────────────────────────────
        var header = " NextNet DevTools ";
        System.Console.ForegroundColor = ConsoleColor.Cyan;
        System.Console.WriteLine(new string('=', width));
        System.Console.ResetColor();
        System.Console.WriteLine(header.PadRight(width));

        // ── Tab bar ──────────────────────────────────────────────
        for (int i = 0; i < _panels.Count; i++)
        {
            var panel = _panels[i];
            var label = $" [{i + 1}] {panel.Icon} {panel.Name} ";

            if (i == _activePanelIndex)
            {
                System.Console.BackgroundColor = ConsoleColor.DarkCyan;
                System.Console.ForegroundColor = ConsoleColor.White;
            }
            else
            {
                System.Console.BackgroundColor = ConsoleColor.DarkGray;
                System.Console.ForegroundColor = ConsoleColor.Gray;
            }

            System.Console.Write(label.PadRight(panelWidth));
        }

        System.Console.ResetColor();
        System.Console.WriteLine();
        System.Console.WriteLine(new string('─', width));

        // ── Panel content ────────────────────────────────────────
        System.Console.WriteLine();
        ActivePanel.Render(new TuiRenderContext(width));
        System.Console.WriteLine();

        // ── Status bar ───────────────────────────────────────────
        System.Console.WriteLine(new string('─', width));
        var status = $" Routes: {_dataStore.GetRoutes().Count} │ Components: {_dataStore.GetComponents().Count} │ Metrics: {_dataStore.GetMetrics().Count} │ Mode: {_options.Mode} ";
        System.Console.ForegroundColor = ConsoleColor.DarkGray;
        System.Console.WriteLine(status.PadRight(width));
        System.Console.ResetColor();

        System.Console.ForegroundColor = ConsoleColor.DarkGray;
        System.Console.WriteLine(" Tab: switch panel │ ↑/↓: navigate │ Enter: expand │ Q: quit ".PadRight(width));
        System.Console.ResetColor();
    }

    private void StartHeadlessAsync(CancellationToken ct)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls($"http://localhost:{_options.Port}");
        // Remainder done synchronously, but WebApplication.StartAsync is awaited externally
        builder.Services.AddSingleton(_dataStore);
        builder.Services.AddSingleton(_eventBus);

        _wsManager = new DevToolsWebSocketManager();
        builder.Services.AddSingleton(_wsManager);

        var app = builder.Build();

        app.UseWebSockets();
        app.UseMiddleware<DevToolsMiddleware>(_dataStore, _wsManager);

        _webApp = app;
        _serverTask = app.StartAsync(ct);
    }

    private IReadOnlyList<IDevToolsPanel> CreatePanels()
    {
        var panels = new List<IDevToolsPanel>();

        if (_options.EnableRouteInspector)
            panels.Add(new RouteInspectorPanel(_dataStore));
        if (_options.EnableComponentGraph)
            panels.Add(new ComponentTreePanel(_dataStore));
        if (_options.EnableProfiler)
            panels.Add(new PerformanceProfilerPanel(_dataStore));
        if (_options.EnableNetworkInspector)
            panels.Add(new NetworkInspectorPanel(_dataStore));
        if (_options.EnableConsolePanel)
            panels.Add(new ConsolePanel(_dataStore));

        return panels;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _wsManager?.Dispose();
        // Fire-and-forget the async disposal to avoid deadlock in synchronous Dispose.
        // The WebApplication disposal is awaited properly in the async overload.
        if (_webApp is not null)
        {
            var app = _webApp;
            _webApp = null;
            _ = DisposeWebAppAsync(app);
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _wsManager?.Dispose();
        if (_webApp is not null)
        {
            await _webApp.DisposeAsync();
            _webApp = null;
        }
    }

    private static async Task DisposeWebAppAsync(WebApplication app)
    {
        try
        {
            await app.DisposeAsync();
        }
        catch
        {
            // Swallow disposal errors during synchronous teardown
        }
    }
}

// ── Panel interface ────────────────────────────────────────────────────

/// <summary>
/// Represents a single DevTools panel that can be rendered in the TUI.
/// </summary>
public interface IDevToolsPanel
{
    /// <summary>Display name of the panel.</summary>
    string Name { get; }

    /// <summary>Unicode icon for the panel.</summary>
    string Icon { get; }

    /// <summary>Render the panel content to the TUI.</summary>
    void Render(TuiRenderContext context);

    /// <summary>Handle keyboard input.</summary>
    void HandleInput(ConsoleKey key);
}

/// <summary>
/// Context provided to panels during rendering.
/// </summary>
public class TuiRenderContext
{
    /// <summary>Width of the terminal in characters.</summary>
    public int Width { get; }

    /// <summary>Creates a new render context.</summary>
    public TuiRenderContext(int width) => Width = width;
}
