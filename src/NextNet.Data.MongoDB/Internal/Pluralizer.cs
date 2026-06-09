using NextNet.Data.Abstractions.Internal;

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
    /// <summary>
    /// Pluralizes the given singular noun.
    /// Delegates to the canonical <see cref="NextNet.Data.Abstractions.Internal.Pluralizer"/> implementation.
    /// </summary>
    /// <param name="singular">The singular form of the noun.</param>
    /// <returns>The pluralized form, preserving the original casing.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="singular"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="singular"/> is empty or whitespace.</exception>
    public static string Pluralize(string singular)
    {
        ArgumentNullException.ThrowIfNull(singular);

        if (string.IsNullOrWhiteSpace(singular))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(singular));

        return Abstractions.Internal.Pluralizer.Pluralize(singular);
    }

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
