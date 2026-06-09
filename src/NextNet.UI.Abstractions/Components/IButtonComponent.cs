namespace NextNet.UI.Abstractions.Components;

/// <summary>
/// Defines the contract for a button UI component.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="IButton"/> extends <see cref="IComponent"/> with properties
/// specific to button elements, including visual variant, sizing, disabled state,
/// click handler, and label text.
/// </para>
/// <para>
/// The <see cref="OnClick"/> delegate is expected to be invoked by the renderer
/// when the button is activated (clicked, tapped, or triggered via keyboard).
/// </para>
/// </remarks>
public interface IButton : IComponent
{
    /// <summary>
    /// Gets the semantic variant that determines the button's visual style.
    /// Defaults to <see cref="ComponentVariant.Primary"/>.
    /// </summary>
    ComponentVariant? Variant { get; }

    /// <summary>
    /// Gets the size of the button.
    /// Defaults to <see cref="ComponentSize.Md"/>.
    /// </summary>
    ComponentSize? Size { get; }

    /// <summary>
    /// Gets a value indicating whether the button is disabled.
    /// When <c>true</c>, the button should not respond to user interaction.
    /// </summary>
    bool Disabled { get; }

    /// <summary>
    /// Gets the delegate invoked when the button is clicked.
    /// May be <c>null</c> if no handler is attached.
    /// </summary>
    Func<Task>? OnClick { get; }

    /// <summary>
    /// Gets the text label displayed on the button.
    /// </summary>
    string? Label { get; }
}
