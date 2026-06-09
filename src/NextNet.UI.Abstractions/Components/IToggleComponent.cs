namespace NextNet.UI.Abstractions.Components;

/// <summary>
/// Defines the contract for a toggle/switch UI component.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="IToggle"/> extends <see cref="IComponent"/> with properties
/// for a binary on/off control, including checked state, disabled state,
/// size, and change callback.
/// </para>
/// <para>
/// Toggles are used for boolean settings where the user can enable or
/// disable a feature. Unlike checkboxes, toggles provide an immediate
/// visual indication of the current state.
/// </para>
/// </remarks>
public interface IToggle : IComponent
{
    /// <summary>
    /// Gets a value indicating whether the toggle is in the checked (on) position.
    /// </summary>
    bool Checked { get; }

    /// <summary>
    /// Gets a value indicating whether the toggle is disabled.
    /// When <c>true</c>, the toggle should not respond to user interaction.
    /// </summary>
    bool Disabled { get; }

    /// <summary>
    /// Gets the size of the toggle.
    /// Defaults to <see cref="ComponentSize.Md"/>.
    /// </summary>
    ComponentSize? Size { get; }

    /// <summary>
    /// Gets the delegate invoked when the toggle state changes.
    /// Receives the new checked state value.
    /// May be <c>null</c> if no handler is attached.
    /// </summary>
    Func<bool, Task>? OnChange { get; }
}
