namespace NextNet.Build.Errors;

/// <summary>
/// Defines standardized error codes for the NextNet.Build package.
/// Each code corresponds to a specific failure mode within the build pipeline.
/// Use these codes as prefixes (e.g. "[DS-200]") in error messages to enable
/// structured diagnostics and monitoring.
/// </summary>
/// <remarks>
/// <para>Error code range: DS-200 to DS-219.</para>
/// <para>
/// Usage example:
/// <code>
/// throw new InvalidOperationException(
///     $"{BuildErrorCodes.BuildPipelineFailed}: Pipeline execution failed due to {reason}.");
/// </code>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Prefixing error messages with DS codes:
/// _logger?.Error("[{Code}] Build failed: {Message}", BuildErrorCodes.BuildPipelineFailed, ex.Message);
/// var error = new SsgError(route, $"[{BuildErrorCodes.PageRenderFailed}] Render error: {message}");
/// </code>
/// </example>
public static class BuildErrorCodes
{
    /// <summary>
    /// The build pipeline as a whole failed to complete.
    /// </summary>
    public const string BuildPipelineFailed = "DS-200";

    /// <summary>
    /// Route discovery (scanning the app directory) failed.
    /// </summary>
    public const string RouteDiscoveryFailed = "DS-201";

    /// <summary>
    /// Rendering a page via the SSR engine returned an error.
    /// </summary>
    public const string PageRenderFailed = "DS-202";

    /// <summary>
    /// Writing a generated file to the output directory failed.
    /// </summary>
    public const string OutputWriteFailed = "DS-203";

    /// <summary>
    /// Copying public/static assets to the output directory failed.
    /// </summary>
    public const string AssetCopyFailed = "DS-204";

    /// <summary>
    /// Generating the build manifest JSON file failed.
    /// </summary>
    public const string ManifestGenerationFailed = "DS-205";

    /// <summary>
    /// Resolving static path parameters for dynamic routes failed.
    /// </summary>
    public const string StaticParamsResolutionFailed = "DS-206";

    /// <summary>
    /// HTML minification of a generated page failed.
    /// </summary>
    public const string HtmlMinificationFailed = "DS-207";

    /// <summary>
    /// Gzip compression of a generated file failed.
    /// </summary>
    public const string GzipCompressionFailed = "DS-208";

    /// <summary>
    /// Asset optimization (CSS/JS/SVG minification) failed.
    /// </summary>
    public const string AssetOptimizationFailed = "DS-209";

    /// <summary>
    /// Critical CSS extraction failed.
    /// </summary>
    public const string CriticalCssExtractionFailed = "DS-210";

    /// <summary>
    /// A configured performance budget was exceeded.
    /// </summary>
    public const string PerformanceBudgetExceeded = "DS-211";

    /// <summary>
    /// Bundle size analysis failed.
    /// </summary>
    public const string BundleAnalysisFailed = "DS-212";

    /// <summary>
    /// Dockerfile generation failed.
    /// </summary>
    public const string DockerfileGenerationFailed = "DS-213";

    /// <summary>
    /// Program.cs code generation failed.
    /// </summary>
    public const string ProgramCsGenerationFailed = "DS-214";

    /// <summary>
    /// MSBuild publish profile generation failed.
    /// </summary>
    public const string PublishProfileFailed = "DS-215";

    /// <summary>
    /// The health check endpoint returned a non-healthy status.
    /// </summary>
    public const string HealthCheckFailed = "DS-216";

    /// <summary>
    /// The security headers configuration is invalid.
    /// </summary>
    public const string SecurityHeadersConfigInvalid = "DS-217";

    /// <summary>
    /// The compression configuration is invalid.
    /// </summary>
    public const string CompressionConfigInvalid = "DS-218";

    /// <summary>
    /// The cache configuration is invalid.
    /// </summary>
    public const string CacheConfigInvalid = "DS-219";
}
