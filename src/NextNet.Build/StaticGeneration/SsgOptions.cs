namespace NextNet.Build.StaticGeneration;

/// <summary>
/// Configuration options for the Static Site Generation (SSG) engine.
/// Controls output directory, optimisation features, and render behaviour.
/// </summary>
public class SsgOptions
{
    /// <summary>
    /// Gets or sets the output directory for generated static files.
    /// Relative paths are resolved against the project root.
    /// Defaults to <c>"dist"</c>.
    /// </summary>
    public string OutputDirectory { get; set; } = "dist";

    /// <summary>
    /// Gets or sets whether generated HTML should be minified.
    /// Defaults to <c>true</c>.
    /// </summary>
    public bool MinifyHtml { get; set; } = true;

    /// <summary>
    /// Gets or sets whether gzip-compressed (<c>.gz</c>) copies of HTML files
    /// should be generated alongside the originals for pre-compressed serving.
    /// Defaults to <c>true</c>.
    /// </summary>
    public bool CompressGzip { get; set; } = true;

    /// <summary>
    /// Gets or sets whether a build manifest JSON file should be generated
    /// in the output directory.
    /// Defaults to <c>true</c>.
    /// </summary>
    public bool GenerateBuildManifest { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum degree of parallelism for rendering pages.
    /// Defaults to the number of available logical processors.
    /// </summary>
    public int MaxParallelism { get; set; } = Environment.ProcessorCount;

    /// <summary>
    /// Gets or sets the list of route patterns to exclude from static generation.
    /// These routes will fall back to SSR.
    /// </summary>
    public IReadOnlyList<string> ExcludePaths { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the maximum time allowed for rendering a single page.
    /// If a page takes longer than this timeout, it is treated as an error.
    /// Defaults to 30 seconds.
    /// </summary>
    public TimeSpan RenderTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets whether the output directory should be cleaned before
    /// generating new output. Defaults to <c>true</c>.
    /// </summary>
    public bool CleanOutput { get; set; } = true;

    /// <summary>
    /// Gets or sets whether public/static assets should be copied to the
    /// output directory. Defaults to <c>true</c>.
    /// </summary>
    public bool CopyAssets { get; set; } = true;
}
