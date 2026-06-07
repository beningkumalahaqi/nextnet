namespace NextNet.Data.MongoDB.Internal;

/// <summary>
/// Provides English pluralization for collection name resolution.
/// Handles regular and common irregular pluralization rules.
/// </summary>
/// <remarks>
/// <para>
/// The pluralizer applies standard English pluralization rules:
/// <list type="bullet">
///   <item><description>Most nouns: append "s" (User → Users)</description></item>
///   <item><description>Ending in -y: change to -ies (Category → Categories)</description></item>
///   <item><description>Ending in -s, -x, -ch, -sh: append "es" (Status → Statuses, Box → Boxes)</description></item>
///   <item><description>Irregular forms: hardcoded map (Person → People)</description></item>
/// </list>
/// </para>
/// <para>
/// The output preserves PascalCase for the first letter, which is then
/// converted to camelCase by <see cref="CollectionNameResolver"/>.
/// </para>
/// </remarks>
internal static class Pluralizer
{
    private static readonly Dictionary<string, string> IrregularPlurals = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Person"] = "People",
        ["Child"] = "Children",
        ["Man"] = "Men",
        ["Woman"] = "Women",
        ["Mouse"] = "Mice",
        ["Goose"] = "Geese",
        ["Foot"] = "Feet",
        ["Tooth"] = "Teeth",
        ["Ox"] = "Oxen",
    };

    /// <summary>
    /// Pluralizes the given singular noun.
    /// </summary>
    /// <param name="singular">The singular form of the noun.</param>
    /// <returns>The pluralized form, preserving the original casing.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="singular"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="singular"/> is empty or whitespace.</exception>
    public static string Pluralize(string singular)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(singular);

        // Check irregular plurals first
        if (IrregularPlurals.TryGetValue(singular, out var irregular))
        {
            return irregular;
        }

        // Rules for common endings
        if (singular.EndsWith("s", StringComparison.OrdinalIgnoreCase) ||
            singular.EndsWith("x", StringComparison.OrdinalIgnoreCase) ||
            singular.EndsWith("ch", StringComparison.OrdinalIgnoreCase) ||
            singular.EndsWith("sh", StringComparison.OrdinalIgnoreCase))
        {
            return singular + "es";
        }

        // -y → -ies (but only when preceded by a consonant)
        if (singular.EndsWith("y", StringComparison.OrdinalIgnoreCase) && singular.Length > 1)
        {
            var precedingChar = singular[singular.Length - 2];
            if (!IsVowel(precedingChar))
            {
                return singular[..^1] + "ies";
            }
        }

        // Default: append "s"
        return singular + "s";
    }

    private static bool IsVowel(char c) => c is 'a' or 'e' or 'i' or 'o' or 'u' or 'A' or 'E' or 'I' or 'O' or 'U';

    /// <summary>
    /// Converts the first character of the given string to lowercase (camelCase).
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <returns>The string with first character lowercased.</returns>
    public static string ToCamelCase(string value)
    {
        if (string.IsNullOrEmpty(value) || char.IsLower(value[0]))
        {
            return value;
        }

        return char.ToLowerInvariant(value[0]) + value[1..];
    }
}
