using System.Text.Json;

namespace NextNet.TemplateRegistry;

/// <summary>
/// A file-system cache for template registry responses with TTL-based expiration.
/// </summary>
/// <remarks>
/// Cached entries are stored as JSON files in a configurable directory. Entries
/// older than the configured TTL are treated as cache misses on read.
/// Write operations use atomic rename to prevent partial writes.
/// This class is consumed internally by <see cref="TemplateRegistry"/> and is
/// public only to support DI registration.
/// </remarks>
public sealed class TemplateRegistryCache
{
    private readonly string _cacheDir;
    private readonly TimeSpan _ttl;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="TemplateRegistryCache"/> class.
    /// </summary>
    /// <param name="options">The registry options containing cache configuration.</param>
    public TemplateRegistryCache(RegistryOptions options)
    {
        var dir = options.CacheDirectory;
        if (dir.Contains("~"))
        {
            dir = dir.Replace("~", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
        }
        _cacheDir = dir;
        _ttl = options.CacheTtl;
        Directory.CreateDirectory(_cacheDir);
    }

    /// <summary>
    /// Retrieves a cached value by key, or <c>null</c> if not found or expired.
    /// </summary>
    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default) where T : class
    {
        var path = GetPath(key);
        if (!File.Exists(path)) return null;

        var info = new FileInfo(path);
        if (DateTime.UtcNow - info.LastWriteTimeUtc > _ttl) return null;

        try
        {
            await using var stream = File.OpenRead(path);
            return await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions, ct);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Stores a value in the cache under the given key using atomic write.
    /// </summary>
    public async Task SetAsync<T>(string key, T value, CancellationToken ct = default)
    {
        var path = GetPath(key);
        var tempPath = path + ".tmp";
        await using (var stream = File.Create(tempPath))
        {
            await JsonSerializer.SerializeAsync(stream, value, JsonOptions, ct);
        }
        File.Move(tempPath, path, overwrite: true);
    }

    /// <summary>
    /// Removes a single entry from the cache.
    /// </summary>
    public void Invalidate(string key)
    {
        var path = GetPath(key);
        if (File.Exists(path)) File.Delete(path);
    }

    /// <summary>
    /// Clears all cached entries.
    /// </summary>
    public void Clear()
    {
        if (Directory.Exists(_cacheDir))
            Directory.Delete(_cacheDir, recursive: true);
        Directory.CreateDirectory(_cacheDir);
    }

    private string GetPath(string key)
    {
        var safeKey = string.Concat(key.Select(c => char.IsLetterOrDigit(c) || c == '-' || c == '_' ? c : '_'));
        return Path.Combine(_cacheDir, $"{safeKey}.json");
    }
}
