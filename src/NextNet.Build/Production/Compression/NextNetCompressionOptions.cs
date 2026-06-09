using NextNet.Build.Production.Compression.AlgorithmAdapters;

namespace NextNet.Build.Production.Compression;

/// <summary>
/// Options for configuring response compression in the NextNet production pipeline.
/// </summary>
public sealed class NextNetCompressionOptions
{
    /// <summary>
    /// Whether to enable response compression middleware.
    /// </summary>
    public bool EnableCompression { get; set; } = true;

    /// <summary>
    /// Whether to pre-compress static assets during build.
    /// </summary>
    public bool PreCompressAssets { get; set; } = true;

    /// <summary>
    /// The compression level to use (0-9, where 9 is maximum compression).
    /// </summary>
    public int CompressionLevel { get; set; } = 5;

    /// <summary>
    /// Minimum response size in bytes before compression is applied.
    /// </summary>
    public int MinimumResponseSize { get; set; } = 256;

    /// <summary>
    /// MIME types to compress. If empty, default types are used.
    /// </summary>
    public HashSet<string> MimeTypes { get; set; } = new(StringComparer.OrdinalIgnoreCase)
    {
        "text/html", "text/css", "text/javascript", "application/javascript",
        "application/json", "application/xml", "text/plain", "image/svg+xml",
        "text/markdown", "application/manifest+json",
    };

    /// <summary>
    /// The preferred compression algorithm (used in Content-Encoding negotiation).
    /// </summary>
    public CompressionAlgorithm PreferredAlgorithm { get; set; } = CompressionAlgorithm.Brotli;

    /// <summary>
    /// Whether to exclude paths from compression.
    /// </summary>
    public HashSet<string> ExcludedPaths { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

/// <summary>
/// Supported compression algorithms.
/// </summary>
public enum CompressionAlgorithm
{
    /// <summary>
    /// Brotli compression (preferred for web content).
    /// </summary>
    Brotli,

    /// <summary>
    /// GZip compression (widely supported).
    /// </summary>
    Gzip,

    /// <summary>
    /// Zstandard compression (high compression ratio).
    /// </summary>
    Zstd,

    /// <summary>
    /// Deflate compression.
    /// </summary>
    Deflate,
}
