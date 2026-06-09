namespace NextNet.Data.Abstractions.Internal;

/// <summary>
/// Provides English pluralization for entity and table name resolution.
/// Handles regular and common irregular pluralization rules.
/// </summary>
/// <remarks>
/// <para>
/// The pluralizer applies standard English pluralization rules:
/// <list type="bullet">
///   <item><description>Most nouns: append "s" (User → Users)</description></item>
///   <item><description>Ending in -y preceded by consonant: change -y to -ies (Category → Categories)</description></item>
///   <item><description>Ending in -s, -x, -z, -ch, -sh: append "es" (Status → Statuses, Box → Boxes)</description></item>
///   <item><description>Ending in -f or -fe: change to -ves (Wolf → Wolves, Knife → Knives)</description></item>
///   <item><description>Irregular forms: hardcoded map (Person → People, Child → Children)</description></item>
/// </list>
/// </para>
/// <para>
/// This is the canonical implementation used across all NextNet data providers.
/// The returned value preserves the casing pattern of the input (e.g., "Bus" → "Buses", "bus" → "buses").
/// </para>
/// </remarks>
internal static class Pluralizer
{
    // Common irregular plurals (lowercase keys and values; casing is applied from input)
    private static readonly Dictionary<string, string> IrregularPlurals = new(StringComparer.OrdinalIgnoreCase)
    {
        ["person"] = "people",
        ["man"] = "men",
        ["woman"] = "women",
        ["child"] = "children",
        ["foot"] = "feet",
        ["tooth"] = "teeth",
        ["goose"] = "geese",
        ["mouse"] = "mice",
        ["deer"] = "deer",
        ["sheep"] = "sheep",
        ["fish"] = "fish",
        ["ox"] = "oxen",
        ["index"] = "indices",
        ["matrix"] = "matrices",
        ["vertex"] = "vertices",
        ["crisis"] = "crises",
        ["analysis"] = "analyses",
        ["thesis"] = "theses",
        ["status"] = "statuses",
        ["bus"] = "buses",
        ["class"] = "classes",
        ["address"] = "addresses",
        ["alias"] = "aliases",
    };

    /// <summary>
    /// Pluralizes the given singular noun.
    /// </summary>
    /// <param name="singular">The singular form of the noun.</param>
    /// <returns>The pluralized form, preserving the original casing of the first letter.</returns>
    public static string Pluralize(string singular)
    {
        if (string.IsNullOrEmpty(singular))
            return singular;

        // Check irregular plurals first
        if (IrregularPlurals.TryGetValue(singular, out var irregular))
            return PreserveCase(singular, irregular);

        // Ends with "s", "x", "z", "ch", "sh" → add "es"
        if (singular.EndsWith("s", StringComparison.OrdinalIgnoreCase) ||
            singular.EndsWith("x", StringComparison.OrdinalIgnoreCase) ||
            singular.EndsWith("z", StringComparison.OrdinalIgnoreCase) ||
            singular.EndsWith("ch", StringComparison.OrdinalIgnoreCase) ||
            singular.EndsWith("sh", StringComparison.OrdinalIgnoreCase))
        {
            return singular + "es";
        }

        // Ends with "y" preceded by consonant → "ies"
        if (singular.EndsWith("y", StringComparison.OrdinalIgnoreCase) && singular.Length > 1)
        {
            var precedingChar = singular[singular.Length - 2];
            if (!IsVowel(precedingChar))
            {
                return singular[..^1] + "ies";
            }
        }

        // Ends with "fe" → "ves"
        if (singular.EndsWith("fe", StringComparison.OrdinalIgnoreCase))
            return singular[..^2] + "ves";

        // Ends with "f" (but not "ff") → "ves"
        if (singular.EndsWith("f", StringComparison.OrdinalIgnoreCase) &&
            !singular.EndsWith("ff", StringComparison.OrdinalIgnoreCase))
            return singular[..^1] + "ves";

        // Default: add "s"
        return singular + "s";
    }

    /// <summary>
    /// Applies the casing pattern of <paramref name="input"/> to <paramref name="value"/>.
    /// If the first character of input is uppercase, the first character of value is also uppercased.
    /// </summary>
    private static string PreserveCase(string input, string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        if (input.Length > 0 && char.IsUpper(input[0]))
        {
            return char.ToUpperInvariant(value[0]) + value[1..];
        }

        return value;
    }

    private static bool IsVowel(char c) => c is 'a' or 'e' or 'i' or 'o' or 'u' or 'A' or 'E' or 'I' or 'O' or 'U';
}
