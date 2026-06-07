using NextNet.DevTools.UI;

namespace NextNet.DevTools.Panels;

/// <summary>
/// Component Tree panel — displays the component dependency graph and render hierarchy.
/// </summary>
public sealed class ComponentTreePanel : IDevToolsPanel
{
    private readonly DevToolsDataStore _dataStore;
    private int _selectedIndex;
    private bool _showDetails;
    private bool _treeMode = true;

    /// <inheritdoc />
    public string Name => "Component Tree";

    /// <inheritdoc />
    public string Icon => "🧩";

    /// <summary>
    /// Creates a new ComponentTreePanel.
    /// </summary>
    public ComponentTreePanel(DevToolsDataStore dataStore)
    {
        _dataStore = dataStore;
    }

    /// <inheritdoc />
    public void Render(TuiRenderContext context)
    {
        var components = _dataStore.GetComponents();

        System.Console.WriteLine($" Component Tree — {components.Count} components");
        System.Console.WriteLine($"  Mode: [{(_treeMode ? "TREE" : "LIST")}]  (press G to toggle)");
        System.Console.WriteLine();

        if (_treeMode)
        {
            RenderTree(components);
        }
        else
        {
            RenderList(components);
        }

        System.Console.ResetColor();
    }

    private void RenderTree(IReadOnlyList<ComponentInfo> components)
    {
        // Render top-level components (those without parents)
        var topLevel = components.Where(c => string.IsNullOrEmpty(c.Parent)).ToList();
        var tree = new DevToolsTree("Components");

        foreach (var comp in topLevel)
        {
            var node = tree.AddNode(
                $"{comp.Name} [{comp.Type}]",
                comp.Type switch
                {
                    "layout" => "📐",
                    "page" => "📄",
                    _ => "🧩"
                },
                comp.RenderCount > 100 ? ConsoleColor.Yellow : ConsoleColor.Gray);

            AddChildren(node, comp.Name, components);
        }

        tree.RenderToConsole();
    }

    private static void AddChildren(DevToolsTreeNode parent, string parentName, IReadOnlyList<ComponentInfo> components)
    {
        var children = components.Where(c => c.Parent == parentName).ToList();
        foreach (var child in children)
        {
            var childNode = parent.AddChild(
                $"{child.Name} [{child.Type}]",
                null,
                ConsoleColor.DarkGray);

            AddChildren(childNode, child.Name, components);
        }
    }

    private void RenderList(IReadOnlyList<ComponentInfo> components)
    {
        var table = new DevToolsTable("Name", "Type", "Renders", "Avg (ms)", "File");

        foreach (var comp in components)
        {
            table.AddRow(
                comp.Name,
                comp.Type,
                comp.RenderCount.ToString(),
                comp.AverageRenderTimeMs.ToString(),
                comp.File ?? "-");
        }

        System.Console.Write(table.Render());
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
                var components = _dataStore.GetComponents();
                _selectedIndex = Math.Min(components.Count - 1, _selectedIndex + 1);
                break;
            case ConsoleKey.Enter:
                _showDetails = !_showDetails;
                break;
            case ConsoleKey.G:
                _treeMode = !_treeMode;
                break;
        }
    }

    /// <summary>
    /// Information about a component.
    /// </summary>
    public class ComponentInfo
    {
        /// <summary>Component name.</summary>
        public string Name { get; init; } = string.Empty;

        /// <summary>Component type: page, layout, component.</summary>
        public string Type { get; init; } = "component";

        /// <summary>Source file path.</summary>
        public string? File { get; init; }

        /// <summary>Parent component name, empty for root-level.</summary>
        public string? Parent { get; init; }

        /// <summary>Number of renders.</summary>
        public int RenderCount { get; set; }

        /// <summary>Average render time in milliseconds.</summary>
        public long AverageRenderTimeMs { get; set; }

        /// <summary>List of child component names.</summary>
        public IReadOnlyList<string> Children { get; init; } = Array.Empty<string>();
    }
}
