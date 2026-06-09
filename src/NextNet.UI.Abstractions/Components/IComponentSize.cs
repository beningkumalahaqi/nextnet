namespace NextNet.UI.Abstractions.Components;

/// <summary>
/// Represents a predefined size for UI components, providing a strongly-typed
/// alternative to raw string or numeric size values.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="ComponentSize"/> is a sealed record type used to define the physical
/// dimensions of a component (padding, font size, height, etc.). Each static field
/// represents a well-known size tier in the design system.
/// </para>
/// <para>
/// Sizes are applicable to buttons, inputs, badges, avatars, and other components
/// that support multiple sizing tiers.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var size = ComponentSize.Md;
/// var avatarSize = ComponentSize.Lg;
/// Console.WriteLine(size); // Outputs: "Md"
/// </code>
/// </example>
public sealed record ComponentSize
{
    /// <summary>
    /// Initializes a new instance of <see cref="ComponentSize"/>.
    /// </summary>
    /// <param name="name">The display name of the size. Must not be null or empty.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="name"/> is null or empty.</exception>
    public ComponentSize(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        Name = name;
    }

    /// <summary>
    /// Gets the display name of the size.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Small size — typically 0.375rem padding, smaller font.
    /// </summary>
    public static ComponentSize Sm { get; } = new("Sm");

    /// <summary>
    /// Medium size — the default size for most components.
    /// </summary>
    public static ComponentSize Md { get; } = new("Md");

    /// <summary>
    /// Large size — typically 0.75rem padding, larger font.
    /// </summary>
    public static ComponentSize Lg { get; } = new("Lg");

    /// <summary>
    /// Extra large size — additional spacing and font size for prominent UI.
    /// </summary>
    public static ComponentSize Xl { get; } = new("Xl");

    /// <inheritdoc />
    public override string ToString() => Name;
}
