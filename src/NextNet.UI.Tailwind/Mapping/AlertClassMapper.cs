using NextNet.UI.Abstractions.Components;
using NextNet.UI.Abstractions.Rendering;

namespace NextNet.UI.Tailwind.Mapping;

/// <summary>
/// Maps <see cref="IAlert"/> component properties to Tailwind CSS utility classes.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="AlertClassMapper"/> generates the following Tailwind class structure:
/// </para>
/// <list type="bullet">
///   <item><description><c>alert</c> — base class</description></item>
///   <item><description><c>rounded-lg border p-4</c> — shape and spacing</description></item>
///   <item><description><c>alert-{variant}</c> — e.g. <c>alert-info</c>, <c>alert-danger</c></description></item>
///   <item><description><c>flex items-start gap-3</c> — layout (when dismissible or icon present)</description></item>
///   <item><description>Any custom classes from <see cref="IComponent.ClassName"/></description></item>
/// </list>
/// <para>
/// Default variant is <c>"info"</c>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var mapper = new AlertClassMapper();
/// var alert = new Alert { Variant = ComponentVariant.Warning, Dismissible = true };
/// var classes = mapper.MapClasses(alert, context);
/// // Result: "alert rounded-lg border p-4 alert-warning flex items-start gap-3"
/// </code>
/// </example>
public sealed class AlertClassMapper : ComponentClassMapper<IAlert>
{
    /// <summary>
    /// Maps an <see cref="IAlert"/> component to Tailwind CSS classes.
    /// </summary>
    /// <param name="component">The alert component instance.</param>
    /// <param name="context">The rendering context (unused in this mapper).</param>
    /// <returns>A space-separated string of Tailwind CSS class names.</returns>
    public override string MapClasses(IAlert component, RenderContext context)
    {
        var classes = "alert rounded-lg border p-4";

        classes = AppendVariant(classes, "alert", component.Variant, "info");
        classes = AppendWhen(classes, "flex items-start gap-3", component.Dismissible);
        classes = AppendCustomClasses(classes, component);

        return classes;
    }
}
