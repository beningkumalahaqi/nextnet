using NextNet.UI.Abstractions.Components;

namespace NextNet.UI.Abstractions.Rendering;

/// <summary>
/// Defines the contract for rendering a UI component of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of component to render. Must implement <see cref="IComponent"/>.</typeparam>
/// <remarks>
/// <para>
/// <see cref="IComponentRenderer{T}"/> is a generic interface that allows
/// type-specific rendering logic for each component type. Implementations
/// interpret the component's properties and produce a <see cref="ComponentRenderResult"/>
/// containing the rendered output and any rendering warnings.
/// </para>
/// <para>
/// Renderers are registered in the dependency injection container and resolved
/// by the rendering engine. Each component type typically has its own renderer
/// implementation.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class ButtonRenderer : IComponentRenderer&lt;IButton&gt;
/// {
///     public ComponentRenderResult Render(IButton component, RenderContext context)
///     {
///         var html = new HtmlString($"&lt;button class=\"{component.ClassName}\"&gt;{component.Label}&lt;/button&gt;");
///         return new ComponentRenderResult(html);
///     }
/// }
/// </code>
/// </example>
public interface IComponentRenderer<T>
    where T : IComponent
{
    /// <summary>
    /// Renders the specified component using the given rendering context.
    /// </summary>
    /// <param name="component">The component instance to render. Must not be null.</param>
    /// <param name="context">The rendering context providing tokens, services, and theme information.</param>
    /// <returns>A <see cref="ComponentRenderResult"/> containing the rendered HTML and any warnings.</returns>
    ComponentRenderResult Render(T component, RenderContext context);
}
