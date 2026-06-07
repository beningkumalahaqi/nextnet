using System.Text.RegularExpressions;
using NextNet.Templates.Abstractions;
using NextNet.Templates.Exceptions;
using NextNet.Templates.Models;

namespace NextNet.Templates.Manifest;

/// <summary>
/// Validates <see cref="TemplateManifest"/> instances for structural and semantic correctness,
/// including field format validation, uniqueness constraints, and cross-reference integrity.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="ManifestValidator"/> performs comprehensive validation of a template manifest
/// by checking all rules defined in the NextNet template manifest specification. Errors
/// are accumulated and returned as a <see cref="ValidationResult"/> rather than failing
/// on the first issue, providing a complete picture of all problems found.
/// </para>
/// <para>
/// Validation rules include:
/// <list type="bullet">
///   <item>Required fields are present and well-formed.</item>
///   <item><c>Name</c> matches the pattern <c>^[a-zA-Z][a-zA-Z0-9_-]*$</c>.</item>
///   <item><c>Version</c> is valid SemVer 2.0.</item>
///   <item><c>NextNetVersion</c> is a parseable SemVer range.</item>
///   <item>File entries have non-empty source and target paths.</item>
///   <item>Variable names are unique.</item>
///   <item>Feature names are unique.</item>
///   <item>Feature dependencies and conflicts reference declared features.</item>
///   <item>Enum-type variables have at least one allowed value.</item>
/// </list>
/// </para>
/// <example>
/// <code>
/// var validator = new ManifestValidator();
/// var result = validator.Validate(manifest);
/// if (!result.IsValid)
/// {
///     foreach (var error in result.Errors)
///         Console.WriteLine($"  - {error}");
/// }
/// </code>
/// </example>
/// </remarks>
public sealed class ManifestValidator
{
    private static readonly Regex NamePattern = new(@"^[a-zA-Z][a-zA-Z0-9_-]*$", RegexOptions.Compiled);
    private static readonly Regex SemVerPattern = new(@"^\d+\.\d+\.\d+(-[a-zA-Z0-9.]+)?(\+[a-zA-Z0-9.]+)?$", RegexOptions.Compiled);

    /// <summary>
    /// Validates the specified <see cref="TemplateManifest"/> and returns a
    /// <see cref="ValidationResult"/> describing any issues found.
    /// </summary>
    /// <param name="manifest">The manifest to validate. Must not be <c>null</c>.</param>
    /// <returns>A <see cref="ValidationResult"/> with accumulated errors and warnings.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="manifest"/> is <c>null</c>.</exception>
    public ValidationResult Validate(TemplateManifest manifest)
    {
        if (manifest is null)
            throw new ArgumentNullException(nameof(manifest));

        var errors = new List<string>();

        // --- Name validation ---
        if (string.IsNullOrWhiteSpace(manifest.Name))
        {
            errors.Add("Template name is required and must not be empty.");
        }
        else if (!NamePattern.IsMatch(manifest.Name))
        {
            errors.Add(
                $"Template name '{manifest.Name}' contains invalid characters. " +
                "Name must start with a letter and contain only letters, digits, underscores, and hyphens.");
        }

        // --- Version validation ---
        if (string.IsNullOrWhiteSpace(manifest.Version))
        {
            errors.Add("Template version is required and must not be empty.");
        }
        else if (!SemVerPattern.IsMatch(manifest.Version))
        {
            errors.Add($"Template version '{manifest.Version}' is not a valid SemVer 2.0 version. " +
                       "Expected format: major.minor.patch (e.g., 1.0.0) with optional pre-release and build metadata.");
        }

        // --- NextNetVersion validation ---
        if (!string.IsNullOrWhiteSpace(manifest.NextNetVersion))
        {
            try
            {
                var checker = new VersionCompatibilityChecker();
                checker.ParseRange(manifest.NextNetVersion);
            }
            catch (Exception ex) when (ex is ArgumentException or FormatException)
            {
                errors.Add($"NextNet version constraint '{manifest.NextNetVersion}' is not a valid SemVer range: {ex.Message}");
            }
        }

        // --- File entries validation ---
        if (manifest.Files is not null)
        {
            for (int i = 0; i < manifest.Files.Count; i++)
            {
                var file = manifest.Files[i];
                if (string.IsNullOrWhiteSpace(file.SourcePath))
                {
                    errors.Add($"File entry at index {i} has an empty or missing source path.");
                }

                if (string.IsNullOrWhiteSpace(file.TargetPath))
                {
                    errors.Add($"File entry at index {i} has an empty or missing target path.");
                }
            }
        }

        // --- Variable validation ---
        if (manifest.Variables is not null)
        {
            var variableNames = new HashSet<string>(StringComparer.Ordinal);

            for (int i = 0; i < manifest.Variables.Count; i++)
            {
                var variable = manifest.Variables[i];

                // Duplicate name check
                if (!string.IsNullOrWhiteSpace(variable.Name) && !variableNames.Add(variable.Name))
                {
                    errors.Add($"Duplicate variable name '{variable.Name}' at index {i}. Variable names must be unique.");
                }

                // Enum variables must have allowed values
                if (string.Equals(variable.Type, "enum", StringComparison.OrdinalIgnoreCase))
                {
                    if (variable.AllowedValues is null || variable.AllowedValues.Count == 0)
                    {
                        errors.Add($"Enum variable '{variable.Name}' must have at least one allowed value defined.");
                    }
                }
            }
        }

        // --- Feature validation ---
        if (manifest.Features is not null)
        {
            // Build the set of declared feature names
            var featureNames = new HashSet<string>(StringComparer.Ordinal);

            foreach (var feature in manifest.Features)
            {
                if (!string.IsNullOrWhiteSpace(feature.Name) && !featureNames.Add(feature.Name))
                {
                    errors.Add($"Duplicate feature name '{feature.Name}'. Feature names must be unique.");
                }
            }

            // Check dependencies and conflicts reference declared features
            foreach (var feature in manifest.Features)
            {
                if (string.IsNullOrWhiteSpace(feature.Name))
                    continue;

                if (feature.Dependencies is not null)
                {
                    foreach (var dep in feature.Dependencies)
                    {
                        if (!featureNames.Contains(dep))
                        {
                            errors.Add($"Feature '{feature.Name}' depends on undefined feature '{dep}'. " +
                                       "All dependencies must reference declared features.");
                        }
                    }
                }

                if (feature.Conflicts is not null)
                {
                    foreach (var conflict in feature.Conflicts)
                    {
                        if (!featureNames.Contains(conflict))
                        {
                            errors.Add($"Feature '{feature.Name}' conflicts with undefined feature '{conflict}'. " +
                                       "All conflicts must reference declared features.");
                        }
                    }
                }
            }
        }

        return new ValidationResult(
            errors.Count == 0,
            errors.Count > 0 ? errors.AsReadOnly() : null
        );
    }

    /// <summary>
    /// Validates the specified <see cref="TemplateManifest"/> and throws a
    /// <see cref="TemplateValidationException"/> if validation fails.
    /// </summary>
    /// <param name="manifest">The manifest to validate. Must not be <c>null</c>.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="manifest"/> is <c>null</c>.</exception>
    /// <exception cref="TemplateValidationException">Thrown when validation fails, containing all accumulated errors.</exception>
    public void ValidateAndThrow(TemplateManifest manifest)
    {
        var result = Validate(manifest);

        if (!result.IsValid && result.Errors is not null)
        {
            throw new TemplateValidationException(result.Errors.ToList().AsReadOnly());
        }
    }
}
