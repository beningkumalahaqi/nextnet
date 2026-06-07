using NextNet.Conventions;
using NextNet.Exceptions;
using NextNet.IO;
using NextNet.Logging;
using NextNet.Routing.Models;

namespace NextNet.Routing;

/// <summary>
/// Scans the application directory for route files and produces a <see cref="RouteManifest"/>.
/// Supports full scans and incremental scans for development hot reload.
/// </summary>
public class RouteScanner
{
    private readonly string _appDir;
    private readonly INextNetLogger? _logger;
    private readonly ISharpFileSystem _fileSystem;
    private readonly RouteConflictDetector _conflictDetector;

    // File suffix to RouteType mapping
    private static readonly Dictionary<string, RouteType> SuffixToType = new(StringComparer.OrdinalIgnoreCase)
    {
        [NextNetConventions.PageFileName] = RouteType.Page,
        [NextNetConventions.LayoutFileName] = RouteType.Layout,
        [NextNetConventions.RouteFileName] = RouteType.Api,
        [NextNetConventions.ErrorFileName] = RouteType.Error,
    };

    /// <summary>
    /// Initializes a new instance of <see cref="RouteScanner"/>.
    /// </summary>
    /// <param name="appDir">The absolute path to the application directory (e.g. <c>app/</c>).</param>
    /// <param name="logger">Optional logger instance.</param>
    /// <param name="fileSystem">Optional file system abstraction. Defaults to <see cref="DefaultSharpFileSystem"/>.</param>
    public RouteScanner(
        string appDir,
        INextNetLogger? logger = null,
        ISharpFileSystem? fileSystem = null)
    {
        _appDir = appDir.Replace('\\', '/').TrimEnd('/');
        _logger = logger;
        _fileSystem = fileSystem ?? new DefaultSharpFileSystem();
        _conflictDetector = new RouteConflictDetector();
    }

    /// <summary>
    /// Performs a full scan of the application directory, discovering all route files.
    /// </summary>
    /// <returns>A <see cref="RouteManifest"/> containing all discovered routes and conflicts.</returns>
    public RouteManifest Scan()
    {
        _logger?.Info("Scanning routes in {AppDir}", _appDir);

        if (!_fileSystem.DirectoryExists(_appDir))
        {
            _logger?.Warn("Application directory '{AppDir}' does not exist.", _appDir);
            return RouteManifest.Empty;
        }

        // Enumerate all .cs files recursively
        var csFiles = EnumerateAllCsFiles(_appDir).ToList();
        _logger?.Debug("Found {Count} .cs files in {AppDir}", csFiles.Count, _appDir);

        var entries = new List<RouteEntry>(csFiles.Count);

        foreach (var filePath in csFiles)
        {
            try
            {
                var entry = CreateEntry(filePath);
                if (entry != null)
                {
                    entries.Add(entry);
                }
            }
            catch (Exception ex)
            {
                _logger?.Warn("Failed to parse route from '{FilePath}': {Message}", filePath, ex.Message);
            }
        }

        // Resolve layout hierarchy
        ResolveLayoutHierarchy(entries);

        // Detect conflicts
        var conflicts = _conflictDetector.Detect(entries);

        // Build manifest
        var manifest = BuildManifest(entries, conflicts);

        _logger?.Info("Route scan complete: {Pages} pages, {Layouts} layouts, {Api} API routes, {Conflicts} conflicts",
            manifest.Pages.Count,
            manifest.Layouts.Count,
            manifest.ApiRoutes.Count,
            manifest.Conflicts.Count);

        return manifest;
    }

