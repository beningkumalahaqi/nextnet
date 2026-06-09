using NextNet.Components;
using NextNet.UI.Abstractions.Components;
using NextNet.UI.Abstractions.Rendering;

namespace NextNet.UI.DesignSystem.Components;

/// <summary>
/// Standard implementation of <see cref="ICard"/> that renders a structured card container
/// with header, body, and optional footer sections.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="Card"/> renders a <c>&lt;div&gt;</c> element with the following structure
/// and CSS classes:
/// </para>
/// <list type="bullet">
///   <item><description><c>card</c> — base class on the root element</description></item>
///   <item><description><c>card-padding-{size}</c> — padding variant</description></item>
///   <item><description><c>card-shadow-{level}</c> — shadow variant</description></item>
///   <item><description><c>card-header</c> — title section</description></item>
///   <item><description><c>card-body</c> — description section</description></item>
///   <item><description><c>card-footer</c> — footer section</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// var card = new Card
/// {
///     Title = "Welcome",
///     Description = "This is a sample card.",
///     Padding = ComponentSize.Lg,
///     Shadow = "md"
/// };
/// var html = card.Render(context);
/// </code>
/// </example>
public sealed class Card : ICard, IRenderableComponent
{
    /// <summary>
    /// Gets or sets the title text displayed in the card header.
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// Gets or sets the description text displayed in the card body below the title.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets or sets the footer content rendered at the bottom of the card.
    /// </summary>
    public IComponent? Footer { get; init; }

    /// <summary>
    /// Gets or sets the padding size applied inside the card.
    /// Defaults to <see cref="ComponentSize.Md"/>.
    /// </summary>
    public ComponentSize? Padding { get; init; } = ComponentSize.Md;

    /// <summary>
    /// Gets or sets the shadow elevation level for the card.
    /// </summary>
    public string? Shadow { get; init; }

    /// <summary>
    /// Gets or sets the CSS class name(s) applied to the card's root element.
    /// </summary>
    public string? ClassName { get; init; }

    /// <summary>
    /// Gets or sets the inline CSS style string applied to the card's root element.
    /// </summary>
    public string? Style { get; init; }

    /// <summary>
    /// Gets or sets the unique identifier for this card instance.
    /// </summary>
    public string? Id { get; init; }

    /// <summary>
    /// Gets the read-only list of child components. Cards render children within their body.
    /// </summary>
    public IReadOnlyList<IComponent> Children { get; init; } = Array.Empty<IComponent>();

    /// <summary>
    /// Renders this card component as HTML using the specified rendering context.
    /// </summary>
    /// <param name="context">The rendering context providing tokens and services.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the rendered card.</returns>
    public IHtmlContent Render(RenderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var paddingName = Padding?.ToString().ToLowerInvariant() ?? "md";
        var cardClass = $"card card-padding-{paddingName}";
        if (!string.IsNullOrEmpty(Shadow)) cardClass += $" card-shadow-{Shadow}";
        if (!string.IsNullOrEmpty(ClassName)) cardClass += $" {ClassName}";

        var attrs = new Dictionary<string, string> { ["class"] = cardClass };
        if (!string.IsNullOrEmpty(Id)) attrs["id"] = Id;
        if (!string.IsNullOrEmpty(Style)) attrs["style"] = Style;

        var sections = new List<IHtmlContent>();

        if (!string.IsNullOrEmpty(Title))
        {
            sections.Add(HtmlHelper.Element(
                "div",
                new Dictionary<string, string> { ["class"] = "card-header" },
                HtmlHelper.Text(Title)));
        }

        if (!string.IsNullOrEmpty(Description))
        {
            sections.Add(HtmlHelper.Element(
                "div",
                new Dictionary<string, string> { ["class"] = "card-body" },
                HtmlHelper.Text(Description)));
        }

        if (Footer != null)
        {
            var footerContent = Footer is IRenderableComponent renderable
                ? renderable.Render(context)
                : HtmlHelper.Text(Footer.ToString() ?? "");
            sections.Add(HtmlHelper.Element(
                "div",
                new Dictionary<string, string> { ["class"] = "card-footer" },
                footerContent));
        }

        return HtmlHelper.Element("div", attrs, HtmlHelper.Fragment(sections.ToArray()));
    }
}
