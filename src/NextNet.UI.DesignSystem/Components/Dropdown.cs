using NextNet.Components;
using NextNet.UI.Abstractions.Components;
using NextNet.UI.Abstractions.Rendering;

namespace NextNet.UI.DesignSystem.Components;

/// <summary>
/// Standard implementation of <see cref="IDropdown"/> that renders a trigger element
/// with a menu overlay for selecting options.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="Dropdown"/> renders a <c>&lt;div class="dropdown"&gt;</c> containing
/// a trigger element and an optional menu <c>&lt;ul&gt;</c>. The following CSS
/// classes are applied:
/// </para>
/// <list type="bullet">
///   <item><description><c>dropdown</c> — root wrapper</description></item>
///   <item><description><c>dropdown-open</c> — root class when menu is visible</description></item>
///   <item><description><c>dropdown-menu</c> — menu list element</description></item>
///   <item><description><c>dropdown-{placement}</c> — e.g. <c>dropdown-bottom-start</c></description></item>
///   <item><description><c>dropdown-item</c> — each menu item</description></item>
///   <item><description><c>dropdown-item-disabled</c> — disabled menu item</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// var dropdown = new Dropdown
/// {
///     Items = new[]
///     {
///         new DropdownItem("Profile", "profile"),
///         new DropdownItem("Settings", "settings"),
///         new DropdownItem("Logout", "logout")
///     },
///     Placement = "bottom-start",
///     Open = true
/// };
/// var html = dropdown.Render(context);
/// </code>
/// </example>
public sealed class Dropdown : IDropdown, IRenderableComponent
{
    /// <summary>
    /// Gets or sets the collection of items displayed in the dropdown menu.
    /// </summary>
    public IReadOnlyList<DropdownItem>? Items { get; init; }

    /// <summary>
    /// Gets or sets the component used as the trigger element that toggles the dropdown.
    /// </summary>
    public IComponent? Trigger { get; init; }

    /// <summary>
    /// Gets or sets the placement of the dropdown menu relative to the trigger.
    /// Defaults to <c>"bottom-start"</c>.
    /// </summary>
    public string? Placement { get; init; } = "bottom-start";

    /// <summary>
    /// Gets or sets a value indicating whether the dropdown menu is currently open.
    /// </summary>
    public bool Open { get; init; }

    /// <summary>
    /// Gets or sets the delegate invoked when a dropdown item is selected.
    /// </summary>
    public Func<string?, Task>? OnSelect { get; init; }

    /// <summary>
    /// Gets or sets the CSS class name(s) applied to the dropdown's root element.
    /// </summary>
    public string? ClassName { get; init; }

    /// <summary>
    /// Gets or sets the inline CSS style string applied to the dropdown's root element.
    /// </summary>
    public string? Style { get; init; }

    /// <summary>
    /// Gets or sets the unique identifier for this dropdown instance.
    /// </summary>
    public string? Id { get; init; }

    /// <summary>
    /// Gets the read-only list of child components. Dropdowns typically have no direct children.
    /// </summary>
    public IReadOnlyList<IComponent> Children { get; init; } = Array.Empty<IComponent>();

    /// <summary>
    /// Renders this dropdown component as HTML using the specified rendering context.
    /// </summary>
    /// <param name="context">The rendering context providing tokens and services.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the rendered dropdown.</returns>
    public IHtmlContent Render(RenderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var placement = !string.IsNullOrEmpty(Placement) ? Placement.ToLowerInvariant() : "bottom-start";
        var dropdownClass = "dropdown";
        if (Open) dropdownClass += " dropdown-open";
        if (!string.IsNullOrEmpty(ClassName)) dropdownClass += $" {ClassName}";

        var rootAttrs = new Dictionary<string, string> { ["class"] = dropdownClass };
        if (!string.IsNullOrEmpty(Id)) rootAttrs["id"] = Id;
        if (!string.IsNullOrEmpty(Style)) rootAttrs["style"] = Style;

        var children = new List<IHtmlContent>();

        // Trigger
        if (Trigger != null)
        {
            var triggerContent = Trigger is IRenderableComponent renderable
                ? renderable.Render(context)
                : HtmlHelper.Text(Trigger.ToString() ?? "");
            children.Add(HtmlHelper.Element(
                "button",
                new Dictionary<string, string> { ["class"] = "dropdown-trigger" },
                triggerContent));
        }

        // Menu
        if (Items != null && Items.Count > 0)
        {
            var itemElements = new List<IHtmlContent>();
            foreach (var item in Items)
            {
                var itemClass = "dropdown-item";
                if (item.Disabled) itemClass += " dropdown-item-disabled";

                var itemAttrs = new Dictionary<string, string>
                {
                    ["class"] = itemClass,
                    ["data-value"] = item.Value ?? item.Label
                };
                if (item.Disabled) itemAttrs["aria-disabled"] = "true";

                itemElements.Add(HtmlHelper.Element("li", itemAttrs, HtmlHelper.Text(item.Label)));
            }

            var menuClass = $"dropdown-menu dropdown-{placement}";
            if (!Open) menuClass += " dropdown-menu-hidden";

            children.Add(HtmlHelper.Element(
                "ul",
                new Dictionary<string, string> { ["class"] = menuClass },
                HtmlHelper.Fragment(itemElements.ToArray())));
        }

        return HtmlHelper.Element("div", rootAttrs, HtmlHelper.Fragment(children.ToArray()));
    }
}
