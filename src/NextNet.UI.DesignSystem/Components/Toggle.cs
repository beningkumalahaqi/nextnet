using NextNet.Components;
using NextNet.UI.Abstractions.Components;
using NextNet.UI.Abstractions.Rendering;

namespace NextNet.UI.DesignSystem.Components;

/// <summary>
/// Standard implementation of <see cref="IToggle"/> that renders a binary on/off
/// toggle switch control.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="Toggle"/> renders a <c>&lt;label class="toggle"&gt;</c> containing
/// a hidden <c>&lt;input type="checkbox"&gt;</c> and a visual slider
/// <c>&lt;span class="toggle-slider"&gt;</c>. The following CSS classes are applied:
/// </para>
/// <list type="bullet">
///   <item><description><c>toggle</c> — base class on the label</description></item>
///   <item><description><c>toggle-{size}</c> — e.g. <c>toggle-sm</c>, <c>toggle-lg</c></description></item>
///   <item><description><c>toggle-checked</c> — when the toggle is in the on position</description></item>
///   <item><description><c>toggle-disabled</c> — when the toggle is disabled</description></item>
///   <item><description><c>toggle-slider</c> — the visual slider element</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// var toggle = new Toggle
/// {
///     Checked = true,
///     Size = ComponentSize.Md
/// };
/// var html = toggle.Render(context);
/// // Produces: &lt;label class="toggle toggle-md toggle-checked"&gt;
/// //   &lt;input type="checkbox" checked /&gt;
/// //   &lt;span class="toggle-slider"&gt;&lt;/span&gt;
/// // &lt;/label&gt;
/// </code>
/// </example>
public sealed class Toggle : IToggle, IRenderableComponent
{
    /// <summary>
    /// Gets or sets a value indicating whether the toggle is in the checked (on) position.
    /// </summary>
    public bool Checked { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether the toggle is disabled.
    /// </summary>
    public bool Disabled { get; init; }

    /// <summary>
    /// Gets or sets the size of the toggle.
    /// Defaults to <see cref="ComponentSize.Md"/>.
    /// </summary>
    public ComponentSize? Size { get; init; } = ComponentSize.Md;

    /// <summary>
    /// Gets or sets the delegate invoked when the toggle state changes.
    /// </summary>
    public Func<bool, Task>? OnChange { get; init; }

    /// <summary>
    /// Gets or sets the CSS class name(s) applied to the toggle's root element.
    /// </summary>
    public string? ClassName { get; init; }

    /// <summary>
    /// Gets or sets the inline CSS style string applied to the toggle's root element.
    /// </summary>
    public string? Style { get; init; }

    /// <summary>
    /// Gets or sets the unique identifier for this toggle instance.
    /// </summary>
    public string? Id { get; init; }

    /// <summary>
    /// Gets the read-only list of child components. Toggles typically have no children.
    /// </summary>
    public IReadOnlyList<IComponent> Children { get; init; } = Array.Empty<IComponent>();

    /// <summary>
    /// Renders this toggle component as HTML using the specified rendering context.
    /// </summary>
    /// <param name="context">The rendering context providing tokens and services.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the rendered toggle.</returns>
    public IHtmlContent Render(RenderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var sizeName = Size?.ToString().ToLowerInvariant() ?? "md";

        var toggleClass = $"toggle toggle-{sizeName}";
        if (Checked) toggleClass += " toggle-checked";
        if (Disabled) toggleClass += " toggle-disabled";
        if (!string.IsNullOrEmpty(ClassName)) toggleClass += $" {ClassName}";

        var attrs = new Dictionary<string, string> { ["class"] = toggleClass };
        if (!string.IsNullOrEmpty(Id)) attrs["id"] = Id;
        if (!string.IsNullOrEmpty(Style)) attrs["style"] = Style;

        var inputAttrs = new Dictionary<string, string>
        {
            ["type"] = "checkbox",
            ["class"] = "toggle-input"
        };
        if (Checked) inputAttrs["checked"] = "checked";
        if (Disabled) inputAttrs["disabled"] = "disabled";

        var children = new List<IHtmlContent>
        {
            HtmlHelper.Element("input", inputAttrs),
            HtmlHelper.Element("span", new Dictionary<string, string> { ["class"] = "toggle-slider" })
        };

        return HtmlHelper.Element("label", attrs, HtmlHelper.Fragment(children.ToArray()));
    }
}
