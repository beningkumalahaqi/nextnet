using NextNet.DevTools.UI;

namespace NextNet.DevTools.Panels;

/// <summary>
/// Performance Profiler panel — displays build and render performance metrics
/// with flame-graph-style horizontal bar visualization.
/// Supports pausing, clearing, and sorting by duration.
/// </summary>
/// <example>
/// <code>
/// var panel = new PerformanceProfilerPanel(dataStore);
/// panel.Render(new TuiRenderContext(120));
/// panel.HandleInput(ConsoleKey.P); // pause
/// panel.HandleInput(ConsoleKey.C); // clear
/// panel.HandleInput(ConsoleKey.S); // sort by duration
/// </code>
/// </example>
public sealed class PerformanceProfilerPanel : IDevToolsPanel
{
    private readonly DevToolsDataStore _dataStore;
    private bool _paused;
    private bool _sortByDuration;

    /// <inheritdoc />
    public string Name => "Performance";

    /// <inheritdoc />
    public string Icon => "⚡";

    /// <summary>
    /// Creates a new PerformanceProfilerPanel.
    /// </summary>
    /// <param name="dataStore">The DevTools data store providing performance metrics.</param>
    public PerformanceProfilerPanel(DevToolsDataStore dataStore)
    {
        _dataStore = dataStore;
    }

    /// <inheritdoc />
    public void Render(TuiRenderContext context)
    {
        var metrics = _dataStore.GetMetrics();
        var sortedMetrics = _sortByDuration
            ? metrics.OrderByDescending(m => m.DurationMs).ToList()
            : metrics.ToList();

        var width = Math.Min(context.Width - 10, 60);

        System.Console.WriteLine($" Performance Profiler — {metrics.Count} metrics");
        System.Console.WriteLine($"  Status: {(_paused ? "⏸ PAUSED" : "▶ RUNNING")}");
        System.Console.WriteLine($"  Sort: {(_sortByDuration ? "by duration" : "by time")}");
        System.Console.WriteLine();

        if (sortedMetrics.Count == 0)
        {
            System.Console.WriteLine("  No metrics collected yet. Start the dev server to collect data.");
            System.Console.WriteLine();
            return;
        }

        // Find max duration for scaling
        var maxMs = sortedMetrics.Max(m => m.DurationMs);
        if (maxMs <= 0) maxMs = 1;

        foreach (var metric in sortedMetrics.Take(20))
        {
            var barLen = (int)(metric.DurationMs * width / maxMs);
            barLen = Math.Max(barLen, 1);
            barLen = Math.Min(barLen, width);

            var bar = new string('█', barLen);
            var padding = new string('░', width - barLen);

            System.Console.ForegroundColor = metric.Category?.ToLowerInvariant() switch
            {
                "build" => ConsoleColor.Cyan,
                "render" => ConsoleColor.Magenta,
                "network" => ConsoleColor.Blue,
                "hmr" => ConsoleColor.Green,
                _ => ConsoleColor.Gray
            };

            var name = metric.Name.Length > 25 ? metric.Name[..22] + "..." : metric.Name;
            System.Console.Write($"  {name,-25} ");
            System.Console.Write(bar);
            System.Console.ResetColor();
            System.Console.Write(padding);
            System.Console.ForegroundColor = ConsoleColor.DarkGray;
            System.Console.WriteLine($" {metric.DurationMs,6}ms");
            System.Console.ResetColor();
        }

        System.Console.ResetColor();

        // Summary
        if (sortedMetrics.Count > 0)
        {
            System.Console.WriteLine();
            System.Console.WriteLine($"  Total metrics: {sortedMetrics.Count}");
            System.Console.WriteLine($"  Total time:    {sortedMetrics.Sum(m => m.DurationMs)}ms");
            System.Console.WriteLine($"  Average:       {sortedMetrics.Average(m => m.DurationMs):F1}ms");
            System.Console.WriteLine($"  Max:           {sortedMetrics.Max(m => m.DurationMs)}ms");
        }

        System.Console.WriteLine();
        System.Console.ResetColor();
    }

    /// <inheritdoc />
    public void HandleInput(ConsoleKey key)
    {
        switch (key)
        {
            case ConsoleKey.P:
                _paused = !_paused;
                break;
            case ConsoleKey.C:
                _dataStore.ClearMetrics();
                break;
            case ConsoleKey.S:
                _sortByDuration = !_sortByDuration;
                break;
        }
    }

    /// <summary>
    /// A single performance metric data point.
    /// Records a named operation with its duration, timestamp, and category for flame-graph visualization.
    /// </summary>
    /// <example>
    /// <code>
    /// var metric = new PerformanceProfilerPanel.PerformanceMetric
    /// {
    ///     Name = "Route discovery",
    ///     DurationMs = 150,
    ///     Category = "build"
    /// };
    /// </code>
    /// </example>
    public sealed record PerformanceMetric
    {
        /// <summary>Metric name (e.g. "Route discovery").</summary>
        public string Name { get; init; } = string.Empty;

        /// <summary>Duration in milliseconds.</summary>
        public long DurationMs { get; init; }

        /// <summary>Timestamp when the metric was recorded.</summary>
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;

        /// <summary>Category: build, render, network, hmr.</summary>
        public string? Category { get; init; }
    }
}
