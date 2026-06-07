using NextNet.Templates.Abstractions;
using NextNet.Templates.Models;

namespace NextNet.TemplateSdk.Validation;

/// <summary>
/// Orchestrates template validation by running a configurable set of validation rules
/// against a template manifest or package.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="TemplateValidator"/> implements <see cref="ITemplateValidator"/> and
/// provides both manifest-only and package-level validation. It executes all registered
/// <see cref="ValidationRule"/> instances, aggregates their results into a
/// <see cref="ValidationReport"/>, and converts the report into an
/// <see cref="NextNet.Templates.Abstractions.ValidationResult"/>.
/// </para>
/// <para>
/// By default, the following built-in rules are registered:
/// <list type="bullet">
///   <item><see cref="Rules.ManifestSchemaRule"/></item>
///   <item><see cref="Rules.FileExistsRule"/></item>
///   <item><see cref="Rules.VariableCompletenessRule"/></item>
///   <item><see cref="Rules.ConditionSyntaxRule"/></item>
///   <item><see cref="Rules.PlaceholderCoverageRule"/></item>
///   <item><see cref="Rules.NoExecutableFilesRule"/></item>
/// </list>
/// </para>
/// <para>
/// Custom rules can be provided via the constructor overload that accepts
/// <c>IEnumerable&lt;ValidationRule&gt;</c>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var validator = new TemplateValidator();
/// var manifest = new TemplateManifest("my-template", "1.0.0", "&gt;=3.0.0");
/// var result = await validator.ValidateAsync(manifest);
/// Console.WriteLine(result.IsValid ? "Valid!" : "Invalid!");
/// </code>
/// </example>
public sealed class TemplateValidator : ITemplateValidator
{
    private readonly List<ValidationRule> _rules;

    /// <summary>
    /// Initializes a new instance of the <see cref="TemplateValidator"/> class with
    /// the default set of built-in validation rules.
    /// </summary>
    public TemplateValidator() : this(DefaultRules())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TemplateValidator"/> class with
    /// a custom set of validation rules.
    /// </summary>
    /// <param name="rules">The validation rules to execute.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="rules"/> is <c>null</c>.</exception>
    public TemplateValidator(IEnumerable<ValidationRule> rules)
    {
        _rules = rules?.ToList() ?? throw new ArgumentNullException(nameof(rules));
    }

    /// <summary>
    /// Returns the default set of built-in validation rules.
    /// </summary>
    private static IEnumerable<ValidationRule> DefaultRules()
    {
        return new ValidationRule[]
        {
            new Rules.ManifestSchemaRule(),
            new Rules.FileExistsRule(),
            new Rules.VariableCompletenessRule(),
            new Rules.ConditionSyntaxRule(),
            new Rules.PlaceholderCoverageRule(),
            new Rules.NoExecutableFilesRule()
        };
    }

    /// <inheritdoc />
    public async Task<NextNet.Templates.Abstractions.ValidationResult> ValidateAsync(
        TemplateManifest manifest,
        CancellationToken cancellationToken = default)
    {
        if (manifest is null)
            throw new ArgumentNullException(nameof(manifest));

        var emptyFiles = new Dictionary<string, byte[]>();
        var context = new ValidationContext(manifest, emptyFiles, cancellationToken);
        var report = await Task.Run(() => ValidateInternal(context), cancellationToken);
        return ToAbstractionsResult(report);
    }

    /// <inheritdoc />
    public async Task<NextNet.Templates.Abstractions.ValidationResult> ValidateAsync(
        TemplatePackage package,
        CancellationToken cancellationToken = default)
    {
        if (package is null)
            throw new ArgumentNullException(nameof(package));

        var files = package.Files ?? new Dictionary<string, byte[]>();
        var context = new ValidationContext(package.Manifest, files, cancellationToken);
        var report = await Task.Run(() => ValidateInternal(context), cancellationToken);
        return ToAbstractionsResult(report);
    }

    /// <summary>
    /// Runs all validation rules against the provided context and produces a report.
    /// </summary>
    private ValidationReport ValidateInternal(ValidationContext context)
    {
        var results = new List<ValidationResult>();
        foreach (var rule in _rules)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            try
            {
                results.AddRange(rule.Validate(context));
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                results.Add(new ValidationResult
                {
                    RuleName = rule.Name,
                    Severity = ValidationSeverity.Warning,
                    Message = $"Rule '{rule.Name}' threw an exception: {ex.Message}",
                    Suggestion = "Please report this rule failure."
                });
            }
        }

        return new ValidationReport { Results = results };
    }

    /// <summary>
    /// Converts a <see cref="ValidationReport"/> to the abstraction-layer
    /// <see cref="NextNet.Templates.Abstractions.ValidationResult"/>.
    /// </summary>
    private static NextNet.Templates.Abstractions.ValidationResult ToAbstractionsResult(ValidationReport report)
    {
        var errors = report.Results
            .Where(r => r.Severity == ValidationSeverity.Error)
            .Select(r => $"[{r.RuleName}] {FormatFile(r)}: {r.Message}{(r.Suggestion is not null ? $" (Suggestion: {r.Suggestion})" : "")}")
            .ToList();

        var warnings = report.Results
            .Where(r => r.Severity == ValidationSeverity.Warning)
            .Select(r => $"[{r.RuleName}] {FormatFile(r)}: {r.Message}")
            .ToList();

        return new NextNet.Templates.Abstractions.ValidationResult(
            IsValid: report.IsValid,
            Errors: errors.Count > 0 ? errors : null,
            Warnings: warnings.Count > 0 ? warnings : null);
    }

    /// <summary>
    /// Formats the file path for display, including line number if present.
    /// </summary>
    private static string FormatFile(ValidationResult result)
    {
        if (result.File is null) return "(general)";
        return result.Line.HasValue
            ? $"{result.File}({result.Line.Value})"
            : result.File;
    }
}
