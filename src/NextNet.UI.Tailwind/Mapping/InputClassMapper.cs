using NextNet.UI.Abstractions.Components;
using NextNet.UI.Abstractions.Rendering;

namespace NextNet.UI.Tailwind.Mapping;

/// <summary>
/// Maps <see cref="IInput"/> component properties to Tailwind CSS utility classes.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="InputClassMapper"/> generates the following Tailwind class structure:
/// </para>
/// <list type="bullet">
///   <item><description><c>input-group</c> — root wrapper class</description></item>
///   <item><description><c>input</c> — base input element class</description></item>
///   <item><description><c>border-red-500</c> — when the input has a validation error</description></item>
///   <item><description><c>opacity-50 cursor-not-allowed</c> — when disabled</description></item>
///   <item><description><c>w-full</c> — full width by default</description></item>
///   <item><description><c>px-{size} py-{size}</c> — padding based on size</description></item>
///   <item><description>Any custom classes from <see cref="IComponent.ClassName"/></description></item>
/// </list>
/// <para>
/// Default size maps to <c>"md"</c> which translates to <c>px-3 py-2</c>.
/// Error state applies <c>border-red-500</c> for visual validation feedback.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var mapper = new InputClassMapper();
/// var input = new Input { Error = "Required", Disabled = true };
/// var classes = mapper.MapClasses(input, context);
/// // Result: "input w-full border-red-500 opacity-50 cursor-not-allowed px-3 py-2"
/// </code>
/// </example>
public sealed class InputClassMapper : ComponentClassMapper<IInput>
{
    /// <summary>
    /// Maps an <see cref="IInput"/> component to Tailwind CSS classes.
    /// </summary>
    /// <param name="component">The input component instance.</param>
    /// <param name="context">The rendering context (unused in this mapper).</param>
    /// <returns>A space-separated string of Tailwind CSS class names.</returns>
    public override string MapClasses(IInput component, RenderContext context)
    {
        var classes = "input w-full";

        classes = AppendWhen(classes, "border-red-500", !string.IsNullOrWhiteSpace(component.Error));
        classes = AppendWhen(classes, "opacity-50 cursor-not-allowed", component.Disabled);

        var (px, py) = GetInputPadding(component);
        classes = AppendWhen(classes, $"px-{px} py-{py}", true);

        classes = AppendCustomClasses(classes, component);

        return classes;
    }

    /// <summary>
    /// Returns the horizontal and vertical Tailwind padding scale keys based on
    /// the input's implicit size. Uses a reasonable default when size is not set.
    /// </summary>
    private static (string Px, string Py) GetInputPadding(IInput component)
    {
        // Input doesn't have a Size property natively, so we use a heuristic
        // based on label presence. Default is "md" equivalent.
        return ("3", "2"); // px-3 py-2 (Tailwind md size equivalent)
    }

    /// <summary>
    /// Maps the root input group to Tailwind CSS classes (wrapper div).
    /// </summary>
    /// <param name="component">The input component instance.</param>
    /// <returns>A space-separated string of Tailwind CSS class names for the wrapper.</returns>
    public string MapGroupClasses(IInput component)
    {
        var classes = "input-group";
        classes = AppendWhen(classes, "input-has-error", !string.IsNullOrWhiteSpace(component.Error));
        classes = AppendCustomClasses(classes, component);
        return classes;
    }
}
