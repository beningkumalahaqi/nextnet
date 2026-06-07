namespace NextNet.TemplatePackages;

/// <summary>
/// Configuration options for the template package cache system.
/// Controls cache directory location, size limits, entry TTL, and checksum verification.
/// </summary>
public sealed class PackageCacheOptions
{
    /// <summary>
    /// The root directory where cached package files are stored.
    /// Defaults to <c>~/.nextnet/cache/packages</c>.
    /// </summary>
    public string CacheDirectory { get; set; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".nextnet", "cache", "packages");

    /// <summary>
    /// Maximum size of the cache in bytes before cleanup is triggered.
    /// Defaults to 1 GB.
    /// </summary>
    public long MaxCacheSizeBytes { get; set; } = 1L * 1024 * 1024 * 1024; // 1GB

    /// <summary>
    /// Time-to-live for cached entries. Entries older than this may be evicted.
    /// Defaults to 30 days.
    /// </summary>
    public TimeSpan EntryTtl { get; set; } = TimeSpan.FromDays(30);

    /// <summary>
    /// Whether to verify SHA-256 checksums when downloading and extracting packages.
    /// Disabling speeds up development but reduces security guarantees.
    /// </summary>
    public bool VerifyChecksums { get; set; } = true;
}
