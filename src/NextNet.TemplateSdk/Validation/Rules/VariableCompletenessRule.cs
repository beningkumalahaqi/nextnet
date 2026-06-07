using NextNet.Templates.Models;

namespace NextNet.TemplateSdk.Validation.Rules;

/// <summary>
/// Validates that template variables are properly configured, including required variables
/// and enum-type variables with allowed values.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="VariableCompletenessRule"/> inspects each <see cref="TemplateVariable"/>
/// declared in the manifest and checks:
/// <list type="bullet">
///   <item>Required variables without defaults — users will be prompted (info-level).</item>
///   <item>Enum-type variables must define <c>AllowedValues</c> (error-level).</item>
/// </list>
/// </para>
/// </remarks>
public sealed class VariableCompletenessRule : ValidationRule
{
    /// <inheritdoc />
    public override string Name => "variableCompleteness";

    /// <inheritdoc />
    public override ValidationSeverity DefaultSeverity => ValidationSeverity.Warning;

    /// <inheritdoc />
    public override IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        var results = new List<ValidationResult>();
        foreach (var v in context.Manifest.Variables ?? Enumerable.Empty<TemplateVariable>())
        {
            if (v.Required && v.Default is null)
            {
                // Required without default — must be prompted (acceptable, just an info)
                results.Add(new ValidationResult
                {
                    RuleName = Name,
                    Severity = ValidationSeverity.Info,
                    Message = $"Required variable '{v.Name}' has no default. Users will be prompted.",
                    File = "template.json",
                    Suggestion = $"Consider providing a default: \"default\": <value>"
                });
            }

            if (string.Equals(v.Type, "enum", StringComparison.OrdinalIgnoreCase) &&
                (v.AllowedValues is null || v.AllowedValues.Count == 0))
            {
                results.Add(new ValidationResult
                {
                    RuleName = Name,
                    Severity = ValidationSeverity.Error,
                    Message = $"Enum variable '{v.Name}' must specify AllowedValues.",
                    File = "template.json",
                    Suggestion = $"Add: \"allowedValues\": [\"value1\", \"value2\"]"
                });
            }
        }
        return results;
    }
}
