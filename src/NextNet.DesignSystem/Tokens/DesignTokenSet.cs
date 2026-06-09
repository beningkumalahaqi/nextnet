namespace NextNet.DesignSystem.Tokens;

/// <summary>
/// Aggregates all design token categories into a single immutable set.
/// </summary>
/// <remarks>
/// <see cref="DesignTokenSet"/> is the top-level container for a complete set of design tokens.
/// Each property is a read-only dictionary keyed by the token's <c>Name</c> for fast lookup.
/// Instances are created via constructors, the <see cref="Defaults.DefaultTokens.Create()"/>
/// factory, or by parsing token files with <see cref="Parsing.TokenParser"/>.
/// </remarks>
/// <example>
/// <code>
/// var tokens = new DesignTokenSet(
///     colors: new Dictionary&lt;string, ColorToken&gt;
///     {
///         ["primary-500"] = new ColorToken("primary-500", "#3B82F6")
///     },
///     spacing: new Dictionary&lt;string, SpacingToken&gt;
///     {
///         ["spacing-4"] = new SpacingToken("spacing-4", "1rem")
///     }
/// );
/// </code>
/// </example>
public sealed class DesignTokenSet : IEquatable<DesignTokenSet>
{
    /// <summary>
    /// Initializes a new instance of <see cref="DesignTokenSet"/> with the specified token collections.
    /// All parameters are optional and default to an empty dictionary when <c>null</c>.
    /// </summary>
    /// <param name="colors">Color tokens keyed by name. Defaults to empty.</param>
    /// <param name="spacing">Spacing tokens keyed by name. Defaults to empty.</param>
    /// <param name="typography">Typography tokens keyed by name. Defaults to empty.</param>
    /// <param name="borders">Border tokens keyed by name. Defaults to empty.</param>
    /// <param name="shadows">Shadow tokens keyed by name. Defaults to empty.</param>
    /// <param name="breakpoints">Breakpoint tokens keyed by name. Defaults to empty.</param>
    public DesignTokenSet(
        IReadOnlyDictionary<string, ColorToken>? colors = null,
        IReadOnlyDictionary<string, SpacingToken>? spacing = null,
        IReadOnlyDictionary<string, TypographyToken>? typography = null,
        IReadOnlyDictionary<string, BorderToken>? borders = null,
        IReadOnlyDictionary<string, ShadowToken>? shadows = null,
        IReadOnlyDictionary<string, BreakpointToken>? breakpoints = null)
    {
        Colors = colors ?? new Dictionary<string, ColorToken>();
        Spacing = spacing ?? new Dictionary<string, SpacingToken>();
        Typography = typography ?? new Dictionary<string, TypographyToken>();
        Borders = borders ?? new Dictionary<string, BorderToken>();
        Shadows = shadows ?? new Dictionary<string, ShadowToken>();
        Breakpoints = breakpoints ?? new Dictionary<string, BreakpointToken>();
    }

    /// <summary>
    /// Gets the collection of color tokens keyed by name.
    /// </summary>
    public IReadOnlyDictionary<string, ColorToken> Colors { get; init; }

    /// <summary>
    /// Gets the collection of spacing tokens keyed by name.
    /// </summary>
    public IReadOnlyDictionary<string, SpacingToken> Spacing { get; init; }

    /// <summary>
    /// Gets the collection of typography tokens keyed by name.
    /// </summary>
    public IReadOnlyDictionary<string, TypographyToken> Typography { get; init; }

    /// <summary>
    /// Gets the collection of border tokens keyed by name.
    /// </summary>
    public IReadOnlyDictionary<string, BorderToken> Borders { get; init; }

    /// <summary>
    /// Gets the collection of shadow tokens keyed by name.
    /// </summary>
    public IReadOnlyDictionary<string, ShadowToken> Shadows { get; init; }

    /// <summary>
    /// Gets the collection of breakpoint tokens keyed by name.
    /// </summary>
    public IReadOnlyDictionary<string, BreakpointToken> Breakpoints { get; init; }

    /// <inheritdoc />
    public bool Equals(DesignTokenSet? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return DictionaryEqual(Colors, other.Colors)
            && DictionaryEqual(Spacing, other.Spacing)
            && DictionaryEqual(Typography, other.Typography)
            && DictionaryEqual(Borders, other.Borders)
            && DictionaryEqual(Shadows, other.Shadows)
            && DictionaryEqual(Breakpoints, other.Breakpoints);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is DesignTokenSet other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(DictionaryHashCode(Colors));
        hash.Add(DictionaryHashCode(Spacing));
        hash.Add(DictionaryHashCode(Typography));
        hash.Add(DictionaryHashCode(Borders));
        hash.Add(DictionaryHashCode(Shadows));
        hash.Add(DictionaryHashCode(Breakpoints));
        return hash.ToHashCode();
    }

    /// <summary>
    /// Determines whether two specified <see cref="DesignTokenSet"/> instances are equal.
    /// </summary>
    /// <param name="left">The first instance.</param>
    /// <param name="right">The second instance.</param>
    /// <returns><c>true</c> if the instances are equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(DesignTokenSet? left, DesignTokenSet? right)
        => EqualityComparer<DesignTokenSet>.Default.Equals(left, right);

    /// <summary>
    /// Determines whether two specified <see cref="DesignTokenSet"/> instances are not equal.
    /// </summary>
    /// <param name="left">The first instance.</param>
    /// <param name="right">The second instance.</param>
    /// <returns><c>true</c> if the instances are not equal; otherwise, <c>false</c>.</returns>
    public static bool operator !=(DesignTokenSet? left, DesignTokenSet? right)
        => !(left == right);

    private static bool DictionaryEqual<T>(IReadOnlyDictionary<string, T> a, IReadOnlyDictionary<string, T> b)
        where T : class
    {
        if (a.Count != b.Count) return false;

        foreach (var (key, value) in a)
        {
            if (!b.TryGetValue(key, out var otherValue)) return false;
            if (!EqualityComparer<T>.Default.Equals(value, otherValue)) return false;
        }

        return true;
    }

    private static int DictionaryHashCode<T>(IReadOnlyDictionary<string, T> dict)
        where T : class
    {
        var hash = new HashCode();
        foreach (var (key, value) in dict.OrderBy(kvp => kvp.Key, StringComparer.Ordinal))
        {
            hash.Add(key);
            hash.Add(value);
        }
        return hash.ToHashCode();
    }
}
