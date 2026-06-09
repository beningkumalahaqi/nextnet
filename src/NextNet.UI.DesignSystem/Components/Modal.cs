using NextNet.Components;
using NextNet.UI.Abstractions.Components;
using NextNet.UI.Abstractions.Rendering;

namespace NextNet.UI.DesignSystem.Components;

/// <summary>
/// Standard implementation of <see cref="IModal"/> that renders a dialog overlay
/// with header, body, and optional footer.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="Modal"/> renders a backdrop <c>&lt;div&gt;</c> containing a modal
/// dialog with the following structure and CSS classes:
/// </para>
/// <list type="bullet">
///   <item><description><c>modal-backdrop</c> — overlay backdrop</description></item>
///   <item><description><c>modal modal-{size}</c> — modal dialog with size variant</description></item>
///   <item><description><c>modal-header</c> — title and close button container</description></item>
///   <item><description><c>modal-body</c> — content area</description></item>
///   <item><description><c>modal-footer</c> — action buttons container</description></item>
///   <item><description><c>modal-close</c> — close button</description></item>
///   <item><description><c>modal-open</c> — root class when modal is visible</description></item>
/// </list>
/// <para>
/// When <see cref="IModal.Open"/> is <c>false</c>, the root element has a
/// <c>hidden</c> attribute and the <c>modal-open</c> class is omitted.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var modal = new Modal
/// {
///     Open = true,
///     Title = "Confirm Delete",
///     Size = ComponentSize.Sm
/// };
/// var html = modal.Render(context);
/// </code>
/// </example>
public sealed class Modal : IModal, IRenderableComponent
{
    /// <summary>
    /// Gets or sets a value indicating whether the modal is currently visible.
    /// </summary>
    public bool Open { get; init; }

    /// <summary>
    /// Gets or sets the delegate invoked when the modal is requested to close.
    /// </summary>
    public Func<Task>? OnClose { get; init; }

    /// <summary>
    /// Gets or sets the title text displayed in the modal header.
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// Gets or sets the size of the modal.
    /// Defaults to <see cref="ComponentSize.Md"/>.
    /// </summary>
    public ComponentSize? Size { get; init; } = ComponentSize.Md;

    /// <summary>
    /// Gets or sets the footer content rendered at the bottom of the modal.
    /// </summary>
    public IComponent? Footer { get; init; }

    /// <summary>
    /// Gets or sets the CSS class name(s) applied to the modal's root element.
    /// </summary>
    public string? ClassName { get; init; }

    /// <summary>
    /// Gets or sets the inline CSS style string applied to the modal's root element.
    /// </summary>
    public string? Style { get; init; }

    /// <summary>
    /// Gets or sets the unique identifier for this modal instance.
    /// </summary>
    public string? Id { get; init; }

    /// <summary>
    /// Gets the read-only list of child components. Modals render children within the body.
    /// </summary>
    public IReadOnlyList<IComponent> Children { get; init; } = Array.Empty<IComponent>();

    /// <summary>
    /// Renders this modal component as HTML using the specified rendering context.
    /// </summary>
    /// <param name="context">The rendering context providing tokens and services.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the rendered modal.</returns>
    public IHtmlContent Render(RenderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var sizeName = Size?.ToString().ToLowerInvariant() ?? "md";
        var backdropClass = "modal-backdrop";
        var modalClass = $"modal modal-{sizeName}";
        if (Open) modalClass += " modal-open";
        if (!string.IsNullOrEmpty(ClassName)) modalClass += $" {ClassName}";

        var backdropAttrs = new Dictionary<string, string> { ["class"] = backdropClass };
        if (!Open) backdropAttrs["hidden"] = "hidden";
        if (!string.IsNullOrEmpty(Id)) backdropAttrs["id"] = Id;

        var modalAttrs = new Dictionary<string, string> { ["class"] = modalClass };
        if (!string.IsNullOrEmpty(Style)) modalAttrs["style"] = Style;
        if (!string.IsNullOrEmpty(Id)) modalAttrs["id"] = $"{Id}-dialog";

        var modalChildren = new List<IHtmlContent>();

        // Header
        var headerChildren = new List<IHtmlContent>();
        if (!string.IsNullOrEmpty(Title))
        {
            headerChildren.Add(HtmlHelper.Element(
                "h3",
                new Dictionary<string, string> { ["class"] = "modal-title" },
                HtmlHelper.Text(Title)));
        }

        if (OnClose != null || Open)
        {
            headerChildren.Add(HtmlHelper.Element(
                "button",
                new Dictionary<string, string>
                {
                    ["class"] = "modal-close",
                    ["type"] = "button",
                    ["aria-label"] = "Close"
                },
                HtmlHelper.Raw("&times;")));
        }

        if (headerChildren.Count > 0)
        {
            modalChildren.Add(HtmlHelper.Element(
                "div",
                new Dictionary<string, string> { ["class"] = "modal-header" },
                HtmlHelper.Fragment(headerChildren.ToArray())));
        }

        // Body
        modalChildren.Add(HtmlHelper.Element(
            "div",
            new Dictionary<string, string> { ["class"] = "modal-body" },
            null));

        // Footer
        if (Footer != null)
        {
            var footerContent = Footer is IRenderableComponent renderable
                ? renderable.Render(context)
                : HtmlHelper.Text(Footer.ToString() ?? "");
            modalChildren.Add(HtmlHelper.Element(
                "div",
                new Dictionary<string, string> { ["class"] = "modal-footer" },
                footerContent));
        }

        var modalContent = HtmlHelper.Element("div", modalAttrs, HtmlHelper.Fragment(modalChildren.ToArray()));
        return HtmlHelper.Element("div", backdropAttrs, modalContent);
    }
}
