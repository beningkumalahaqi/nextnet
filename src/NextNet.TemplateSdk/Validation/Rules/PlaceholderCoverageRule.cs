using System.Text.RegularExpressions;
using NextNet.Templates.Models;

namespace NextNet.TemplateSdk.Validation.Rules;

/// <summary>
/// Validates that all placeholders used in template files are declared as variables in the manifest.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="PlaceholderCoverageRule"/> scans each text file in the template package
/// for <c>{{variable}}</c> patterns and cross-references them against the declared
/// variables in the manifest. Any placeholder referencing an undeclared variable is
/// reported as a warning with a suggestion to add the variable.
/// </para>
/// <para>
/// Binary files (e.g., <c>.png</c>, <c>.jpg</c>, <c>.dll</c>) are skipped automatically
/// to avoid false positives from non-text content.
/// </para>
/// </remarks>
public sealed class PlaceholderCoverageRule : ValidationRule
{
    private static readonly Regex PlaceholderRegex = new(@"\{\{([a-zA-Z0-9_.]+)\}\}", RegexOptions.Compiled);

    /// <inheritdoc />
    public override string Name => "placeholder-coverage";

    /// <inheritdoc />
    public override ValidationSeverity DefaultSeverity => ValidationSeverity.Warning;

    /// <inheritdoc />
    public override IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        var results = new List<ValidationResult>();
        var declaredVars = new HashSet<string>(
            (context.Manifest.Variables ?? Enumerable.Empty<TemplateVariable>()).Select(v => v.Name),
            StringComparer.Ordinal);

        foreach (var (path, content) in context.Files)
        {
            // Skip binary files
            if (IsBinary(path)) continue;

            var text = System.Text.Encoding.UTF8.GetString(content);
            var matches = PlaceholderRegex.Matches(text);
            foreach (Match match in matches)
            {
                var key = match.Groups[1].Value;
                // Extract root name (before first dot)
                var rootName = key.Split('.')[0];
                if (!declaredVars.Contains(rootName))
                {
                    results.Add(new ValidationResult
                    {
                        RuleName = Name,
                        Severity = ValidationSeverity.Warning,
                        Message = $"Placeholder '{{{key}}}' references undefined variable '{rootName}'.",
                        File = path,
                        Suggestion = $"Add variable '{rootName}' to the manifest's 'variables' section."
                    });
                }
            }
        }

        return results;
    }

    private static bool IsBinary(string path)
    {
        var ext = System.IO.Path.GetExtension(path).ToLowerInvariant();
        return ext is ".png" or ".jpg" or ".gif" or ".pdf" or ".zip" or ".dll" or ".exe";
    }
}
