using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NextNet.Middleware.Attributes;

namespace NextNet.Middleware.BuiltIn;

/// <summary>
/// Options for the <see cref="StaticFilesMiddleware"/>.
/// </summary>
/// <example>
/// <code>
/// // Serve static files from a custom directory
/// services.Configure&lt;StaticFilesOptions&gt;(options =>
/// {
///     options.RequestPath = "/assets";
///     options.CacheMaxAgeSeconds = 3600;
///     options.ServeDefaultFiles = true;
/// });
/// </code>
/// </example>
public sealed record StaticFilesOptions
{
    /// <summary>
    /// Gets or sets the request path prefix for static files.
    /// Defaults to "/static".
    /// </summary>
    public string RequestPath { get; set; } = "/static";

    /// <summary>
    /// Gets or sets the physical or embedded file provider.
    /// If not set, defaults to the "wwwroot" folder in the content root.
    /// </summary>
    public IFileProvider? FileProvider { get; set; }

    /// <summary>
    /// Gets or sets the content root path for resolving default wwwroot.
    /// </summary>
    public string? ContentRootPath { get; set; }

    /// <summary>
    /// Gets or sets the cache max-age in seconds for static files.
    /// Defaults to 86400 (1 day).
    /// </summary>
    public int CacheMaxAgeSeconds { get; set; } = 86400;

    /// <summary>
    /// Gets or sets whether to serve default files (index.html).
    /// Defaults to false.
    /// </summary>
    public bool ServeDefaultFiles { get; set; }
}

/// <summary>
/// Middleware that serves static files from a configured file provider.
/// Supports caching headers and default file serving.
/// </summary>
/// <example>
/// <code>
/// // In pipeline configuration:
/// pipeline.Use&lt;StaticFilesMiddleware&gt;();
///
/// // Requests to /static/css/app.css will serve files from wwwroot/css/app.css
/// // with caching headers for cacheable file types.
/// </code>
/// </example>
[MiddlewareOrderAttribute(MiddlewareOrder.StaticFiles)]
public sealed class StaticFilesMiddleware : IMiddleware
{
    private readonly IFileProvider _fileProvider;
    private readonly StaticFilesOptions _options;
    private readonly ILogger<StaticFilesMiddleware> _logger;
    private static readonly HashSet<string> s_staticExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".html", ".htm", ".css", ".js", ".jsx", ".ts", ".tsx", ".map",
        ".json", ".xml", ".txt", ".csv",
        ".png", ".jpg", ".jpeg", ".gif", ".svg", ".ico", ".webp", ".bmp",
        ".woff", ".woff2", ".ttf", ".eot", ".otf",
        ".pdf", ".doc", ".docx", ".xls", ".xlsx",
        ".mp4", ".webm", ".avi", ".mov",
        ".mp3", ".wav", ".ogg",
        ".wasm"
    };

    private static readonly HashSet<string> s_cacheableExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".css", ".js", ".png", ".jpg", ".jpeg", ".gif", ".svg", ".ico",
        ".webp", ".woff", ".woff2", ".ttf", ".eot", ".otf",
        ".wasm"
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="StaticFilesMiddleware"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="options">The static files options.</param>
    /// <param name="environment">The web host environment.</param>
    public StaticFilesMiddleware(
        ILogger<StaticFilesMiddleware> logger,
        IOptions<StaticFilesOptions>? options = null,
        Microsoft.AspNetCore.Hosting.IWebHostEnvironment? environment = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? new StaticFilesOptions();

        _fileProvider = _options.FileProvider
            ?? environment?.WebRootFileProvider
            ?? new PhysicalFileProvider(
                _options.ContentRootPath
                ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"));
    }

    /// <inheritdoc />
    public async Task InvokeAsync(MiddlewareContext context, RequestDelegate next)
    {
        var httpContext = context.HttpContext;
        var request = httpContext.Request;
        var path = request.Path.Value ?? "/";

        // Check if request path starts with the configured request path prefix
        if (!string.IsNullOrEmpty(_options.RequestPath) &&
            !path.StartsWith(_options.RequestPath, StringComparison.OrdinalIgnoreCase))
        {
            await next(httpContext);
            return;
        }

        // Strip the request path prefix to get the relative file path
        var relativePath = path;
        if (!string.IsNullOrEmpty(_options.RequestPath) &&
            path.StartsWith(_options.RequestPath, StringComparison.OrdinalIgnoreCase))
        {
            relativePath = path[_options.RequestPath.Length..];
            if (string.IsNullOrEmpty(relativePath)) relativePath = "/";
        }

        // Try to serve the file
        var fileInfo = _fileProvider.GetFileInfo(relativePath);

        if (!fileInfo.Exists || fileInfo.IsDirectory)
        {
            // Try default files if directory
            if (_options.ServeDefaultFiles && (fileInfo.IsDirectory || relativePath == "/"))
            {
                var defaultFiles = new[] { "index.html", "index.htm", "default.html" };
                foreach (var defaultFile in defaultFiles)
                {
                    var defaultInfo = _fileProvider.GetFileInfo(
                        relativePath.TrimEnd('/') + "/" + defaultFile);
                    if (defaultInfo.Exists)
                    {
                        fileInfo = defaultInfo;
                        break;
                    }
                }

                if (!fileInfo.Exists)
                {
                    await next(httpContext);
                    return;
                }
            }
            else
            {
                await next(httpContext);
                return;
            }
        }

        // Determine content type
        var extension = Path.GetExtension(fileInfo.Name);
        var contentType = GetContentType(extension);

        // Set response headers
        httpContext.Response.StatusCode = 200;
        httpContext.Response.ContentType = contentType;
        httpContext.Response.ContentLength = fileInfo.Length;

        // Cache control for cacheable extensions
        if (s_cacheableExtensions.Contains(extension) && _options.CacheMaxAgeSeconds > 0)
        {
            httpContext.Response.Headers.CacheControl =
                $"public, max-age={_options.CacheMaxAgeSeconds}";
        }

        // Write file content
        await using var stream = fileInfo.CreateReadStream();
        await stream.CopyToAsync(httpContext.Response.Body);
    }

    private static string GetContentType(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".html" or ".htm" => "text/html; charset=utf-8",
            ".css" => "text/css; charset=utf-8",
            ".js" => "application/javascript; charset=utf-8",
            ".jsx" or ".tsx" => "text/plain; charset=utf-8",
            ".ts" => "application/x-typescript",
            ".map" => "application/json",
            ".json" => "application/json",
            ".xml" => "application/xml",
            ".txt" => "text/plain",
            ".csv" => "text/csv",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".svg" => "image/svg+xml",
            ".ico" => "image/x-icon",
            ".webp" => "image/webp",
            ".bmp" => "image/bmp",
            ".woff" => "font/woff",
            ".woff2" => "font/woff2",
            ".ttf" => "font/ttf",
            ".eot" => "application/vnd.ms-fontobject",
            ".otf" => "font/otf",
            ".pdf" => "application/pdf",
            ".wasm" => "application/wasm",
            _ => "application/octet-stream",
        };
    }
}
