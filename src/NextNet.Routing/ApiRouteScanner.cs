using System.Text.RegularExpressions;
using NextNet.Conventions;
using NextNet.Exceptions;
using NextNet.IO;
using NextNet.Logging;
using NextNet.Routing.Models;

namespace NextNet.Routing;

/// <summary>
/// Discovers API route files (<c>route.cs</c>) in <c>app/api/</c> directories recursively,
/// converts file paths to route patterns, and detects supported HTTP methods
/// by scanning method overrides in the source file.
/// </summary>
public class ApiRouteScanner
{
    private readonly string _appDir;
    private readonly INextNetLogger? _logger;
    private readonly ISharpFileSystem _fileSystem;

    /// <summary>
    /// Initializes a new instance of <see cref="ApiRouteScanner"/>.
    /// </summary>
    /// <param name="appDir">The absolute path to the application directory (e.g. <c>app/</c>).</param>
    /// <param name="logger">Optional logger instance.</param>
    /// <param name="fileSystem">Optional file system abstraction. Defaults to <see cref="DefaultSharpFileSystem"/>.</param>
    public ApiRouteScanner(
        string appDir,
        INextNetLogger? logger = null,
        ISharpFileSystem? fileSystem = null)
    {
        _appDir = appDir.Replace('\\', '/').TrimEnd('/');
        _logger = logger;
        _fileSystem = fileSystem ?? new DefaultSharpFileSystem();
    }

    /// <summary>
    /// Scans the <c>app/api/</c> directory for all <c>route.cs</c> files.
    /// </summary>
    /// <returns>A list of discovered API route entries.</returns>
    public IReadOnlyList<RouteEntry> Scan()
    {
        var apiDir = _appDir + "/api";

        if (!_fileSystem.DirectoryExists(apiDir))
        {
            _logger?.Debug("API directory '{ApiDir}' does not exist; skipping API route scan.", apiDir);
            return Array.Empty<RouteEntry>();
        }

        _logger?.Info("Scanning API routes in {ApiDir}", apiDir);

        var routeFiles = EnumerateApiRouteFiles(apiDir).ToList();
        _logger?.Debug("Found {Count} API route files in {ApiDir}", routeFiles.Count, apiDir);

        var entries = new List<RouteEntry>(routeFiles.Count);

        foreach (var filePath in routeFiles)
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
                _logger?.Warn("Failed to parse API route from '{FilePath}': {Message}",
                    filePath, ex.Message);
            }
        }

        return entries;
    }

    /// <summary>
    /// Scans the API directory asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation with the discovered entries.</returns>
    public Task<IReadOnlyList<RouteEntry>> ScanAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.Run(() => Scan(), cancellationToken);
    }

    /// <summary>
    /// Enumerates all <c>route.cs</c> files recursively under the given directory.
    /// </summary>
    private IEnumerable<string> EnumerateApiRouteFiles(string directory)
    {
        var files = new List<string>();
        EnumerateRouteFilesRecursive(directory, files);
        return files;
    }

    private void EnumerateRouteFilesRecursive(string directory, List<string> files)
    {
        try
        {
            foreach (var file in _fileSystem.EnumerateFiles(directory, "*.cs"))
            {
                var fileName = _fileSystem.GetFileName(file);
                if (NextNetConventions.IsRouteFile(fileName))
                {
                    files.Add(file.Replace('\\', '/'));
                }
            }

            foreach (var subDir in _fileSystem.EnumerateDirectories(directory))
            {
                EnumerateRouteFilesRecursive(subDir, files);
            }
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or DirectoryNotFoundException)
        {
            _logger?.Warn("Cannot access directory '{Directory}': {Message}", directory, ex.Message);
        }
    }

    /// <summary>
    /// Creates a <see cref="RouteEntry"/> from an API route file path.
    /// Includes HTTP method detection by scanning the source content.
    /// </summary>
    private RouteEntry? CreateEntry(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return null;

        var fileName = _fileSystem.GetFileName(filePath);
        if (!NextNetConventions.IsRouteFile(fileName))
            return null;

        try
        {
            var (routePattern, segmentKind) = RoutePatternParser.Parse(filePath, _appDir);

            var entry = new RouteEntry(routePattern, filePath, RouteType.Api, segmentKind);

            // Detect HTTP methods from source file content
            var methods = DetectHttpMethods(filePath);
            foreach (var method in methods)
            {
                entry.HttpMethods.Add(method);
            }

            return entry;
        }
        catch (Exception ex)
        {
            throw new RouteDiscoveryException(
                $"Failed to parse API route pattern from '{filePath}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Detects overridden HTTP methods by scanning the source file content
    /// for method override patterns. Uses regex for a lightweight scan.
    /// </summary>
    /// <param name="filePath">The source file path.</param>
    /// <returns>A set of HTTP method names (e.g. "GET", "POST").</returns>
    private ISet<string> DetectHttpMethods(string filePath)
    {
        var methods = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        try
        {
            if (!_fileSystem.FileExists(filePath))
                return methods;

            var content = _fileSystem.ReadAllText(filePath);
            if (string.IsNullOrEmpty(content))
                return methods;

            // Match method override patterns:
            //   public override async Task<IResult> Get()
            //   public override Task<IResult> Post()
            //   public override async Task<IResult> Put(int id)
            //   etc.
            var regex = new Regex(
                @"override\s+(async\s+)?Task<IResult>\s+(Get|Post|Put|Patch|Delete)\s*\(",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

            var matches = regex.Matches(content);
            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                if (match.Groups.Count >= 3)
                {
                    var methodName = match.Groups[2].Value;
                    if (!string.IsNullOrEmpty(methodName))
                    {
                        methods.Add(methodName.ToUpperInvariant());
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.Warn("Failed to detect HTTP methods in '{FilePath}': {Message}",
                filePath, ex.Message);
        }

        return methods;
    }

    /// <summary>
    /// Converts an API route pattern to a clean URL path suitable for endpoint registration.
    /// Ensures the pattern starts with <c>/api/</c>.
    /// </summary>
    /// <param name="routePattern">The route pattern from the scanner (e.g. <c>/api/users</c>).</param>
    /// <returns>A normalized API route pattern.</returns>
    public static string NormalizeApiRoutePattern(string routePattern)
    {
        if (string.IsNullOrEmpty(routePattern))
            return "/api";

        var normalized = routePattern.Trim().TrimEnd('/');
        if (string.IsNullOrEmpty(normalized))
            return "/api";

        // Ensure leading /
        if (!normalized.StartsWith('/'))
            normalized = "/" + normalized;

        return normalized;
    }
}
