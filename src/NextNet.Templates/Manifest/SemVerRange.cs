namespace NextNet.Templates.Manifest;

/// <summary>
/// Represents a parsed Semantic Versioning 2.0 range expression that can be evaluated
/// against a specific SDK version to determine compatibility.
/// </summary>
/// <remarks>
/// <para>
/// A <see cref="SemVerRange"/> consists of a set of AND-ed <see cref="SemVerRangeClause"/>
/// conditions, with optional OR-ed <see cref="Alternatives"/>. The range grammar supports
/// exact versions, comparison operators (<c>&gt;=</c>, <c>&lt;=</c>, <c>&gt;</c>, <c>&lt;</c>),
/// caret (<c>^</c>), and tilde (<c>~</c>) ranges.
/// </para>
/// <para>
/// During parsing, caret and tilde operators are expanded into comparison clauses:
/// <c>^1.0.0</c> becomes <c>&gt;=1.0.0 &lt;2.0.0</c>, and <c>~1.0.0</c> becomes
/// <c>&gt;=1.0.0 &lt;1.1.0</c>.
/// </para>
/// <example>
/// <code>
/// // Range: >=1.0.0 &lt;2.0.0 || &gt;=3.0.0
/// var range = new SemVerRange
/// {
///     Clauses = new[] { new SemVerRangeClause(">=", new Version(1, 0, 0)) },
///     Alternatives = new[]
///     {
///         new SemVerRange
///         {
///             Clauses = new[] { new SemVerRangeClause(">=", new Version(3, 0, 0)) }
///         }
///     }
/// };
/// </code>
/// </example>
/// </remarks>
public sealed record SemVerRange
{
    /// <summary>
    /// Gets the AND-ed clauses that must all be satisfied for this range group.
    /// </summary>
    public IReadOnlyList<SemVerRangeClause> Clauses { get; init; } = Array.Empty<SemVerRangeClause>();

    /// <summary>
    /// Gets the OR-ed alternative range groups. If this is non-empty, any alternative
    /// group can satisfy the overall range in addition to the root <see cref="Clauses"/>.
    /// </summary>
    public IReadOnlyList<SemVerRange>? Alternatives { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SemVerRange"/> class.
    /// </summary>
    public SemVerRange() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="SemVerRange"/> class with the specified clauses.
    /// </summary>
    /// <param name="clauses">The AND-ed clauses for this range.</param>
    public SemVerRange(IReadOnlyList<SemVerRangeClause> clauses)
    {
        Clauses = clauses;
    }
}

/// <summary>
/// Represents a single clause within a SemVer range expression, consisting of an operator
/// and a version.
/// </summary>
/// <remarks>
/// <para>
/// Supported operators:
/// <list type="bullet">
///   <item><c>""</c> (empty) — exact version match.</item>
///   <item><c>&gt;=</c> — greater than or equal to the version.</item>
///   <item><c>&lt;=</c> — less than or equal to the version.</item>
///   <item><c>&gt;</c> — strictly greater than the version.</item>
///   <item><c>&lt;</c> — strictly less than the version.</item>
/// </list>
/// </para>
/// <para>
/// Caret (<c>^</c>) and tilde (<c>~</c>) operators are expanded into multiple comparison
/// clauses during range parsing and do not appear in the final clause list.
/// </para>
/// </remarks>
/// <param name="Operator">The comparison operator. Empty string means exact match.</param>
/// <param name="Version">The version to compare against.</param>
public sealed record SemVerRangeClause(
    string Operator,
    Version Version
);
