using NextNet.DesignSystem.Defaults;
using NextNet.DesignSystem.Tokens;

namespace NextNet.UI.Tailwind.Config;

/// <summary>
/// Generates a complete <see cref="TailwindConfig"/> from a <see cref="DesignTokenSet"/>,
/// including color scales, spacing scales, and typography configuration.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="TailwindConfigGenerator"/> is the primary entry point for converting NextNet
/// design tokens into a Tailwind CSS configuration. It coordinates the individual scale
/// generators (<see cref="TailwindColorScale"/>, <see cref="TailwindSpacingScale"/>,
/// <see cref="TailwindTypographyScale"/>) and populates a <see cref="TailwindConfig"/>
/// instance.
/// </para>
/// <para>
/// When no token set is provided (or <c>null</c> is passed), the generator falls back
/// to the default token set from <see cref="DefaultTokens.Create"/>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var tokenSet = new DesignTokenSet(
///     colors: new Dictionary&lt;string, ColorToken&gt; { ["primary-500"] = new ColorToken("primary-500", "#3B82F6") },
///     spacing: new Dictionary&lt;string, SpacingToken&gt; { ["spacing-4"] = new SpacingToken("spacing-4", "1rem") }
/// );
///
/// var config = TailwindConfigGenerator.Generate(tokenSet);
/// var jsModule = config.ToJsModuleString();
/// // Produces a complete tailwind.config.js content
/// </code>
/// </example>
public static class TailwindConfigGenerator
{
    /// <summary>
    /// Generates a <see cref="TailwindConfig"/> from the specified design token set.
    /// Uses <see cref="DefaultTokens.Create"/> when <paramref name="tokenSet"/> is <c>null</c>.
    /// </summary>
    /// <param name="tokenSet">The design token set to convert. If <c>null</c>, the default token set is used.</param>
    /// <param name="contentPaths">
    /// Optional content paths for the Tailwind config. Defaults to <c>["./**/*.{html,cshtml,razor}"]</c>.
    /// </param>
    /// <param name="safelistPatterns">Optional safelist patterns to include in the Tailwind config.</param>
    /// <returns>A populated <see cref="TailwindConfig"/> instance.</returns>
    public static TailwindConfig Generate(
        DesignTokenSet? tokenSet = null,
        IReadOnlyList<string>? contentPaths = null,
        IReadOnlyList<string>? safelistPatterns = null)
    {
        var tokens = tokenSet ?? DefaultTokens.Create();

        var colors = TailwindColorScale.Generate(tokens.Colors);
        var spacing = TailwindSpacingScale.Generate(tokens.Spacing);
        var typography = TailwindTypographyScale.Generate(tokens.Typography);

        return new TailwindConfig
        {
            ContentPaths = contentPaths ?? new[] { "./**/*.{html,cshtml,razor}" },
            SafelistPatterns = safelistPatterns ?? Array.Empty<string>(),
            Colors = colors,
            Spacing = spacing,
            FontFamilies = typography.FontFamilies,
            FontSizes = typography.FontSizes,
            FontWeights = typography.FontWeights
        };
    }
}