    /// <summary>
    /// Performs an asynchronous full scan of the application directory.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation, with a <see cref="RouteManifest"/> result.</returns>
    public Task<RouteManifest> ScanAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.Run(() => Scan(), cancellationToken);
    }

    /// <summary>
    /// Performs an incremental scan based on a set of changed file paths.
    /// Only rescans the affected entries rather than the entire directory.
    /// </summary>
    /// <param name="previous">The previous route manifest to update.</param>
    /// <param name="changedFiles">The set of file paths that have been added, modified, or deleted.</param>
    /// <returns>An updated <see cref="RouteManifest"/>.</returns>
    public RouteManifest IncrementalScan(RouteManifest previous, IReadOnlySet<string> changedFiles)
    {
        if (previous == null)
            throw new ArgumentNullException(nameof(previous));
        if (changedFiles == null)
            throw new ArgumentNullException(nameof(changedFiles));

        if (changedFiles.Count == 0)
            return previous;

        _logger?.Debug("Incremental scan: {Count} changed files", changedFiles.Count);

        // Build mutable collections from the previous manifest
        var entries = new List<RouteEntry>(previous.Routes);

        foreach (var changedFile in changedFiles)
        {
            var normalizedPath = changedFile.Replace('\\', '/');

            // Check if the file exists (added or modified)
            if (_fileSystem.FileExists(normalizedPath))
            {
                // Remove any existing entry for this file path
                entries.RemoveAll(e =>
                    string.Equals(e.FilePath, normalizedPath, StringComparison.OrdinalIgnoreCase));

                // Create a new entry
                try
                {
                    var entry = CreateEntry(normalizedPath);
                    if (entry != null)
                    {
                        entries.Add(entry);
                        _logger?.Debug("Added/updated entry for '{FilePath}': {RoutePattern}",
                            normalizedPath, entry.RoutePattern);
                    }
                }
                catch (Exception ex)
                {
                    _logger?.Warn("Failed to parse changed file '{FilePath}': {Message}",
                        normalizedPath, ex.Message);
                }
            }
            else
            {
                // File was deleted; remove existing entry
                var removed = entries.RemoveAll(e =>
                    string.Equals(e.FilePath, normalizedPath, StringComparison.OrdinalIgnoreCase));
                if (removed > 0)
                {
                    _logger?.Debug("Removed entry for deleted file '{FilePath}'", normalizedPath);
                }
            }
        }

        // Re-resolve layout hierarchy and detect conflicts
        ResolveLayoutHierarchy(entries);
        var conflicts = _conflictDetector.Detect(entries);

        return BuildManifest(entries, conflicts);
    }

    /// <summary>
    /// Enumerates all .cs files recursively in the given directory.
    /// Uses the file system abstraction to allow testing.
    /// </summary>
    private IEnumerable<string> EnumerateAllCsFiles(string directory)
    {
        var files = new List<string>();
        EnumerateCsFilesRecursive(directory, files);
        return files;
    }

    private void EnumerateCsFilesRecursive(string directory, List<string> files)
    {
        try
        {
            foreach (var file in _fileSystem.EnumerateFiles(directory, "*.cs"))
            {
                files.Add(file.Replace('\\', '/'));
            }

            foreach (var subDir in _fileSystem.EnumerateDirectories(directory))
            {
                EnumerateCsFilesRecursive(subDir, files);
            }
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or DirectoryNotFoundException)
        {
            _logger?.Warn("Cannot access directory '{Directory}': {Message}", directory, ex.Message);
        }
    }

    /// <summary>
    /// Creates a <see cref="RouteEntry"/> from a file path.
    /// Returns <c>null</c> if the file does not match a known route convention.
    /// </summary>
    private RouteEntry? CreateEntry(string filePath)
    {
        var fileName = _fileSystem.GetFileName(filePath);

        // Determine route type from file name
        if (!SuffixToType.TryGetValue(fileName, out var routeType))
        {
            // Not a known route file; skip
            _logger?.Debug("Skipping non-route file: {FilePath}", filePath);
            return null;
        }

        try
        {
            var (routePattern, segmentKind) = RoutePatternParser.Parse(filePath, _appDir);
            return new RouteEntry(routePattern, filePath, routeType, segmentKind);
        }
        catch (Exception ex)
        {
            throw new RouteDiscoveryException(
                $"Failed to parse route pattern from '{filePath}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Resolves the layout hierarchy for all page entries.
    /// For each page, finds the nearest parent layout(s) by matching directory prefixes.
    /// </summary>
    private void ResolveLayoutHierarchy(List<RouteEntry> entries)
    {
        var layouts = entries
            .Where(e => e.Type == RouteType.Layout)
            .OrderByDescending(e => e.RoutePattern.Length) // longest (most specific) first
            .ThenByDescending(e => e.RoutePattern)
            .ToList();

        var pages = entries.Where(e => e.Type == RouteType.Page).ToList();

        foreach (var page in pages)
        {
            var chain = new List<string>();

            // Find all layouts that are parents of this page
            var parentLayouts = layouts
                .Where(layout =>
                    page.RoutePattern.StartsWith(layout.RoutePattern, StringComparison.OrdinalIgnoreCase)
                    && layout.RoutePattern.Length < page.RoutePattern.Length)
                .OrderByDescending(l => l.RoutePattern.Length) // nearest (most specific) first
                .ToList();

            if (parentLayouts.Count > 0)
            {
                // The nearest layout is the first one (longest pattern match)
                page.LayoutPath = parentLayouts[0].FilePath;

                // Build the full chain: nearest → root
                foreach (var layout in parentLayouts)
                {
                    chain.Add(layout.FilePath);
                }
            }
            else
            {
                page.LayoutPath = null;
            }

            page.LayoutChain = chain;
        }
    }

    /// <summary>
    /// Builds a <see cref="RouteManifest"/> from the list of entries and conflicts.
    /// </summary>
    private static RouteManifest BuildManifest(
        List<RouteEntry> entries,
        IReadOnlyList<RouteConflict> conflicts)
    {
        return new RouteManifest(
            routes: entries.AsReadOnly(),
            pages: entries.Where(e => e.Type == RouteType.Page).ToList().AsReadOnly(),
            layouts: entries.Where(e => e.Type == RouteType.Layout).ToList().AsReadOnly(),
            apiRoutes: entries.Where(e => e.Type == RouteType.Api).ToList().AsReadOnly(),
            errorPage: entries.FirstOrDefault(e => e.Type == RouteType.Error),
            conflicts: conflicts);
    }
}
