using NextNet.Components;
using NextNet.UI.Abstractions.Components;
using NextNet.UI.Abstractions.Rendering;

namespace NextNet.UI.DesignSystem.Components;

/// <summary>
/// Standard implementation of <see cref="IAvatar"/> that renders a user or entity avatar
/// with image, fallback text, and configurable size and shape.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="Avatar"/> renders a <c>&lt;div&gt;</c> element containing an
/// <c>&lt;img&gt;</c> element and a fallback <c>&lt;span&gt;</c>. The following
/// CSS classes are applied:
/// </para>
/// <list type="bullet">
///   <item><description><c>avatar</c> — base class</description></item>
///   <item><description><c>avatar-{size}</c> — e.g. <c>avatar-sm</c>, <c>avatar-lg</c></description></item>
///   <item><description><c>avatar-{shape}</c> — e.g. <c>avatar-circle</c> (default), <c>avatar-square</c></description></item>
///   <item><description><c>avatar-fallback</c> — fallback text element</description></item>
/// </list>
/// <para>
/// When <see cref="IAvatar.Src"/> is provided, an <c>&lt;img&gt;</c> tag is rendered.
/// The <see cref="IAvatar.Fallback"/> text is rendered alongside the image and displayed
/// when the image fails to load.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var avatar = new Avatar
/// {
///     Src = "/images/user.jpg",
///     Alt = "John Doe",
///     Size = ComponentSize.Lg,
///     Shape = "circle",
///     Fallback = "JD"
/// };
/// var html = avatar.Render(context);
/// </code>
/// </example>
public sealed class Avatar : IAvatar, IRenderableComponent
{
    /// <summary>
    /// Gets or sets the URL or path to the avatar image.
    /// </summary>
    public string? Src { get; init; }

    /// <summary>
    /// Gets or sets the alternative text for the avatar image.
    /// </summary>
    public string? Alt { get; init; }

    /// <summary>
    /// Gets or sets the size of the avatar.
    /// Defaults to <see cref="ComponentSize.Md"/>.
    /// </summary>
    public ComponentSize? Size { get; init; } = ComponentSize.Md;

    /// <summary>
    /// Gets or sets the shape of the avatar.
    /// Defaults to <c>"circle"</c>.
    /// </summary>
    public string? Shape { get; init; } = "circle";

    /// <summary>
    /// Gets or sets the fallback text displayed when the avatar image cannot be loaded.
    /// </summary>
    public string? Fallback { get; init; }

    /// <summary>
    /// Gets or sets the CSS class name(s) applied to the avatar's root element.
    /// </summary>
    public string? ClassName { get; init; }

    /// <summary>
    /// Gets or sets the inline CSS style string applied to the avatar's root element.
    /// </summary>
    public string? Style { get; init; }

    /// <summary>
    /// Gets or sets the unique identifier for this avatar instance.
    /// </summary>
    public string? Id { get; init; }

    /// <summary>
    /// Gets the read-only list of child components. Avatars typically have no children.
    /// </summary>
    public IReadOnlyList<IComponent> Children { get; init; } = Array.Empty<IComponent>();

    /// <summary>
    /// Renders this avatar component as HTML using the specified rendering context.
    /// </summary>
    /// <param name="context">The rendering context providing tokens and services.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the rendered avatar.</returns>
    public IHtmlContent Render(RenderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var sizeName = Size?.ToString().ToLowerInvariant() ?? "md";
        var shapeName = !string.IsNullOrEmpty(Shape) ? Shape.ToLowerInvariant() : "circle";

        var avatarClass = $"avatar avatar-{sizeName} avatar-{shapeName}";
        if (!string.IsNullOrEmpty(ClassName)) avatarClass += $" {ClassName}";

        var attrs = new Dictionary<string, string> { ["class"] = avatarClass };
        if (!string.IsNullOrEmpty(Id)) attrs["id"] = Id;
        if (!string.IsNullOrEmpty(Style)) attrs["style"] = Style;

        var children = new List<IHtmlContent>();

        if (!string.IsNullOrEmpty(Src))
        {
            var imgAttrs = new Dictionary<string, string>
            {
                ["class"] = "avatar-image",
                ["src"] = Src
            };
            if (!string.IsNullOrEmpty(Alt)) imgAttrs["alt"] = Alt;
            children.Add(HtmlHelper.Element("img", imgAttrs));
        }

        if (!string.IsNullOrEmpty(Fallback))
        {
            children.Add(HtmlHelper.Element(
                "span",
                new Dictionary<string, string> { ["class"] = "avatar-fallback" },
                HtmlHelper.Text(Fallback)));
        }

        var content = children.Count > 0 ? HtmlHelper.Fragment(children.ToArray()) : null;
        return HtmlHelper.Element("div", attrs, content);
    }
}
