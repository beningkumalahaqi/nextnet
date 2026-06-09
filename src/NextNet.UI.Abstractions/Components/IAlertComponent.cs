namespace NextNet.UI.Abstractions.Components;

/// <summary>
/// Defines the contract for an alert UI component.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="IAlert"/> extends <see cref="IComponent"/> with properties
/// for displaying contextual notification messages, including semantic variant,
/// title, message body, dismissible state, and dismiss callback.
/// </para>
/// <para>
/// Alerts are used to communicate system state to users. When <see cref="Dismissible"/>
/// is <c>true</c>, a close button is rendered and <see cref="OnDismiss"/> is invoked
/// when the user dismisses the alert.
/// </para>
/// </remarks>
public interface IAlert : IComponent
{
    /// <summary>
    /// Gets the semantic variant that determines the alert's color and icon.
    /// Defaults to <see cref="ComponentVariant.Info"/>.
    /// </summary>
    ComponentVariant? Variant { get; }

    /// <summary>
    /// Gets the title text displayed prominently at the top of the alert.
    /// </summary>
    string? Title { get; }

    /// <summary>
    /// Gets the message body text providing details about the alert.
    /// </summary>
    string? Message { get; }

    /// <summary>
    /// Gets a value indicating whether the alert can be dismissed by the user.
    /// </summary>
    bool Dismissible { get; }

    /// <summary>
    /// Gets the delegate invoked when the alert is dismissed.
    /// May be <c>null</c> if no handler is attached.
    /// </summary>
    Func<Task>? OnDismiss { get; }
}
