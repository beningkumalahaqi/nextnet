using System.Reflection;
using NextNet.Data.Abstractions.Internal;
using NextNet.Data.Abstractions.Models;
using NextNet.Data.EntityFramework.Scaffolding.Internal;

namespace NextNet.Data.EntityFramework;

/// <summary>
/// EF Core implementation of <see cref="IScaffoldProvider"/> for generating
/// model classes, repository classes, and CRUD action files.
/// </summary>
/// <remarks>
/// <para>
/// Code generation uses embedded template resources in <c>NextNet.Data.EntityFramework.Templates</c>.
/// Templates use {{PLACEHOLDER}} syntax for substitution.
/// </para>
/// <para>
/// Output paths and namespaces are configured via <see cref="ScaffoldOptions"/>.
/// </para>
/// </remarks>
public sealed class EfCoreScaffoldProvider : IScaffoldProvider
{
    private static readonly Assembly ProviderAssembly = typeof(EfCoreScaffoldProvider).Assembly;
    private const string TemplatePrefix = "NextNet.Data.EntityFramework.Templates";
    private const string ToolVersion = "1.0.0";

    /// <summary>
    /// The EF Core scaffold provider version.
    /// </summary>
    public const string Version = "1.0.0";

    /// <inheritdoc />
    public string ProviderName => "EntityFramework";

    /// <inheritdoc />
    public Task<ScaffoldArtifact> GenerateModelAsync(
        string entityName,
        ScaffoldOptions options,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(entityName))
            throw new ArgumentNullException(nameof(entityName));

        var ns = TemplateEngine.ResolveNamespace(options.ModelsNamespace, options.ProjectNamespace);
        var pluralName = TemplateEngine.Pluralize(entityName);
        var camelName = char.ToLowerInvariant(entityName[0]) + entityName[1..];
        var keyType = DetermineKeyType(options.Properties);
        var propertiesBlock = TemplatePropertyBuilder.BuildPropertyDeclarations(options.Properties);

        var placeholders = new Dictionary<string, string>
        {
            ["ToolVersion"] = ToolVersion,
            ["GeneratedDate"] = DateTime.Now.ToString("yyyy-MM-dd"),
            ["EntityName"] = entityName,
            ["EntityNamePlural"] = pluralName,
            ["EntityNameCamel"] = camelName,
            ["ModelNamespace"] = ns,
            ["CollectionName"] = pluralName,
            ["KeyType"] = keyType,
            ["Properties"] = propertiesBlock
        };

        var outputDir = Path.GetFullPath(Path.Combine(options.OutputDirectory, options.ModelsDirectory));
        var filePath = Path.Combine(outputDir, $"{entityName}.cs");
        var relativePath = Path.Combine(options.ModelsDirectory, $"{entityName}.cs");
        var resourceName = $"{TemplatePrefix}.Model.template";

        // Check if file should be skipped BEFORE writing
        var fileExists = File.Exists(filePath);
        var shouldSkip = !options.DryRun && fileExists && !options.OverwriteExisting;

        int lines;
        if (shouldSkip)
        {
            // Read existing file for line count
            var existingContent = File.ReadAllText(filePath);
            lines = TemplateEngine.CountLines(existingContent);
        }
        else
        {
            lines = TemplateEngine.GenerateFromTemplate(
                ProviderAssembly, resourceName, filePath, placeholders, options.DryRun);
        }

        var artifact = new ScaffoldArtifact(
            FilePath: filePath,
            RelativePath: relativePath,
            ArtifactType: ScaffoldArtifactType.Model,
            EntityName: entityName,
            LinesOfCode: lines,
            WasSkipped: shouldSkip);

