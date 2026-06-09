using NextNet.DesignSystem.Tokens;

namespace NextNet.UI.Tailwind.Config;

/// <summary>
/// Generates a Tailwind CSS spacing scale from <see cref="SpacingToken"/> collections.
/// Produces a dictionary mapping spacing key names (e.g., <c>"0"</c>, <c>"4"</c>, <c>"px"</c>)
/// to their CSS length values.
/// </summary>
/// <remarks>
/// <para>
/// Spacing tokens with names following the pattern <c>spacing-{key}</c> (e.g., <c>"spacing-4"</c>)
/// are normalized to use just <c>{key}</c> (e.g., <c>"4"</c>) as the Tailwind key. Tokens with
/// other names are used as-is.
/// </para>
/// <para>
/// The resulting dictionary is suitable for use in Tailwind's <c>theme.extend.spacing</c>
/// configuration section.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var spacing = new Dictionary&lt;string, SpacingToken&gt;
/// {
///     ["spacing-0"] = new SpacingToken("spacing-0", "0px"),
///     ["spacing-1"] = new SpacingToken("spacing-1", "0.25rem"),
///     ["spacing-4"] = new SpacingToken("spacing-4", "1rem"),
///     ["custom-gap"] = new SpacingToken("custom-gap", "2rem")
/// };
///
/// var scale = TailwindSpacingScale.Generate(spacing);
/// // Result:
/// // {
/// //   ["0"] = "0px",
/// //   ["1"] = "0.25rem",
/// //   ["4"] = "1rem",
/// //   ["custom-gap"] = "2rem"
/// // }
/// </code>
/// </example>
public static class TailwindSpacingScale
{
    private const string SpacingPrefix = "spacing-";

    /// <summary>
    /// Generates a Tailwind-compatible spacing scale dictionary from the specified spacing tokens.
    /// </summary>
    /// <param name="spacingTokens">The collection of spacing tokens keyed by name.</param>
    /// <returns>
    /// A dictionary mapping spacing key names to CSS length values.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="spacingTokens"/> is null.</exception>
    public static Dictionary<string, string> Generate(IReadOnlyDictionary<string, SpacingToken> spacingTokens)
    {
        ArgumentNullException.ThrowIfNull(spacingTokens);

        var result = new Dictionary<string, string>();

        foreach (var (name, token) in spacingTokens)
        {
            var key = name.StartsWith(SpacingPrefix, StringComparison.Ordinal)
                ? name[SpacingPrefix.Length..]
                : name;

            result[key] = token.Value;
        }

        return result;
    }
}
