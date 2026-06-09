namespace NextNet.UI.Abstractions.Components;

/// <summary>
/// Defines a single item within a dropdown menu.
/// </summary>
/// <param name="Label">The display text for the dropdown item.</param>
/// <param name="Value">The underlying value associated with the item.</param>
/// <param name="Disabled">Whether the item is disabled and cannot be selected.</param>
/// <param name="Icon">An optional icon class or identifier for the item.</param>
public sealed record DropdownItem(
    string Label,
    string? Value = null,
    bool Disabled = false,
    string? Icon = null);

/// <summary>
/// Defines the contract for a dropdown UI component.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="IDropdown"/> extends <see cref="IComponent"/> with properties
/// for displaying a selectable menu of options, including items collection,
/// trigger element, placement direction, open state, and selection callback.
/// </para>
/// <para>
/// The <see cref="Trigger"/> component defines what element the user interacts
/// with to open the dropdown. <see cref="Placement"/> controls where the menu
/// appears relative to the trigger (e.g., "bottom-start", "top-end").
/// </para>
/// </remarks>
public interface IDropdown : IComponent
{
    /// <summary>
    /// Gets the collection of items displayed in the dropdown menu.
    /// </summary>
    IReadOnlyList<DropdownItem>? Items { get; }

    /// <summary>
    /// Gets the component used as the trigger element that toggles the dropdown.
    /// </summary>
    IComponent? Trigger { get; }

    /// <summary>
    /// Gets the placement of the dropdown menu relative to the trigger.
    /// Common values include "bottom-start" (default), "bottom-end", "top-start", "top-end".
    /// </summary>
    string? Placement { get; }

    /// <summary>
    /// Gets a value indicating whether the dropdown menu is currently open.
    /// </summary>
    bool Open { get; }

    /// <summary>
    /// Gets the delegate invoked when a dropdown item is selected.
    /// Receives the <see cref="DropdownItem.Value"/> of the selected item.
    /// May be <c>null</c> if no handler is attached.
    /// </summary>
    Func<string?, Task>? OnSelect { get; }
}
