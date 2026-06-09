using NextNet.Data.Dapper.Templates;

namespace NextNet.Data.Dapper.Scaffolding;

/// <summary>
/// Dapper implementation of <see cref="IScaffoldProvider"/> that generates
/// model classes, repository classes, and CRUD API routes using embedded
/// string templates.
/// </summary>
/// <remarks>
/// <para>
/// This provider uses <see cref="DapperTemplates"/> for code generation
/// with {{PLACEHOLDER}} substitution. Generated repositories extend
/// <see cref="DapperRepository{T}"/> and use parameterized SQL exclusively.
/// </para>
/// <para>
/// Output paths, namespaces, and overwrite behavior are controlled by
/// <see cref="ScaffoldOptions"/>.
/// </para>
/// <example>
/// <code>
/// var provider = new DapperScaffoldProvider(logger);
///
/// // Generate a model only
/// var model = await provider.GenerateModelAsync("User", new ScaffoldOptions(
///     OutputDirectory: "/project/src",
///     ModelsNamespace: "MyApp.Models",
///     Properties: new[] {
///         new ScaffoldProperty("Id", "int", IsKey: true),
///         new ScaffoldProperty("Name", "string", IsRequired: true),
///         new ScaffoldProperty("Email", "string")
///     }));
///
/// // Generate full CRUD
/// var artifacts = await provider.GenerateCrudAsync("Product", new ScaffoldOptions(
///     ProjectNamespace: "MyApp",
///     OverwriteExisting: true));
/// </code>
/// </example>
/// </remarks>
public sealed class DapperScaffoldProvider : IScaffoldProvider
{
    private readonly ILogger<DapperScaffoldProvider>? _logger;

    private const string DefaultConnectionName = "Default";

    /// <summary>
    /// Gets the provider name "Dapper", matching the data provider registration.
    /// </summary>
    public string ProviderName => "Dapper";

    /// <summary>
    /// Initializes a new instance of <see cref="DapperScaffoldProvider"/>.
    /// </summary>
    /// <param name="logger">Optional logger for diagnostics.</param>
    public DapperScaffoldProvider(ILogger<DapperScaffoldProvider>? logger = null)
    {
        _logger = logger;
    }

    /// <summary>
    /// Generates a plain entity class for the specified entity.
    /// </summary>
    /// <param name="entityName">The entity name (e.g., "User", "Product").</param>
    /// <param name="options">Options controlling output paths, namespace, and overwrite behavior.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="ScaffoldArtifact"/> describing the generated file.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entityName"/> is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
    public Task<ScaffoldArtifact> GenerateModelAsync(
        string entityName,
        ScaffoldOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityName);
        ArgumentNullException.ThrowIfNull(options);

        _logger?.LogInformation("Generating model for '{EntityName}'...", entityName);

        var placeholders = BuildModelPlaceholders(entityName, options);
        var content = ReplacePlaceholders(DapperTemplates.MODEL_TEMPLATE, placeholders);

        var filePath = GetModelFilePath(entityName, options);
        var relativePath = GetRelativePath(filePath, options.OutputDirectory);
        var wasSkipped = WriteOrSkip(filePath, content, options.DryRun, options.OverwriteExisting);
        var lineCount = CountLines(content);

        _logger?.LogDebug(
            "Model '{EntityName}' {Action}: {FilePath} ({Lines} lines)",
            entityName, wasSkipped ? "skipped" : "written", filePath, lineCount);

