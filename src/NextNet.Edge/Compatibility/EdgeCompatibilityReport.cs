namespace NextNet.Edge.Compatibility;

/// <summary>
/// Represents the result of an edge compatibility check for a set of source files or a project.
/// </summary>
public sealed class EdgeCompatibilityReport
{
    private readonly List<EdgeViolation> _violations;

    /// <summary>
    /// Gets the read-only list of violations found during the compatibility check.
    /// </summary>
    public IReadOnlyList<EdgeViolation> Violations => _violations.AsReadOnly();

    /// <summary>
    /// Gets whether the report contains any error-severity violations.
    /// </summary>
    public bool HasErrors => _violations.Any(v => v.Severity == EdgeViolationSeverity.Error);

    /// <summary>
    /// Gets whether the report contains any violations at all.
    /// </summary>
    public bool HasViolations => _violations.Count > 0;

    /// <summary>
    /// Gets the total number of violations.
    /// </summary>
    public int TotalCount => _violations.Count;

    /// <summary>
    /// Gets the number of error-severity violations.
    /// </summary>
    public int ErrorCount => _violations.Count(v => v.Severity == EdgeViolationSeverity.Error);

    /// <summary>
    /// Gets the number of warning-severity violations.
    /// </summary>
    public int WarningCount => _violations.Count(v => v.Severity == EdgeViolationSeverity.Warning);

    /// <summary>
    /// Gets the number of info-severity violations.
    /// </summary>
    public int InfoCount => _violations.Count(v => v.Severity == EdgeViolationSeverity.Info);

    /// <summary>
    /// Initializes a new instance of <see cref="EdgeCompatibilityReport"/>.
    /// </summary>
    /// <param name="violations">The list of violations found. If null, an empty list is used.</param>
    public EdgeCompatibilityReport(IEnumerable<EdgeViolation>? violations = null)
    {
        _violations = violations?.ToList() ?? new List<EdgeViolation>();
    }

    /// <summary>
    /// Adds a violation to the report.
    /// </summary>
    /// <param name="violation">The violation to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="violation"/> is null.</exception>
    public void AddViolation(EdgeViolation violation)
    {
        if (violation == null) throw new ArgumentNullException(nameof(violation));
        _violations.Add(violation);
    }

    /// <summary>
    /// Adds a range of violations to the report.
    /// </summary>
    /// <param name="violations">The violations to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="violations"/> is null.</exception>
    public void AddViolations(IEnumerable<EdgeViolation> violations)
    {
        if (violations == null) throw new ArgumentNullException(nameof(violations));
        _violations.AddRange(violations);
    }

    /// <summary>
    /// Throws an <see cref="EdgeCompatibilityException"/> if the report has any error violations
    /// and <paramref name="throwOnErrors"/> is <c>true</c>.
    /// </summary>
    /// <param name="throwOnErrors">Whether to throw on error violations.</param>
    /// <exception cref="EdgeCompatibilityException">Thrown when there are errors and <paramref name="throwOnErrors"/> is true.</exception>
    public void ThrowIfErrors(bool throwOnErrors = true)
    {
        if (throwOnErrors && HasErrors)
            throw new EdgeCompatibilityException(this);
    }

    /// <summary>
    /// Returns a formatted string summary of all violations.
    /// </summary>
    public override string ToString()
    {
        if (!HasViolations)
            return "No edge compatibility violations found.";

        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"Edge Compatibility Report: {TotalCount} violation(s) found");
        sb.AppendLine($"  Errors: {ErrorCount}, Warnings: {WarningCount}, Info: {InfoCount}");
        sb.AppendLine();

        foreach (var violation in Violations)
        {
            sb.AppendLine($"  {violation}");
        }

        return sb.ToString();
    }
}

/// <summary>
/// Exception thrown when edge compatibility errors are detected.
/// </summary>
public sealed class EdgeCompatibilityException : Exception
{
    /// <summary>
    /// Gets the compatibility report that caused this exception.
    /// </summary>
    public EdgeCompatibilityReport Report { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="EdgeCompatibilityException"/>.
    /// </summary>
    /// <param name="report">The compatibility report containing errors.</param>
    public EdgeCompatibilityException(EdgeCompatibilityReport report)
        : base($"[{EdgeErrorCodes.CompatibilityError}] Edge compatibility check failed with {report.ErrorCount} error(s).\n{report}")
    {
        Report = report ?? throw new ArgumentNullException(nameof(report));
    }
}
