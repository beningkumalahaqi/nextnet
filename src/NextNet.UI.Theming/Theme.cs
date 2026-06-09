using NextNet.DesignSystem.Tokens;

namespace NextNet.UI.Theming;

/// <summary>
/// Represents a named theme with its associated design tokens and metadata.
/// </summary>
/// <remarks>
/// <para>
/// A <see cref="Theme"/> is the top-level unit of theming in NextNet. It bundles
/// a <see cref="DesignTokenSet"/> (containing colors, spacing, typography, borders,
/// shadows, and breakpoints) with descriptive <see cref="ThemeMetadata"/>.
/// </para>
/// <para>
/// Themes are immutable by default. To create a derived theme with overrides, use
/// the <c>with</c> expression or merge token sets via
/// <c>DesignSystem.Extensions.TokenSetExtensions</c>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var theme = new Theme(
///     "custom",
///     new DesignTokenSet(
///         colors: new Dictionary&lt;string, ColorToken&gt;
///         {
///             ["primary-500"] = new ColorToken("primary-500", "#FF6200")
///         }),
///     new ThemeMetadata(
///         isDark: false,
///         displayName: "Custom Theme",
///         description: "My custom orange theme",
///         iconUrl: null));
/// </code>
/// </example>
/// <param name="Name">The unique name identifier for this theme (e.g., <c>"light"</c>, <c>"dark"</c>). Must not be null or empty.</param>
/// <param name="Tokens">The <see cref="DesignTokenSet"/> containing all design tokens for this theme.</param>
/// <param name="Metadata">The <see cref="ThemeMetadata"/> providing descriptive information about this theme.</param>
public sealed record Theme(
    string Name,
    DesignTokenSet Tokens,
    ThemeMetadata Metadata);
