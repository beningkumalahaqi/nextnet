namespace NextNet.TemplateMarketplace;

/// <summary>
/// Local file-system cache for marketplace API responses.
/// Cached entries expire based on <see cref="MarketplaceOptions.CacheTtl"/>.
/// Uses atomic write (write to .tmp then rename) to avoid partial reads.
/// </summary>
public sealed class MarketplaceCache
{
    private readonly MarketplaceOptions _options;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    /// <summary>Initializes a new instance of the <see cref="MarketplaceCache"/>.</summary>
    public MarketplaceCache(MarketplaceOptions options) => _options = options;

    /// <summary>Retrieves a cached value. Returns null if not found or expired.</summary>
    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default) where T : class
    {
        var path = GetPath(key);
        if (!File.Exists(path)) return null;

        var info = new FileInfo(path);
        if (DateTime.UtcNow - info.LastWriteTimeUtc > _options.CacheTtl)
        {
            return null;
        }

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

    /// <summary>Stores a value in the cache, overwriting any existing entry.</summary>
    public async Task SetAsync<T>(string key, T value, CancellationToken ct = default) where T : class
    {
        var path = GetPath(key);
        var dir = Path.GetDirectoryName(path)!;
        Directory.CreateDirectory(dir);

        // Atomic write: write to temp, then rename
        var tempPath = path + ".tmp";
        await using (var stream = File.Create(tempPath))
        {
            await JsonSerializer.SerializeAsync(stream, value, JsonOptions, ct);
        }

        File.Move(tempPath, path, overwrite: true);
    }

    /// <summary>Converts a cache key to a safe file-system path.</summary>
    private string GetPath(string key)
    {
        var safeKey = string.Concat(key.Select(c => char.IsLetterOrDigit(c) || c == '-' || c == '_' ? c : '_'));
        return Path.Combine(_options.CacheDirectory, $"{safeKey}.json");
    }
}
