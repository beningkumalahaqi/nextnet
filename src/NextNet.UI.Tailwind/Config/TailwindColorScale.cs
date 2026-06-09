using NextNet.DesignSystem.Tokens;

namespace NextNet.UI.Tailwind.Config;

/// <summary>
/// Generates Tailwind CSS color scale objects from <see cref="ColorToken"/> collections.
/// Produces a dictionary mapping color family names (e.g., <c>"primary"</c>, <c>"gray"</c>)
/// to nested dictionaries of shade numbers (e.g., <c>"50"</c>, <c>"500"</c>, <c>"900"</c>)
/// and their hex color values.
/// </summary>
/// <remarks>
/// <para>
/// This utility parses color tokens whose names follow the pattern <c>{family}-{shade}</c>
/// (e.g., <c>"primary-500"</c>, <c>"gray-100"</c>) and groups them into family/shade
/// dictionaries suitable for Tailwind's <c>theme.extend.colors</c> configuration.
/// </para>
/// <para>
/// Tokens that do not follow the <c>{family}-{shade}</c> pattern are treated as
/// standalone color values and mapped directly using their full name.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var colors = new Dictionary&lt;string, ColorToken&gt;
/// {
///     ["primary-500"] = new ColorToken("primary-500", "#3B82F6"),
///     ["primary-600"] = new ColorToken("primary-600", "#2563EB"),
///     ["gray-100"] = new ColorToken("gray-100", "#F3F4F6")
/// };
///
/// var scale = TailwindColorScale.Generate(colors);
/// // Result:
/// // {
/// //   ["primary"] = { ["500"] = "#3B82F6", ["600"] = "#2563EB" },
/// //   ["gray"] = { ["100"] = "#F3F4F6" }
/// // }
/// </code>
/// </example>
public static class TailwindColorScale
{
    /// <summary>
    /// Generates a Tailwind-compatible color scale dictionary from the specified color tokens.
    /// </summary>
    /// <param name="colorTokens">The collection of color tokens keyed by name.</param>
    /// <returns>
    /// A dictionary mapping color family names to either a string hex value (for standalone tokens)
    /// or a nested dictionary of shade-to-hex mappings.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="colorTokens"/> is null.</exception>
    public static Dictionary<string, object> Generate(IReadOnlyDictionary<string, ColorToken> colorTokens)
    {
        ArgumentNullException.ThrowIfNull(colorTokens);

        var result = new Dictionary<string, object>();
        var families = new Dictionary<string, Dictionary<string, string>>();

        foreach (var (name, token) in colorTokens)
        {
            var lastDash = name.LastIndexOf('-');
            if (lastDash > 0 && lastDash < name.Length - 1)
            {
                var family = name[..lastDash];
                var shade = name[(lastDash + 1)..];

                if (!families.TryGetValue(family, out var shades))
                {
                    shades = new Dictionary<string, string>();
                    families[family] = shades;
                }

                shades[shade] = token.Value;
            }
            else
            {
                // Standalone token, map directly
                result[name] = token.Value;
            }
        }

        foreach (var (family, shades) in families)
        {
            result[family] = shades;
        }

        return result;
    }
}
