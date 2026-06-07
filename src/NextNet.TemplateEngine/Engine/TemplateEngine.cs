using System.Diagnostics;
using NextNet.IO;
using NextNet.Templates.Abstractions;
using NextNet.Templates.Exceptions;
using NextNet.Templates.Models;
using NextNet.TemplateEngine.Conditionals;
using NextNet.TemplateEngine.Variables;

namespace NextNet.TemplateEngine;

/// <summary>
/// Orchestrates the template generation pipeline: load → parse → validate → filter → replace → write.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="TemplateEngine"/> is the central orchestrator that coordinates all
/// Phase 1 components into a complete template generation workflow. It processes
/// <see cref="TemplatePackage"/> instances through a multi-stage pipeline:
/// </para>
/// <list type="number">
///   <item><description><b>Validate</b> — Validates all required variables are present and correctly typed using <see cref="VariableTypeValidator"/>.</description></item>
///   <item><description><b>Resolve features</b> — Resolves feature dependencies and detects conflicts using <see cref="FeatureResolver"/>.</description></item>
///   <item><description><b>Filter files</b> — Evaluates per-file conditions and excludes files with false conditions using <see cref="ConditionalFileFilter"/>.</description></item>
///   <item><description><b>Generate</b> — Performs variable substitution via <see cref="VariableReplacer"/> and writes output files.</description></item>
/// </list>
/// <para>
/// Binary files are detected via <see cref="BinaryDetector"/> and copied as-is without
/// text processing. The engine also performs path traversal security checks to ensure
/// generated files cannot escape the output directory.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var fileSystem = new DefaultSharpFileSystem();
/// var engine = new TemplateEngine(fileSystem);
///
/// var package = new TemplatePackage(manifest, files);
/// var variables = VariableContext.CreateBuilder()
///     .Set("projectName", "MyApp")
///     .Build();
///
/// var result = await engine.GenerateAsync(package, variables, "./output");
/// Console.WriteLine($"Generated {result.GeneratedFiles.Count} files");
/// </code>
/// </example>
public sealed class TemplateEngine : ITemplateEngine
{
    private readonly ISharpFileSystem _fileSystem;

