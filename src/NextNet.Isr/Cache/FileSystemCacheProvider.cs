using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace NextNet.Isr.Cache;

/// <summary>
/// File-system based implementation of <see cref="IIsrCacheStore"/>.
/// Stores cached pages as files in a <c>.nextnet/isr-cache/</c> directory.
/// JSON metadata and HTML content files are persisted across application restarts.
/// Tag-based lookups are supported via a separate tag index file.
/// </summary>
public sealed class FileSystemCacheProvider : IIsrCacheStore, IDisposable
{
    private readonly string _cacheDirectory;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private bool _disposed;

    // In-memory tag index for fast lookups, persisted to disk on mutations
    private readonly Dictionary<string, HashSet<string>> _tagIndex = new(StringComparer.OrdinalIgnoreCase);
    private readonly string _tagIndexPath;

    private const string ContentExtension = ".html";
    private const string MetadataExtension = ".meta.json";
    private const string TagIndexFileName = "tag-index.json";

    /// <summary>
    /// Initializes a new instance of <see cref="FileSystemCacheProvider"/>.
    /// The cache directory is located at <c>.nextnet/isr-cache/</c> under the
    /// specified base path, or the current directory if not specified.
    /// </summary>
    /// <param name="basePath">
    /// The base path for the cache directory. If null, uses the current directory.
    /// </param>
    /// <exception cref="DirectoryNotFoundException">
    /// Thrown when <paramref name="basePath"/> is provided but does not exist.
    /// </exception>
    public FileSystemCacheProvider(string? basePath = null)
    {
        var resolvedBase = string.IsNullOrEmpty(basePath)
            ? Directory.GetCurrentDirectory()
            : basePath;

        if (!Directory.Exists(resolvedBase))
            throw new DirectoryNotFoundException($"[{IsrErrorCodes.BasePathDoesNotExist}] Base path '{resolvedBase}' does not exist.");

        _cacheDirectory = Path.Combine(resolvedBase, ".nextnet", "isr-cache");
        _tagIndexPath = Path.Combine(_cacheDirectory, TagIndexFileName);

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        // Ensure the cache directory exists
        Directory.CreateDirectory(_cacheDirectory);

        // Load the persisted tag index
        LoadTagIndex();
    }

    /// <inheritdoc />
    public async Task<CachedPage?> GetAsync(string route, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var (contentPath, metadataPath) = GetFilePaths(route);

        if (!File.Exists(contentPath) || !File.Exists(metadataPath))
            return null;

        try
        {
            var content = await File.ReadAllTextAsync(contentPath, Encoding.UTF8, cancellationToken);
            var metaJson = await File.ReadAllTextAsync(metadataPath, Encoding.UTF8, cancellationToken);
            var entry = JsonSerializer.Deserialize<CacheEntry>(metaJson, _jsonOptions);

            if (entry == null)
                return null;

            return new CachedPage(route, content, entry);
        }
        catch (JsonException)
        {
            // Corrupt metadata file — treat as cache miss
            return null;
        }
        catch (IOException)
        {
            // File may have been deleted concurrently
            return null;
        }
    }

    /// <inheritdoc />
    public async Task SetAsync(string route, string content, CacheEntry entry, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var (contentPath, metadataPath) = GetFilePaths(route);

        // Ensure the directory structure exists
        var contentDir = Path.GetDirectoryName(contentPath);
        if (contentDir != null)
        {
            Directory.CreateDirectory(contentDir);
        }

        var metadataDir = Path.GetDirectoryName(metadataPath);
        if (metadataDir != null)
        {
            Directory.CreateDirectory(metadataDir);
        }

        // Write content and metadata in parallel
        var contentTask = File.WriteAllTextAsync(contentPath, content, Encoding.UTF8, cancellationToken);
        var metaJson = JsonSerializer.Serialize(entry, _jsonOptions);
        var metaTask = File.WriteAllTextAsync(metadataPath, metaJson, Encoding.UTF8, cancellationToken);

        await Task.WhenAll(contentTask, metaTask);

        // Update tag index in memory and persist to disk
        if (entry.Tags.Count > 0)
        {
            await _lock.WaitAsync(cancellationToken);
            try
            {
                foreach (var tag in entry.Tags)
                {
                    if (!_tagIndex.TryGetValue(tag, out var routes))
                    {
                        routes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        _tagIndex[tag] = routes;
                    }
                    routes.Add(NormalizeRoute(route));
                }

                await PersistTagIndexAsync(cancellationToken);
            }
            finally
            {
                _lock.Release();
            }
        }
    }

    /// <inheritdoc />
    public async Task<bool> RemoveAsync(string route, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var (contentPath, metadataPath) = GetFilePaths(route);
        var normalizedRoute = NormalizeRoute(route);
        var removed = false;

        // Remove the content and metadata files
        if (File.Exists(contentPath))
        {
            try
            {
                File.Delete(contentPath);
                removed = true;
            }
            catch (IOException)
            {
                // File locked or concurrently deleted
            }
        }

        if (File.Exists(metadataPath))
        {
            try
            {
                File.Delete(metadataPath);
                removed = true;
            }
            catch (IOException)
            {
                // File locked or concurrently deleted
            }
        }

        // Clean up tag index
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var tagsToRemove = new List<string>();
            foreach (var kvp in _tagIndex)
            {
                kvp.Value.Remove(normalizedRoute);
                if (kvp.Value.Count == 0)
                {
                    tagsToRemove.Add(kvp.Key);
                }
            }

            foreach (var tag in tagsToRemove)
            {
                _tagIndex.Remove(tag);
            }

            if (tagsToRemove.Count > 0 || removed)
            {
                await PersistTagIndexAsync(cancellationToken);
            }
        }
        finally
        {
            _lock.Release();
        }

