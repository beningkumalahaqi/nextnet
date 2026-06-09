using System.Text;

namespace NextNet.DevTools.UI;

/// <summary>
/// A terminal-renderable tree structure for DevTools panels.
/// Supports indentation, icons, and color-coded nodes for rendering component hierarchies and route trees.
/// </summary>
/// <example>
/// <code>
/// var tree = new DevToolsTree("Components");
/// var child = tree.AddNode("Layout", icon: "📐");
/// child.AddChild("Header", color: ConsoleColor.Blue);
/// child.AddChild("Footer");
/// tree.AddNode("Page", icon: "📄");
/// tree.RenderToConsole();
/// </code>
/// </example>
public sealed class DevToolsTree
{
    private readonly string _rootLabel;
    private readonly List<DevToolsTreeNode> _rootNodes = new();

    /// <summary>
    /// Creates a new tree with the specified root label.
    /// </summary>
    /// <param name="rootLabel">The label for the root of the tree.</param>
    public DevToolsTree(string rootLabel)
    {
        _rootLabel = rootLabel;
    }

    /// <summary>Add a root-level node.</summary>
    /// <param name="label">The node label text.</param>
    /// <param name="icon">Optional icon (emoji/unicode).</param>
    /// <param name="color">Optional console color for the node.</param>
    /// <returns>The created <see cref="DevToolsTreeNode"/>.</returns>
    public DevToolsTreeNode AddNode(string label, string? icon = null, ConsoleColor? color = null)
    {
        var node = new DevToolsTreeNode(label, icon, color);
        _rootNodes.Add(node);
        return node;
    }

    /// <summary>Render the tree as a string for terminal output.</summary>
    public string Render()
    {
        var sb = new StringBuilder();

        // Root label
        sb.AppendLine(_rootLabel);
        sb.AppendLine();

        // Render each root node
        for (int i = 0; i < _rootNodes.Count; i++)
        {
            var isLast = i == _rootNodes.Count - 1;
            RenderNode(sb, _rootNodes[i], "", isLast);
        }

        return sb.ToString();
    }

    private static void RenderNode(StringBuilder sb, DevToolsTreeNode node, string indent, bool isLast)
    {
        var prefix = isLast ? "└── " : "├── ";
        var childIndent = indent + (isLast ? "    " : "│   ");

        if (node.Color.HasValue)
        {
            // We can't use ConsoleColor in a string, so write the color info as a marker
            sb.Append(indent);
            sb.Append(prefix);
            if (node.Icon is not null)
                sb.Append($"{node.Icon} ");
            sb.AppendLine(node.Label);
        }
        else
        {
            sb.Append(indent);
            sb.Append(prefix);
            if (node.Icon is not null)
                sb.Append($"{node.Icon} ");
            sb.AppendLine(node.Label);
        }

        for (int i = 0; i < node.Children.Count; i++)
        {
            RenderNode(sb, node.Children[i], childIndent, i == node.Children.Count - 1);
        }
    }

    /// <summary>Render the tree to the console with colors.</summary>
    public void RenderToConsole()
    {
        System.Console.WriteLine(_rootLabel);
        System.Console.WriteLine();

        for (int i = 0; i < _rootNodes.Count; i++)
        {
            var isLast = i == _rootNodes.Count - 1;
            RenderNodeToConsole(_rootNodes[i], "", isLast);
        }
    }

    private static void RenderNodeToConsole(DevToolsTreeNode node, string indent, bool isLast)
    {
        var prefix = isLast ? "└── " : "├── ";
        var childIndent = indent + (isLast ? "    " : "│   ");

        System.Console.Write(indent);
        System.Console.Write(prefix);

        if (node.Color.HasValue)
            System.Console.ForegroundColor = node.Color.Value;

        if (node.Icon is not null)
            System.Console.Write($"{node.Icon} ");

        System.Console.WriteLine(node.Label);
        System.Console.ResetColor();

        for (int i = 0; i < node.Children.Count; i++)
        {
            RenderNodeToConsole(node.Children[i], childIndent, i == node.Children.Count - 1);
        }
    }
}

/// <summary>
/// A single node in a <see cref="DevToolsTree"/>.
/// Contains a label, optional icon, optional color, and a list of child nodes.
/// </summary>
/// <example>
/// <code>
/// var node = new DevToolsTreeNode("Header", icon: "🧩", color: ConsoleColor.Blue);
/// var child = node.AddChild("Logo", icon: "🔧");
/// Console.WriteLine(node.Label); // "Header"
/// Console.WriteLine(node.Children.Count); // 1
/// </code>
/// </example>
public sealed class DevToolsTreeNode
{
    private readonly List<DevToolsTreeNode> _children = new();

    /// <summary>The node label text.</summary>
    public string Label { get; set; }

    /// <summary>Optional icon prefix (emoji/unicode).</summary>
    public string? Icon { get; set; }

    /// <summary>Optional color for the node text.</summary>
    public ConsoleColor? Color { get; set; }

    /// <summary>Child nodes.</summary>
    public IReadOnlyList<DevToolsTreeNode> Children => _children;

    /// <summary>Creates a new tree node.</summary>
    /// <param name="label">The node label text.</param>
    /// <param name="icon">Optional icon (emoji/unicode).</param>
    /// <param name="color">Optional console color.</param>
    public DevToolsTreeNode(string label, string? icon = null, ConsoleColor? color = null)
    {
        Label = label;
        Icon = icon;
        Color = color;
    }

    /// <summary>Add a child node and return it.</summary>
    /// <param name="label">The child node label.</param>
    /// <param name="icon">Optional icon for the child.</param>
    /// <param name="color">Optional console color for the child.</param>
    /// <returns>The created <see cref="DevToolsTreeNode"/> child.</returns>
    public DevToolsTreeNode AddChild(string label, string? icon = null, ConsoleColor? color = null)
    {
        var child = new DevToolsTreeNode(label, icon, color);
        _children.Add(child);
        return child;
    }
}
