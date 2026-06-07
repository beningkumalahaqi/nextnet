namespace NextNet.Build.Production.Caching;

/// <summary>
/// Options for configuring cache headers on static assets and responses.
/// </summary>
public class CacheHeaderOptions
{
    /// <summary>
    /// Whether to add cache headers to responses.
    /// </summary>
    public bool EnableCaching { get; set; } = true;

    /// <summary>
    /// Max-age for static assets with content hashes (immutable).
    /// </summary>
    public TimeSpan ImmutableMaxAge { get; set; } = TimeSpan.FromDays(365);

    /// <summary>
    /// Max-age for HTML pages and dynamic content.
    /// </summary>
    public TimeSpan DefaultMaxAge { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Whether to flag static assets as immutable.
    /// </summary>
    public bool SetImmutable { get; set; } = true;

    /// <summary>
    /// Whether to include ETag headers for cache validation.
    /// </summary>
    public bool EnableETag { get; set; } = true;

    /// <summary>
    /// Whether to include Last-Modified headers.
    /// </summary>
    public bool EnableLastModified { get; set; } = true;

    /// <summary>
    /// Paths that should be treated as immutable (containing content hashes).
    /// These get the long max-age + immutable directive.
    /// </summary>
    public HashSet<string> ImmutableExtensions { get; set; } = new(StringComparer.OrdinalIgnoreCase)
    {
        ".css", ".js", ".mjs", ".png", ".jpg", ".jpeg", ".gif",
        ".webp", ".svg", ".ico", ".woff", ".woff2", ".ttf", ".eot",
    };

    /// <summary>
    /// Paths that should not have cache headers added.
    /// </summary>
    public HashSet<string> ExcludedPaths { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
