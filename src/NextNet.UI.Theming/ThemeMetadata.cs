namespace NextNet.UI.Theming;

/// <summary>
/// Provides descriptive metadata about a theme.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="ThemeMetadata"/> carries human-readable information about a theme
/// that can be used for display in theme pickers, developer tooling, or
/// documentation generation. The <see cref="IsDark"/> flag allows consumers
/// to make layout or branding decisions based on the theme's brightness.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var metadata = new ThemeMetadata(
///     isDark: true,
///     displayName: "Dark Mode",
///     description: "A dark theme optimized for low-light environments",
///     iconUrl: "/icons/theme-dark.svg");
/// </code>
/// </example>
/// <param name="IsDark">Indicates whether this theme is a dark variant (<c>true</c>) or light variant (<c>false</c>).</param>
/// <param name="DisplayName">The human-readable display name for this theme (e.g., <c>"Dark Mode"</c>). Must not be null or empty.</param>
/// <param name="Description">An optional description providing more detail about the theme's intended use or style.</param>
/// <param name="IconUrl">An optional URL pointing to an icon or preview image for the theme.</param>
public sealed record ThemeMetadata(
    bool IsDark,
    string DisplayName,
    string? Description,
    string? IconUrl);
