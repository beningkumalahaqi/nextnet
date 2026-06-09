using NextNet.Components;
using NextNet.UI.Abstractions.Components;
using NextNet.UI.Abstractions.Rendering;

namespace NextNet.UI.DesignSystem.Components;

/// <summary>
/// Standard implementation of <see cref="IInput"/> that renders a labeled input field
/// with validation error support.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="Input"/> renders a <c>&lt;div class="input-group"&gt;</c> containing
/// an optional <c>&lt;label&gt;</c>, the <c>&lt;input&gt;</c> element, and an
/// optional error message. The following CSS classes are applied:
/// </para>
/// <list type="bullet">
///   <item><description><c>input-group</c> — root wrapper</description></item>
///   <item><description><c>input-group-label</c> — label element</description></item>
///   <item><description><c>input</c> — input element</description></item>
///   <item><description><c>input-error</c> — error message element</description></item>
///   <item><description><c>input-has-error</c> — root class when validation error exists</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// var input = new Input
/// {
///     Type = "email",
///     Label = "Email",
///     Placeholder = "you@example.com",
///     Required = true
/// };
/// var html = input.Render(context);
/// </code>
/// </example>
public sealed class Input : IInput, IRenderableComponent
{
    /// <summary>
    /// Gets or sets the HTML input type (e.g., "text", "email", "password", "number").
    /// Defaults to "text".
    /// </summary>
    public string? Type { get; init; } = "text";

    /// <summary>
    /// Gets or sets the placeholder text displayed when the input is empty.
    /// </summary>
    public string? Placeholder { get; init; }

    /// <summary>
    /// Gets or sets the current value of the input.
    /// </summary>
    public string? Value { get; init; }

    /// <summary>
    /// Gets or sets the label text displayed adjacent to the input.
    /// </summary>
    public string? Label { get; init; }

    /// <summary>
    /// Gets or sets the validation error message.
    /// </summary>
    public string? Error { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether the input is disabled.
    /// </summary>
    public bool Disabled { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether the input is required for form submission.
    /// </summary>
    public bool Required { get; init; }

    /// <summary>
    /// Gets or sets the CSS class name(s) applied to the input's root element.
    /// </summary>
    public string? ClassName { get; init; }

    /// <summary>
    /// Gets or sets the inline CSS style string applied to the input's root element.
    /// </summary>
    public string? Style { get; init; }

    /// <summary>
    /// Gets or sets the unique identifier for this input instance.
    /// </summary>
    public string? Id { get; init; }

    /// <summary>
    /// Gets the read-only list of child components. Inputs typically have no children.
    /// </summary>
    public IReadOnlyList<IComponent> Children { get; init; } = Array.Empty<IComponent>();

    /// <summary>
    /// Renders this input component as HTML using the specified rendering context.
    /// </summary>
    /// <param name="context">The rendering context providing tokens and services.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the rendered input.</returns>
    public IHtmlContent Render(RenderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var rootClass = "input-group";
        var hasError = !string.IsNullOrEmpty(Error);
        if (hasError) rootClass += " input-has-error";
        if (!string.IsNullOrEmpty(ClassName)) rootClass += $" {ClassName}";

        var rootAttrs = new Dictionary<string, string> { ["class"] = rootClass };
        if (!string.IsNullOrEmpty(Id)) rootAttrs["id"] = Id;
        if (!string.IsNullOrEmpty(Style)) rootAttrs["style"] = Style;

        var children = new List<IHtmlContent>();

        if (!string.IsNullOrEmpty(Label))
        {
            children.Add(HtmlHelper.Element(
                "label",
                new Dictionary<string, string> { ["class"] = "input-group-label" },
                HtmlHelper.Text(Label)));
        }

        var inputType = Type ?? "text";
        var inputAttrs = new Dictionary<string, string>
        {
            ["class"] = "input",
            ["type"] = inputType
        };
        if (!string.IsNullOrEmpty(Placeholder)) inputAttrs["placeholder"] = Placeholder;
        if (!string.IsNullOrEmpty(Value)) inputAttrs["value"] = Value;
        if (Disabled) inputAttrs["disabled"] = "disabled";
        if (Required) inputAttrs["required"] = "required";

        children.Add(HtmlHelper.Element("input", inputAttrs));

        if (hasError)
        {
            children.Add(HtmlHelper.Element(
                "span",
                new Dictionary<string, string> { ["class"] = "input-error" },
                HtmlHelper.Text(Error!)));
        }

        return HtmlHelper.Element("div", rootAttrs, HtmlHelper.Fragment(children.ToArray()));
    }
}
