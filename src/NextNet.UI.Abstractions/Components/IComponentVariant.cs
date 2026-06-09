namespace NextNet.UI.Abstractions.Components;

/// <summary>
/// Represents a semantic variant for UI components, defining the visual style
/// category (e.g., primary, danger, ghost).
/// </summary>
/// <remarks>
/// <para>
/// <see cref="ComponentVariant"/> is a sealed record type used as a strongly-typed
/// replacement for string-based variant enums. Each static field represents a
/// well-known variant that maps to specific styling in the design system.
/// </para>
/// <para>
/// Variants are typically applied to buttons, badges, alerts, and other
/// components that support multiple visual styles.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var variant = ComponentVariant.Primary;
/// var badgeVariant = ComponentVariant.Success;
/// Console.WriteLine(variant); // Outputs: "Primary"
/// </code>
/// </example>
public sealed record ComponentVariant
{
    /// <summary>
    /// Initializes a new instance of <see cref="ComponentVariant"/>.
    /// </summary>
    /// <param name="name">The display name of the variant. Must not be null or empty.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="name"/> is null or empty.</exception>
    public ComponentVariant(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        Name = name;
    }

    /// <summary>
    /// Gets the display name of the variant.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Primary action variant — typically rendered with the brand color.
    /// </summary>
    public static ComponentVariant Primary { get; } = new("Primary");

    /// <summary>
    /// Secondary action variant — typically rendered with a neutral color.
    /// </summary>
    public static ComponentVariant Secondary { get; } = new("Secondary");

    /// <summary>
    /// Destructive action variant — typically rendered with a danger/red color.
    /// </summary>
    public static ComponentVariant Danger { get; } = new("Danger");

    /// <summary>
    /// Ghost variant — rendered with minimal styling, often transparent background.
    /// </summary>
    public static ComponentVariant Ghost { get; } = new("Ghost");

    /// <summary>
    /// Outline variant — rendered with a border but transparent background.
    /// </summary>
    public static ComponentVariant Outline { get; } = new("Outline");

    /// <summary>
    /// Success variant — typically rendered with a green color indicating positive outcomes.
    /// </summary>
    public static ComponentVariant Success { get; } = new("Success");

    /// <summary>
    /// Warning variant — typically rendered with a yellow/amber color indicating caution.
    /// </summary>
    public static ComponentVariant Warning { get; } = new("Warning");

    /// <summary>
    /// Informational variant — typically rendered with a blue color for notifications.
    /// </summary>
    public static ComponentVariant Info { get; } = new("Info");

    /// <inheritdoc />
    public override string ToString() => Name;
}
