using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Http;
using NextNet.Components;
using NextNet.Conventions;
using NextNet.IO;
using NextNet.Logging;
using NextNet.Build.Optimization;
using NextNet.Rendering;
using NextNet.Routing;
using NextNet.Routing.Models;

namespace NextNet.Build.StaticGeneration;

/// <summary>
/// Main orchestrator for static site generation (SSG) in NextNet.
/// Discovers routes, resolves dynamic params, renders pages via SSR engine,
/// optionally minifies and compresses HTML, writes output, copies assets,
/// and generates a build manifest.
/// </summary>
public class StaticGenerationEngine
{
    private readonly SsgOptions _options;
    private readonly SsrRenderer _ssrRenderer;
    private readonly RouteManifest _routeManifest;
    private readonly StaticParamsResolver _paramsResolver;
    private readonly OutputWriter _outputWriter;
    private readonly PublicAssetCopier _assetCopier;
    private readonly BuildManifestGenerator _manifestGenerator;
    private readonly INextNetLogger? _logger;
    private readonly string _publicDir;
    private readonly string _appDir;

    /// <summary>
    /// Initializes a new instance of <see cref="StaticGenerationEngine"/>.
    /// </summary>
    /// <param name="options">SSG configuration options.</param>
    /// <param name="ssrRenderer">The SSR renderer for pre-rendering pages.</param>
    /// <param name="routeManifest">The route manifest from scanning.</param>
    /// <param name="paramsResolver">Resolver for dynamic route parameters.</param>
    /// <param name="outputWriter">Writer for output files.</param>
    /// <param name="assetCopier">Copier for public/static assets.</param>
    /// <param name="manifestGenerator">Generator for build manifest.</param>
    /// <param name="logger">Optional logger.</param>
    /// <param name="appDir">Optional application directory path. Defaults to <c>"app"</c>.</param>
    /// <param name="publicDir">Optional public assets directory path. Defaults to <c>"public"</c>.</param>
    /// <exception cref="ArgumentNullException">Thrown when required parameters are null.</exception>
    public StaticGenerationEngine(
        SsgOptions options,
        SsrRenderer ssrRenderer,
        RouteManifest routeManifest,
        StaticParamsResolver paramsResolver,
        OutputWriter outputWriter,
        PublicAssetCopier assetCopier,
        BuildManifestGenerator manifestGenerator,
        INextNetLogger? logger = null,
        string? appDir = null,
        string? publicDir = null)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _ssrRenderer = ssrRenderer ?? throw new ArgumentNullException(nameof(ssrRenderer));
        _routeManifest = routeManifest ?? throw new ArgumentNullException(nameof(routeManifest));
        _paramsResolver = paramsResolver ?? throw new ArgumentNullException(nameof(paramsResolver));
        _outputWriter = outputWriter ?? throw new ArgumentNullException(nameof(outputWriter));
        _assetCopier = assetCopier ?? throw new ArgumentNullException(nameof(assetCopier));
        _manifestGenerator = manifestGenerator ?? throw new ArgumentNullException(nameof(manifestGenerator));
        _logger = logger;
        _appDir = appDir ?? NextNetConventions.AppDirectory;
        _publicDir = publicDir ?? NextNetConventions.PublicDirectory;
    }

    /// <summary>
    /// Runs the full static site generation pipeline.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="SsgResult"/> describing the outcome.</returns>
    public async Task<SsgResult> GenerateAsync(CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        var generatedFiles = new List<string>();
        var errors = new ConcurrentBag<SsgError>();
        var generatedRoutes = new ConcurrentBag<(string route, string file, bool isStatic, IReadOnlyList<string>? paramNames, long size)>();

        _logger?.Info("Starting static site generation...");

        // 1. Clean output directory if configured
        if (_options.CleanOutput)
        {
            _logger?.Debug("Cleaning output directory: {OutputDir}", _options.OutputDirectory);
            _outputWriter.CleanOutputDirectory();
        }

        // 2. Classify routes into static (pre-render) and SSR (skip)
        var staticRoutes = new List<(RouteEntry entry, IReadOnlyList<Dictionary<string, string>>? paramSets)>();
        var routeExcludeSet = new HashSet<string>(_options.ExcludePaths, StringComparer.OrdinalIgnoreCase);

        foreach (var page in _routeManifest.Pages)
        {
            if (routeExcludeSet.Contains(page.RoutePattern))
            {
                _logger?.Debug("Excluding route from SSG: {Route}", page.RoutePattern);
                continue;
            }

            if (page.SegmentKind == RouteSegmentKind.Static)
            {
                // Fully static route (no dynamic segments)
                staticRoutes.Add((page, null));
            }
            else
            {
                // Dynamic route — try to resolve static params
                var paramSets = await _paramsResolver.ResolveAsync(page);
                if (paramSets != null && paramSets.Count > 0)
                {
                    staticRoutes.Add((page, paramSets));
                }
                else
                {
                    _logger?.Debug("Skipping dynamic route (no static params): {Route}", page.RoutePattern);
                }
            }
        }

        _logger?.Info("Found {Count} static routes to pre-render", staticRoutes.Count);

        // 3. Render pages in parallel
        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = _options.MaxParallelism,
            CancellationToken = cancellationToken,
        };

        await Parallel.ForEachAsync(staticRoutes, parallelOptions, async (route, ct) =>
        {
            await RenderAndWriteRouteAsync(route.entry, route.paramSets, generatedFiles, generatedRoutes, errors, ct);
        });

        // 4. Copy public/ assets
        IReadOnlyList<string> assets = Array.Empty<string>();
        if (_options.CopyAssets)
        {
            _logger?.Info("Copying public assets...");
            assets = await _assetCopier.CopyAsync();
            _logger?.Debug("Copied {Count} assets", assets.Count);
        }

        // 5. Generate build manifest
        if (_options.GenerateBuildManifest)
        {
            _logger?.Info("Generating build manifest...");
            await _manifestGenerator.GenerateAsync(
                _routeManifest,
                generatedRoutes.ToList(),
                assets,
                sw.Elapsed,
                _routeManifest.Pages.Count);
        }

        sw.Stop();

        var result = new SsgResult(
            generatedFiles,
            errors.ToList(),
            sw.Elapsed,
            _outputWriter.TotalBytesWritten,
            staticRoutes.Count,
            assets.Count);

        if (result.Success)
        {
            _logger?.Info("Static generation complete: {Pages} pages, {Assets} assets, {Duration}",
                result.PageCount, result.AssetCount, result.Duration);
        }
        else
        {
            _logger?.Warn("Static generation completed with {ErrorCount} errors", errors.Count);
        }

        return result;
    }

    /// <summary>
    /// Generates static files for a single route. Used for incremental builds.
    /// </summary>
    /// <param name="route">The route entry to generate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="SsgResult"/> for this single route.</returns>
    public async Task<SsgResult> GenerateForRouteAsync(RouteEntry route, CancellationToken cancellationToken = default)
    {
        if (route == null) throw new ArgumentNullException(nameof(route));

        var sw = Stopwatch.StartNew();
        var generatedFiles = new List<string>();
        var errors = new ConcurrentBag<SsgError>();
        var generatedRoutes = new ConcurrentBag<(string route, string file, bool isStatic, IReadOnlyList<string>? paramNames, long size)>();

        IReadOnlyList<Dictionary<string, string>>? paramSets = null;
        if (route.SegmentKind != RouteSegmentKind.Static)
        {
            paramSets = await _paramsResolver.ResolveAsync(route);
        }

        await RenderAndWriteRouteAsync(route, paramSets, generatedFiles, generatedRoutes, errors, cancellationToken);

        sw.Stop();

        return new SsgResult(
            generatedFiles,
            errors.ToList(),
            sw.Elapsed,
            _outputWriter.TotalBytesWritten,
            paramSets?.Count ?? 1,
            0);
    }

    /// <summary>
    /// Renders and writes a route (with optional param sets) to the output directory.
    /// </summary>
    private async Task RenderAndWriteRouteAsync(
        RouteEntry entry,
        IReadOnlyList<Dictionary<string, string>>? paramSets,
        List<string> generatedFiles,
        ConcurrentBag<(string route, string file, bool isStatic, IReadOnlyList<string>? paramNames, long size)> generatedRoutes,
        ConcurrentBag<SsgError> errors,
        CancellationToken cancellationToken)
    {
        try
        {
            if (paramSets != null && paramSets.Count > 0)
            {
                // Dynamic route with multiple param sets
                foreach (var paramSet in paramSets)
                {
                    var resolvedRoute = ResolveRoutePattern(entry.RoutePattern, paramSet);
                    await RenderSinglePageAsync(entry, resolvedRoute, paramSet, generatedFiles, generatedRoutes, errors, cancellationToken);
                }
            }
            else
            {
                // Static route (no params)
                await RenderSinglePageAsync(entry, entry.RoutePattern, null, generatedFiles, generatedRoutes, errors, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            var error = new SsgError(entry.RoutePattern, $"Unexpected error: {ex.Message}", ex);
            errors.Add(error);
            _logger?.Error("Failed to render route {Route}: {Message}", entry.RoutePattern, ex.Message);
        }
    }

    /// <summary>
    /// Renders a single page for a given (potentially resolved) route and writes it to output.
    /// </summary>
    private async Task RenderSinglePageAsync(
        RouteEntry entry,
        string resolvedRoute,
        Dictionary<string, string>? paramSet,
        List<string> generatedFiles,
        ConcurrentBag<(string route, string file, bool isStatic, IReadOnlyList<string>? paramNames, long size)> generatedRoutes,
        ConcurrentBag<SsgError> errors,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger?.Debug("Rendering: {Route}", resolvedRoute);

            // Create an HttpContext with route values (important for dynamic routes)
            var httpContext = new DefaultHttpContext();
            if (paramSet != null)
            {
                foreach (var kvp in paramSet)
                {
                    httpContext.Request.RouteValues[kvp.Key] = kvp.Value;
                }
            }

            var componentContext = new ComponentContext(httpContext);

            // Render the page via SSR engine (includes layout chain composition)
            var htmlResponse = await _ssrRenderer.RenderAsync(resolvedRoute, componentContext, cancellationToken);

            if (htmlResponse.StatusCode != 200)
            {
                errors.Add(new SsgError(resolvedRoute, $"Render returned status {htmlResponse.StatusCode}"));
                return;
            }

            var html = htmlResponse.Content.ToHtml();

            // Minify HTML if configured
            if (_options.MinifyHtml)
            {
                html = HtmlMinifier.Minify(html);
            }

            // Write to output directory
            var relativePath = OutputWriter.RouteToFilePath(resolvedRoute);
            var writtenPath = await _outputWriter.WriteAsync(resolvedRoute, html);

            var fileSize = System.Text.Encoding.UTF8.GetByteCount(html);
            generatedFiles.Add(writtenPath);

            var isStatic = entry.SegmentKind == RouteSegmentKind.Static;
            var paramNames = isStatic ? null : ExtractParamNames(entry.RoutePattern);

            generatedRoutes.Add((entry.RoutePattern, writtenPath, isStatic, paramNames, fileSize));

            // Gzip compress if configured
            if (_options.CompressGzip)
            {
                var gzipPath = writtenPath + ".gz";
                var compressed = GzipCompressor.Compress(html);
                await _outputWriter.WriteBytesAsync(gzipPath, compressed);
                generatedFiles.Add(gzipPath);
            }

            _logger?.Debug("  → {Path} ({Size} bytes)", writtenPath, fileSize);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            errors.Add(new SsgError(resolvedRoute, $"Render failed: {ex.Message}", ex));
            _logger?.Error("  ✗ {Route}: {Message}", resolvedRoute, ex.Message);
        }
    }

    /// <summary>
    /// Resolves a route pattern like <c>/blog/{slug}</c> with the given param values
    /// to produce a concrete route like <c>/blog/hello-world</c>.
    /// </summary>
    internal static string ResolveRoutePattern(string pattern, Dictionary<string, string> paramSet)
    {
        if (pattern == null) throw new ArgumentNullException(nameof(pattern));
        if (paramSet == null) throw new ArgumentNullException(nameof(paramSet));

        var segments = pattern.Trim('/').Split('/');
        for (int i = 0; i < segments.Length; i++)
        {
            var seg = segments[i];
            if (seg.StartsWith('{') && seg.EndsWith('}'))
            {
                var paramName = seg.Trim('{', '}');
                if (paramSet.TryGetValue(paramName, out var value))
                {
                    segments[i] = value;
                }
            }
        }

        return "/" + string.Join("/", segments);
    }

    /// <summary>
    /// Extracts parameter names from a route pattern like <c>/blog/{slug}</c>
    /// returning <c>["slug"]</c>.
    /// </summary>
    internal static IReadOnlyList<string> ExtractParamNames(string pattern)
    {
        if (string.IsNullOrEmpty(pattern))
            return Array.Empty<string>();

        var names = new List<string>();
        var segments = pattern.Trim('/').Split('/');
        foreach (var seg in segments)
        {
            if (seg.StartsWith('{') && seg.EndsWith('}'))
            {
                names.Add(seg.Trim('{', '}'));
            }
        }

        return names;
    }
}
