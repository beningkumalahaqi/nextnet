using NextNet.Components;
using NextNet.UI.Abstractions.Rendering;

namespace NextNet.UI.DesignSystem.Components;

/// <summary>
/// Defines the contract for a component that can render itself to HTML
/// given a <see cref="RenderContext"/>.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="IRenderableComponent"/> extends <see cref="UI.Abstractions.Components.IComponent"/>
/// with a <see cref="Render"/> method that produces <see cref="IHtmlContent"/>. This
/// enables typed rendering without dynamic dispatch.
/// </para>
/// <para>
/// All concrete component implementations in the design system
/// (e.g., <see cref="Button"/>, <see cref="Card"/>, <see cref="Modal"/>)
/// implement this interface.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class MyComponent : IRenderableComponent
/// {
///     public string? ClassName { get; init; }
///     public string? Style { get; init; }
///     public string? Id { get; init; }
///     public IReadOnlyList&lt;IComponent&gt; Children { get; init; } = Array.Empty&lt;IComponent&gt;();
///
///     public IHtmlContent Render(RenderContext context)
///     {
///         return HtmlHelper.Element("div", null, HtmlHelper.Text("Hello"));
///     }
/// }
/// </code>
/// </example>
public interface IRenderableComponent : UI.Abstractions.Components.IComponent
{
    /// <summary>
    /// Renders this component to HTML using the specified rendering context.
    /// </summary>
    /// <param name="context">The rendering context providing tokens and services.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the rendered HTML.</returns>
    IHtmlContent Render(RenderContext context);
}