        return removed;
    }

    /// <inheritdoc />
    public Task<bool> ExistsAsync(string route, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var (contentPath, metadataPath) = GetFilePaths(route);
        var exists = File.Exists(contentPath) && File.Exists(metadataPath);
        return Task.FromResult(exists);
    }

    /// <inheritdoc />
    public async Task<CacheEntry?> GetMetadataAsync(string route, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var (_, metadataPath) = GetFilePaths(route);

        if (!File.Exists(metadataPath))
            return null;

        try
        {
            var metaJson = await File.ReadAllTextAsync(metadataPath, Encoding.UTF8, cancellationToken);
            return JsonSerializer.Deserialize<CacheEntry>(metaJson, _jsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
        catch (IOException)
        {
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> GetRoutesByTagAsync(IReadOnlyList<string> tags, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await _lock.WaitAsync(cancellationToken);
        try
        {
            var results = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var tag in tags)
            {
                if (_tagIndex.TryGetValue(tag, out var routes))
                {
                    foreach (var route in routes)
                    {
                        results.Add(route);
                    }
                }
            }

            return results.ToList();
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <inheritdoc />
    public async Task AddTagAsync(string route, string tag, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (!_tagIndex.TryGetValue(tag, out var routes))
            {
                routes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                _tagIndex[tag] = routes;
            }

            routes.Add(NormalizeRoute(route));
            await PersistTagIndexAsync(cancellationToken);
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <inheritdoc />
    public async Task RemoveTagAsync(string route, string tag, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (_tagIndex.TryGetValue(tag, out var routes))
            {
                routes.Remove(NormalizeRoute(route));

                if (routes.Count == 0)
                {
                    _tagIndex.Remove(tag);
                }

                await PersistTagIndexAsync(cancellationToken);
            }
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Removes all cached entries and clears the tag index.
    /// </summary>
    public void Clear()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(FileSystemCacheProvider));

        _lock.Wait();
        try
        {
            // Delete all files in the cache directory
            if (Directory.Exists(_cacheDirectory))
            {
                foreach (var file in Directory.EnumerateFiles(_cacheDirectory, "*.*", SearchOption.AllDirectories))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch (IOException)
                    {
                        // Skip locked files
                    }
                }
            }

            _tagIndex.Clear();

            // Write empty tag index
            PersistTagIndexSync();
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Gets the approximate number of cached entries (based on HTML files).
    /// </summary>
    public int Count
    {
        get
        {
            if (!Directory.Exists(_cacheDirectory))
                return 0;

            return Directory.EnumerateFiles(_cacheDirectory, $"*{ContentExtension}", SearchOption.AllDirectories)
                .Count();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _lock.Dispose();
    }

    private (string contentPath, string metadataPath) GetFilePaths(string route)
    {
        var normalizedRoute = NormalizeRoute(route);

        // Sanitize the route to create a valid file system path
        var safePath = SanitizePath(normalizedRoute);

        // Handle root route
        if (string.IsNullOrEmpty(safePath))
            safePath = "_index";

        var contentPath = Path.Combine(_cacheDirectory, safePath + ContentExtension);
        var metadataPath = Path.Combine(_cacheDirectory, safePath + MetadataExtension);

        return (contentPath, metadataPath);
    }

    private static string NormalizeRoute(string route)
    {
        if (string.IsNullOrEmpty(route))
            return "/";

        var normalized = route.StartsWith('/') ? route : "/" + route;
        return normalized.TrimEnd('/').ToLowerInvariant();
    }

    private static string SanitizePath(string route)
    {
        // Convert route segments to directory-safe paths
        // "/blog/hello-world" -> "blog/hello-world"
        var segments = route.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (segments.Length == 0)
            return "_index";

        var sanitizedSegments = segments.Select(s =>
        {
            // Replace characters that are invalid in file names
            var sanitized = s;
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                sanitized = sanitized.Replace(c, '_');
            }
            return sanitized;
        });

        return string.Join("/", sanitizedSegments);
    }

    private void LoadTagIndex()
    {
        if (!File.Exists(_tagIndexPath))
            return;

        try
        {
            var json = File.ReadAllText(_tagIndexPath, Encoding.UTF8);

            if (string.IsNullOrWhiteSpace(json))
                return;

            var deserialized = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json, _jsonOptions);

            if (deserialized == null)
                return;

            foreach (var kvp in deserialized)
            {
                _tagIndex[kvp.Key] = new HashSet<string>(kvp.Value, StringComparer.OrdinalIgnoreCase);
            }
        }
        catch (JsonException)
        {
            // Corrupt tag index — start fresh
            _tagIndex.Clear();
        }
        catch (IOException)
        {
            // Cannot read tag index — start fresh
            _tagIndex.Clear();
        }
    }

    private async Task PersistTagIndexAsync(CancellationToken cancellationToken = default)
    {
        // Convert HashSet to List for JSON serialization
        var serializable = _tagIndex.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.ToList(),
            StringComparer.OrdinalIgnoreCase);

        var json = JsonSerializer.Serialize(serializable, _jsonOptions);
        await File.WriteAllTextAsync(_tagIndexPath, json, Encoding.UTF8, cancellationToken);
    }

    private void PersistTagIndexSync()
    {
        var serializable = _tagIndex.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.ToList(),
            StringComparer.OrdinalIgnoreCase);

        var json = JsonSerializer.Serialize(serializable, _jsonOptions);
        File.WriteAllText(_tagIndexPath, json, Encoding.UTF8);
    }
}
