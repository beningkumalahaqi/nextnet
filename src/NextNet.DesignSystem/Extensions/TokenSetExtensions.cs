using NextNet.DesignSystem.Tokens;

namespace NextNet.DesignSystem.Extensions;

/// <summary>
/// Provides extension methods for <see cref="DesignTokenSet"/> to enable merging,
/// overriding, and querying design tokens.
/// </summary>
/// <remarks>
/// These extensions support composable token customization while preserving immutability.
/// The <see cref="Merge"/> method combines two token sets with the source taking precedence.
/// The <see cref="Override"/> method applies overrides from a secondary set, with the
/// override set taking precedence.
/// </remarks>
/// <example>
/// <code>
/// var baseTokens = DefaultTokens.Create();
/// var customColor = new ColorToken("primary-500", "#FF0000");
/// var overridden = baseTokens.Override(new DesignTokenSet(
///     colors: new Dictionary&lt;string, ColorToken&gt; { ["primary-500"] = customColor }
/// ));
/// </code>
/// </example>
public static class TokenSetExtensions
{
    /// <summary>
    /// Merges the <paramref name="source"/> token set into <paramref name="base"/>,
    /// with <paramref name="source"/> values taking precedence over <paramref name="base"/> values
    /// when duplicate keys exist.
    /// </summary>
    /// <param name="base">The base token set.</param>
    /// <param name="source">The source token set whose values take precedence.</param>
    /// <returns>A new <see cref="DesignTokenSet"/> with merged values.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="base"/> or <paramref name="source"/> is <c>null</c>.</exception>
    public static DesignTokenSet Merge(this DesignTokenSet @base, DesignTokenSet source)
    {
        ArgumentNullException.ThrowIfNull(@base);
        ArgumentNullException.ThrowIfNull(source);

        return new DesignTokenSet(
            colors: MergeDictionaries(@base.Colors, source.Colors),
            spacing: MergeDictionaries(@base.Spacing, source.Spacing),
            typography: MergeDictionaries(@base.Typography, source.Typography),
            borders: MergeDictionaries(@base.Borders, source.Borders),
            shadows: MergeDictionaries(@base.Shadows, source.Shadows),
            breakpoints: MergeDictionaries(@base.Breakpoints, source.Breakpoints));
    }

    /// <summary>
    /// Overrides the <paramref name="base"/> token set with values from <paramref name="overrides"/>,
    /// where <paramref name="overrides"/> values take precedence over <paramref name="base"/> values.
    /// This is an alias for <see cref="Merge"/> with reversed semantics documentation.
    /// </summary>
    /// <param name="base">The base token set.</param>
    /// <param name="overrides">The override token set whose values take precedence.</param>
    /// <returns>A new <see cref="DesignTokenSet"/> with overridden values.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="base"/> or <paramref name="overrides"/> is <c>null</c>.</exception>
    public static DesignTokenSet Override(this DesignTokenSet @base, DesignTokenSet overrides)
    {
        ArgumentNullException.ThrowIfNull(@base);
        ArgumentNullException.ThrowIfNull(overrides);

        return @base.Merge(overrides);
    }

    /// <summary>
    /// Gets a color token by name, or <c>null</c> if not found.
    /// </summary>
    /// <param name="set">The token set to search.</param>
    /// <param name="name">The name of the color token.</param>
    /// <returns>The <see cref="ColorToken"/> if found; otherwise <c>null</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="set"/> is <c>null</c>.</exception>
    public static ColorToken? GetColor(this DesignTokenSet set, string name)
    {
        ArgumentNullException.ThrowIfNull(set);
        return set.Colors.TryGetValue(name, out var token) ? token : null;
    }

    /// <summary>
    /// Gets a spacing token by name, or <c>null</c> if not found.
    /// </summary>
    /// <param name="set">The token set to search.</param>
    /// <param name="name">The name of the spacing token.</param>
    /// <returns>The <see cref="SpacingToken"/> if found; otherwise <c>null</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="set"/> is <c>null</c>.</exception>
    public static SpacingToken? GetSpacing(this DesignTokenSet set, string name)
    {
        ArgumentNullException.ThrowIfNull(set);
        return set.Spacing.TryGetValue(name, out var token) ? token : null;
    }

    /// <summary>
    /// Gets a typography token by name, or <c>null</c> if not found.
    /// </summary>
    /// <param name="set">The token set to search.</param>
    /// <param name="name">The name of the typography token.</param>
    /// <returns>The <see cref="TypographyToken"/> if found; otherwise <c>null</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="set"/> is <c>null</c>.</exception>
    public static TypographyToken? GetTypography(this DesignTokenSet set, string name)
    {
        ArgumentNullException.ThrowIfNull(set);
        return set.Typography.TryGetValue(name, out var token) ? token : null;
    }

    /// <summary>
    /// Gets a border token by name, or <c>null</c> if not found.
    /// </summary>
    /// <param name="set">The token set to search.</param>
    /// <param name="name">The name of the border token.</param>
    /// <returns>The <see cref="BorderToken"/> if found; otherwise <c>null</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="set"/> is <c>null</c>.</exception>
    public static BorderToken? GetBorder(this DesignTokenSet set, string name)
    {
        ArgumentNullException.ThrowIfNull(set);
        return set.Borders.TryGetValue(name, out var token) ? token : null;
    }

    /// <summary>
    /// Gets a shadow token by name, or <c>null</c> if not found.
    /// </summary>
    /// <param name="set">The token set to search.</param>
    /// <param name="name">The name of the shadow token.</param>
    /// <returns>The <see cref="ShadowToken"/> if found; otherwise <c>null</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="set"/> is <c>null</c>.</exception>
    public static ShadowToken? GetShadow(this DesignTokenSet set, string name)
    {
        ArgumentNullException.ThrowIfNull(set);
        return set.Shadows.TryGetValue(name, out var token) ? token : null;
    }

    /// <summary>
    /// Gets a breakpoint token by name, or <c>null</c> if not found.
    /// </summary>
    /// <param name="set">The token set to search.</param>
    /// <param name="name">The name of the breakpoint token.</param>
    /// <returns>The <see cref="BreakpointToken"/> if found; otherwise <c>null</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="set"/> is <c>null</c>.</exception>
    public static BreakpointToken? GetBreakpoint(this DesignTokenSet set, string name)
    {
        ArgumentNullException.ThrowIfNull(set);
        return set.Breakpoints.TryGetValue(name, out var token) ? token : null;
    }

    private static Dictionary<string, T> MergeDictionaries<T>(IReadOnlyDictionary<string, T> first, IReadOnlyDictionary<string, T> second)
    {
        var result = new Dictionary<string, T>(first);
        foreach (var (key, value) in second)
        {
            result[key] = value;
        }
        return result;
    }
}
