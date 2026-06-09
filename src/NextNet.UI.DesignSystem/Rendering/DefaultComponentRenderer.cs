using Microsoft.AspNetCore.Html;
using NextNet.Components;
using NextNet.UI.Abstractions.Components;
using NextNet.UI.Abstractions.Rendering;
using NextNet.UI.DesignSystem.Components;

namespace NextNet.UI.DesignSystem.Rendering;

/// <summary>
/// Default implementation of <see cref="IComponentRenderer{T}"/> that dispatches
/// rendering to the component's <c>Render(RenderContext)</c> method via the
/// <see cref="IRenderableComponent"/> interface.
/// </summary>
/// <typeparam name="T">The type of component to render. Must implement <see cref="IRenderableComponent"/>.</typeparam>
/// <remarks>
/// <para>
/// <see cref="DefaultComponentRenderer{T}"/> calls <see cref="IRenderableComponent.Render"/>
/// directly on the component instance, avoiding the performance and safety issues
/// of C# <c>dynamic</c> dispatch.
/// </para>
/// <para>
/// If the component does not implement <see cref="IRenderableComponent"/>, the
/// renderer throws an <see cref="InvalidOperationException"/> with error code
/// <c>DS-150</c>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Register in DI:
/// services.AddSingleton&lt;IComponentRenderer&lt;IButton&gt;, DefaultComponentRenderer&lt;IButton&gt;&gt;();
///
/// // Usage:
/// var renderer = serviceProvider.GetRequiredService&lt;IComponentRenderer&lt;IButton&gt;&gt;();
/// var result = renderer.Render(button, context);
/// </code>
/// </example>
public class DefaultComponentRenderer<T> : IComponentRenderer<T>
    where T : IComponent
{
    /// <summary>
    /// Renders the specified component by calling <see cref="IRenderableComponent.Render"/>
    /// when the component implements that interface.
    /// </summary>
    /// <param name="component">The component instance to render. Must not be null.</param>
    /// <param name="context">The rendering context providing tokens and services.</param>
    /// <returns>A <see cref="ComponentRenderResult"/> containing the rendered HTML.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="component"/> or <paramref name="context"/> is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the component does not implement <see cref="IRenderableComponent"/>. (Error DS-150)
    /// </exception>
    public ComponentRenderResult Render(T component, RenderContext context)
    {
        ArgumentNullException.ThrowIfNull(component);
        ArgumentNullException.ThrowIfNull(context);

        if (component is IRenderableComponent renderable)
        {
            var html = renderable.Render(context);
            return new ComponentRenderResult(new HtmlString(html.ToHtml()));
        }

        throw new InvalidOperationException(
            $"DS-150: The component of type '{typeof(T).FullName}' does not implement " +
            $"'{nameof(IRenderableComponent)}' and cannot be rendered by " +
            $"{nameof(DefaultComponentRenderer<T>)}_{typeof(T).Name}.");
    }
}
