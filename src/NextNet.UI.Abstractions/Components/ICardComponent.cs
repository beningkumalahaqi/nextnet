namespace NextNet.UI.Abstractions.Components;

/// <summary>
/// Defines the contract for a card UI component.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="ICard"/> extends <see cref="IComponent"/> with properties
/// for displaying structured content in a contained box, including title,
/// description, footer, padding configuration, and shadow level.
/// </para>
/// <para>
/// Cards are commonly used for dashboards, product listings, and content previews.
/// The <see cref="Footer"/> allows placing actions or metadata at the bottom of the card.
/// </para>
/// </remarks>
public interface ICard : IComponent
{
    /// <summary>
    /// Gets the title text displayed in the card header.
    /// </summary>
    string? Title { get; }

    /// <summary>
    /// Gets the description text displayed in the card body below the title.
    /// </summary>
    string? Description { get; }

    /// <summary>
    /// Gets the footer content rendered at the bottom of the card.
    /// Typically contains action buttons or metadata.
    /// </summary>
    IComponent? Footer { get; }

    /// <summary>
    /// Gets the padding size applied inside the card.
    /// Defaults to <see cref="ComponentSize.Md"/>.
    /// </summary>
    ComponentSize? Padding { get; }

    /// <summary>
    /// Gets the shadow elevation level for the card.
    /// <c>null</c> means no shadow is applied.
    /// </summary>
    string? Shadow { get; }
}
