using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using NextNet.Core.Extensions;

namespace NextNet.Cli.Templates;

/// <summary>
/// Manages project template discovery, placeholder replacement, and file copying.
/// Templates are embedded as resources in the assembly.
/// Each file's relative path is encoded in the resource name.
/// </summary>
[Obsolete("Use TemplateEngine instead")]
public sealed class TemplateManager
{
    private const string ResourcePrefix = "NextNet.Cli.Templates";
    private static readonly Assembly Assembly = typeof(TemplateManager).Assembly;

    private static readonly HashSet<string> KnownTemplates = new(StringComparer.OrdinalIgnoreCase)
    {
        "fullstack", "minimal", "api", "empty"
    };

    /// <summary>
    /// Maps known templates to their embedded file resource suffixes.
    /// The key is the suffix after "NextNet.Cli.Templates.Templates.{templateName}.".
    /// Dots in the suffix represent either directory separators or dots in filenames.
    /// We use a simple heuristic: the last segment is the file extension, and
    /// everything before that (minus known multi-part extensions) forms the filename + path.
    /// </summary>
    private static readonly Dictionary<string, string[]> TemplateFileMappings = new()
    {
        ["fullstack"] = new[]
        {
            "{{PROJECT_NAME}}.csproj",
            "nextnet.config.json",
            "app/layout.cs",
            "app/page.cs",
            "app/api/health/route.cs"
        },
        ["minimal"] = new[]
        {
            "{{PROJECT_NAME}}.csproj",
            "nextnet.config.json",
            "app/layout.cs",
            "app/page.cs"
        },
        ["api"] = new[]
        {
            "{{PROJECT_NAME}}.csproj",
            "nextnet.config.json",
            "app/api/health/route.cs"
        },
        ["empty"] = new[]
        {
            "{{PROJECT_NAME}}.csproj",
            "nextnet.config.json"
        }
    };

    /// <summary>
    /// Returns the list of available template names.
    /// </summary>
    public static string[] GetAvailableTemplates() => KnownTemplates.ToArray();

    /// <summary>
    /// Check if a template name is known.
    /// </summary>
    public static bool IsKnownTemplate(string name) => KnownTemplates.Contains(name);

    /// <summary>
    /// The placeholder replacement values.
    /// </summary>
    public sealed record PlaceholderValues(
        string ProjectName,
        string ProjectNamePascal,
        string Version,
        string Date,
        string DotnetVersion,
        string Namespace);

    /// <summary>
    /// Detect placeholder values from the project name and environment.
    /// </summary>
    public static PlaceholderValues DetectValues(string projectName)
    {
        var kebabName = ToKebabCase(projectName);
        var pascalName = StringCaseHelper.ToPascalCase(kebabName);
        var dotnetVersion = DetectDotNetVersion();

        return new PlaceholderValues(
            ProjectName: kebabName,
            ProjectNamePascal: pascalName,
            Version: "1.0.0",
            Date: DateTime.Now.ToString("yyyy-MM-dd"),
            DotnetVersion: dotnetVersion,
            Namespace: pascalName
        );
    }

    /// <summary>
    /// Scaffold a project from the specified template.
    /// </summary>
    /// <param name="templateName">Template name (fullstack, minimal, api, empty).</param>
    /// <param name="outputDirectory">Output directory for the new project.</param>
    /// <param name="values">Placeholder values for replacement.</param>
    /// <param name="dryRun">If true, only list files without writing.</param>
    /// <returns>A list of files that would be/were created.</returns>
    public static List<string> Scaffold(string templateName, string outputDirectory, PlaceholderValues values, bool dryRun = false)
    {
        if (!IsKnownTemplate(templateName))
            throw new ArgumentException($"Unknown template '{templateName}'. Available: {string.Join(", ", KnownTemplates)}", nameof(templateName));

        var files = new List<string>();

        foreach (var relativePath in TemplateFileMappings[templateName])
        {
            // Convert relative path to resource name format
            // "app/layout.cs" → "Templates.{templateName}.app.layout.cs"
            var resourcePath = relativePath.Replace('/', '.').Replace('\\', '.');
            var resourceName = $"{ResourcePrefix}.Templates.{templateName}.{resourcePath}";

            var filePath = relativePath.Replace("{{PROJECT_NAME}}", values.ProjectName)
                .Replace("{{PROJECT_NAME_PASCAL}}", values.ProjectNamePascal);

            if (dryRun)
            {
                files.Add(Path.Combine(outputDirectory, filePath));
                continue;
            }

            using var stream = Assembly.GetManifestResourceStream(resourceName);
            if (stream is null)
                continue;

            using var reader = new StreamReader(stream);
            var content = reader.ReadToEnd();

            // Replace placeholders
            content = ReplacePlaceholders(content, values);

            var fullPath = Path.Combine(outputDirectory, filePath);
            var dir = Path.GetDirectoryName(fullPath);
            if (dir is not null) Directory.CreateDirectory(dir);

            File.WriteAllText(fullPath, content, Encoding.UTF8);
            files.Add(fullPath);
        }

        return files;
    }

    /// <summary>
    /// List all files that would be created for a template (used by --dry-run).
    /// </summary>
    public static List<string> ListFiles(string templateName, string outputDirectory, PlaceholderValues values)
    {
        return Scaffold(templateName, outputDirectory, values, dryRun: true);
    }

    private static string ReplacePlaceholders(string content, PlaceholderValues values)
    {
        return content
            .Replace("{{PROJECT_NAME}}", values.ProjectName)
            .Replace("{{PROJECT_NAME_PASCAL}}", values.ProjectNamePascal)
            .Replace("{{VERSION}}", values.Version)
            .Replace("{{DATE}}", values.Date)
            .Replace("{{DOTNET_VERSION}}", values.DotnetVersion)
            .Replace("{{NAMESPACE}}", values.Namespace);
    }

    private static string ToKebabCase(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        var result = new StringBuilder();
        foreach (var ch in name.ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(ch))
                result.Append(ch);
            else if (ch is '-' or '_' or ' ')
                result.Append('-');
        }
        return result.ToString().Trim('-');
    }

    private static string DetectDotNetVersion()
    {
        try
        {
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = "--version",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit(5000);

            if (process.ExitCode == 0 && !string.IsNullOrWhiteSpace(output))
            {
                var version = output.Trim();
                var match = Regex.Match(version, @"^(\d+\.\d+)");
                return match.Success ? match.Groups[1].Value : "10.0";
            }
        }
        catch
        {
            // Ignore errors detecting SDK version
        }
        return "10.0";
    }
}