        return Task.FromResult(new ScaffoldArtifact(
            filePath,
            relativePath,
            ScaffoldArtifactType.Model,
            entityName,
            lineCount,
            WasSkipped: wasSkipped));
    }

    /// <summary>
    /// Generates a Dapper repository class for the specified entity.
    /// The repository extends <see cref="DapperRepository{T}"/>.
    /// </summary>
    /// <param name="entityName">The entity name (e.g., "User", "Product").</param>
    /// <param name="options">Options controlling output paths, namespace, and overwrite behavior.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="ScaffoldArtifact"/> describing the generated file.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entityName"/> is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
    public Task<ScaffoldArtifact> GenerateRepositoryAsync(
        string entityName,
        ScaffoldOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityName);
        ArgumentNullException.ThrowIfNull(options);

        _logger?.LogInformation("Generating repository for '{EntityName}'...", entityName);

        var modelNs = ResolveNamespace(options.ModelsNamespace, options.ProjectNamespace);
        var repoNs = ResolveNamespace(options.RepositoriesNamespace, options.ProjectNamespace);

        var placeholders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["ModelNamespace"] = modelNs,
            ["RepositoryNamespace"] = repoNs,
            ["EntityName"] = entityName,
            ["ConnectionName"] = DefaultConnectionName
        };

        var content = ReplacePlaceholders(DapperTemplates.REPOSITORY_TEMPLATE, placeholders);

        var filePath = GetRepositoryFilePath(entityName, options);
        var relativePath = GetRelativePath(filePath, options.OutputDirectory);
        var wasSkipped = WriteOrSkip(filePath, content, options.DryRun, options.OverwriteExisting);
        var lineCount = CountLines(content);

        _logger?.LogDebug(
            "Repository '{EntityName}' {Action}: {FilePath} ({Lines} lines)",
            entityName, wasSkipped ? "skipped" : "written", filePath, lineCount);

        return Task.FromResult(new ScaffoldArtifact(
            filePath,
            relativePath,
            ScaffoldArtifactType.Repository,
            entityName,
            lineCount,
            WasSkipped: wasSkipped));
    }

    /// <summary>
    /// Generates a full set of CRUD artifacts: model, repository, and API route.
    /// </summary>
    /// <param name="entityName">The entity name (e.g., "User", "Product").</param>
    /// <param name="options">Options controlling output paths, namespace, and overwrite behavior.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An array of <see cref="ScaffoldArtifact"/> describing each generated file.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entityName"/> is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
    public async Task<ScaffoldArtifact[]> GenerateCrudAsync(
        string entityName,
        ScaffoldOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityName);
        ArgumentNullException.ThrowIfNull(options);

        _logger?.LogInformation("Generating full CRUD for '{EntityName}'...", entityName);

        var results = new List<ScaffoldArtifact>(3);

        // 1. Generate model
        var modelArtifact = await GenerateModelAsync(entityName, options, cancellationToken);
        results.Add(modelArtifact);

        // 2. Generate repository
        var repoArtifact = await GenerateRepositoryAsync(entityName, options, cancellationToken);
        results.Add(repoArtifact);

        // 3. Generate API route
        var routeArtifact = GenerateRoute(entityName, options);
        results.Add(routeArtifact);

        _logger?.LogInformation(
            "CRUD for '{EntityName}' generated: {Count} artifact(s).",
            entityName, results.Count);

        return results.ToArray();
    }

    /// <summary>
    /// Generates the combined API route file for CRUD operations.
    /// </summary>
    private ScaffoldArtifact GenerateRoute(string entityName, ScaffoldOptions options)
    {
        var modelNs = ResolveNamespace(options.ModelsNamespace, options.ProjectNamespace);
        var repoNs = ResolveNamespace(options.RepositoriesNamespace, options.ProjectNamespace);
        var actionNs = ResolveNamespace(options.ActionsNamespace, options.ProjectNamespace);

        var placeholders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["ModelNamespace"] = modelNs,
            ["RepositoryNamespace"] = repoNs,
            ["ActionsNamespace"] = actionNs,
            ["EntityName"] = entityName,
            ["EntityNameLower"] = entityName.ToLowerInvariant(),
            ["ConnectionName"] = DefaultConnectionName
        };

        var content = ReplacePlaceholders(DapperTemplates.CRUD_ROUTE_TEMPLATE, placeholders);

        var relativeDir = string.IsNullOrWhiteSpace(options.ActionsDirectory)
            ? "app/api"
            : options.ActionsDirectory;
        var filePath = Path.GetFullPath(
            Path.Combine(options.OutputDirectory, relativeDir, $"{entityName}Controller.cs"));
        var relativePath = GetRelativePath(filePath, options.OutputDirectory);
        var wasSkipped = WriteOrSkip(filePath, content, options.DryRun, options.OverwriteExisting);
        var lineCount = CountLines(content);

        _logger?.LogDebug(
            "Route '{EntityName}' {Action}: {FilePath} ({Lines} lines)",
            entityName, wasSkipped ? "skipped" : "written", filePath, lineCount);

        return new ScaffoldArtifact(
            filePath,
            relativePath,
            ScaffoldArtifactType.Action,
            entityName,
            lineCount,
            WasSkipped: wasSkipped);
    }

    /// <summary>
    /// Builds placeholders for the model template, processing properties defined in options.
    /// </summary>
    private static IReadOnlyDictionary<string, string> BuildModelPlaceholders(
        string entityName, ScaffoldOptions options)
    {
        var modelNs = ResolveNamespace(options.ModelsNamespace, options.ProjectNamespace);

        // Generate table attribute
        var tableAttribute = string.Empty;
        if (!string.IsNullOrWhiteSpace(options.ProjectNamespace))
        {
            tableAttribute = $"[Table(\"{Pluralize(entityName)}\")]";
        }

        // Generate property declarations
        var properties = options.Properties;
        string keyDeclaration;
        string otherProperties;

        if (properties is null || properties.Count == 0)
        {
            // Default: generate Id key property only
            keyDeclaration = "    /// <summary>\n    /// Gets or sets the primary key.\n    /// </summary>\n    [Key]\n    public int Id { get; set; }";
            otherProperties = string.Empty;
        }
        else
        {
            var keyProps = properties.Where(p => p.IsKey).ToList();
            var nonKeyProps = properties.Where(p => !p.IsKey).ToList();

            // Build key declaration
            if (keyProps.Count > 0)
            {
                var keyBuilder = new System.Text.StringBuilder();
                foreach (var prop in keyProps)
                {
                    keyBuilder.AppendLine(GenerateProperty(prop));
                }
                keyDeclaration = keyBuilder.ToString().TrimEnd();
            }
            else
            {
                // If no explicit key found, assume first property or default Id
                keyDeclaration = "    /// <summary>\n    /// Gets or sets the primary key.\n    /// </summary>\n    [Key]\n    public int Id { get; set; }";
            }

            // Build other properties
            if (nonKeyProps.Count > 0)
            {
                var propsBuilder = new System.Text.StringBuilder();
                foreach (var prop in nonKeyProps)
                {
                    propsBuilder.AppendLine(GenerateProperty(prop));
                }
                otherProperties = propsBuilder.ToString().TrimEnd();
            }
            else
            {
                otherProperties = string.Empty;
            }
        }

        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["ModelNamespace"] = modelNs,
            ["EntityName"] = entityName,
            ["TableAttribute"] = tableAttribute,
            ["Summary"] = $"Represents a {entityName} entity.",
            ["KeyDeclaration"] = keyDeclaration,
            ["Properties"] = otherProperties
        };
    }

    /// <summary>
    /// Generates a C# property declaration from a <see cref="ScaffoldProperty"/>.
    /// </summary>
    private static string GenerateProperty(ScaffoldProperty prop)
    {
        var type = string.IsNullOrWhiteSpace(prop.Type) ? "string" : prop.Type;
        var isNullable = !prop.IsRequired && type != "string"
            ? "?"
            : string.Empty;
        var defaultValue = type == "string"
            ? " = string.Empty;"
            : string.Empty;

        var builder = new System.Text.StringBuilder();

        // XML doc comment
        builder.AppendLine($"    /// <summary>");
        builder.AppendLine($"    /// Gets or sets the {prop.Name}.");
        builder.AppendLine($"    /// </summary>");

        // Key attribute
        if (prop.IsKey)
        {
            builder.AppendLine("    [Key]");
        }

        // MaxLength attribute for strings
        if (prop.MaxLength.HasValue && type == "string")
        {
            builder.AppendLine($"    [MaxLength({prop.MaxLength.Value})]");
        }

        // Property declaration
        builder.AppendLine($"    public {type}{isNullable} {prop.Name} {{ get; set; }}{defaultValue}");

        return builder.ToString().TrimEnd();
    }

    /// <summary>
    /// Gets the full file path for a generated model class.
    /// </summary>
    private static string GetModelFilePath(string entityName, ScaffoldOptions options)
    {
        var relativeDir = string.IsNullOrWhiteSpace(options.ModelsDirectory)
            ? "Models"
            : options.ModelsDirectory;
        return Path.GetFullPath(Path.Combine(options.OutputDirectory, relativeDir, $"{entityName}.cs"));
    }

    /// <summary>
    /// Gets the full file path for a generated repository class.
    /// </summary>
    private static string GetRepositoryFilePath(string entityName, ScaffoldOptions options)
    {
        var relativeDir = string.IsNullOrWhiteSpace(options.RepositoriesDirectory)
            ? "Repositories"
            : options.RepositoriesDirectory;
        return Path.GetFullPath(Path.Combine(options.OutputDirectory, relativeDir, $"{entityName}Repository.cs"));
    }

    /// <summary>
    /// Computes a relative path from an absolute path and a base directory.
    /// </summary>
    private static string GetRelativePath(string fullPath, string baseDirectory)
    {
        var fullDir = Path.GetFullPath(baseDirectory);
        var fullFile = Path.GetFullPath(fullPath);

        if (!fullDir.EndsWith(Path.DirectorySeparatorChar.ToString()))
        {
            fullDir += Path.DirectorySeparatorChar;
        }

        if (fullFile.StartsWith(fullDir, StringComparison.OrdinalIgnoreCase))
        {
            return fullFile.Substring(fullDir.Length);
        }

        return fullFile;
    }

    // ===== Template Helper Methods =====

    /// <summary>
    /// Replaces {{PLACEHOLDER}} tokens in the content with the provided values.
    /// </summary>
    private static string ReplacePlaceholders(string content, IReadOnlyDictionary<string, string> placeholders)
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
    private static string ResolveNamespace(string namespacePattern, string? projectNamespace)
    {
        if (string.IsNullOrWhiteSpace(namespacePattern))
            return "App";

        var ns = projectNamespace ?? "App";
        return namespacePattern.Replace("{Project}", ns).Replace("{{Project}}", ns);
    }

    /// <summary>
    /// Simple English pluralization helper.
    /// Delegates to the canonical <see cref="NextNet.Data.Abstractions.Internal.Pluralizer"/> implementation.
    /// </summary>
    private static string Pluralize(string singular)
        => NextNet.Data.Abstractions.Internal.Pluralizer.Pluralize(singular);

    /// <summary>
    /// Counts the number of lines in the given string.
    /// </summary>
    private static int CountLines(string content)
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
    private static bool WriteOrSkip(string filePath, string content, bool dryRun, bool overwriteExisting)
    {
        if (dryRun)
            return false;

        if (!overwriteExisting && File.Exists(filePath))
            return true; // Skipped

        var dir = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        File.WriteAllText(filePath, content, System.Text.Encoding.UTF8);
        return false; // Written successfully
    }
}
