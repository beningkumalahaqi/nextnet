using NextNet.Routing;

namespace NextNet.Routing.Models;

/// <summary>
/// Represents a single node in the route tree, which mirrors the directory
/// structure of the <c>app/</c> directory and holds references to route entries.
/// </summary>
/// <param name="Segment">The segment name for this node (e.g. <c>"blog"</c>, <c>"{slug}"</c>).</param>
/// <param name="RoutePattern">The full route pattern at this node, if one is defined.</param>
/// <param name="Entry">The route entry associated with this node, if any.</param>
/// <param name="Parent">The parent node, or <c>null</c> if this is the root.</param>
/// <param name="Children">The child nodes of this tree node.</param>
public sealed record RouteTreeNode(string Segment, string? RoutePattern, RouteEntry? Entry, RouteTreeNode? Parent, IReadOnlyList<RouteTreeNode> Children)
{
    /// <inheritdoc />
    public override string ToString()
        => $"{Segment} ({RoutePattern ?? "(no route)"})";
}
