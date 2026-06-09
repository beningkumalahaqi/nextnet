using NextNet.UI.Abstractions.Components;
using NextNet.UI.Abstractions.Rendering;

namespace NextNet.UI.Tailwind.Mapping;

/// <summary>
/// Maps <see cref="ICard"/> component properties to Tailwind CSS utility classes.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="CardClassMapper"/> generates the following Tailwind class structure:
/// </para>
/// <list type="bullet">
///   <item><description><c>card</c> — base class</description></item>
///   <item><description><c>p-{size}</c> — padding size, where size maps to Tailwind padding scale keys</description></item>
///   <item><description><c>shadow-{level}</c> — shadow elevation level</description></item>
///   <item><description><c>rounded-lg</c> — consistent border radius</description></item>
///   <item><description><c>border</c> — default border</description></item>
///   <item><description>Any custom classes from <see cref="IComponent.ClassName"/></description></item>
/// </list>
/// <para>
/// Default padding is <c>"md"</c> which maps to <c>p-4</c>. Shadow is optional;
/// when null, no shadow class is applied.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var mapper = new CardClassMapper();
/// var card = new Card { Padding = ComponentSize.Lg, Shadow = "md" };
/// var classes = mapper.MapClasses(card, context);
/// // Result: "card p-6 shadow-md rounded-lg border"
/// </code>
/// </example>
public sealed class CardClassMapper : ComponentClassMapper<ICard>
{
    /// <summary>
    /// Maps an <see cref="ICard"/> component to Tailwind CSS classes.
    /// </summary>
    /// <param name="component">The card component instance.</param>
    /// <param name="context">The rendering context (unused in this mapper).</param>
    /// <returns>A space-separated string of Tailwind CSS class names.</returns>
    public override string MapClasses(ICard component, RenderContext context)
    {
        var classes = "card rounded-lg border";

        classes = AppendWhen(classes, $"p-{MapPaddingSize(component.Padding)}", true);
        classes = AppendWhen(classes, $"shadow-{component.Shadow}", !string.IsNullOrWhiteSpace(component.Shadow));
        classes = AppendCustomClasses(classes, component);

        return classes;
    }

    /// <summary>
    /// Maps a <see cref="ComponentSize"/> to a Tailwind padding scale key.
    /// </summary>
    private static string MapPaddingSize(ComponentSize? size)
    {
        return size?.ToString().ToLowerInvariant() switch
        {
            "sm" => "3",    // p-3 = 0.75rem
            "md" => "4",    // p-4 = 1rem
            "lg" => "6",    // p-6 = 1.5rem
            "xl" => "8",    // p-8 = 2rem
            _ => "4"        // default to p-4
        };
    }
}