        return Task.FromResult(artifact);
    }

    /// <inheritdoc />
    public Task<ScaffoldArtifact> GenerateRepositoryAsync(
        string entityName,
        ScaffoldOptions options,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(entityName))
            throw new ArgumentNullException(nameof(entityName));

        var modelsNs = TemplateEngine.ResolveNamespace(options.ModelsNamespace, options.ProjectNamespace);
        var reposNs = TemplateEngine.ResolveNamespace(options.RepositoriesNamespace, options.ProjectNamespace);
        var pluralName = TemplateEngine.Pluralize(entityName);
        var camelName = char.ToLowerInvariant(entityName[0]) + entityName[1..];
        var keyType = DetermineKeyType(options.Properties);

        var placeholders = new Dictionary<string, string>
        {
            ["ToolVersion"] = ToolVersion,
            ["GeneratedDate"] = DateTime.Now.ToString("yyyy-MM-dd"),
            ["EntityName"] = entityName,
            ["EntityNamePlural"] = pluralName,
            ["EntityNameCamel"] = camelName,
            ["ModelNamespace"] = modelsNs,
            ["RepositoryNamespace"] = reposNs,
            ["CollectionName"] = pluralName,
            ["DbContextName"] = "AppDbContext",
            ["KeyType"] = keyType
        };

        var outputDir = Path.GetFullPath(Path.Combine(options.OutputDirectory, options.RepositoriesDirectory));
        var filePath = Path.Combine(outputDir, $"{entityName}Repository.cs");
        var relativePath = Path.Combine(options.RepositoriesDirectory, $"{entityName}Repository.cs");
        var resourceName = $"{TemplatePrefix}.Repository.template";

        // Check if file should be skipped BEFORE writing
        var fileExists = File.Exists(filePath);
        var shouldSkip = !options.DryRun && fileExists && !options.OverwriteExisting;

        int lines;
        if (shouldSkip)
        {
            var existingContent = File.ReadAllText(filePath);
            lines = TemplateEngine.CountLines(existingContent);
        }
        else
        {
            lines = TemplateEngine.GenerateFromTemplate(
                ProviderAssembly, resourceName, filePath, placeholders, options.DryRun);
        }

        var artifact = new ScaffoldArtifact(
            FilePath: filePath,
            RelativePath: relativePath,
            ArtifactType: ScaffoldArtifactType.Repository,
            EntityName: entityName,
            LinesOfCode: lines,
            WasSkipped: shouldSkip);

        return Task.FromResult(artifact);
    }

    /// <inheritdoc />
    public Task<ScaffoldArtifact[]> GenerateCrudAsync(
        string entityName,
        ScaffoldOptions options,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(entityName))
            throw new ArgumentNullException(nameof(entityName));

        var results = new List<ScaffoldArtifact>();

        // Generate model
        var modelArtifact = GenerateModelAsync(entityName, options, cancellationToken).GetAwaiter().GetResult();
        results.Add(modelArtifact);

        // Generate repository
        var repoArtifact = GenerateRepositoryAsync(entityName, options, cancellationToken).GetAwaiter().GetResult();
        results.Add(repoArtifact);

        // Generate CRUD route
        var modelsNs = TemplateEngine.ResolveNamespace(options.ModelsNamespace, options.ProjectNamespace);
        var reposNs = TemplateEngine.ResolveNamespace(options.RepositoriesNamespace, options.ProjectNamespace);
        var actionsNs = TemplateEngine.ResolveNamespace(options.ActionsNamespace, options.ProjectNamespace);
        var pluralName = TemplateEngine.Pluralize(entityName);
        var camelName = char.ToLowerInvariant(entityName[0]) + entityName[1..];
        var keyType = DetermineKeyType(options.Properties);

        var routePlaceholders = new Dictionary<string, string>
        {
            ["ToolVersion"] = ToolVersion,
            ["GeneratedDate"] = DateTime.Now.ToString("yyyy-MM-dd"),
            ["EntityName"] = entityName,
            ["EntityNamePlural"] = pluralName,
            ["EntityNameCamel"] = camelName,
            ["ModelNamespace"] = modelsNs,
            ["RepositoryNamespace"] = reposNs,
            ["ActionsNamespace"] = actionsNs,
            ["CollectionName"] = pluralName,
            ["KeyType"] = keyType
        };

        var actionsDir = Path.GetFullPath(Path.Combine(options.OutputDirectory, options.ActionsDirectory, entityName));
        var routePath = Path.Combine(actionsDir, "route.cs");
        var routeRelativePath = Path.Combine(options.ActionsDirectory, entityName, "route.cs");
        var routeResourceName = $"{TemplatePrefix}.Crud.Route.template";

        // Check if file should be skipped BEFORE writing
        var routeFileExists = File.Exists(routePath);
        var routeShouldSkip = !options.DryRun && routeFileExists && !options.OverwriteExisting;

        int routeLines;
        if (routeShouldSkip)
        {
            var existingContent = File.ReadAllText(routePath);
            routeLines = TemplateEngine.CountLines(existingContent);
        }
        else
        {
            routeLines = TemplateEngine.GenerateFromTemplate(
                ProviderAssembly, routeResourceName, routePath, routePlaceholders, options.DryRun);
        }

        results.Add(new ScaffoldArtifact(
            FilePath: routePath,
            RelativePath: routeRelativePath,
            ArtifactType: ScaffoldArtifactType.Action,
            EntityName: entityName,
            LinesOfCode: routeLines,
            WasSkipped: routeShouldSkip));

        return Task.FromResult(results.ToArray());
    }

    private static string DetermineKeyType(IReadOnlyList<ScaffoldProperty>? properties)
    {
        if (properties is not null)
        {
            foreach (var prop in properties)
            {
                if (prop.IsKey)
                    return prop.Type;
            }
        }
        return "int";
    }
}
