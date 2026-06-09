using System.Diagnostics;
using System.Net.WebSockets;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NextNet.DevTools.Errors;
using NextNet.DevTools.Headless;
using NextNet.DevTools.Panels;
using NextNet.DevTools.UI;

namespace NextNet.DevTools;

/// <summary>
/// Defines the DevTools operating mode.
/// </summary>
/// <example>
/// <code>
/// var options = new DevToolsOptions { Mode = DevToolsMode.Tui };
/// var server = new DevToolsServer(options);
/// </code>
/// </example>
public enum DevToolsMode
{
    /// <summary>Terminal UI mode (default in terminal). Renders panels interactively.</summary>
    Tui,

    /// <summary>HTTP API + WebSocket mode for external consumers (VS Code extension, browser).</summary>
    Headless,

    /// <summary>Disabled — no DevTools UI or API is started.</summary>
    Off
}

/// <summary>
/// Configuration options for the DevTools host.
/// Controls operating mode, port, and which panels are enabled.
/// </summary>
/// <example>
/// <code>
/// var options = new DevToolsOptions
/// {
///     Mode = DevToolsMode.Headless,
///     Port = 9000,
///     EnableRouteInspector = true,
///     EnableProfiler = false
/// };
/// </code>
/// </example>
public sealed record DevToolsOptions
{
    /// <summary>Operating mode: Tui, Headless, or Off (default Tui).</summary>
    public DevToolsMode Mode { get; init; } = DevToolsMode.Tui;

    /// <summary>Port for the headless HTTP API (default 3001).</summary>
    public int Port { get; init; } = 3001;

    /// <summary>Initial active panel index.</summary>
    public int ActivePanel { get; init; }

    /// <summary>Enable the performance profiler panel (default true).</summary>
    public bool EnableProfiler { get; init; } = true;

    /// <summary>Enable the route inspector panel (default true).</summary>
    public bool EnableRouteInspector { get; init; } = true;

    /// <summary>Enable the component graph panel (default true).</summary>
    public bool EnableComponentGraph { get; init; } = true;

    /// <summary>Enable the network inspector panel (default true).</summary>
    public bool EnableNetworkInspector { get; init; } = true;

    /// <summary>Enable the console log panel (default true).</summary>
    public bool EnableConsolePanel { get; init; } = true;

    /// <summary>
    /// Whether the terminal uses a dark background (default true).
    /// Controls which <see cref="TerminalColorPalette"/> is used for console output.
    /// Set to false for light terminal themes.
    /// </summary>
    public bool IsDark { get; init; } = true;
}

/// <summary>
/// Context provided to panels during TUI rendering.
/// Carries terminal dimensions for responsive layout.
/// </summary>
/// <example>
/// <code>
/// var ctx = new TuiRenderContext(120);
/// myPanel.Render(ctx);
/// </code>
/// </example>
public sealed record TuiRenderContext
{
    /// <summary>Width of the terminal in characters.</summary>
    public int Width { get; }

    /// <summary>Creates a new render context with the specified terminal width.</summary>
    /// <param name="width">Terminal width in characters.</param>
    public TuiRenderContext(int width) => Width = width;
}

// ── DevTools Server ─────────────────────────────────────────────────────

/// <summary>
/// Main DevTools host that manages the TUI or headless server lifecycle.
/// Coordinates panels, event bus, data store, and WebSocket connections.
/// </summary>
/// <example>
/// <code>
/// // TUI mode
/// var options = new DevToolsOptions { Mode = DevToolsMode.Tui };
/// using var server = new DevToolsServer(options);
/// await server.StartAsync();
/// server.RunTuiLoop();
///
/// // Headless mode
/// var headlessOptions = new DevToolsOptions { Mode = DevToolsMode.Headless, Port = 4000 };
/// using var headlessServer = new DevToolsServer(headlessOptions);
/// await headlessServer.StartAsync();
/// </code>
/// </example>
public sealed class DevToolsServer : IDisposable, IAsyncDisposable
{
    private readonly DevToolsOptions _options;
    private readonly DevToolsDataStore _dataStore;
    private readonly DevToolsEventBus _eventBus;
    private readonly IReadOnlyList<IDevToolsPanel> _panels;
    private readonly TerminalColorPalette _palette;
    private readonly DevToolsConsole _console;
    private CancellationTokenSource? _cts;
    private Task? _serverTask;
    private WebApplication? _webApp;
    private DevToolsWebSocketManager? _wsManager;
    private int _activePanelIndex;

