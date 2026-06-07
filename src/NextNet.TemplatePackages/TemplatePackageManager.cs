namespace NextNet.TemplatePackages;

using System.Security.Cryptography;

/// <summary>
/// High-level orchestration service for downloading, caching, and extracting
/// template packages. Coordinates between <see cref="HttpPackageDownloader"/>,
/// <see cref="TemplatePackageExtractor"/>, and <see cref="TemplatePackageCache"/>.
/// </summary>
public sealed class TemplatePackageManager
{
    private readonly HttpPackageDownloader _downloader;
    private readonly TemplatePackageExtractor _extractor;
    private readonly TemplatePackageCache _cache;
    private readonly PackageCacheOptions _options;

    /// <summary>
    /// Initializes a new instance of <see cref="TemplatePackageManager"/>.
    /// </summary>
    public TemplatePackageManager(
        HttpPackageDownloader downloader,
        TemplatePackageExtractor extractor,
        TemplatePackageCache cache,
        PackageCacheOptions options)
    {
        _downloader = downloader;
        _extractor = extractor;
        _cache = cache;
        _options = options;
    }

    /// <summary>
    /// Downloads, caches, and installs a template package in a single operation.
    /// Checks the local cache first and reuses cached files if the checksum matches.
    /// </summary>
    /// <param name="name">Package name (used for cache key and install directory naming).</param>
    /// <param name="version">Package version string.</param>
    /// <param name="downloadUrl">HTTP(S) URL from which to download the package.</param>
    /// <param name="checksum">
    /// Optional SHA-256 hex digest for integrity verification.
    /// If provided, the cached file is re-verified before reuse.
    /// </param>
    /// <param name="progress">Optional progress reporter for the download phase.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The absolute path to the directory where the package was extracted.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="name"/>, <paramref name="version"/>, or
    /// <paramref name="downloadUrl"/> is null or whitespace.
    /// </exception>
    /// <exception cref="InvalidDataException">
    /// Thrown when checksum verification fails or the archive contains path-traversal entries.
    /// </exception>
    /// <exception cref="HttpRequestException">
    /// Thrown when the HTTP download fails.
    /// </exception>
    public async Task<string> DownloadAndInstallAsync(
        string name,
        string version,
        string downloadUrl,
        string? checksum = null,
        IProgress<DownloadProgress>? progress = null,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(version);
        ArgumentException.ThrowIfNullOrWhiteSpace(downloadUrl);

        // 1. Check the local cache for a valid entry
        if (_cache.Has(name, version))
        {
            using var cachedStream = _cache.Load(name, version);
            var cachedChecksum = await ComputeSha256FromStreamAsync(cachedStream, ct);

            // If no checksum was provided, or the cached file matches, use it
            if (checksum is null ||
                string.Equals(cachedChecksum, checksum, StringComparison.OrdinalIgnoreCase))
            {
                return await ExtractToInstallDir(cachedStream, name, version, checksum, ct);
            }
        }

        // 2. Download fresh copy from the registry
        using var downloadStream = await _downloader.DownloadAsync(downloadUrl, progress, ct);

        // 3. Save to cache (this consumes the stream, so reset position)
        await _cache.SaveAsync(name, version, downloadStream, ct);

        // 4. Load from cache and extract
        using var packageStream = _cache.Load(name, version);
        return await ExtractToInstallDir(packageStream, name, version, checksum, ct);
    }

    private async Task<string> ExtractToInstallDir(
        Stream packageStream,
        string name,
        string version,
        string? checksum,
        CancellationToken ct)
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var installDir = Path.Combine(home, ".nextnet", "templates", $"{name}-{version}");

        await _extractor.ExtractAsync(
            packageStream,
            installDir,
            _options.VerifyChecksums ? checksum : null,
            ct);

        return installDir;
    }

    private static async Task<string> ComputeSha256FromStreamAsync(Stream stream, CancellationToken ct)
    {
        using var sha = SHA256.Create();
        var hash = await sha.ComputeHashAsync(stream, ct);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
