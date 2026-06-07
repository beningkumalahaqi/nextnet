namespace NextNet.TemplateSdk.Validation.Rules;

/// <summary>
/// Validates that required fields in the template manifest are present and non-empty.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="ManifestSchemaRule"/> checks that the manifest contains values for
/// required metadata fields: <c>name</c>, <c>version</c>, and <c>nextnetVersion</c>.
/// Missing or whitespace-only values are reported as errors with actionable suggestions.
/// This rule runs only against the manifest and does not inspect file contents.
/// </para>
/// </remarks>
public sealed class ManifestSchemaRule : ValidationRule
{
    /// <inheritdoc />
    public override string Name => "manifestSchema";

    /// <inheritdoc />
    public override ValidationSeverity DefaultSeverity => ValidationSeverity.Error;

    /// <inheritdoc />
    public override IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        var results = new List<ValidationResult>();
        var m = context.Manifest;

        if (string.IsNullOrWhiteSpace(m.Name))
            results.Add(Error("Manifest 'name' is required.", "template.json", "Set a name like 'blog' or 'api'."));
        if (string.IsNullOrWhiteSpace(m.Version))
            results.Add(Error("Manifest 'version' is required.", "template.json", "Use SemVer 2.0 format, e.g., '1.0.0'."));
        if (string.IsNullOrWhiteSpace(m.NextNetVersion))
            results.Add(Error("Manifest 'nextnetVersion' is required.", "template.json", "Specify a version range, e.g., '>=3.0.0'."));

        return results;
    }

    private static ValidationResult Error(string message, string? file = null, string? suggestion = null)
        => new()
        {
            RuleName = "manifestSchema",
            Severity = ValidationSeverity.Error,
            Message = message,
            File = file,
            Suggestion = suggestion
        };
}
