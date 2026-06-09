using NextNet.DevTools.UI;

namespace NextNet.DevTools.Panels;

/// <summary>
/// Route Inspector panel — displays the route tree with color-coded route types
/// and per-route metadata including render counts and timing.
/// </summary>
/// <example>
/// <code>
/// var panel = new RouteInspectorPanel(dataStore);
/// panel.Render(new TuiRenderContext(120));
/// panel.HandleInput(ConsoleKey.DownArrow);
/// </code>
/// </example>
public sealed class RouteInspectorPanel : IDevToolsPanel
{
    private readonly DevToolsDataStore _dataStore;
    private int _selectedIndex;
    private string _filterText = string.Empty;
    private bool _expanded;

    /// <inheritdoc />
    public string Name => "Route Inspector";

    /// <inheritdoc />
    public string Icon => "🗺";

    /// <summary>
    /// Creates a new RouteInspectorPanel.
    /// </summary>
    /// <param name="dataStore">The DevTools data store providing route data.</param>
    public RouteInspectorPanel(DevToolsDataStore dataStore)
    {
        _dataStore = dataStore;
    }

    /// <inheritdoc />
    public void Render(TuiRenderContext context)
    {
        var routes = _dataStore.GetRoutes();
        var filteredRoutes = routes
            .Where(r => string.IsNullOrEmpty(_filterText) ||
                        r.Path.Contains(_filterText, StringComparison.OrdinalIgnoreCase) ||
                        r.File.Contains(_filterText, StringComparison.OrdinalIgnoreCase))
            .ToList();

        System.Console.WriteLine($" Route Inspector — {routes.Count} routes total");
        System.Console.WriteLine($"  ├─ Static:  {routes.Count(r => r.Type == "static")}");
        System.Console.WriteLine($"  ├─ Dynamic: {routes.Count(r => r.Type == "dynamic")}");
        System.Console.WriteLine($"  ├─ API:     {routes.Count(r => r.Type == "api")}");
        System.Console.WriteLine($"  └─ Layout:  {routes.Count(r => r.Type == "layout")}");
        System.Console.WriteLine();

        if (!string.IsNullOrEmpty(_filterText))
        {
            System.Console.ForegroundColor = ConsoleColor.Yellow;
            System.Console.WriteLine($"  Filter: '{_filterText}' ({filteredRoutes.Count} matches)");
            System.Console.ResetColor();
            System.Console.WriteLine();
        }

        foreach (var route in filteredRoutes)
        {
            var color = route.Type switch
            {
                "static" => ConsoleColor.Blue,
                "dynamic" => ConsoleColor.Magenta,
                "api" => ConsoleColor.Red,
                "layout" => ConsoleColor.Green,
                _ => ConsoleColor.Gray
            };

            System.Console.ForegroundColor = color;
            var icon = route.Type switch
            {
                "static" => "📄",
                "dynamic" => "🔀",
                "api" => "🔌",
                "layout" => "📐",
                _ => "📁"
            };

            System.Console.WriteLine($"  {icon} {route.Path}");
            System.Console.ResetColor();

            if (_expanded && filteredRoutes.IndexOf(route) == _selectedIndex)
            {
                System.Console.WriteLine($"     File:       {route.File}");
                System.Console.WriteLine($"     Layout:     {route.Layout ?? "(none)"}");
                System.Console.WriteLine($"     SSR:        {route.Ssr}");
                System.Console.WriteLine($"     SSG:        {route.Ssg}");
                System.Console.WriteLine($"     Renders:    {route.RenderCount}");
                System.Console.WriteLine($"     Avg time:   {route.AverageRenderTimeMs}ms");
                System.Console.WriteLine();
            }
        }

        System.Console.ResetColor();
    }

    /// <inheritdoc />
    public void HandleInput(ConsoleKey key)
    {
        switch (key)
        {
            case ConsoleKey.UpArrow:
                _selectedIndex = Math.Max(0, _selectedIndex - 1);
                break;
            case ConsoleKey.DownArrow:
                var routes = _dataStore.GetRoutes();
                _selectedIndex = Math.Min(routes.Count - 1, _selectedIndex + 1);
                break;
            case ConsoleKey.Enter:
                _expanded = !_expanded;
                break;
        }
    }

    /// <summary>
    /// Information about a discovered route.
    /// Encapsulates route metadata including path pattern, type, source file, layout, SSR/SSG flags, and render statistics.
    /// </summary>
    /// <example>
    /// <code>
    /// var info = new RouteInspectorPanel.RouteInfo
    /// {
    ///     Path = "/blog/[slug]",
    ///     Type = "dynamic",
    ///     File = "app/blog/[slug]/page.cs",
    ///     Layout = "app/layout.cs",
    ///     Ssr = true,
    ///     Ssg = false,
    ///     RenderCount = 42,
    ///     AverageRenderTimeMs = 18
    /// };
    /// </code>
    /// </example>
    public sealed record RouteInfo
    {
        /// <summary>Route path pattern (e.g. "/blog/[slug]").</summary>
        public string Path { get; init; } = string.Empty;

        /// <summary>Route type: static, dynamic, api, layout.</summary>
        public string Type { get; init; } = "static";

        /// <summary>Source file path relative to app directory.</summary>
        public string File { get; init; } = string.Empty;

        /// <summary>Layout file path, if applicable.</summary>
        public string? Layout { get; init; }

        /// <summary>Whether server-side rendering is enabled.</summary>
        public bool Ssr { get; init; } = true;

        /// <summary>Whether static site generation is enabled.</summary>
        public bool Ssg { get; init; }

        /// <summary>Number of times this route has been rendered.</summary>
        public int RenderCount { get; init; }

        /// <summary>Average render time in milliseconds.</summary>
        public long AverageRenderTimeMs { get; init; }
    }
}
