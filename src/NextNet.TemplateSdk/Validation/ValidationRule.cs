namespace NextNet.TemplateSdk.Validation;

/// <summary>
/// Defines the abstract base class for all template validation rules.
/// </summary>
/// <remarks>
/// <para>
/// Each validation rule encapsulates a single check that can be performed against a
/// template's manifest and/or file content. Rules are registered with the
/// <see cref="TemplateValidator"/> and executed in order during validation.
/// </para>
/// <para>
/// To create a custom rule, derive from <see cref="ValidationRule"/> and implement
/// <see cref="Name"/>, <see cref="DefaultSeverity"/>, and <see cref="Validate"/>.
/// </para>
/// <example>
/// <code>
/// public sealed class MyCustomRule : ValidationRule
/// {
///     public override string Name => "my-custom-rule";
///     public override ValidationSeverity DefaultSeverity => ValidationSeverity.Warning;
///
///     public override IEnumerable&lt;ValidationResult&gt; Validate(ValidationContext context)
///     {
///         // Perform checks and yield results
///     }
/// }
/// </code>
/// </example>
/// </remarks>
public abstract class ValidationRule
{
    /// <summary>
    /// Gets the unique name identifier for this rule.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Gets the default severity level for findings produced by this rule.
    /// </summary>
    public abstract ValidationSeverity DefaultSeverity { get; }

    /// <summary>
    /// Validates the template context and returns a sequence of validation results.
    /// </summary>
    /// <param name="context">The validation context containing manifest and files.</param>
    /// <returns>A sequence of <see cref="ValidationResult"/> instances describing findings.</returns>
    public abstract IEnumerable<ValidationResult> Validate(ValidationContext context);
}
