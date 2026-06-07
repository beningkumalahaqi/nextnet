using NextNet.Routing;

namespace NextNet.Routing.Models;

/// <summary>
/// Represents a single node in the route tree, which mirrors the directory
/// structure of the <c>app/</c> directory and holds references to route entries.
/// </summary>
public class RouteTreeNode
{
    /// <summary>
    /// Gets the segment name for this node (e.g. <c>"blog"</c>, <c>"{slug}"</c>).
    /// </summary>
    public string Segment { get; }

    /// <summary>
    /// Gets the full route pattern at this node, if one is defined.
    /// </summary>
    public string? RoutePattern { get; }

    /// <summary>
    /// Gets the route entry associated with this node, if any.
    /// </summary>
    public RouteEntry? Entry { get; }

    /// <summary>
    /// Gets the parent node, or <c>null</c> if this is the root.
    /// </summary>
    public RouteTreeNode? Parent { get; }

    /// <summary>
    /// Gets the child nodes of this tree node.
    /// </summary>
    public IReadOnlyList<RouteTreeNode> Children { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="RouteTreeNode"/>.
    /// </summary>
    public RouteTreeNode(
        string segment,
        string? routePattern,
        RouteEntry? entry,
        RouteTreeNode? parent,
        IReadOnlyList<RouteTreeNode> children)
    {
        Segment = segment;
        RoutePattern = routePattern;
        Entry = entry;
        Parent = parent;
        Children = children;
    }

    /// <inheritdoc />
    public override string ToString()
        => $"{Segment} ({RoutePattern ?? "(no route)"})";
}
