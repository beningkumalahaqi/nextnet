using NextNet.TemplateEngine.Conditionals;
using NextNet.Templates.Models;

namespace NextNet.TemplateSdk.Validation.Rules;

/// <summary>
/// Validates that conditional expressions in template file entries are syntactically correct.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="ConditionSyntaxRule"/> parses each <see cref="TemplateFile.Condition"/>
/// expression using the <see cref="ConditionParser"/> from the template engine. If a
/// condition contains a syntax error, a <see cref="ParseException"/> is caught and
/// reported as an error with the position of the failure.
/// </para>
/// <para>
/// Supported expression syntax includes <c>==</c>, <c>!=</c>, <c>&amp;&amp;</c>, <c>||</c>,
/// <c>!</c>, <c>in</c>, and comparison operators. String literals use single or double quotes.
/// </para>
/// </remarks>
public sealed class ConditionSyntaxRule : ValidationRule
{
    /// <inheritdoc />
    public override string Name => "condition-syntax";

    /// <inheritdoc />
    public override ValidationSeverity DefaultSeverity => ValidationSeverity.Error;

    /// <inheritdoc />
    public override IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        var results = new List<ValidationResult>();
        var parser = new ConditionParser();

        foreach (var file in context.Manifest.Files ?? Enumerable.Empty<TemplateFile>())
        {
            if (string.IsNullOrWhiteSpace(file.Condition)) continue;

            try
            {
                parser.Parse(file.Condition);
            }
            catch (ParseException ex)
            {
                results.Add(new ValidationResult
                {
                    RuleName = Name,
                    Severity = ValidationSeverity.Error,
                    Message = $"Invalid condition '{file.Condition}': {ex.Message}",
                    File = file.SourcePath,
                    Line = ex.Position,
                    Suggestion = "Check the expression syntax. Supported: ==, !=, &&, ||, !, in, comparison operators."
                });
            }
        }

        return results;
    }
}
