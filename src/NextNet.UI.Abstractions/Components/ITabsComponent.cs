namespace NextNet.UI.Abstractions.Components;

/// <summary>
/// Defines a single tab item within a tabs component.
/// </summary>
/// <param name="Label">The display text for the tab header.</param>
/// <param name="Content">The content component rendered when this tab is active.</param>
/// <param name="Disabled">Whether the tab is disabled and cannot be selected.</param>
public sealed record TabItem(
    string Label,
    IComponent? Content = null,
    bool Disabled = false);

/// <summary>
/// Defines the contract for a tabs UI component.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="ITabs"/> extends <see cref="IComponent"/> with properties
/// for displaying a tabbed interface, including tab items, active tab index,
/// orientation (horizontal/vertical), and change callback.
/// </para>
/// <para>
/// Tabs allow users to switch between different content views without
/// navigating to a new page. The <see cref="ActiveIndex"/> property controls
/// which tab is currently selected.
/// </para>
/// </remarks>
public interface ITabs : IComponent
{
    /// <summary>
    /// Gets the collection of tab items.
    /// </summary>
    IReadOnlyList<TabItem>? Items { get; }

    /// <summary>
    /// Gets the index of the currently active tab.
    /// Defaults to 0 (the first tab).
    /// </summary>
    int ActiveIndex { get; }

    /// <summary>
    /// Gets the orientation of the tabs.
    /// Common values are "horizontal" (default) and "vertical".
    /// </summary>
    string? Orientation { get; }

    /// <summary>
    /// Gets the delegate invoked when the active tab changes.
    /// Receives the index of the newly selected tab.
    /// May be <c>null</c> if no handler is attached.
    /// </summary>
    Func<int, Task>? OnChange { get; }
}
