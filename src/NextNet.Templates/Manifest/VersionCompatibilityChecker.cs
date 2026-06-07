using System.Text.RegularExpressions;
using NextNet.Templates.Models;

namespace NextNet.Templates.Manifest;

/// <summary>
/// Evaluates whether a <see cref="TemplateManifest"/>'s <c>NextNetVersion</c> constraint
/// is satisfied by a given NextNet SDK version.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="VersionCompatibilityChecker"/> parses the SemVer range expression from the
/// manifest's <c>NextNetVersion</c> property and checks it against a specified SDK version.
/// The range expression supports exact versions, comparison operators (<c>&gt;=</c>, <c>&lt;=</c>,
/// <c>&gt;</c>, <c>&lt;</c>), caret (<c>^</c>), tilde (<c>~</c>), AND (space-separated), and
/// OR (<c>||</c>) combinations.
/// </para>
/// <para>
/// Caret and tilde ranges are expanded according to SemVer conventions:
/// <list type="bullet">
///   <item><c>^1.0.0</c> expands to <c>&gt;=1.0.0 &lt;2.0.0</c> (major &gt; 0: allows minor/patch changes).</item>
///   <item><c>^0.1.0</c> expands to <c>&gt;=0.1.0 &lt;0.2.0</c> (major = 0, minor &gt; 0: locks minor).</item>
///   <item><c>^0.0.1</c> expands to <c>&gt;=0.0.1 &lt;0.0.2</c> (major = 0, minor = 0: locks patch).</item>
///   <item><c>~1.0.0</c> expands to <c>&gt;=1.0.0 &lt;1.1.0</c> (allows patch changes only).</item>
/// </list>
/// </para>
/// <example>
/// <code>
/// var checker = new VersionCompatibilityChecker();
/// var manifest = new TemplateManifest("my-template", "1.0.0", "^3.0.0");
/// var result = checker.IsCompatible(manifest, new Version(3, 5, 0));
///
/// if (result.IsCompatible)
///     Console.WriteLine("Template is compatible with SDK 3.5.0");
/// else
///     Console.WriteLine(result.Message);
/// </code>
/// </example>
/// </remarks>
public sealed class VersionCompatibilityChecker
{
    private static readonly Regex RangeExpressionPattern = new(
        @"^\s*(?:\^|~|>=?|<=?)?\s*\d+\.\d+\.\d+\s*",
        RegexOptions.Compiled);

    private static readonly Regex VersionPattern = new(
        @"(\d+)\.(\d+)\.(\d+)",
        RegexOptions.Compiled);

    /// <summary>
    /// Determines whether the specified SDK version satisfies the template manifest's
    /// <c>NextNetVersion</c> constraint.
    /// </summary>
    /// <param name="manifest">The template manifest containing the <c>NextNetVersion</c> constraint.</param>
    /// <param name="sdkVersion">The NextNet SDK version to check.</param>
    /// <returns>A <see cref="CompatibilityResult"/> describing whether the versions are compatible.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="manifest"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when the manifest's <c>NextNetVersion</c> is not a valid SemVer range.</exception>
    public CompatibilityResult IsCompatible(TemplateManifest manifest, Version sdkVersion)
    {
        if (manifest is null)
            throw new ArgumentNullException(nameof(manifest));

        if (string.IsNullOrWhiteSpace(manifest.NextNetVersion))
        {
            throw new ArgumentException(
                "Template manifest NextNetVersion is not set. A valid SemVer range is required.",
                nameof(manifest));
        }

        var range = ParseRange(manifest.NextNetVersion);
        var isCompatible = EvaluateRange(range, sdkVersion);

        return new CompatibilityResult(
            isCompatible,
            manifest.Version,
            manifest.NextNetVersion,
            sdkVersion,
            isCompatible
                ? null
                : $"SDK version {sdkVersion} does not satisfy the constraint '{manifest.NextNetVersion}'."
        );
    }

