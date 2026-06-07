using Spectre.Console;
using Spectre.Console.Rendering;

namespace NextNet.Cli.UI;

/// <summary>
/// Wraps Spectre.Console's <see cref="Tree"/> with NextNet branding.
/// Used for route hierarchies, dependency graphs, and file structure display.
/// Uses an internal model so nodes can be mutated after creation.
/// </summary>
public sealed class NextNetTree
{
    private readonly string _rootLabel;
    private readonly OutputMode _mode;
    private readonly List<NextNetTreeNode> _rootNodes = new();

    /// <summary>
    /// Creates a new tree with NextNet-styled root label.
    /// </summary>
    /// <param name="rootLabel">The root node label.</param>
    /// <param name="mode">Output mode (Color or Plain).</param>
    public NextNetTree(string rootLabel, OutputMode mode = OutputMode.Color)
    {
        _rootLabel = rootLabel;
        _mode = mode;
    }

    /// <summary>Adds a child node to the root.</summary>
    public NextNetTreeNode AddNode(string label, string? icon = null)
    {
        var node = new NextNetTreeNode(label, _mode, icon: icon);
        _rootNodes.Add(node);
        return node;
    }

    /// <summary>Adds a child node with explicit style.</summary>
    public NextNetTreeNode AddNode(string label, Style style)
    {
        var node = new NextNetTreeNode(label, _mode, style: style);
        _rootNodes.Add(node);
        return node;
    }

    /// <summary>Build the Spectre.Console tree from the internal model.</summary>
    public Tree Build()
    {
        var tree = new Tree(_rootLabel)
            .Style(_mode == OutputMode.Color
                ? new Style(foreground: Theme.NextNetTeal, decoration: Decoration.Bold)
                : new Style(decoration: Decoration.Bold));

        foreach (var rootNode in _rootNodes)
        {
            tree.AddNode(BuildNode(rootNode));
        }

        return tree;
    }

    /// <summary>Expose the underlying Spectre.Console tree (builds from model).</summary>
    public Tree GetSpectreTree() => Build();

    /// <summary>Render the tree to the console.</summary>
    public void Render(IAnsiConsole console) => console.Write(Build());

    private TreeNode BuildNode(NextNetTreeNode node)
    {
        var label = node.Icon is not null && _mode != OutputMode.Plain
            ? $"{node.Icon} {node.Label}"
            : node.Label;

        var renderable = node.Style is not null
            ? new Markup(label, node.Style)
            : new Markup(label) as IRenderable;

        var treeNode = new TreeNode(renderable);

        foreach (var child in node.Children)
        {
            treeNode.AddNode(BuildNode(child));
        }

        return treeNode;
    }
}

/// <summary>
/// Represents a single node in a <see cref="NextNetTree"/>.
/// Provides convenience methods for common state indicators.
/// Supports mutation; the tree is rebuilt when rendered.
/// </summary>
public sealed class NextNetTreeNode
{
    private readonly OutputMode _mode;
    private readonly List<NextNetTreeNode> _children = new();

    internal NextNetTreeNode(string label, OutputMode mode, string? icon = null, Style? style = null)
    {
        Label = label;
        _mode = mode;
        Icon = icon;
        Style = style;
    }

    /// <summary>The node's text label.</summary>
    public string Label { get; set; }

    /// <summary>Optional icon prefix.</summary>
    public string? Icon { get; set; }

    /// <summary>Optional style override.</summary>
    public Style? Style { get; set; }

    /// <summary>Child nodes.</summary>
    public IReadOnlyList<NextNetTreeNode> Children => _children;

    /// <summary>Add a child node with a label and optional icon prefix.</summary>
    public NextNetTreeNode AddChild(string label, string? icon = null)
    {
        var child = new NextNetTreeNode(label, _mode, icon: icon);
        _children.Add(child);
        return child;
    }

    /// <summary>Add a child node with a styled label.</summary>
    public NextNetTreeNode AddChild(string label, Style style)
    {
        var child = new NextNetTreeNode(label, _mode, style: style);
        _children.Add(child);
        return child;
    }

    /// <summary>Mark this node as successful (green + ✓).</summary>
    public void MarkSuccess(string? label = null)
    {
        if (label is not null) Label = label;
        var prefix = _mode == OutputMode.Plain ? "[OK]" : "✓";
        Icon = prefix;
        Style = Theme.SuccessStyle;
    }

    /// <summary>Mark this node as an error (red + ✗).</summary>
    public void MarkError(string? label = null)
    {
        if (label is not null) Label = label;
        var prefix = _mode == OutputMode.Plain ? "[ERR]" : "✗";
        Icon = prefix;
        Style = Theme.ErrorStyle;
    }

    /// <summary>Mark this node as pending (muted + ○).</summary>
    public void MarkPending(string? label = null)
    {
        if (label is not null) Label = label;
        Icon = _mode == OutputMode.Plain ? "o" : "○";
        Style = Theme.MutedStyle;
    }

    /// <summary>Mark this node as active (teal + spinner placeholder).</summary>
    public void MarkActive(string? label = null)
    {
        if (label is not null) Label = label;
        Icon = _mode == OutputMode.Plain ? ">" : "▶";
        Style = Theme.HeadingStyle;
    }
}
