namespace NextNet.UI.Abstractions.Components;

/// <summary>
/// Defines the contract for an avatar UI component.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="IAvatar"/> extends <see cref="IComponent"/> with properties
/// for displaying user or entity avatars, including image source, alt text,
/// size, shape, and fallback content.
/// </para>
/// <para>
/// When the image specified by <see cref="Src"/> fails to load, the renderer
/// should display <see cref="Fallback"/> instead, which may be initials or
/// a placeholder icon.
/// </para>
/// </remarks>
public interface IAvatar : IComponent
{
    /// <summary>
    /// Gets the URL or path to the avatar image.
    /// </summary>
    string? Src { get; }

    /// <summary>
    /// Gets the alternative text for the avatar image.
    /// Used for accessibility and when the image fails to load.
    /// </summary>
    string? Alt { get; }

    /// <summary>
    /// Gets the size of the avatar.
    /// Defaults to <see cref="ComponentSize.Md"/>.
    /// </summary>
    ComponentSize? Size { get; }

    /// <summary>
    /// Gets the shape of the avatar.
    /// Common values include "circle" (default) and "square".
    /// </summary>
    string? Shape { get; }

    /// <summary>
    /// Gets the fallback content displayed when the avatar image
    /// cannot be loaded. Typically initials or a placeholder icon.
    /// </summary>
    string? Fallback { get; }
}
