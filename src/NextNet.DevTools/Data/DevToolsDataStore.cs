using NextNet.DevTools.Panels;

namespace NextNet.DevTools;

/// <summary>
/// Thread-safe in-memory store for DevTools collected data.
/// Manages routes, components, performance metrics, network requests, and console logs.
/// </summary>
/// <example>
/// <code>
/// var store = new DevToolsDataStore();
/// store.SetRoutes(new[] { new RouteInspectorPanel.RouteInfo { Path = "/", Type = "static", File = "app/page.cs" } });
/// var routes = store.GetRoutes();
///
/// store.AddMetric(new PerformanceProfilerPanel.PerformanceMetric { Name = "Build", DurationMs = 500 });
/// store.AddNetworkRequest(new NetworkInspectorPanel.NetworkRequest { Method = "GET", Url = "/api/test", StatusCode = 200 });
/// store.AddConsoleLog("[INFO] Server started");
/// </code>
/// </example>
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

    /// <summary>Set the current route list (replaces all existing routes).</summary>
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

    /// <summary>Set the current component list (replaces all existing components).</summary>
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

    /// <summary>Add a network request. Trims oldest entry if <see cref="MaxNetworkEntries"/> is exceeded.</summary>
    public void AddNetworkRequest(NetworkInspectorPanel.NetworkRequest request)
    {
        lock (_lock)
        {
            _networkRequests.Add(request);
            if (_networkRequests.Count > _maxNetworkEntries)
            {
                _networkRequests.RemoveAt(0);
                // DS-903: Data store capacity exceeded — oldest network request trimmed
                System.Diagnostics.Debug.WriteLine(
                    $"{Errors.DevToolsErrorCodes.DataStoreCapacityExceeded}: Network request capacity exceeded ({_maxNetworkEntries}), oldest entry trimmed.");
            }
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

    /// <summary>Add a console log entry. Trims oldest entry if <see cref="MaxConsoleEntries"/> is exceeded.</summary>
    public void AddConsoleLog(string entry)
    {
        lock (_lock)
        {
            _consoleLogs.Add(entry);
            if (_consoleLogs.Count > _maxConsoleEntries)
            {
                _consoleLogs.RemoveAt(0);
                // DS-903: Data store capacity exceeded — oldest console log trimmed
                System.Diagnostics.Debug.WriteLine(
                    $"{Errors.DevToolsErrorCodes.DataStoreCapacityExceeded}: Console log capacity exceeded ({_maxConsoleEntries}), oldest entry trimmed.");
            }
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
