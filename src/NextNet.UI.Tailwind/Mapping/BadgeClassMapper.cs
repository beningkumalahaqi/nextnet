using NextNet.UI.Abstractions.Components;
using NextNet.UI.Abstractions.Rendering;

namespace NextNet.UI.Tailwind.Mapping;

/// <summary>
/// Maps <see cref="IBadge"/> component properties to Tailwind CSS utility classes.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="BadgeClassMapper"/> generates the following Tailwind class structure:
/// </para>
/// <list type="bullet">
///   <item><description><c>badge</c> — base class</description></item>
///   <item><description><c>inline-flex items-center rounded-full</c> — layout and shape</description></item>
///   <item><description><c>badge-{variant}</c> — e.g. <c>badge-primary</c>, <c>badge-success</c></description></item>
///   <item><description><c>badge-{size}</c> — e.g. <c>badge-sm</c>, <c>badge-lg</c></description></item>
///   <item><description><c>w-2 h-2</c> — dot indicator dimensions (when <see cref="IBadge.Dot"/> is true)</description></item>
///   <item><description><c>rounded-full</c> — dot shape (when dot is enabled)</description></item>
///   <item><description>Any custom classes from <see cref="IComponent.ClassName"/></description></item>
/// </list>
/// <para>
/// Default variant is <c>"primary"</c>. Default size is <c>"sm"</c>.
/// When <see cref="IBadge.Dot"/> is <c>true</c>, the badge renders as a small colored dot
/// with <c>w-2 h-2 rounded-full</c> classes.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var mapper = new BadgeClassMapper();
/// var badge = new Badge { Variant = ComponentVariant.Success, Dot = true };
/// var classes = mapper.MapClasses(badge, context);
/// // Result: "badge inline-flex items-center rounded-full badge-success badge-sm w-2 h-2 rounded-full"
/// </code>
/// </example>
public sealed class BadgeClassMapper : ComponentClassMapper<IBadge>
{
    /// <summary>
    /// Maps an <see cref="IBadge"/> component to Tailwind CSS classes.
    /// </summary>
    /// <param name="component">The badge component instance.</param>
    /// <param name="context">The rendering context (unused in this mapper).</param>
    /// <returns>A space-separated string of Tailwind CSS class names.</returns>
    public override string MapClasses(IBadge component, RenderContext context)
    {
        var classes = "badge inline-flex items-center rounded-full";

        classes = AppendVariant(classes, "badge", component.Variant, "primary");
        classes = AppendSize(classes, "badge", component.Size, "sm");
        classes = AppendWhen(classes, "w-2 h-2", component.Dot);
        classes = AppendCustomClasses(classes, component);

        return classes;
    }
}
