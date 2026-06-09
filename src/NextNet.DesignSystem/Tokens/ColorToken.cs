namespace NextNet.DesignSystem.Tokens;

/// <summary>
/// Represents a color design token with optional interactive state overrides.
/// </summary>
/// <remarks>
/// Color tokens define the palette of a design system. Each token carries a base
/// <see cref="Value"/> and may optionally specify <see cref="Hover"/>, <see cref="Active"/>,
/// and <see cref="Foreground"/> colors for interactive elements such as buttons and links.
/// Values are typically hex-encoded strings (e.g., <c>"#3B82F6"</c>).
/// </remarks>
/// <example>
/// <code>
/// var primary = new ColorToken("primary-500", "#3B82F6")
/// {
///     Hover = "#2563EB",
///     Active = "#1D4ED8",
///     Foreground = "#FFFFFF"
/// };
/// </code>
/// </example>
/// <param name="Name">The unique identifier for this token (e.g., <c>"primary-500"</c>).</param>
/// <param name="Value">The base color value (e.g., <c>"#3B82F6"</c>).</param>
public sealed record ColorToken(string Name, string Value)
{
    /// <summary>
    /// Gets or sets the hover state color value. May be <c>null</c> if not applicable.
    /// </summary>
    public string? Hover { get; init; }

    /// <summary>
    /// Gets or sets the active/pressed state color value. May be <c>null</c> if not applicable.
    /// </summary>
    public string? Active { get; init; }

    /// <summary>
    /// Gets or sets the foreground (text) color that contrasts with this token's value.
    /// May be <c>null</c> if not applicable.
    /// </summary>
    public string? Foreground { get; init; }
}
