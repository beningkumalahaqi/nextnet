using NextNet.UI.Abstractions.Components;
using NextNet.UI.Abstractions.Rendering;

namespace NextNet.UI.Tailwind.Mapping;

/// <summary>
/// Defines the contract for mapping a UI component to a set of Tailwind CSS classes.
/// </summary>
/// <remarks>
/// <para>
/// Implementations of <see cref="IComponentClassMapper{T}"/> translate component
/// properties (variant, size, state) into Tailwind CSS utility class strings.
/// The mapper is invoked during rendering to produce the final class attribute
/// value for the component's root element.
/// </para>
/// <para>
/// Mappers are typically registered in the <see cref="ClassMapperRegistry"/> and
/// resolved by the component rendering pipeline. Each component type should have
/// its own mapper implementation.
/// </para>
/// </remarks>
/// <typeparam name="T">The component type to map. Must implement <see cref="IComponent"/>.</typeparam>
/// <example>
/// <code>
/// public class MyButtonMapper : IComponentClassMapper&lt;IButton&gt;
/// {
///     public string MapClasses(IButton component, RenderContext context)
///     {
///         return $"btn btn-{component.Variant?.ToString().ToLowerInvariant() ?? "primary"}";
///     }
/// }
/// </code>
/// </example>
public interface IComponentClassMapper<in T> where T : IComponent
{
    /// <summary>
    /// Maps the specified component's properties to a set of Tailwind CSS class names.
    /// </summary>
    /// <param name="component">The component instance to map.</param>
    /// <param name="context">The rendering context providing tokens and services.</param>
    /// <returns>A space-separated string of Tailwind CSS class names.</returns>
    string MapClasses(T component, RenderContext context);
}
