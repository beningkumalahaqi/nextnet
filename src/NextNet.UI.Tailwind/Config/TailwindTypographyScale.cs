using NextNet.DesignSystem.Tokens;

namespace NextNet.UI.Tailwind.Config;

/// <summary>
/// Generates Tailwind CSS typography configuration from <see cref="TypographyToken"/> collections.
/// Produces separate dictionaries for font families, font sizes, and font weights suitable for
/// Tailwind's <c>theme.extend</c> configuration sections.
/// </summary>
/// <remarks>
/// <para>
/// Typography tokens are parsed and grouped into three categories:
/// </para>
/// <list type="bullet">
///   <item><description>Font families — extracted from the <see cref="TypographyToken.FontFamily"/> property.</description></item>
///   <item><description>Font sizes — extracted from the <see cref="TypographyToken.FontSize"/> property.</description></item>
///   <item><description>Font weights — extracted from the <see cref="TypographyToken.FontWeight"/> property.</description></item>
/// </list>
/// <para>
/// Each category is returned as a separate dictionary that can be independently
/// added to the Tailwind config.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var typography = new Dictionary&lt;string, TypographyToken&gt;
/// {
///     ["body-base"] = new TypographyToken("body-base", "Inter, sans-serif", "1rem", "400", "1.5", "normal"),
///     ["heading-xl"] = new TypographyToken("heading-xl", "Inter, sans-serif", "1.25rem", "600", "1.75", "-0.01em")
/// };
///
/// var result = TailwindTypographyScale.Generate(typography);
/// // result.FontFamilies: { ["sans"] = "Inter, sans-serif" }
/// // result.FontSizes:    { ["body-base"] = "1rem", ["heading-xl"] = "1.25rem" }
/// // result.FontWeights:  { ["body-base"] = "400", ["heading-xl"] = "600" }
/// </code>
/// </example>
public static class TailwindTypographyScale
{
    /// <summary>
    /// Holds the generated typography configuration split into font families, sizes, and weights.
    /// </summary>
    /// <param name="FontFamilies">Font family names mapped to their CSS font stacks.</param>
    /// <param name="FontSizes">Token names mapped to their CSS font-size values.</param>
    /// <param name="FontWeights">Token names mapped to their CSS font-weight values.</param>
    public sealed record TypographyScaleResult(
        IReadOnlyDictionary<string, string> FontFamilies,
        IReadOnlyDictionary<string, string> FontSizes,
        IReadOnlyDictionary<string, string> FontWeights);

    /// <summary>
    /// Generates Tailwind-compatible typography configuration from the specified typography tokens.
    /// </summary>
    /// <param name="typographyTokens">The collection of typography tokens keyed by name.</param>
    /// <returns>
    /// A <see cref="TypographyScaleResult"/> containing the font family, font size, and font weight mappings.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="typographyTokens"/> is null.</exception>
    public static TypographyScaleResult Generate(IReadOnlyDictionary<string, TypographyToken> typographyTokens)
    {
        ArgumentNullException.ThrowIfNull(typographyTokens);

        var fontFamilies = new Dictionary<string, string>();
        var fontSizes = new Dictionary<string, string>();
        var fontWeights = new Dictionary<string, string>();

        // Collect unique font families from all typography tokens
        var uniqueFamilies = new HashSet<string>(StringComparer.Ordinal);

        foreach (var (name, token) in typographyTokens)
        {
            // Skip tokens with empty font family or size
            if (!string.IsNullOrWhiteSpace(token.FontFamily))
            {
                uniqueFamilies.Add(token.FontFamily);
            }

            if (!string.IsNullOrWhiteSpace(token.FontSize))
            {
                fontSizes[name] = token.FontSize;
            }

            if (!string.IsNullOrWhiteSpace(token.FontWeight))
            {
                fontWeights[name] = token.FontWeight;
            }
        }

        // Map unique font families to Tailwind-style keys (sans, serif, mono)
        var familyIndex = 0;
        foreach (var family in uniqueFamilies.OrderBy(f => f, StringComparer.Ordinal))
        {
            var key = familyIndex switch
            {
                0 => "sans",
                1 => "serif",
                2 => "mono",
                _ => $"family-{familyIndex}"
            };
            fontFamilies[key] = family;
            familyIndex++;
        }

        return new TypographyScaleResult(fontFamilies, fontSizes, fontWeights);
    }
}
