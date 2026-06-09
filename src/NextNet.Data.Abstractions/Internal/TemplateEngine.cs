using System.Linq;
using System.Reflection;
using System.Text;

namespace NextNet.Data.Abstractions.Internal;

/// <summary>
/// Internal helper for processing embedded template resources.
/// Reads template content from assembly resources, replaces {{PLACEHOLDER}} tokens,
/// and writes the result to a file.
/// </summary>
internal static class TemplateEngine
{
    /// <summary>
    /// Processes a template embedded resource and returns the generated content.
    /// </summary>
    /// <param name="assembly">The assembly containing the embedded resource.</param>
    /// <param name="resourceName">The fully qualified resource name.</param>
    /// <param name="placeholders">The placeholder values to substitute.</param>
    /// <returns>The processed template content.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the embedded resource is not found.</exception>
    public static string ProcessTemplate(
        Assembly assembly,
        string resourceName,
        IReadOnlyDictionary<string, string> placeholders)
    {
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
            throw new InvalidOperationException(
                $"[{DataAbstractionsErrorCodes.MigrationFailed}] Embedded resource '{resourceName}' not found in assembly '{assembly.GetName().Name}'.");

        using var reader = new StreamReader(stream, Encoding.UTF8);
        var content = reader.ReadToEnd();

        return ReplacePlaceholders(content, placeholders);
    }

    /// <summary>
    /// Processes a template and writes the output to a file.
    /// </summary>
    /// <param name="assembly">The assembly containing the embedded resource.</param>
    /// <param name="resourceName">The fully qualified resource name.</param>
    /// <param name="outputPath">The full output file path.</param>
    /// <param name="placeholders">The placeholder values to substitute.</param>
    /// <param name="dryRun">If true, skip writing and only return line count.</param>
    /// <returns>The number of lines in the generated content.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the embedded resource is not found.</exception>
    public static int GenerateFromTemplate(
        Assembly assembly,
        string resourceName,
        string outputPath,
        IReadOnlyDictionary<string, string> placeholders,
        bool dryRun = false)
    {
        var content = ProcessTemplate(assembly, resourceName, placeholders);
        var lines = content.Count(c => c == '\n') + 1;

        if (!dryRun)
        {
            var dir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            File.WriteAllText(outputPath, content, Encoding.UTF8);
        }

        return lines;
    }

    /// <summary>
    /// Replaces {{PLACEHOLDER}} tokens in the content with the provided values.
    /// </summary>
    public static string ReplacePlaceholders(string content, IReadOnlyDictionary<string, string> placeholders)
    {
        var result = content;
        foreach (var (key, value) in placeholders)
        {
            result = result.Replace($"{{{{{key}}}}}", value);
        }
        return result;
    }

    /// <summary>
    /// Resolves a {{PROJECT}} token in a namespace pattern to the actual project namespace.
    /// E.g., "{Project}.Models" with projectNamespace="MyApp" becomes "MyApp.Models".
    /// </summary>
    /// <param name="namespacePattern">The namespace pattern possibly containing "{Project}".</param>
    /// <param name="projectNamespace">The project root namespace to substitute.</param>
    /// <returns>The resolved namespace string.</returns>
    public static string ResolveNamespace(string namespacePattern, string? projectNamespace)
    {
        if (string.IsNullOrWhiteSpace(namespacePattern))
            return "App";

        var ns = projectNamespace ?? "App";
        return namespacePattern.Replace("{Project}", ns).Replace("{{Project}}", ns);
    }

    /// <summary>
    /// Simple English pluralization helper.
    /// Delegates to the canonical <see cref="Pluralizer"/> implementation.
    /// </summary>
    public static string Pluralize(string singular) => Pluralizer.Pluralize(singular);

    /// <summary>
    /// Counts the number of lines in the given string.
    /// </summary>
    internal static int CountLines(string content)
    {
        if (string.IsNullOrEmpty(content))
            return 0;

        int count = 1;
        foreach (var c in content)
        {
            if (c == '\n') count++;
        }
        return content.EndsWith('\n') ? count - 1 : count;
    }

    /// <summary>
    /// Writes content to a file, respecting dry-run and overwrite settings.
    /// Returns true if the file was skipped (already exists and OverwriteExisting was false).
    /// </summary>
    internal static bool WriteOrSkip(string filePath, string content, bool dryRun, bool overwriteExisting)
    {
        if (dryRun)
            return false;

        if (!overwriteExisting && File.Exists(filePath))
            return true; // Skipped

        var dir = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        File.WriteAllText(filePath, content, Encoding.UTF8);
        return false; // Written successfully
    }
}
