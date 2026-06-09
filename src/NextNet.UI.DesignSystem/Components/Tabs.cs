using NextNet.Components;
using NextNet.UI.Abstractions.Components;
using NextNet.UI.Abstractions.Rendering;

namespace NextNet.UI.DesignSystem.Components;

/// <summary>
/// Standard implementation of <see cref="ITabs"/> that renders a tabbed interface
/// with navigation headers and content panels.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="Tabs"/> renders a <c>&lt;div&gt;</c> container with a tab list and
/// content panel. The following CSS classes are applied:
/// </para>
/// <list type="bullet">
///   <item><description><c>tabs</c> — base class</description></item>
///   <item><description><c>tabs-horizontal</c> or <c>tabs-vertical</c> — based on orientation</description></item>
///   <item><description><c>tab-list</c> — tab navigation container</description></item>
///   <item><description><c>tab</c> — each tab button</description></item>
///   <item><description><c>tab-active</c> — currently active tab</description></item>
///   <item><description><c>tab-disabled</c> — disabled tab</description></item>
///   <item><description><c>tab-panel</c> — content panel for the active tab</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// var tabs = new Tabs
/// {
///     Items = new[]
///     {
///         new TabItem("Profile"),
///         new TabItem("Settings"),
///         new TabItem("Notifications")
///     },
///     ActiveIndex = 0,
///     Orientation = "horizontal"
/// };
/// var html = tabs.Render(context);
/// </code>
/// </example>
public sealed class Tabs : ITabs, IRenderableComponent
{
    /// <summary>
    /// Gets or sets the collection of tab items.
    /// </summary>
    public IReadOnlyList<TabItem>? Items { get; init; }

    /// <summary>
    /// Gets or sets the index of the currently active tab.
    /// Defaults to 0.
    /// </summary>
    public int ActiveIndex { get; init; }

    /// <summary>
    /// Gets or sets the orientation of the tabs.
    /// Defaults to <c>"horizontal"</c>.
    /// </summary>
    public string? Orientation { get; init; } = "horizontal";

    /// <summary>
    /// Gets or sets the delegate invoked when the active tab changes.
    /// </summary>
    public Func<int, Task>? OnChange { get; init; }

    /// <summary>
    /// Gets or sets the CSS class name(s) applied to the tabs root element.
    /// </summary>
    public string? ClassName { get; init; }

    /// <summary>
    /// Gets or sets the inline CSS style string applied to the tabs root element.
    /// </summary>
    public string? Style { get; init; }

    /// <summary>
    /// Gets or sets the unique identifier for this tabs instance.
    /// </summary>
    public string? Id { get; init; }

    /// <summary>
    /// Gets the read-only list of child components. Tabs typically have no children.
    /// </summary>
    public IReadOnlyList<IComponent> Children { get; init; } = Array.Empty<IComponent>();

    /// <summary>
    /// Renders this tabs component as HTML using the specified rendering context.
    /// </summary>
    /// <param name="context">The rendering context providing tokens and services.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the rendered tabs.</returns>
    public IHtmlContent Render(RenderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var orientation = !string.IsNullOrEmpty(Orientation) ? Orientation.ToLowerInvariant() : "horizontal";
        var tabsClass = $"tabs tabs-{orientation}";
        if (!string.IsNullOrEmpty(ClassName)) tabsClass += $" {ClassName}";

        var attrs = new Dictionary<string, string> { ["class"] = tabsClass };
        if (!string.IsNullOrEmpty(Id)) attrs["id"] = Id;
        if (!string.IsNullOrEmpty(Style)) attrs["style"] = Style;

        var children = new List<IHtmlContent>();

        // Tab list
        if (Items != null && Items.Count > 0)
        {
            var tabButtons = new List<IHtmlContent>();
            for (int i = 0; i < Items.Count; i++)
            {
                var item = Items[i];
                var tabClass = "tab";
                if (i == ActiveIndex) tabClass += " tab-active";
                if (item.Disabled) tabClass += " tab-disabled";

                var tabAttrs = new Dictionary<string, string>
                {
                    ["class"] = tabClass,
                    ["role"] = "tab",
                    ["aria-selected"] = (i == ActiveIndex).ToString().ToLowerInvariant()
                };
                if (item.Disabled) tabAttrs["aria-disabled"] = "true";

                tabButtons.Add(HtmlHelper.Element("button", tabAttrs, HtmlHelper.Text(item.Label)));
            }

            children.Add(HtmlHelper.Element(
                "div",
                new Dictionary<string, string> { ["class"] = "tab-list", ["role"] = "tablist" },
                HtmlHelper.Fragment(tabButtons.ToArray())));
        }

        // Active tab content panel
        if (Items != null && ActiveIndex >= 0 && ActiveIndex < Items.Count)
        {
            var activeItem = Items[ActiveIndex];
            var panelContent = activeItem.Content != null
                ? (activeItem.Content is IRenderableComponent renderable
                    ? renderable.Render(context)
                    : HtmlHelper.Text(activeItem.Content.ToString() ?? ""))
                : null;

            children.Add(HtmlHelper.Element(
                "div",
                new Dictionary<string, string>
                {
                    ["class"] = "tab-panel",
                    ["role"] = "tabpanel"
                },
                panelContent));
        }

        return HtmlHelper.Element("div", attrs, HtmlHelper.Fragment(children.ToArray()));
    }
}
