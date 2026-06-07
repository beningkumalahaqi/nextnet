namespace NextNet.TemplateSdk.Validation;

/// <summary>
/// Defines the severity levels for validation results.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="ValidationSeverity"/> is used by <see cref="ValidationResult"/> to indicate
/// how severe a validation finding is. Errors block template generation, warnings indicate
/// potential issues, and info entries are informational suggestions.
/// </para>
/// </remarks>
public enum ValidationSeverity
{
    /// <summary>
    /// Informational finding — a suggestion or observation that does not affect correctness.
    /// </summary>
    Info,

    /// <summary>
    /// Warning — a potential issue that should be reviewed but does not block generation.
    /// </summary>
    Warning,

    /// <summary>
    /// Error — a definite problem that must be fixed before the template can be used.
    /// </summary>
    Error
}
