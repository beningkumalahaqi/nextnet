namespace NextNet.TemplateSdk.Validation;

/// <summary>
/// Aggregates the results of all validation rules applied to a template.
/// </summary>
/// <remarks>
/// <para>
/// A <see cref="ValidationReport"/> collects the output from every <see cref="ValidationRule"/>
/// executed by the <see cref="TemplateValidator"/>. It provides summary properties like
/// <see cref="ErrorCount"/>, <see cref="WarningCount"/>, and <see cref="IsValid"/> for
/// quick inspection of overall template health.
/// </para>
/// <para>
/// A report is considered valid (<see cref="IsValid"/> is <c>true</c>) only when there
/// are zero errors. Warnings alone do not invalidate a report.
/// </para>
/// </remarks>
public sealed record ValidationReport
{
    /// <summary>
    /// Gets the list of individual validation results from all rules.
    /// </summary>
    public IReadOnlyList<ValidationResult> Results { get; init; } = Array.Empty<ValidationResult>();

    /// <summary>
    /// Gets the count of results with <see cref="ValidationSeverity.Error"/> severity.
    /// </summary>
    public int ErrorCount => Results.Count(r => r.Severity == ValidationSeverity.Error);

    /// <summary>
    /// Gets the count of results with <see cref="ValidationSeverity.Warning"/> severity.
    /// </summary>
    public int WarningCount => Results.Count(r => r.Severity == ValidationSeverity.Warning);

    /// <summary>
    /// Gets whether the validation passed with no errors.
    /// </summary>
    public bool IsValid => ErrorCount == 0;

    /// <summary>
    /// Gets a singleton empty report representing a valid template with no findings.
    /// </summary>
    public static ValidationReport Empty { get; } = new();
}
