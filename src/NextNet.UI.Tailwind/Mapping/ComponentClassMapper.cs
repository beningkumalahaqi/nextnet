using NextNet.UI.Abstractions.Components;
using NextNet.UI.Abstractions.Rendering;

namespace NextNet.UI.Tailwind.Mapping;

/// <summary>
/// Abstract base class for component-to-Tailwind CSS class mappers.
/// Provides common helper methods for building class strings based on
/// variant, size, and state properties.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="ComponentClassMapper{T}"/> implements <see cref="IComponentClassMapper{T}"/>
/// and provides utility methods that derived mappers can use to consistently
/// format variant, size, and state-based class names.
/// </para>
/// </remarks>
/// <typeparam name="T">The component type to map. Must implement <see cref="IComponent"/>.</typeparam>
public abstract class ComponentClassMapper<T> : IComponentClassMapper<T>
    where T : IComponent
{
    /// <summary>
    /// Maps the specified component's properties to a set of Tailwind CSS class names.
    /// Must be implemented by derived classes.
    /// </summary>
    /// <param name="component">The component instance to map.</param>
    /// <param name="context">The rendering context providing tokens and services.</param>
    /// <returns>A space-separated string of Tailwind CSS class names.</returns>
    public abstract string MapClasses(T component, RenderContext context);

    /// <summary>
    /// Appends the variant class name (e.g., <c>btn-primary</c>, <c>badge-success</c>)
    /// to the class builder string if the component has a non-null variant.
    /// </summary>
    /// <param name="classes">The current class string builder.</param>
    /// <param name="prefix">The CSS class prefix (e.g., <c>"btn"</c>, <c>"badge"</c>).</param>
    /// <param name="variant">The component variant, or <c>null</c>.</param>
    /// <param name="defaultVariant">The default variant name to use when <paramref name="variant"/> is <c>null</c>.</param>
    /// <returns>The updated classes string.</returns>
    protected static string AppendVariant(string classes, string prefix, ComponentVariant? variant, string defaultVariant)
    {
        var variantName = variant?.ToString().ToLowerInvariant() ?? defaultVariant;
        return $"{classes} {prefix}-{variantName}";
    }

    /// <summary>
    /// Appends the size class name (e.g., <c>btn-sm</c>, <c>input-lg</c>)
    /// to the class builder string if the component has a non-null size.
    /// </summary>
    /// <param name="classes">The current class string builder.</param>
    /// <param name="prefix">The CSS class prefix (e.g., <c>"btn"</c>, <c>"input"</c>).</param>
    /// <param name="size">The component size, or <c>null</c>.</param>
    /// <param name="defaultSize">The default size name to use when <paramref name="size"/> is <c>null</c>.</param>
    /// <returns>The updated classes string.</returns>
    protected static string AppendSize(string classes, string prefix, ComponentSize? size, string defaultSize)
    {
        var sizeName = size?.ToString().ToLowerInvariant() ?? defaultSize;
        return $"{classes} {prefix}-{sizeName}";
    }

    /// <summary>
    /// Appends a state class (e.g., <c>disabled</c>, <c>btn-disabled</c>) to the
    /// class builder string when the condition is <c>true</c>.
    /// </summary>
    /// <param name="classes">The current class string builder.</param>
    /// <param name="className">The class name to append (e.g., <c>"opacity-50"</c>, <c>"cursor-not-allowed"</c>).</param>
    /// <param name="condition">The condition that determines whether the class is appended.</param>
    /// <returns>The updated classes string.</returns>
    protected static string AppendWhen(string classes, string className, bool condition)
    {
        return condition ? $"{classes} {className}" : classes;
    }

    /// <summary>
    /// Appends any additional custom class names from the component's <see cref="IComponent.ClassName"/>
    /// property to the class builder string.
    /// </summary>
    /// <param name="classes">The current class string builder.</param>
    /// <param name="component">The component instance.</param>
    /// <returns>The updated classes string.</returns>
    protected static string AppendCustomClasses(string classes, IComponent component)
    {
        return !string.IsNullOrWhiteSpace(component.ClassName)
            ? $"{classes} {component.ClassName}"
            : classes;
    }
}
