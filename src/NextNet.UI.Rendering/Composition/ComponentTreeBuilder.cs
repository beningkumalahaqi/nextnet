using NextNet.UI.Abstractions.Components;

namespace NextNet.UI.Rendering.Composition;

/// <summary>
/// Provides a fluent API for constructing a <see cref="ComponentNode"/> tree
/// representing the UI component hierarchy of a page.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="ComponentTreeBuilder"/> enables declarative construction of component
/// trees. Trees are built from a set of flat <see cref="IComponent"/> instances and
/// parent-child relationships established via the <see cref="AddChild"/> method.
/// </para>
/// <para>
/// Builder instances must not be reused after calling <see cref="Build"/>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var tree = new ComponentTreeBuilder()
///     .Add(new Button { Label = "Submit" })
///     .Add(new Card { Title = "Profile" })
///     .AddChild(0, 1) // Button is child of Card
///     .Build();
/// </code>
/// </example>
public sealed class ComponentTreeBuilder
{
    private readonly List<ComponentNode> _roots = new();
    private readonly Dictionary<int, List<int>> _adjacency = new();
    private readonly List<IComponent> _components = new();
    private int _nextId;

    /// <summary>
    /// Adds a root-level <see cref="IComponent"/> to the tree and returns this builder
    /// for chaining.
    /// </summary>
    /// <param name="component">The component to add. Must not be null.</param>
    /// <returns>This builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="component"/> is null.</exception>
    public ComponentTreeBuilder Add(IComponent component)
    {
        if (component == null) throw new ArgumentNullException(nameof(component));

        var id = _nextId++;
        _components.Add(component);
        _roots.Add(new ComponentNode(component));
        return this;
    }

    /// <summary>
    /// Adds a child relationship where the component at <paramref name="parentIndex"/>
    /// (0-based, in order of <see cref="Add"/>) contains the component at
    /// <paramref name="childIndex"/>.
    /// </summary>
    /// <param name="parentIndex">The 0-based index of the parent component in the order added.</param>
    /// <param name="childIndex">The 0-based index of the child component in the order added.</param>
    /// <returns>This builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when either index is out of range or equal.</exception>
    public ComponentTreeBuilder AddChild(int parentIndex, int childIndex)
    {
        if (parentIndex < 0 || parentIndex >= _components.Count)
            throw new ArgumentOutOfRangeException(nameof(parentIndex));
        if (childIndex < 0 || childIndex >= _components.Count)
            throw new ArgumentOutOfRangeException(nameof(childIndex));
        if (parentIndex == childIndex)
            throw new ArgumentException("A component cannot be a child of itself.", nameof(childIndex));

        // Remove the child from roots if it was added as root
        var childNode = new ComponentNode(_components[childIndex]);
        _roots.RemoveAll(n => ReferenceEquals(n.Component, _components[childIndex]));

        // Track the adjacency
        if (!_adjacency.TryGetValue(parentIndex, out var children))
        {
            children = new List<int>();
            _adjacency[parentIndex] = children;
        }
        children.Add(childIndex);

        return this;
    }

    /// <summary>
    /// Builds and returns the root <see cref="ComponentNode"/> instances of the component tree.
    /// </summary>
    /// <returns>A read-only list of root-level <see cref="ComponentNode"/> objects.</returns>
    /// <remarks>
    /// After calling <see cref="Build"/>, do not reuse this builder instance.
    /// </remarks>
    public IReadOnlyList<ComponentNode> Build()
    {
        // Resolve adjacency into actual ComponentNode references
        BuildChildren(0, _components.Count - 1);
        return _roots.AsReadOnly();
    }

    private ComponentNode BuildChildren(int startIndex, int endIndex)
    {
        // This method recursively builds the tree; for simplicity we return
        // a resolved root by looking up adjacency. The tree is built by
        // processing nodes in index order.
        var result = new List<ComponentNode>();

        for (int i = startIndex; i <= endIndex; i++)
        {
            if (_roots.Any(n => ReferenceEquals(n.Component, _components[i])))
            {
                var children = new List<ComponentNode>();
                if (_adjacency.TryGetValue(i, out var childIndices))
                {
                    foreach (var ci in childIndices)
                    {
                        children.Add(new ComponentNode(_components[ci]));
                    }
                }
                result.Add(new ComponentNode(_components[i], children.AsReadOnly()));
            }
        }

        // Rebuild roots
        _roots.Clear();
        _roots.AddRange(result);

        // Return single root or first root if there's exactly one
        return _roots.Count == 1 ? _roots[0] : new ComponentNode(new NullComponent(), _roots.AsReadOnly());
    }

    /// <summary>
    /// Internal sentinel component used as a virtual root when multiple roots exist.
    /// </summary>
    private sealed class NullComponent : IComponent
    {
        public string? ClassName => null;
        public string? Style => null;
        public string? Id => null;
        public IReadOnlyList<IComponent> Children => Array.Empty<IComponent>();
    }
}
