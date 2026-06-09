namespace NextNet.UI.Abstractions.Components;

/// <summary>
/// Defines the contract for a modal dialog UI component.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="IModal"/> extends <see cref="IComponent"/> with properties
/// for displaying a dialog overlay, including open state, close callback,
/// title, size, and footer content.
/// </para>
/// <para>
/// Modals are used for focused interactions that require user attention.
/// The <see cref="OnClose"/> delegate should be invoked when the user attempts
/// to close the modal (via close button, backdrop click, or Escape key).
/// </para>
/// </remarks>
public interface IModal : IComponent
{
    /// <summary>
    /// Gets a value indicating whether the modal is currently visible.
    /// </summary>
    bool Open { get; }

    /// <summary>
    /// Gets the delegate invoked when the modal is requested to close.
    /// May be <c>null</c> if no handler is attached.
    /// </summary>
    Func<Task>? OnClose { get; }

    /// <summary>
    /// Gets the title text displayed in the modal header.
    /// </summary>
    string? Title { get; }

    /// <summary>
    /// Gets the size of the modal.
    /// Defaults to <see cref="ComponentSize.Md"/>.
    /// </summary>
    ComponentSize? Size { get; }

    /// <summary>
    /// Gets the footer content rendered at the bottom of the modal.
    /// Typically contains action buttons.
    /// </summary>
    IComponent? Footer { get; }
}
