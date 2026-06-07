namespace NextNet.TemplateSdk.Validation;

/// <summary>
/// Represents the outcome of a single validation rule applied to a template.
/// </summary>
/// <remarks>
/// <para>
/// A <see cref="ValidationResult"/> captures a specific finding from one rule, including
/// the severity, a human-readable message, and optional file/line location information.
/// Results are aggregated into a <see cref="ValidationReport"/>.
/// </para>
/// <para>
/// The <see cref="Suggestion"/> property provides an actionable recommendation for
/// resolving the issue if one is available.
/// </para>
/// </remarks>
public sealed record ValidationResult
{
    /// <summary>
    /// Gets the name of the rule that produced this result.
    /// </summary>
    public string RuleName { get; init; } = "";

    /// <summary>
    /// Gets the severity level of this finding.
    /// </summary>
    public ValidationSeverity Severity { get; init; }

    /// <summary>
    /// Gets a human-readable description of the validation finding.
    /// </summary>
    public string Message { get; init; } = "";

    /// <summary>
    /// Gets the file path associated with this finding, if applicable.
    /// </summary>
    public string? File { get; init; }

    /// <summary>
    /// Gets the line number within the file associated with this finding, if applicable.
    /// </summary>
    public int? Line { get; init; }

    /// <summary>
    /// Gets an actionable suggestion for resolving the issue, if available.
    /// </summary>
    public string? Suggestion { get; init; }
}
