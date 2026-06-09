namespace NextNet.UI.Abstractions.Components;

/// <summary>
/// Defines the base contract that all UI components must implement.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="IComponent"/> is the root interface of the NextNet component hierarchy.
/// Every component provides a class name for CSS targeting, an optional inline style,
/// an optional unique identifier, and a list of child components.
/// </para>
/// <para>
/// Component implementations are expected to be renderer-agnostic. The rendering
/// layer (see <see cref="Rendering.IComponentRenderer{T}"/>) interprets these
/// properties to produce the appropriate output (HTML, JSON, etc.).
/// </para>
/// </remarks>
public interface IComponent
{
    /// <summary>
    /// Gets the CSS class name(s) applied to the component's root element.
    /// Multiple classes are separated by spaces, consistent with standard CSS conventions.
    /// </summary>
    string? ClassName { get; }

    /// <summary>
    /// Gets the inline CSS style string applied to the component's root element.
    /// The value follows standard CSS property:value; syntax.
    /// </summary>
    string? Style { get; }

    /// <summary>
    /// Gets the unique identifier for the component instance.
    /// This value maps to the HTML <c>id</c> attribute when rendering to HTML.
    /// </summary>
    string? Id { get; }

    /// <summary>
    /// Gets the read-only list of child components nested within this component.
    /// Returns an empty list when there are no children. Never returns <c>null</c>.
    /// </summary>
    IReadOnlyList<IComponent> Children { get; }
}
