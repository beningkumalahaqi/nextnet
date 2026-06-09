namespace NextNet.UI.Abstractions.Components;

/// <summary>
/// Defines the contract for a badge UI component.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="IBadge"/> extends <see cref="IComponent"/> with properties
/// for displaying a small label or status indicator, including semantic variant,
/// size, optional dot indicator, and label text.
/// </para>
/// <para>
/// Badges are used for counts, statuses, tags, and notifications. When
/// <see cref="Dot"/> is <c>true</c>, the badge renders as a small colored
/// dot without text, suitable for unread indicators.
/// </para>
/// </remarks>
public interface IBadge : IComponent
{
    /// <summary>
    /// Gets the semantic variant that determines the badge's color.
    /// Defaults to <see cref="ComponentVariant.Primary"/>.
    /// </summary>
    ComponentVariant? Variant { get; }

    /// <summary>
    /// Gets the size of the badge.
    /// Defaults to <see cref="ComponentSize.Sm"/>.
    /// </summary>
    ComponentSize? Size { get; }

    /// <summary>
    /// Gets a value indicating whether the badge renders as a dot indicator
    /// instead of a labeled badge.
    /// </summary>
    bool Dot { get; }

    /// <summary>
    /// Gets the text label displayed inside the badge.
    /// Ignored when <see cref="Dot"/> is <c>true</c>.
    /// </summary>
    string? Label { get; }
}
