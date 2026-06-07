using System.Text;
using System.Text.Json;
using NextNet.Routing;

namespace NextNet.Build.StaticGeneration;

/// <summary>
/// Generates a build manifest JSON file that describes all generated routes,
/// assets, and build metadata.
/// </summary>
public class BuildManifestGenerator
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    private readonly Version _nextnetVersion;
    private readonly string _outputPath;

    /// <summary>
    /// Initializes a new instance of <see cref="BuildManifestGenerator"/>.
    /// </summary>
    /// <param name="outputPath">Absolute path to write the manifest to (e.g. <c>dist/_buildManifest.json</c>).</param>
    /// <param name="nextnetVersion">The NextNet version to record in the manifest. Defaults to <c>1.0.0</c>.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="outputPath"/> is null.</exception>
    public BuildManifestGenerator(string outputPath, Version? nextnetVersion = null)
    {
        _outputPath = outputPath ?? throw new ArgumentNullException(nameof(outputPath));
        _nextnetVersion = nextnetVersion ?? new Version(1, 0, 0);
    }

    /// <summary>
    /// Represents a single route entry in the build manifest.
    /// </summary>
    public class ManifestRouteEntry
    {
        /// <summary>The route pattern (e.g. <c>"/"</c>, <c>"/blog/{slug}"</c>).</summary>
        public string Route { get; set; } = string.Empty;

        /// <summary>The relative file path in the output directory (e.g. <c>"index.html"</c>).</summary>
        public string File { get; set; } = string.Empty;

        /// <summary>Whether this route is fully static (no dynamic segments).</summary>
        public bool Static { get; set; }

        /// <summary>The parameter names for dynamic routes (e.g. <c>["slug"]</c>).</summary>
        public List<string>? Params { get; set; }

        /// <summary>The file size in bytes.</summary>
        public long Size { get; set; }
    }

    /// <summary>
    /// Generates the build manifest and writes it to the output path.
    /// </summary>
    /// <param name="manifest">The route manifest from scanning.</param>
    /// <param name="generatedRoutes">List of (routePattern, relativeFilePath, isStatic, params, size) for generated pages.</param>
    /// <param name="assets">List of relative asset paths that were copied.</param>
    /// <param name="duration">The total build duration.</param>
    /// <param name="pageCount">Total number of pages (static + SSR).</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task GenerateAsync(
        RouteManifest manifest,
        IReadOnlyList<(string route, string file, bool isStatic, IReadOnlyList<string>? paramNames, long size)> generatedRoutes,
        IReadOnlyList<string> assets,
        TimeSpan duration,
        int pageCount)
    {
        if (manifest == null) throw new ArgumentNullException(nameof(manifest));
        if (generatedRoutes == null) throw new ArgumentNullException(nameof(generatedRoutes));
        if (assets == null) throw new ArgumentNullException(nameof(assets));

        var manifestObject = new Dictionary<string, object>
        {
            ["generatedAt"] = DateTime.UtcNow.ToString("o"),
            ["nextnetVersion"] = _nextnetVersion.ToString(),
            ["routes"] = generatedRoutes.Select(r => new ManifestRouteEntry
            {
                Route = r.route,
                File = r.file,
                Static = r.isStatic,
                Params = r.paramNames?.ToList(),
                Size = r.size,
            }).ToList(),
            ["assets"] = assets.ToList(),
            ["build"] = new Dictionary<string, object>
            {
                ["duration"] = duration.TotalMilliseconds,
                ["totalPages"] = pageCount,
                ["totalSize"] = generatedRoutes.Sum(r => r.size),
                ["ssrPages"] = manifest.Pages.Count - generatedRoutes.Count,
            },
        };

        var json = JsonSerializer.Serialize(manifestObject, JsonOptions);

        var directory = Path.GetDirectoryName(_outputPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllTextAsync(_outputPath, json, Encoding.UTF8);
    }
}
