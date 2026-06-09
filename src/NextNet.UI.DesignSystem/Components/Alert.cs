using NextNet.Components;
using NextNet.UI.Abstractions.Components;
using NextNet.UI.Abstractions.Rendering;

namespace NextNet.UI.DesignSystem.Components;

/// <summary>
/// Standard implementation of <see cref="IAlert"/> that renders a contextual
/// notification message with optional dismiss button.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="Alert"/> renders a <c>&lt;div&gt;</c> with <c>role="alert"</c>
/// containing an optional title, message body, and dismiss button. The following
/// CSS classes are applied:
/// </para>
/// <list type="bullet">
///   <item><description><c>alert</c> — base class</description></item>
///   <item><description><c>alert-{variant}</c> — e.g. <c>alert-info</c>, <c>alert-danger</c></description></item>
///   <item><description><c>alert-dismissible</c> — when the alert can be dismissed</description></item>
///   <item><description><c>alert-title</c> — title element</description></item>
///   <item><description><c>alert-message</c> — message element</description></item>
///   <item><description><c>alert-dismiss</c> — dismiss button</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// var alert = new Alert
/// {
///     Variant = ComponentVariant.Warning,
///     Title = "Warning",
///     Message = "Your session will expire soon.",
///     Dismissible = true
/// };
/// var html = alert.Render(context);
/// </code>
/// </example>
public sealed class Alert : IAlert, IRenderableComponent
{
    /// <summary>
    /// Gets or sets the semantic variant that determines the alert's color and icon.
    /// Defaults to <see cref="ComponentVariant.Info"/>.
    /// </summary>
    public ComponentVariant? Variant { get; init; } = ComponentVariant.Info;

    /// <summary>
    /// Gets or sets the title text displayed prominently at the top of the alert.
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// Gets or sets the message body text providing details about the alert.
    /// </summary>
    public string? Message { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether the alert can be dismissed by the user.
    /// </summary>
    public bool Dismissible { get; init; }

    /// <summary>
    /// Gets or sets the delegate invoked when the alert is dismissed.
    /// </summary>
    public Func<Task>? OnDismiss { get; init; }

    /// <summary>
    /// Gets or sets the CSS class name(s) applied to the alert's root element.
    /// </summary>
    public string? ClassName { get; init; }

    /// <summary>
    /// Gets or sets the inline CSS style string applied to the alert's root element.
    /// </summary>
    public string? Style { get; init; }

    /// <summary>
    /// Gets or sets the unique identifier for this alert instance.
    /// </summary>
    public string? Id { get; init; }

    /// <summary>
    /// Gets the read-only list of child components. Alerts typically have no children.
    /// </summary>
    public IReadOnlyList<IComponent> Children { get; init; } = Array.Empty<IComponent>();

    /// <summary>
    /// Renders this alert component as HTML using the specified rendering context.
    /// </summary>
    /// <param name="context">The rendering context providing tokens and services.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the rendered alert.</returns>
    public IHtmlContent Render(RenderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var variantName = Variant?.ToString().ToLowerInvariant() ?? "info";

        var alertClass = $"alert alert-{variantName}";
        if (Dismissible) alertClass += " alert-dismissible";
        if (!string.IsNullOrEmpty(ClassName)) alertClass += $" {ClassName}";

        var attrs = new Dictionary<string, string>
        {
            ["class"] = alertClass,
            ["role"] = "alert"
        };
        if (!string.IsNullOrEmpty(Id)) attrs["id"] = Id;
        if (!string.IsNullOrEmpty(Style)) attrs["style"] = Style;

        var children = new List<IHtmlContent>();

        if (!string.IsNullOrEmpty(Title))
        {
            children.Add(HtmlHelper.Element(
                "h4",
                new Dictionary<string, string> { ["class"] = "alert-title" },
                HtmlHelper.Text(Title)));
        }

        if (!string.IsNullOrEmpty(Message))
        {
            children.Add(HtmlHelper.Element(
                "p",
                new Dictionary<string, string> { ["class"] = "alert-message" },
                HtmlHelper.Text(Message)));
        }

        if (Dismissible)
        {
            children.Add(HtmlHelper.Element(
                "button",
                new Dictionary<string, string>
                {
                    ["class"] = "alert-dismiss",
                    ["type"] = "button",
                    ["aria-label"] = "Dismiss"
                },
                HtmlHelper.Raw("&times;")));
        }

        return HtmlHelper.Element("div", attrs, HtmlHelper.Fragment(children.ToArray()));
    }
}
