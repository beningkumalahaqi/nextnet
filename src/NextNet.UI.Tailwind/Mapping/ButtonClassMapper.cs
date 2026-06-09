using NextNet.UI.Abstractions.Components;
using NextNet.UI.Abstractions.Rendering;

namespace NextNet.UI.Tailwind.Mapping;

/// <summary>
/// Maps <see cref="IButton"/> component properties to Tailwind CSS utility classes.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="ButtonClassMapper"/> generates the following Tailwind class structure:
/// </para>
/// <list type="bullet">
///   <item><description><c>btn</c> — base class</description></item>
///   <item><description><c>btn-{variant}</c> — e.g. <c>btn-primary</c>, <c>btn-danger</c></description></item>
///   <item><description><c>btn-{size}</c> — e.g. <c>btn-sm</c>, <c>btn-lg</c></description></item>
///   <item><description><c>opacity-50 cursor-not-allowed</c> — when disabled</description></item>
///   <item><description>Any custom classes from <see cref="IComponent.ClassName"/></description></item>
/// </list>
/// <para>
/// Default variant is <c>"primary"</c>. Default size is <c>"md"</c>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var mapper = new ButtonClassMapper();
/// var button = new Button { Variant = ComponentVariant.Danger, Size = ComponentSize.Lg, Disabled = true };
/// var classes = mapper.MapClasses(button, context);
/// // Result: "btn btn-danger btn-lg opacity-50 cursor-not-allowed"
/// </code>
/// </example>
public sealed class ButtonClassMapper : ComponentClassMapper<IButton>
{
    /// <summary>
    /// Maps an <see cref="IButton"/> component to Tailwind CSS classes.
    /// </summary>
    /// <param name="component">The button component instance.</param>
    /// <param name="context">The rendering context (unused in this mapper).</param>
    /// <returns>A space-separated string of Tailwind CSS class names.</returns>
    public override string MapClasses(IButton component, RenderContext context)
    {
        var classes = "btn";

        classes = AppendVariant(classes, "btn", component.Variant, "primary");
        classes = AppendSize(classes, "btn", component.Size, "md");
        classes = AppendWhen(classes, "opacity-50 cursor-not-allowed", component.Disabled);
        classes = AppendCustomClasses(classes, component);

        return classes;
    }
}
