namespace NextNet.TemplatePackages;

/// <summary>
/// Local on-disk cache for downloaded .nntemplate package files.
/// Provides atomic save semantics (write to .tmp, then rename) and automatic
/// cache cleanup when the total size exceeds the configured maximum.
/// </summary>
public sealed class TemplatePackageCache
{
    private readonly PackageCacheOptions _options;

    /// <summary>
    /// Initializes a new instance of <see cref="TemplatePackageCache"/>.
    /// </summary>
    /// <param name="options">Configuration options controlling cache behavior.</param>
    public TemplatePackageCache(PackageCacheOptions options) => _options = options;

    /// <summary>Gets the root cache directory path.</summary>
    public string CacheDirectory => _options.CacheDirectory;

    /// <summary>
    /// Returns the expected file path for a cached package with the given name and version.
    /// Both name and version are sanitized to prevent path injection.
    /// </summary>
    /// <param name="name">The package name.</param>
    /// <param name="version">The package version.</param>
    /// <returns>Full path to the cached .nntemplate file.</returns>
    public string GetPackagePath(string name, string version)
    {
        var safeName = SanitizeIdentifier(name, allowDots: false);
        var safeVersion = SanitizeIdentifier(version, allowDots: true);
        return Path.Combine(_options.CacheDirectory, $"{safeName}-{safeVersion}.nntemplate");
    }

    /// <summary>
    /// Checks whether a package with the given name and version exists in the cache.
    /// </summary>
    public bool Has(string name, string version)
    {
        var path = GetPackagePath(name, version);
        return File.Exists(path);
    }

    /// <summary>
    /// Saves a package stream atomically to the cache.
    /// Writes to a temporary file first and then renames to the final path.
    /// </summary>
    /// <param name="name">The package name.</param>
    /// <param name="version">The package version.</param>
    /// <param name="content">Stream containing the package data.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task SaveAsync(string name, string version, Stream content, CancellationToken ct = default)
    {
        Directory.CreateDirectory(_options.CacheDirectory);
        var targetPath = GetPackagePath(name, version);
        var tempPath = targetPath + ".tmp";

        await using (var fs = File.Create(tempPath))
        {
            await content.CopyToAsync(fs, ct);
        }

        File.Move(tempPath, targetPath, overwrite: true);
    }

    /// <summary>
    /// Loads a cached package as a readable stream.
    /// </summary>
    /// <param name="name">The package name.</param>
    /// <param name="version">The package version.</param>
    /// <returns>A readable stream of the cached package file.</returns>
    /// <exception cref="FileNotFoundException">The package is not in the cache.</exception>
    public Stream Load(string name, string version)
    {
        var path = GetPackagePath(name, version);
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("Package not in cache", path);
        }

        return File.OpenRead(path);
    }

    /// <summary>
    /// Calculates the total size of all cached packages in bytes.
    /// </summary>
    public async Task<long> GetSizeAsync()
    {
        if (!Directory.Exists(_options.CacheDirectory))
        {
            return 0;
        }

        long total = 0;
        foreach (var file in Directory.EnumerateFiles(_options.CacheDirectory))
        {
            total += new FileInfo(file).Length;
        }

        return await Task.FromResult(total);
    }

    /// <summary>
    /// Removes the least-recently-used cached packages until the total size
    /// is within the configured <see cref="PackageCacheOptions.MaxCacheSizeBytes"/>.
    /// Packages are evicted in order of <c>LastAccessTimeUtc</c>.
    /// </summary>
    /// <returns>The number of files removed.</returns>
    public async Task<int> CleanupAsync()
    {
        if (!Directory.Exists(_options.CacheDirectory))
        {
            return 0;
        }

        var totalSize = await GetSizeAsync();
        if (totalSize <= _options.MaxCacheSizeBytes)
        {
            return 0;
        }

        var files = Directory.EnumerateFiles(_options.CacheDirectory)
            .Select(f => new FileInfo(f))
            .OrderBy(f => f.LastAccessTimeUtc)
            .ToList();

        var removed = 0;
        foreach (var file in files)
        {
            if (totalSize <= _options.MaxCacheSizeBytes)
            {
                break;
            }

            totalSize -= file.Length;
            file.Delete();
            removed++;
        }

        return removed;
    }

    /// <summary>
    /// Sanitizes a string for safe use as part of a file path.
    /// Replaces any character that is not a letter, digit, hyphen, or underscore with '_'.
    /// </summary>
    /// <param name="value">The string to sanitize.</param>
    /// <param name="allowDots">Whether to preserve the '.' character.</param>
    /// <returns>A sanitized identifier safe for use in file names.</returns>
    private static string SanitizeIdentifier(string value, bool allowDots)
    {
        return string.Concat(value.Select(c =>
            char.IsLetterOrDigit(c) || c == '-' || c == '_' || (allowDots && c == '.')
                ? c
                : '_'));
    }
}
