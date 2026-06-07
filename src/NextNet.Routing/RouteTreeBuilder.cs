using NextNet.Routing.Models;

namespace NextNet.Routing;

/// <summary>
/// Builds a tree structure from a <see cref="RouteManifest"/> where each node
/// corresponds to a directory segment and leaf nodes hold route entries.
/// </summary>
public class RouteTreeBuilder
{
    /// <summary>
    /// Builds a route tree from the given manifest.
    /// </summary>
    /// <param name="manifest">The route manifest to build a tree from.</param>
    /// <returns>The root <see cref="RouteTreeNode"/> of the tree.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="manifest"/> is null.</exception>
    public RouteTreeNode BuildTree(RouteManifest manifest)
    {
        ArgumentNullException.ThrowIfNull(manifest);

        var root = new RouteTreeNode(
            segment: "/",
            routePattern: "/",
            entry: manifest.Layouts.FirstOrDefault(l =>
                string.Equals(l.RoutePattern, "/", StringComparison.Ordinal)),
            parent: null,
            children: new List<RouteTreeNode>());

        foreach (var entry in manifest.Routes)
        {
            if (string.Equals(entry.RoutePattern, "/", StringComparison.Ordinal))
                continue; // Root is already handled

            InsertEntry(root, entry);
        }

        return root;
    }

    /// <summary>
    /// Inserts a route entry into the tree by splitting its pattern into segments.
    /// </summary>
    private static void InsertEntry(RouteTreeNode root, RouteEntry entry)
    {
        var segments = entry.RoutePattern
            .Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (segments.Length == 0)
            return;

        var current = root;
        var children = (List<RouteTreeNode>)current.Children;

        for (int i = 0; i < segments.Length; i++)
        {
            var segment = segments[i];
            var isLast = i == segments.Length - 1;
            var routePattern = "/" + string.Join("/", segments.Take(i + 1));

            // Find or create child node
            var child = children.FirstOrDefault(c =>
                string.Equals(c.Segment, segment, StringComparison.Ordinal));

            if (child == null)
            {
                child = new RouteTreeNode(
                    segment: segment,
                    routePattern: isLast ? routePattern : null,
                    entry: isLast ? entry : null,
                    parent: current,
                    children: new List<RouteTreeNode>());

                children.Add(child);
            }
            else if (isLast)
            {
                // Update the existing node with the entry (if it doesn't have one already)
                // Use reflection to create a new node since the properties are immutable
                child = new RouteTreeNode(
                    segment: child.Segment,
                    routePattern: routePattern,
                    entry: entry,
                    parent: current,
                    children: child.Children);

                // Replace the old node in the children list
                var index = children.FindIndex(c =>
                    string.Equals(c.Segment, segment, StringComparison.Ordinal));
                children[index] = child;
            }

            current = child;
            children = (List<RouteTreeNode>)current.Children;
        }
    }
}
