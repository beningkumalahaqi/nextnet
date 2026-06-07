using System.CommandLine;
using NextNet.TemplateSdk.Validation;

namespace NextNet.TemplateSdk.CLI;

/// <summary>
/// Implements the <c>template validate</c> command — validates a template directory
/// or package against all built-in rules.
/// </summary>
/// <remarks>
/// <para>
/// The <c>template validate</c> command loads a <c>template.json</c> manifest from the
/// specified directory and runs all validation rules (manifest schema, variable
/// completeness, placeholder coverage, file existence, condition syntax, and
/// executable file detection).
/// </para>
/// <para>
/// Results are displayed with severity levels: errors block usage, warnings
/// indicate potential issues, and info messages provide suggestions.
/// </para>
/// </remarks>
public sealed class TemplateValidateCommand : Command
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TemplateValidateCommand"/> class.
    /// </summary>
    public TemplateValidateCommand() : base("validate", "Validate a template directory")
    {
        var dirArg = new Argument<string>("directory", () => ".", "Template directory to validate");

        AddArgument(dirArg);

        this.SetHandler(HandleAsync, dirArg);
    }

    private static async Task<int> HandleAsync(string directory)
    {
        try
        {
            var manifestPath = Path.Combine(directory, "template.json");
            if (!File.Exists(manifestPath))
            {
                Console.Error.WriteLine($"Error: template.json not found in {directory}");
                return 1;
            }

            var manifestJson = await File.ReadAllTextAsync(manifestPath);
            var manifest = System.Text.Json.JsonSerializer.Deserialize<NextNet.Templates.Models.TemplateManifest>(
                manifestJson,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (manifest is null)
            {
                Console.Error.WriteLine("Error: Failed to parse template.json");
                return 1;
            }

            var validator = new TemplateValidator();
            var result = await validator.ValidateAsync(manifest);

            Console.WriteLine($"Validation results for {manifest.Name} v{manifest.Version}:");
            Console.WriteLine();

            if (result.Errors is { Count: > 0 })
            {
                Console.WriteLine($"  Errors ({result.Errors.Count}):");
                foreach (var error in result.Errors)
                    Console.WriteLine($"    - {error}");
            }

            if (result.Warnings is { Count: > 0 })
            {
                Console.WriteLine($"  Warnings ({result.Warnings.Count}):");
                foreach (var warning in result.Warnings)
                    Console.WriteLine($"    - {warning}");
            }

            Console.WriteLine();
            Console.WriteLine(result.IsValid ? "  ✓ Valid" : "  ✗ Invalid");
            return result.IsValid ? 0 : 1;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }
}
