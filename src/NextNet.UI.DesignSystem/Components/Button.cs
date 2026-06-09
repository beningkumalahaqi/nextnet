using NextNet.Components;
using NextNet.UI.Abstractions.Components;
using NextNet.UI.Abstractions.Rendering;

namespace NextNet.UI.DesignSystem.Components;

/// <summary>
/// Standard implementation of <see cref="IButton"/> that renders a <c>&lt;button&gt;</c> element
/// with CSS classes for variant, size, and state.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="Button"/> renders a semantic HTML button element with configurable
/// <see cref="IButton.Variant"/> and <see cref="IButton.Size"/>. The following CSS
/// classes are applied:
/// </para>
/// <list type="bullet">
///   <item><description><c>btn</c> — base class</description></item>
///   <item><description><c>btn-{variant}</c> — e.g. <c>btn-primary</c>, <c>btn-danger</c></description></item>
///   <item><description><c>btn-{size}</c> — e.g. <c>btn-sm</c>, <c>btn-lg</c></description></item>
///   <item><description><c>btn-disabled</c> — when <see cref="IButton.Disabled"/> is <c>true</c></description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// var button = new Button
/// {
///     Label = "Submit",
///     Variant = ComponentVariant.Primary,
///     Size = ComponentSize.Lg,
///     Disabled = false
/// };
/// var html = button.Render(context);
/// // Produces: &lt;button class="btn btn-primary btn-lg"&gt;Submit&lt;/button&gt;
/// </code>
/// </example>
public sealed class Button : IButton, IRenderableComponent
{
    /// <summary>
    /// Gets or sets the semantic variant that determines the button's visual style.
    /// Defaults to <see cref="ComponentVariant.Primary"/>.
    /// </summary>
    public ComponentVariant? Variant { get; init; } = ComponentVariant.Primary;

    /// <summary>
    /// Gets or sets the size of the button.
    /// Defaults to <see cref="ComponentSize.Md"/>.
    /// </summary>
    public ComponentSize? Size { get; init; } = ComponentSize.Md;

    /// <summary>
    /// Gets or sets a value indicating whether the button is disabled.
    /// </summary>
    public bool Disabled { get; init; }

    /// <summary>
    /// Gets or sets the delegate invoked when the button is clicked.
    /// </summary>
    public Func<Task>? OnClick { get; init; }

    /// <summary>
    /// Gets or sets the text label displayed on the button.
    /// </summary>
    public string? Label { get; init; }

    /// <summary>
    /// Gets or sets the CSS class name(s) applied to the button's root element.
    /// </summary>
    public string? ClassName { get; init; }

    /// <summary>
    /// Gets or sets the inline CSS style string applied to the button's root element.
    /// </summary>
    public string? Style { get; init; }

    /// <summary>
    /// Gets or sets the unique identifier for this button instance.
    /// </summary>
    public string? Id { get; init; }

    /// <summary>
    /// Gets the read-only list of child components. Buttons typically have no children.
    /// </summary>
    public IReadOnlyList<IComponent> Children { get; init; } = Array.Empty<IComponent>();

    /// <summary>
    /// Renders this button component as HTML using the specified rendering context.
    /// </summary>
    /// <param name="context">The rendering context providing tokens and services.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the rendered button.</returns>
    public IHtmlContent Render(RenderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var variantName = Variant?.ToString().ToLowerInvariant() ?? "primary";
        var sizeName = Size?.ToString().ToLowerInvariant() ?? "md";

        var btnClass = $"btn btn-{variantName} btn-{sizeName}";
        if (Disabled) btnClass += " btn-disabled";
        if (!string.IsNullOrEmpty(ClassName)) btnClass += $" {ClassName}";

        var attrs = new Dictionary<string, string> { ["class"] = btnClass };
        if (!string.IsNullOrEmpty(Id)) attrs["id"] = Id;
        if (!string.IsNullOrEmpty(Style)) attrs["style"] = Style;
        if (Disabled) attrs["disabled"] = "disabled";

        return HtmlHelper.Element("button", attrs, HtmlHelper.Text(Label ?? ""));
    }
}
