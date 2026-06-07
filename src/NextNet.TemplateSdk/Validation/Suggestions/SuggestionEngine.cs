namespace NextNet.TemplateSdk.Validation.Suggestions;

/// <summary>
/// Provides intelligent suggestions for resolving common template validation issues.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="SuggestionEngine"/> examines validation results and generates
/// actionable recommendations. It can be used standalone or integrated into the
/// validation pipeline to enrich <see cref="ValidationResult.Suggestion"/> values.
/// </para>
/// <para>
/// Currently supported suggestion types include:
/// <list type="bullet">
///   <item>Missing manifest fields — suggests example values.</item>
///   <item>Missing files — suggests adding the file or removing the manifest entry.</item>
///   <item>Undefined variables — suggests adding a variable declaration.</item>
///   <item>Unused variables — suggests removing or using the variable.</item>
///   <item>Invalid condition syntax — points to the error location.</item>
/// </list>
/// </para>
/// </remarks>
public sealed class SuggestionEngine
{
    /// <summary>
    /// Generates a suggestion for a given validation result based on its rule name and context.
    /// </summary>
    /// <param name="result">The validation result to generate a suggestion for.</param>
    /// <returns>A human-readable suggestion string, or <c>null</c> if no suggestion is available.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is <c>null</c>.</exception>
    public string? GetSuggestion(ValidationResult result)
    {
        if (result is null)
            throw new ArgumentNullException(nameof(result));

        return result.RuleName switch
        {
            "manifest-schema" => result.Message switch
            {
                string m when m.Contains("name") => "Set a descriptive name like 'blog' or 'webapi'.",
                string m when m.Contains("version") => "Use SemVer 2.0 format, e.g., '1.0.0'.",
                string m when m.Contains("nextnetVersion") => "Specify a version range, e.g., '>=3.0.0'.",
                _ => null
            },
            "file-exists" => $"Ensure the file '{result.File}' exists in the template package directory.",
            "placeholder-coverage" => $"Add variable '{result.Message.Split('\'')[1]}' to the manifest's 'variables' section.",
            "variable-completeness" when result.Message.Contains("enum") =>
                "Add an 'allowedValues' array with at least one permitted value.",
            "condition-syntax" => "Verify the expression syntax. Supported operators: ==, !=, &&, ||, !, in.",
            "no-executable-files" => "Replace the executable with source files or remove it from the template.",
            _ => null
        };
    }
}