    /// <summary>
    /// Creates a new DevTools server instance.
    /// </summary>
    /// <param name="options">Configuration options. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
    public DevToolsServer(DevToolsOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _dataStore = new DevToolsDataStore();
        _eventBus = new DevToolsEventBus();
        _panels = CreatePanels();
        _activePanelIndex = options.ActivePanel;
        _palette = new TerminalColorPalette(options.IsDark);
        _console = new DevToolsConsole(_palette);
        DevToolsConsole.Default = _console;
    }

    /// <summary>
    /// Gets the event bus for publishing/subscribing to DevTools events.
    /// </summary>
    public IDevToolsEventBus EventBus => _eventBus;

    /// <summary>
    /// Gets the data store for DevTools collected data (routes, components, metrics).
    /// </summary>
    public DevToolsDataStore DataStore => _dataStore;

    /// <summary>
    /// Gets the list of registered panels.
    /// </summary>
    public IReadOnlyList<IDevToolsPanel> Panels => _panels;

    /// <summary>
    /// Gets or sets the active panel index.
    /// Set to a value between 0 and <see cref="Panels"/>.Count - 1.
    /// Values outside this range are silently ignored.
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
    /// Gets the WebSocket manager for headless mode. Returns null in TUI mode.
    /// </summary>
    public DevToolsWebSocketManager? WsManager => _wsManager;

    /// <summary>
    /// Whether the server is currently running.
    /// </summary>
    public bool IsRunning => _serverTask is { IsCompleted: false };

    /// <summary>
    /// Start the DevTools server.
    /// Throws <see cref="InvalidOperationException"/> if the server is already running (DS-906).
    /// </summary>
    /// <param name="ct">Cancellation token for the server lifecycle.</param>
    /// <exception cref="InvalidOperationException">DS-906: Server is already running.</exception>
    public Task StartAsync(CancellationToken ct = default)
    {
        if (IsRunning)
            throw new InvalidOperationException(
                $"{DevToolsErrorCodes.DevToolsServerAlreadyRunning}: DevTools server is already running.");

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
    /// Runs the TUI live display loop. Blocks until the user presses Q or Ctrl+C.
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
    /// Renders the TUI to the console, including header, tab bar, active panel, and status bar.
    /// </summary>
    public void RenderTui()
    {
        System.Console.Clear();

        var width = System.Console.WindowWidth;
        var panelWidth = (width - 2) / _panels.Count;

        // ── Header bar ───────────────────────────────────────────
        var header = " NextNet DevTools ";
        System.Console.ForegroundColor = _palette.Resolve(DevToolsColorRole.Primary);
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
                System.Console.BackgroundColor = _palette.Resolve(DevToolsColorRole.PrimaryMuted);
                System.Console.ForegroundColor = _palette.Resolve(DevToolsColorRole.Highlight);
            }
            else
            {
                System.Console.BackgroundColor = _palette.Resolve(DevToolsColorRole.Muted);
                System.Console.ForegroundColor = _palette.Resolve(DevToolsColorRole.Foreground);
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
        System.Console.ForegroundColor = _palette.Resolve(DevToolsColorRole.Muted);
        System.Console.WriteLine(status.PadRight(width));
        System.Console.ResetColor();

        System.Console.ForegroundColor = _palette.Resolve(DevToolsColorRole.Muted);
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