    /// <summary>
    /// Parses a SemVer range expression string into a <see cref="SemVerRange"/>.
    /// </summary>
    /// <param name="rangeExpression">The range expression to parse (e.g., <c>"&gt;=1.0.0 &lt;2.0.0 || &gt;=3.0.0"</c>).</param>
    /// <returns>A <see cref="SemVerRange"/> representing the parsed expression.</returns>
    /// <exception cref="ArgumentException">Thrown when the expression is <c>null</c>, empty, or malformed.</exception>
    public SemVerRange ParseRange(string rangeExpression)
    {
        if (string.IsNullOrWhiteSpace(rangeExpression))
            throw new ArgumentException("Range expression must not be null or empty.", nameof(rangeExpression));

        // Step 1: Split on "||" to get OR alternatives
        var alternativeStrings = rangeExpression.Split(
            new[] { "||" },
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (alternativeStrings.Length == 0)
            throw new ArgumentException("Range expression is empty after splitting on '||'.", nameof(rangeExpression));

        SemVerRange? root = null;
        List<SemVerRange>? alternatives = null;

        foreach (var altStr in alternativeStrings)
        {
            var clauses = ParseAndGroup(altStr);
            var range = new SemVerRange(clauses);

            if (root is null)
            {
                root = range;
            }
            else
            {
                (alternatives ??= new List<SemVerRange>()).Add(range);
            }
        }

        return root! with { Alternatives = alternatives?.AsReadOnly() };
    }

    /// <summary>
    /// Parses a single AND-group (space-separated clauses) into a list of clauses.
    /// Caret and tilde operators are expanded during parsing.
    /// </summary>
    private static List<SemVerRangeClause> ParseAndGroup(string expression)
    {
        var clauses = new List<SemVerRangeClause>();
        var parts = expression.Split(
            (char[]?)null,
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var part in parts)
        {
            var (op, versionStr) = ExtractOperatorAndVersion(part);
            var version = ParseVersion(versionStr);

            switch (op)
            {
                case "^":
                    // Caret range expansion
                    clauses.Add(new SemVerRangeClause(">=", version));
                    clauses.Add(new SemVerRangeClause("<", GetCaretUpperBound(version)));
                    break;

                case "~":
                    // Tilde range expansion
                    clauses.Add(new SemVerRangeClause(">=", version));
                    clauses.Add(new SemVerRangeClause("<", GetTildeUpperBound(version)));
                    break;

                default:
                    clauses.Add(new SemVerRangeClause(op, version));
                    break;
            }
        }

        if (clauses.Count == 0)
            throw new ArgumentException($"AND-group expression '{expression}' produced no valid clauses.");

        return clauses;
    }

    /// <summary>
    /// Extracts the operator prefix and version string from a clause expression.
    /// </summary>
    private static (string Operator, string Version) ExtractOperatorAndVersion(string clause)
    {
        clause = clause.Trim();

        if (clause.StartsWith(">="))
            return (">=", clause[2..].Trim());
        if (clause.StartsWith("<="))
            return ("<=", clause[2..].Trim());
        if (clause.StartsWith(">"))
            return (">", clause[1..].Trim());
        if (clause.StartsWith("<"))
            return ("<", clause[1..].Trim());
        if (clause.StartsWith("^"))
            return ("^", clause[1..].Trim());
        if (clause.StartsWith("~"))
            return ("~", clause[1..].Trim());

        // Exact version (no operator)
        return ("", clause);
    }

    /// <summary>
    /// Parses a version string into a <see cref="Version"/>, stripping any pre-release
    /// or build metadata suffix.
    /// </summary>
    private static Version ParseVersion(string versionStr)
    {
        versionStr = versionStr.Trim();

        // Match the numeric portion: major.minor.patch
        var match = VersionPattern.Match(versionStr);
        if (!match.Success)
            throw new FormatException($"'{versionStr}' is not a valid version number. Expected format: major.minor.patch.");

        var major = int.Parse(match.Groups[1].ValueSpan);
        var minor = int.Parse(match.Groups[2].ValueSpan);
        var patch = int.Parse(match.Groups[3].ValueSpan);

        return new Version(major, minor, patch);
    }

    /// <summary>
    /// Computes the upper bound for a caret (<c>^</c>) range.
    /// </summary>
    private static Version GetCaretUpperBound(Version version)
    {
        if (version.Major > 0)
        {
            // ^1.0.0 -> <2.0.0
            return new Version(version.Major + 1, 0, 0);
        }

        if (version.Minor > 0)
        {
            // ^0.1.0 -> <0.2.0
            return new Version(0, version.Minor + 1, 0);
        }

        // ^0.0.1 -> <0.0.2
        return new Version(0, 0, version.Build + 1);
    }

    /// <summary>
    /// Computes the upper bound for a tilde (<c>~</c>) range.
    /// </summary>
    private static Version GetTildeUpperBound(Version version)
    {
        // ~1.0.0 -> <1.1.0
        // ~0.1.0 -> <0.2.0
        return new Version(version.Major, version.Minor + 1, 0);
    }

    /// <summary>
    /// Evaluates whether a <see cref="SemVerRange"/> is satisfied by the given SDK version.
    /// </summary>
    private static bool EvaluateRange(SemVerRange range, Version sdkVersion)
    {
        // Check the root AND group
        var rootSatisfied = EvaluateAndGroup(range.Clauses, sdkVersion);

        if (rootSatisfied)
            return true;

        // Check OR alternatives
        if (range.Alternatives is not null)
        {
            foreach (var alt in range.Alternatives)
            {
                if (EvaluateRange(alt, sdkVersion))
                    return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Evaluates whether all clauses in an AND group are satisfied by the SDK version.
    /// </summary>
    private static bool EvaluateAndGroup(IReadOnlyList<SemVerRangeClause> clauses, Version sdkVersion)
    {
        foreach (var clause in clauses)
        {
            if (!EvaluateClause(clause, sdkVersion))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Evaluates a single clause against the SDK version.
    /// </summary>
    private static bool EvaluateClause(SemVerRangeClause clause, Version sdkVersion)
    {
        return clause.Operator switch
        {
            "" => sdkVersion.Equals(clause.Version),
            ">=" => sdkVersion >= clause.Version,
            "<=" => sdkVersion <= clause.Version,
            ">" => sdkVersion > clause.Version,
            "<" => sdkVersion < clause.Version,
            _ => throw new ArgumentException($"Unknown operator '{clause.Operator}' in range clause.")
        };
    }
}