    /// <summary>
    /// Initializes a new instance of the <see cref="TemplateEngine"/> class.
    /// </summary>
    /// <param name="fileSystem">The file system abstraction used for all I/O operations.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="fileSystem"/> is null.</exception>
    public TemplateEngine(ISharpFileSystem fileSystem)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }

    /// <summary>
    /// Generates output files from the provided template package and variable context,
    /// writing them to the specified output directory.
    /// </summary>
    /// <param name="package">The resolved template package containing manifest and files.</param>
    /// <param name="variables">The variable values to use during generation.</param>
    /// <param name="outputDirectory">The root directory where generated files will be written.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="GenerationResult"/> describing the outcome of generation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="package"/>, <paramref name="variables"/>, or <paramref name="outputDirectory"/> is null.</exception>
    /// <exception cref="TemplateValidationException">Thrown when the manifest or variables fail validation.</exception>
    public async Task<GenerationResult> GenerateAsync(
        TemplatePackage package,
        IVariableContext variables,
        string outputDirectory,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(package);
        ArgumentNullException.ThrowIfNull(variables);
        ArgumentNullException.ThrowIfNull(outputDirectory);

        var options = new GenerationOptions { OutputDirectory = outputDirectory };
        return await GenerateAsync(package, variables, options, null, cancellationToken);
    }

    /// <summary>
    /// Generates output files from the provided template package and variable context
    /// using the specified options, with progress reporting.
    /// </summary>
    /// <param name="package">The resolved template package containing manifest and files.</param>
    /// <param name="variables">The variable values to use during generation.</param>
    /// <param name="options">Configuration options for the generation process.</param>
    /// <param name="progress">An optional progress reporter for tracking generation status.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="GenerationResult"/> describing the outcome of generation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="package"/>, <paramref name="variables"/>, or <paramref name="options"/> is null.</exception>
    /// <exception cref="TemplateValidationException">Thrown when the manifest or variables fail validation.</exception>
    /// <example>
    /// <code>
    /// var progress = new Progress&lt;GenerationProgress&gt;(p =>
    ///     Console.WriteLine($"{p.Stage}: {p.Percentage:F0}%"));
    ///
    /// var options = new GenerationOptions
    /// {
    ///     OutputDirectory = "./output",
    ///     DryRun = true
    /// };
    ///
    /// var result = await engine.GenerateAsync(package, variables, options, progress);
    /// </code>
    /// </example>
    public async Task<GenerationResult> GenerateAsync(
        TemplatePackage package,
        IVariableContext variables,
        GenerationOptions options,
        IProgress<GenerationProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(package);
        ArgumentNullException.ThrowIfNull(variables);
        ArgumentNullException.ThrowIfNull(options);

        // Convert IVariableContext to VariableContext for downstream components
        var variableContext = variables as VariableContext ?? ConvertToVariableContext(variables);

        var stopwatch = Stopwatch.StartNew();
        var generatedFiles = new List<string>();
        var skippedFiles = new List<string>();
        var errors = new List<GenerationError>();
        var warnings = new List<string>();

        try
        {
            // Stage 1: Validate manifest and variables
            progress?.Report(new GenerationProgress { Stage = "Validating", Percentage = 0 });

            var typeValidator = new VariableTypeValidator();
            var manifestVariables = package.Manifest.Variables ?? new List<TemplateVariable>();

            if (manifestVariables.Count > 0)
            {
                var validationResult = typeValidator.ValidateAll(manifestVariables, variableContext);
                if (!validationResult.IsValid)
                {
                    throw new TemplateValidationException(validationResult.Errors ?? new List<string>());
                }
            }

            // Stage 2: Resolve features
            progress?.Report(new GenerationProgress { Stage = "Resolving features", Percentage = 10 });
            var featureResolver = new FeatureResolver();
            var selectedFeatures = ExtractSelectedFeatures(variables);
            if (package.Manifest.Features is { Count: > 0 })
            {
                var featureResult = featureResolver.Resolve(package.Manifest.Features, selectedFeatures);
                if (featureResult.Errors is { Count: > 0 })
                {
                    throw new TemplateValidationException(featureResult.Errors);
                }
            }

            // Stage 3: Filter files by conditions
            progress?.Report(new GenerationProgress { Stage = "Filtering files", Percentage = 20 });
            var fileFilter = new ConditionalFileFilter();
            var filterResult = fileFilter.Filter(package.Manifest.Files ?? new List<TemplateFile>(), variables);

            // Stage 4: Create output directory
            if (!options.DryRun && !_fileSystem.DirectoryExists(options.OutputDirectory))
            {
                _fileSystem.CreateDirectory(options.OutputDirectory);
            }

            // Stage 5: Process each file
            var totalFiles = filterResult.Included.Count;
            var processed = 0;
            var replacer = new VariableReplacer();

            foreach (var file in filterResult.Included)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    // Resolve target path with variable replacement
                    var targetPath = await replacer.ReplaceAsync(file.TargetPath, variableContext, cancellationToken);

                    if (string.IsNullOrWhiteSpace(targetPath))
                    {
                        warnings.Add($"Target path resolved to empty for source: {file.SourcePath}");
                        continue;
                    }

                    var fullPath = ValidateAndResolvePath(options.OutputDirectory, targetPath);

                    // Get file contents
                    if (package.Files is null || !package.Files.TryGetValue(file.SourcePath, out var content))
                    {
                        warnings.Add($"Source file not found in package: {file.SourcePath}");
                        continue;
                    }

                    // Skip if file exists and overwrite is disabled
                    if (!options.Overwrite && !options.DryRun && _fileSystem.FileExists(fullPath))
                    {
                        warnings.Add($"File already exists, skipping: {targetPath}");
                        skippedFiles.Add(fullPath);
                        continue;
                    }

                    // Check if binary
                    var isBinary = file.IsBinary || BinaryDetector.IsBinary(content);

                    if (isBinary)
                    {
                        // Copy as-is
                        if (!options.DryRun)
                        {
                            var dir = _fileSystem.GetDirectoryName(fullPath);
                            if (!string.IsNullOrEmpty(dir) && !_fileSystem.DirectoryExists(dir))
                            {
                                _fileSystem.CreateDirectory(dir);
                            }
                            await _fileSystem.WriteAllBytesAsync(fullPath, content, cancellationToken);
                        }
                    }
                    else
                    {
                        // Replace variables in text content
                        var text = System.Text.Encoding.UTF8.GetString(content);
                        var replaced = await replacer.ReplaceAsync(text, variableContext, cancellationToken);
                        var encoded = System.Text.Encoding.UTF8.GetBytes(replaced);

                        if (!options.DryRun)
                        {
                            var dir = _fileSystem.GetDirectoryName(fullPath);
                            if (!string.IsNullOrEmpty(dir) && !_fileSystem.DirectoryExists(dir))
                            {
                                _fileSystem.CreateDirectory(dir);
                            }
                            await _fileSystem.WriteAllBytesAsync(fullPath, encoded, cancellationToken);
                        }
                    }

                    generatedFiles.Add(fullPath);
                }
                catch (Exception ex) when (ex is not OperationCanceledException and not TemplateValidationException)
                {
                    errors.Add(new GenerationError
                    {
                        File = file.SourcePath,
                        Stage = "Generate",
                        Message = ex.Message
                    });
                }

                processed++;
                var percentage = totalFiles > 0 ? 20 + (double)processed / totalFiles * 80 : 100;
                progress?.Report(new GenerationProgress
                {
                    Stage = "Generating",
                    CurrentFile = file.SourcePath,
                    FilesProcessed = processed,
                    TotalFiles = totalFiles,
                    Percentage = percentage
                });
            }

            // Track skipped files
            foreach (var excluded in filterResult.Excluded)
            {
                skippedFiles.Add(excluded.File.TargetPath);
            }

            stopwatch.Stop();

            return new GenerationResult(
                GeneratedFiles: generatedFiles.AsReadOnly(),
                SkippedFiles: skippedFiles.AsReadOnly(),
                Warnings: warnings.AsReadOnly(),
                Elapsed: stopwatch.Elapsed
            )
            {
                Success = errors.Count == 0,
                Errors = errors.Count > 0
                    ? errors.AsReadOnly()
                    : null
            };
        }
        catch (OperationCanceledException)
        {
            // Cleanup partial output
            if (!options.DryRun && _fileSystem.DirectoryExists(options.OutputDirectory))
            {
                try { _fileSystem.DeleteDirectory(options.OutputDirectory, recursive: true); } catch { /* Best effort cleanup */ }
            }
            throw;
        }
    }

    /// <summary>
    /// Validates that the given manifest and variable context are compatible and complete
    /// without performing actual file generation.
    /// </summary>
    /// <param name="manifest">The template manifest to validate.</param>
    /// <param name="variables">The variable values to validate against the manifest.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="ValidationResult"/> describing any validation issues found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="manifest"/> or <paramref name="variables"/> is null.</exception>
    /// <example>
    /// <code>
    /// var result = await engine.ValidateAsync(manifest, variables);
    /// if (!result.IsValid)
    /// {
    ///     foreach (var error in result.Errors!)
    ///         Console.WriteLine(error);
    /// }
    /// </code>
    /// </example>
    public Task<ValidationResult> ValidateAsync(
        TemplateManifest manifest,
        IVariableContext variables,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        ArgumentNullException.ThrowIfNull(variables);

        var typeValidator = new VariableTypeValidator();
        var manifestVariables = manifest.Variables ?? new List<TemplateVariable>();

        if (manifestVariables.Count == 0)
        {
            return Task.FromResult(new ValidationResult(
                IsValid: true,
                Errors: null,
                Warnings: null
            ));
        }

        var variableContext = variables as VariableContext ?? ConvertToVariableContext(variables);
        return Task.FromResult(typeValidator.ValidateAll(manifestVariables, variableContext));
    }

    /// <summary>
    /// Validates and resolves a target path, ensuring it does not escape the output directory
    /// (path traversal protection).
    /// </summary>
    /// <param name="outputDir">The root output directory.</param>
    /// <param name="targetPath">The relative target path to validate.</param>
    /// <returns>The fully resolved absolute path.</returns>
    /// <exception cref="TemplateGenerationException">Thrown when path traversal is detected.</exception>
    private string ValidateAndResolvePath(string outputDir, string targetPath)
    {
        // Security: prevent path traversal
        var normalized = targetPath.Replace('\\', '/');
        if (normalized.Contains("../") || normalized.StartsWith("/"))
        {
            throw new TemplateGenerationException(
                $"Invalid target path (path traversal detected): {targetPath}", targetPath);
        }

        var fullPath = _fileSystem.GetFullPath(_fileSystem.Combine(outputDir, normalized));
        var normalizedOutput = _fileSystem.GetFullPath(outputDir);
        if (!fullPath.StartsWith(normalizedOutput, StringComparison.Ordinal))
        {
            throw new TemplateGenerationException(
                $"Resolved path escapes output directory: {targetPath}", targetPath);
        }

        return fullPath;
    }

    /// <summary>
    /// Extracts the set of selected feature names from the variable context.
    /// Features are stored as boolean variables with <c>true</c> values.
    /// </summary>
    /// <param name="variables">The variable context to extract features from.</param>
    /// <returns>A set of feature names that are enabled in the context.</returns>
    private static HashSet<string> ExtractSelectedFeatures(IVariableContext variables)
    {
        var features = new HashSet<string>(StringComparer.Ordinal);
        foreach (var key in variables.Keys)
        {
            if (variables.Get(key) is true)
            {
                features.Add(key);
            }
        }
        return features;
    }

    /// <summary>
    /// Converts an <see cref="IVariableContext"/> to a <see cref="VariableContext"/>
    /// by copying all key-value pairs into a new builder.
    /// </summary>
    /// <param name="variables">The variable context to convert.</param>
    /// <returns>A new <see cref="VariableContext"/> containing all values from the source.</returns>
    private static VariableContext ConvertToVariableContext(IVariableContext variables)
    {
        var builder = VariableContext.CreateBuilder();
        foreach (var key in variables.Keys)
        {
            builder.Set(key, variables.Get(key));
        }
        return builder.Build();
    }
}
