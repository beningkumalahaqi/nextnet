using NextNet.Templates.Models;

namespace NextNet.TemplateSdk.Validation.Rules;

/// <summary>
/// Validates that every file referenced in the manifest exists within the template package.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="FileExistsRule"/> checks each <see cref="TemplateFile.SourcePath"/> entry
/// in the manifest against the files dictionary provided in the validation context.
/// If a referenced file is missing from the package, an error is reported with a suggestion
/// to either add the file or remove the manifest entry.
/// </para>
/// <para>
/// This rule requires a package-level validation where the <c>Files</c> dictionary is populated.
/// When validating a manifest alone (without files), the rule will not produce any findings
/// since there are no files to check against.
/// </para>
/// </remarks>
public sealed class FileExistsRule : ValidationRule
{
    /// <inheritdoc />
    public override string Name => "file-exists";

    /// <inheritdoc />
    public override ValidationSeverity DefaultSeverity => ValidationSeverity.Error;

    /// <inheritdoc />
    public override IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        var results = new List<ValidationResult>();
        foreach (var file in context.Manifest.Files ?? Enumerable.Empty<TemplateFile>())
        {
            if (!context.Files.ContainsKey(file.SourcePath))
            {
                results.Add(new ValidationResult
                {
                    RuleName = Name,
                    Severity = ValidationSeverity.Error,
                    Message = $"File '{file.SourcePath}' is referenced in manifest but not present in package.",
                    File = file.SourcePath,
                    Suggestion = $"Add the file '{file.SourcePath}' to the template or remove it from the manifest."
                });
            }
        }
        return results;
    }
}
