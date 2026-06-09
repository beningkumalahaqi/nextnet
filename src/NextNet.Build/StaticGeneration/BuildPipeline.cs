using NextNet.Conventions;
using NextNet.IO;
using NextNet.Logging;
using NextNet.Rendering;
using NextNet.Routing;
using NextNet.Build.Errors;

namespace NextNet.Build.StaticGeneration;

/// <summary>
/// High-level build pipeline that orchestrates the full static site generation process:
/// route discovery, parameter resolution, SSR rendering, minification, compression,
/// asset copying, and manifest generation.
/// </summary>
/// <example>
/// <code>
/// var pipeline = new BuildPipeline(options, serviceProvider);
/// var result = await pipeline.ExecuteAsync(cancellationToken);
/// if (result.Success) { Console.WriteLine("Build succeeded!"); }
/// </code>
/// </example>
public sealed class BuildPipeline
{
    private readonly SsgOptions _options;
    private readonly IServiceProvider _serviceProvider;
    private readonly ISharpFileSystem _fileSystem;
    private readonly INextNetLogger? _logger;
    private readonly string _appDir;
    private readonly string _publicDir;

    /// <summary>
    /// Initializes a new instance of <see cref="BuildPipeline"/>.
    /// </summary>
    /// <param name="options">SSG configuration options.</param>
    /// <param name="serviceProvider">The DI service provider for resolving components.</param>
    /// <param name="fileSystem">Optional file system abstraction.</param>
    /// <param name="logger">Optional logger.</param>
    /// <param name="appDir">Optional application directory path.</param>
    /// <param name="publicDir">Optional public assets directory path.</param>
    public BuildPipeline(
        SsgOptions options,
        IServiceProvider serviceProvider,
        ISharpFileSystem? fileSystem = null,
        INextNetLogger? logger = null,
        string? appDir = null,
        string? publicDir = null)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _fileSystem = fileSystem ?? new DefaultSharpFileSystem();
        _logger = logger;
        _appDir = appDir ?? NextNetConventions.AppDirectory;
        _publicDir = publicDir ?? NextNetConventions.PublicDirectory;
    }

    /// <summary>
    /// Runs the full SSG build pipeline.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="SsgResult"/> describing the outcome.</returns>
    public async Task<SsgResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        _logger?.Info("Build pipeline starting...");

        // 1. Route discovery
        _logger?.Info("Scanning routes in {AppDir}...", _appDir);
        RouteManifest routeManifest;
        try
        {
            var scanner = new RouteScanner(_appDir, _logger, _fileSystem);
            routeManifest = await scanner.ScanAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger?.Error("[{Code}] Route discovery failed for {AppDir}: {Message}", BuildErrorCodes.RouteDiscoveryFailed, _appDir, ex.Message);
            return new SsgResult(
                Array.Empty<string>(),
                new[] { new SsgError("(discovery)", $"[{BuildErrorCodes.RouteDiscoveryFailed}] Route discovery failed: {ex.Message}", ex) },
                TimeSpan.Zero,
                0,
                0,
                0);
        }

        if (routeManifest.HasConflicts)
        {
            foreach (var conflict in routeManifest.Conflicts)
            {
                _logger?.Warn("Route conflict: {Message}", conflict);
            }
        }

        // 2. Resolve the output directory to an absolute path
        var outputDir = _fileSystem.GetFullPath(_options.OutputDirectory);

        // 3. Build engine dependencies
        var outputWriter = new OutputWriter(outputDir, _fileSystem);
        var publicDir = _fileSystem.GetFullPath(_publicDir);
        var assetCopier = new PublicAssetCopier(publicDir, outputDir, _fileSystem);

        // 4. Create the SsrRenderer (resolves components from DI)
        var ssrRenderer = new SsrRenderer(
            _serviceProvider,
            routeManifest,
            logger: _logger);

        // 5. Create StaticParamsResolver
        var paramsResolver = new StaticParamsResolver(
            ssrRenderer.ComponentResolver,
            _serviceProvider,
            _fileSystem);

        // 6. Create BuildManifestGenerator
        var manifestPath = _fileSystem.Combine(outputDir, "_buildManifest.json");
        var manifestGenerator = new BuildManifestGenerator(manifestPath);

        // 7. Create and run the generation engine
        SsgResult result;
        try
        {
            var engine = new StaticGenerationEngine(
                _options,
                ssrRenderer,
                routeManifest,
                paramsResolver,
                outputWriter,
                assetCopier,
                manifestGenerator,
                _logger,
                _appDir,
                _publicDir);

            result = await engine.GenerateAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger?.Error("[{Code}] Build pipeline execution failed: {Message}", BuildErrorCodes.BuildPipelineFailed, ex.Message);
            return new SsgResult(
                Array.Empty<string>(),
                new[] { new SsgError("(pipeline)", $"[{BuildErrorCodes.BuildPipelineFailed}] Build pipeline failed: {ex.Message}", ex) },
                TimeSpan.Zero,
                0,
                0,
                0);
        }

        _logger?.Info("Build pipeline complete: {Result}", result.Success ? "Success" : "Failed");

        return result;
    }
}
