using NextNet.Components;
using NextNet.UI.Abstractions.Components;
using NextNet.UI.Abstractions.Rendering;

namespace NextNet.UI.DesignSystem.Components;

/// <summary>
/// Standard implementation of <see cref="IBadge"/> that renders a small label or
/// status indicator with semantic coloring.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="Badge"/> renders a <c>&lt;span&gt;</c> element with the following
/// CSS classes:
/// </para>
/// <list type="bullet">
///   <item><description><c>badge</c> — base class</description></item>
///   <item><description><c>badge-{variant}</c> — e.g. <c>badge-primary</c>, <c>badge-success</c></description></item>
///   <item><description><c>badge-{size}</c> — e.g. <c>badge-sm</c>, <c>badge-lg</c></description></item>
///   <item><description><c>badge-dot</c> — when <see cref="IBadge.Dot"/> is <c>true</c></description></item>
/// </list>
/// <para>
/// When <see cref="IBadge.Dot"/> is <c>true</c>, only the dot is rendered without label text.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var badge = new Badge
/// {
///     Label = "New",
///     Variant = ComponentVariant.Success,
///     Size = ComponentSize.Sm
/// };
/// var html = badge.Render(context);
/// // Produces: &lt;span class="badge badge-success badge-sm"&gt;New&lt;/span&gt;
/// </code>
/// </example>
public sealed class Badge : IBadge, IRenderableComponent
{
    /// <summary>
    /// Gets or sets the semantic variant that determines the badge's color.
    /// Defaults to <see cref="ComponentVariant.Primary"/>.
    /// </summary>
    public ComponentVariant? Variant { get; init; } = ComponentVariant.Primary;

    /// <summary>
    /// Gets or sets the size of the badge.
    /// Defaults to <see cref="ComponentSize.Sm"/>.
    /// </summary>
    public ComponentSize? Size { get; init; } = ComponentSize.Sm;

    /// <summary>
    /// Gets or sets a value indicating whether the badge renders as a dot indicator.
    /// </summary>
    public bool Dot { get; init; }

    /// <summary>
    /// Gets or sets the text label displayed inside the badge.
    /// </summary>
    public string? Label { get; init; }

    /// <summary>
    /// Gets or sets the CSS class name(s) applied to the badge's root element.
    /// </summary>
    public string? ClassName { get; init; }

    /// <summary>
    /// Gets or sets the inline CSS style string applied to the badge's root element.
    /// </summary>
    public string? Style { get; init; }

    /// <summary>
    /// Gets or sets the unique identifier for this badge instance.
    /// </summary>
    public string? Id { get; init; }

    /// <summary>
    /// Gets the read-only list of child components. Badges typically have no children.
    /// </summary>
    public IReadOnlyList<IComponent> Children { get; init; } = Array.Empty<IComponent>();

    /// <summary>
    /// Renders this badge component as HTML using the specified rendering context.
    /// </summary>
    /// <param name="context">The rendering context providing tokens and services.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the rendered badge.</returns>
    public IHtmlContent Render(RenderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var variantName = Variant?.ToString().ToLowerInvariant() ?? "primary";
        var sizeName = Size?.ToString().ToLowerInvariant() ?? "sm";

        var badgeClass = $"badge badge-{variantName} badge-{sizeName}";
        if (Dot) badgeClass += " badge-dot";
        if (!string.IsNullOrEmpty(ClassName)) badgeClass += $" {ClassName}";

        var attrs = new Dictionary<string, string> { ["class"] = badgeClass };
        if (!string.IsNullOrEmpty(Id)) attrs["id"] = Id;
        if (!string.IsNullOrEmpty(Style)) attrs["style"] = Style;

        var content = Dot ? null : HtmlHelper.Text(Label ?? "");
        return HtmlHelper.Element("span", attrs, content);
    }
}
