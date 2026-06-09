namespace NextNet.UI.Abstractions.Components;

/// <summary>
/// Defines the contract for an input UI component.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="IInput"/> extends <see cref="IComponent"/> with properties
/// specific to form input elements, including type, placeholder, current value,
/// label, validation error, and disabled/required states.
/// </para>
/// <para>
/// The <see cref="Type"/> property determines the underlying HTML input type
/// (e.g., "text", "email", "password", "number"). Validation is driven by
/// the <see cref="Required"/> and <see cref="Error"/> properties.
/// </para>
/// </remarks>
public interface IInput : IComponent
{
    /// <summary>
    /// Gets the HTML input type (e.g., "text", "email", "password", "number").
    /// Defaults to "text" when <c>null</c>.
    /// </summary>
    string? Type { get; }

    /// <summary>
    /// Gets the placeholder text displayed when the input is empty.
    /// </summary>
    string? Placeholder { get; }

    /// <summary>
    /// Gets the current value of the input.
    /// </summary>
    string? Value { get; }

    /// <summary>
    /// Gets the label text displayed adjacent to the input.
    /// </summary>
    string? Label { get; }

    /// <summary>
    /// Gets the validation error message. Non-null indicates the input
    /// is in an invalid state with the provided error description.
    /// </summary>
    string? Error { get; }

    /// <summary>
    /// Gets a value indicating whether the input is disabled.
    /// When <c>true</c>, the input should not respond to user interaction.
    /// </summary>
    bool Disabled { get; }

    /// <summary>
    /// Gets a value indicating whether the input is required for form submission.
    /// </summary>
    bool Required { get; }
}
