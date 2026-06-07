using System.IO.Compression;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NextNet.Middleware.Attributes;

namespace NextNet.Middleware.BuiltIn;

/// <summary>
/// Options for the <see cref="CompressionMiddleware"/>.
/// </summary>
public class CompressionOptions
{
    /// <summary>
    /// Gets or sets the compression level. Defaults to <see cref="CompressionLevel.Fastest"/>.
    /// </summary>
    public CompressionLevel Level { get; set; } = CompressionLevel.Fastest;

    /// <summary>
    /// Gets or sets the minimum response size in bytes to trigger compression.
    /// Responses smaller than this will not be compressed. Defaults to 256 bytes.
    /// </summary>
    public int MinimumSizeBytes { get; set; } = 256;

    /// <summary>
    /// Gets or sets the comma-separated list of MIME types to compress.
    /// Defaults to common text-based types.
    /// </summary>
    public HashSet<string> MimeTypes { get; set; } = new(StringComparer.OrdinalIgnoreCase)
    {
        "text/plain",
        "text/html",
        "text/css",
        "text/javascript",
        "application/javascript",
        "application/json",
        "application/xml",
        "application/xhtml+xml",
        "image/svg+xml",
    };

    /// <summary>
    /// Gets or sets which compression algorithm to prefer when client supports multiple.
    /// Defaults to Brotli.
    /// </summary>
    public CompressionAlgorithm PreferredAlgorithm { get; set; } = CompressionAlgorithm.Brotli;
}

/// <summary>
/// Compression algorithm options.
/// </summary>
public enum CompressionAlgorithm
{
    /// <summary>Brotli compression.</summary>
    Brotli,
    /// <summary>GZip compression.</summary>
    GZip,
    /// <summary>Deflate compression.</summary>
    Deflate,
}

/// <summary>
/// Middleware that compresses HTTP responses using Brotli, GZip, or Deflate
/// based on the client's Accept-Encoding header.
/// </summary>
[MiddlewareOrderAttribute(MiddlewareOrder.Compression)]
public class CompressionMiddleware : IMiddleware
{
    private readonly CompressionOptions _options;
    private readonly ILogger<CompressionMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompressionMiddleware"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="options">Optional compression options.</param>
    public CompressionMiddleware(
        ILogger<CompressionMiddleware> logger,
        IOptions<CompressionOptions>? options = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? new CompressionOptions();
    }

    /// <inheritdoc />
    public async Task InvokeAsync(MiddlewareContext context, RequestDelegate next)
    {
        var httpContext = context.HttpContext;
        var request = httpContext.Request;
        var response = httpContext.Response;

        // Check if client accepts compression
        var acceptEncoding = request.Headers.AcceptEncoding.ToString();
        if (string.IsNullOrEmpty(acceptEncoding))
        {
            await next(httpContext);
            return;
        }

        // Parse accepted encodings to determine best algorithm
        var selectedEncoding = SelectEncoding(acceptEncoding);
        if (selectedEncoding == null)
        {
            await next(httpContext);
            return;
        }

        // Check content type (will be set by downstream middleware)
        var originalBody = response.Body;
        var bufferStream = new MemoryStream();
        response.Body = bufferStream;

        try
        {
            await next(httpContext);

            // Determine compression eligibility after downstream has run
            var contentLength = bufferStream.Length;
            var contentType = response.ContentType ?? "";

            // Skip if too small
            if (contentLength < _options.MinimumSizeBytes)
            {
                bufferStream.Position = 0;
                response.Body = originalBody;
                response.ContentLength = contentLength;
                await bufferStream.CopyToAsync(originalBody);
                return;
            }

            // Skip if content type not compressible
            var baseContentType = contentType.Split(';')[0].Trim();
            if (!string.IsNullOrEmpty(baseContentType) &&
                !_options.MimeTypes.Contains(baseContentType))
            {
                bufferStream.Position = 0;
                response.Body = originalBody;
                response.ContentLength = contentLength;
                await bufferStream.CopyToAsync(originalBody);
                return;
            }

            // Compress the response
            response.Headers.ContentEncoding = selectedEncoding;
            response.Body = originalBody;

            await using var compressedStream = CreateCompressionStream(originalBody, selectedEncoding);
            bufferStream.Position = 0;
            await bufferStream.CopyToAsync(compressedStream);
            await compressedStream.FlushAsync();

            // Remove Content-Length since it changes after compression
            response.ContentLength = null;
        }
        catch
        {
            // On exception, restore original body
            response.Body = originalBody;
            throw;
        }
        finally
        {
            if (response.Body == bufferStream)
            {
                response.Body = originalBody;
            }
            await bufferStream.DisposeAsync();
        }
    }

    private string? SelectEncoding(string acceptEncoding)
    {
        var encodings = acceptEncoding.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        // Check if identity or no compression preferred
        if (encodings.Any(e => e.Equals("identity", StringComparison.OrdinalIgnoreCase)))
        {
            // identity means no compression; but if * is also present, we still compress
        }

        bool hasBrotli = false;
        bool hasGzip = false;
        bool hasDeflate = false;
        bool hasAny = false;

        foreach (var encoding in encodings)
        {
            var parts = encoding.Split(';', StringSplitOptions.TrimEntries);
            var name = parts[0];

            if (name.Equals("br", StringComparison.OrdinalIgnoreCase)) hasBrotli = true;
            else if (name.Equals("gzip", StringComparison.OrdinalIgnoreCase)) hasGzip = true;
            else if (name.Equals("deflate", StringComparison.OrdinalIgnoreCase)) hasDeflate = true;
            else if (name.Equals("*", StringComparison.OrdinalIgnoreCase)) hasAny = true;
        }

        if (_options.PreferredAlgorithm == CompressionAlgorithm.Brotli && (hasBrotli || hasAny))
            return "br";
        if (hasGzip || hasAny)
            return "gzip";
        if (hasDeflate)
            return "deflate";

        return null;
    }

    private static Stream CreateCompressionStream(Stream destination, string encoding)
    {
        return encoding switch
        {
            "br" => new BrotliStream(destination, CompressionLevel.Fastest),
            "gzip" => new GZipStream(destination, CompressionLevel.Fastest),
            "deflate" => new DeflateStream(destination, CompressionLevel.Fastest),
            _ => throw new NotSupportedException($"Compression encoding '{encoding}' is not supported."),
        };
    }
}
