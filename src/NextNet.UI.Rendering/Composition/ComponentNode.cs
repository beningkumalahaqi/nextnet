using NextNet.UI.Abstractions.Components;

namespace NextNet.UI.Rendering.Composition;

/// <summary>
/// Represents a single node in a component tree, associating a <see cref="IComponent"/>
/// with its child nodes.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="ComponentNode"/> is the fundamental building block of the component tree
/// data structure used by <see cref="ComponentTreeBuilder"/> and
/// <see cref="ComponentTreeRenderer"/>. Each node holds a reference to a UI component
/// and an ordered list of its children.
/// </para>
/// <para>
/// Instances are immutable after construction. Use <see cref="ComponentTreeBuilder"/>
/// to construct a tree via the fluent API.
/// </para>
/// </remarks>
/// <param name="Component">The UI component instance at this node. Must not be null.</param>
/// <param name="Children">The read-only list of child nodes. Never null; defaults to an empty list.</param>
public sealed record ComponentNode(
    IComponent Component,
    IReadOnlyList<ComponentNode> Children)
{
    /// <summary>
    /// Initializes a new instance of <see cref="ComponentNode"/> with the specified
    /// component and no children.
    /// </summary>
    /// <param name="component">The UI component instance. Must not be null.</param>
    public ComponentNode(IComponent component)
        : this(component, Array.Empty<ComponentNode>())
    {
    }
}
