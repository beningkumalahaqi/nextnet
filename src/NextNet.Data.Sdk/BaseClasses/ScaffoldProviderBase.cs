#if NET8_0_OR_GREATER
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NextNet.Data.Abstractions.Abstractions;
using NextNet.Data.Abstractions.Configuration;
using NextNet.Data.Abstractions.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace NextNet.Data.Sdk.Base;

/// <summary>
/// Base class for provider-specific scaffolding. Implements <see cref="IScaffoldProvider"/>
/// with file output helpers, namespace resolution, and template rendering infrastructure.
/// </summary>
/// <remarks>
/// <para>
/// Provider authors override <see cref="GenerateModelCoreAsync"/>,
/// <see cref="GenerateRepositoryCoreAsync"/>, and <see cref="GenerateCrudCoreAsync"/>
/// to emit provider-specific code. The base class handles file I/O, overwrite protection,
/// and namespace resolution from <see cref="NextNet.Data.Abstractions.Configuration.ScaffoldingConfig"/>.
/// </para>
/// </remarks>
public abstract class ScaffoldProviderBase : IScaffoldProvider
{
    private readonly IOptions<ScaffoldingConfig> _config;
    private readonly ILogger? _logger;

    /// <summary>
    /// Gets the scaffolding configuration.
    /// </summary>
    protected IOptions<ScaffoldingConfig> Config => _config;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScaffoldProviderBase"/> class.
    /// </summary>
    /// <param name="config">Scaffolding configuration.</param>
    /// <param name="logger">An optional logger.</param>
    protected ScaffoldProviderBase(
        IOptions<ScaffoldingConfig> config,
        ILogger? logger = null)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = logger;
    }

    /// <summary>
    /// Gets the name of this scaffold provider (e.g., "EntityFramework", "Dapper", "MongoDB").
    /// </summary>
    public abstract string ProviderName { get; }

    /// <summary>
    /// Generates a model class file. Validates entity name, resolves output path,
    /// and delegates to <see cref="GenerateModelCoreAsync"/>.
    /// </summary>
    /// <param name="entityName">The name of the entity (e.g., "User", "Product").</param>
    /// <param name="options">Options controlling output paths, namespace, and overwrite behavior.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="ScaffoldArtifact"/> describing the generated file.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="entityName"/> is null or empty.</exception>
    public async Task<ScaffoldArtifact> GenerateModelAsync(
        string entityName,
        ScaffoldOptions options,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(entityName))
            throw new ArgumentNullException(nameof(entityName));

        if (options is null)
            throw new ArgumentNullException(nameof(options));

        try
        {
            var outputPath = ResolveOutputPath(options.ModelsDirectory, $"{entityName}.cs");
            return await GenerateModelCoreAsync(entityName, outputPath, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to generate model for entity '{EntityName}'.", entityName);
            return new ScaffoldArtifact(
                FilePath: string.Empty,
                RelativePath: $"{options.ModelsDirectory}/{entityName}.cs",
                ArtifactType: ScaffoldArtifactType.Model,
                EntityName: entityName,
                LinesOfCode: 0,
                ErrorMessage: ex.Message);
        }
    }

    /// <summary>
    /// Generates a repository class file. Validates model name, resolves output path,
    /// and delegates to <see cref="GenerateRepositoryCoreAsync"/>.
    /// </summary>
    /// <param name="entityName">The name of the entity (e.g., "User", "Product").</param>
    /// <param name="options">Options controlling output paths, namespace, and overwrite behavior.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="ScaffoldArtifact"/> describing the generated file.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="entityName"/> is null or empty.</exception>
    public async Task<ScaffoldArtifact> GenerateRepositoryAsync(
        string entityName,
        ScaffoldOptions options,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(entityName))
            throw new ArgumentNullException(nameof(entityName));

        if (options is null)
            throw new ArgumentNullException(nameof(options));

        try
        {
            var outputPath = ResolveOutputPath(options.RepositoriesDirectory, $"{entityName}Repository.cs");
            return await GenerateRepositoryCoreAsync(entityName, outputPath, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to generate repository for entity '{EntityName}'.", entityName);
            return new ScaffoldArtifact(
                FilePath: string.Empty,
                RelativePath: $"{options.RepositoriesDirectory}/{entityName}Repository.cs",
                ArtifactType: ScaffoldArtifactType.Repository,
                EntityName: entityName,
                LinesOfCode: 0,
                ErrorMessage: ex.Message);
        }
    }

    /// <summary>
    /// Generates a full set of CRUD files. Delegates to <see cref="GenerateCrudCoreAsync"/>.
    /// </summary>
    /// <param name="entityName">The name of the entity (e.g., "User", "Product").</param>
    /// <param name="options">Options controlling output paths, namespace, and overwrite behavior.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An array of <see cref="ScaffoldArtifact"/> describing each generated file.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="entityName"/> is null or empty.</exception>
    public async Task<ScaffoldArtifact[]> GenerateCrudAsync(
        string entityName,
        ScaffoldOptions options,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(entityName))
            throw new ArgumentNullException(nameof(entityName));

        if (options is null)
            throw new ArgumentNullException(nameof(options));

        try
        {
            var outputPath = ResolveOutputPath(options.ActionsDirectory, string.Empty);
            return await GenerateCrudCoreAsync(entityName, outputPath, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to generate CRUD for entity '{EntityName}'.", entityName);
            return new[]
            {
                new ScaffoldArtifact(
                    FilePath: string.Empty,
                    RelativePath: $"{options.ActionsDirectory}/{entityName}",
                    ArtifactType: ScaffoldArtifactType.Action,
                    EntityName: entityName,
                    LinesOfCode: 0,
                    ErrorMessage: ex.Message)
            };
        }
    }

    /// <summary>
    /// Provider-specific model generation.
    /// </summary>
    /// <param name="entityName">The entity name.</param>
    /// <param name="outputPath">The resolved output file path.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="ScaffoldArtifact"/> describing the generated file.</returns>
    protected abstract Task<ScaffoldArtifact> GenerateModelCoreAsync(
        string entityName,
        string outputPath,
        CancellationToken cancellationToken);

    /// <summary>
    /// Provider-specific repository generation.
    /// </summary>
    /// <param name="entityName">The entity name.</param>
    /// <param name="outputPath">The resolved output file path.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="ScaffoldArtifact"/> describing the generated file.</returns>
    protected abstract Task<ScaffoldArtifact> GenerateRepositoryCoreAsync(
        string entityName,
        string outputPath,
        CancellationToken cancellationToken);

    /// <summary>
    /// Provider-specific CRUD generation.
    /// </summary>
    /// <param name="entityName">The entity name.</param>
    /// <param name="outputPath">The resolved output directory path.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An array of <see cref="ScaffoldArtifact"/> describing each generated file.</returns>
    protected abstract Task<ScaffoldArtifact[]> GenerateCrudCoreAsync(
        string entityName,
        string outputPath,
        CancellationToken cancellationToken);

    /// <summary>
    /// Writes content to a file, respecting <see cref="ScaffoldingConfig.OverwriteExisting"/>.
    /// Returns the full path to the written file.
    /// </summary>
    /// <param name="filePath">The full path to the output file.</param>
    /// <param name="content">The content to write.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The full path to the written file.</returns>
    protected async Task<string> WriteFileAsync(string filePath, string content, CancellationToken cancellationToken)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        if (File.Exists(filePath) && !_config.Value.OverwriteExisting)
        {
            _logger?.LogWarning("File already exists and OverwriteExisting is false: '{FilePath}'.", filePath);
            return filePath;
        }

        await File.WriteAllTextAsync(filePath, content, Encoding.UTF8, cancellationToken).ConfigureAwait(false);
        _logger?.LogInformation("Generated file: '{FilePath}'.", filePath);
        return filePath;
    }

    /// <summary>
    /// Renders a template string by replacing placeholders with the provided values.
    /// Placeholders follow the <c>{{Key}}</c> syntax.
    /// </summary>
    /// <param name="template">The template string containing <c>{{Key}}</c> placeholders.</param>
    /// <param name="values">A dictionary mapping placeholder keys to replacement values.</param>
    /// <returns>The rendered string with placeholders replaced.</returns>
    protected string RenderTemplate(string template, IReadOnlyDictionary<string, string> values)
    {
        if (string.IsNullOrEmpty(template))
            return template ?? string.Empty;

        if (values == null || values.Count == 0)
            return template;

        var result = template;
        foreach (var kvp in values)
        {
            result = result.Replace("{{" + kvp.Key + "}}", kvp.Value);
        }

        return result;
    }

    /// <summary>
    /// Gets the full output path for a generated file, resolving against the project root.
    /// </summary>
    /// <param name="relativeDirectory">The relative directory path.</param>
    /// <param name="fileName">The file name.</param>
    /// <returns>The resolved full path.</returns>
    protected string ResolveOutputPath(string relativeDirectory, string fileName)
    {
        var basePath = string.IsNullOrEmpty(_config.Value.ModelsDirectory)
            ? Directory.GetCurrentDirectory()
            : Path.GetFullPath(_config.Value.ModelsDirectory);

        // If the relativeDirectory is already absolute, use it directly
        if (Path.IsPathRooted(relativeDirectory))
            return Path.Combine(relativeDirectory, fileName);

        return Path.Combine(basePath, relativeDirectory, fileName);
    }
}
#endif
