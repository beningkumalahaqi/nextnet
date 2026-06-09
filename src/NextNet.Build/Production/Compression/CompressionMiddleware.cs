using Microsoft.AspNetCore.Http;
using NextNet.Build.Production.Compression.AlgorithmAdapters;

namespace NextNet.Build.Production.Compression;

/// <summary>
/// Middleware that compresses HTTP responses using the configured algorithm.
/// Designed to work alongside ASP.NET Core's response compression middleware
/// with NextNet-specific defaults and control.
/// </summary>
/// <example>
/// <code>
/// // Registered automatically via app.UseNextNetProduction():
/// app.UseMiddleware&lt;CompressionMiddleware&gt;();
/// // Supports Brotli, Gzip, Deflate, and Zstd content negotiation
/// </code>
/// </example>
public sealed class CompressionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly NextNetCompressionOptions _options;
    private readonly ICompressionAdapter _adapter;

    /// <summary>
    /// Initializes a new instance of <see cref="CompressionMiddleware"/>.
    /// </summary>
    public CompressionMiddleware(
        RequestDelegate next,
        NextNetCompressionOptions options)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _adapter = CreateAdapter(options.PreferredAlgorithm, options.CompressionLevel);
    }

    /// <summary>
    /// Processes the request and compresses the response if applicable.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        if (!_options.EnableCompression)
        {
            await _next(context);
            return;
        }

        var path = context.Request.Path.Value ?? string.Empty;
        if (_options.ExcludedPaths.Contains(path))
        {
            await _next(context);
            return;
        }

        // Check if client accepts compression
        var acceptEncoding = context.Request.Headers["Accept-Encoding"].ToString();
        if (string.IsNullOrEmpty(acceptEncoding))
        {
            await _next(context);
            return;
        }

        // Determine the best algorithm the client accepts
        var selectedAlgo = NegotiateAlgorithm(acceptEncoding);
        if (selectedAlgo == null)
        {
            await _next(context);
            return;
        }

        // Intercept the response body stream
        var originalBody = context.Response.Body;
        using var compressedStream = new MemoryStream();

        context.Response.Body = compressedStream;
        context.Response.Headers.Append("Content-Encoding", selectedAlgo.Value.GetEncodingName());
        context.Response.Headers.Remove("Content-Length");

        try
        {
            await _next(context);

            compressedStream.Seek(0, SeekOrigin.Begin);

            if (compressedStream.Length < _options.MinimumResponseSize)
            {
                // Too small to compress; write uncompressed
                context.Response.Headers.Remove("Content-Encoding");
                await compressedStream.CopyToAsync(originalBody);
                return;
            }

            using var outputStream = new MemoryStream();
            await _adapter.CompressAsync(compressedStream, outputStream);
            outputStream.Seek(0, SeekOrigin.Begin);
            await outputStream.CopyToAsync(originalBody);
        }
        finally
        {
            context.Response.Body = originalBody;
        }
    }

    private static ICompressionAdapter CreateAdapter(CompressionAlgorithm algorithm, int level)
    {
        return algorithm switch
        {
            CompressionAlgorithm.Brotli => new BrotliAdapter(level),
            CompressionAlgorithm.Gzip => new GzipAdapter(level),
            CompressionAlgorithm.Zstd => new ZstdAdapter(level),
            CompressionAlgorithm.Deflate => new DeflateAdapter(level),
            _ => new BrotliAdapter(level),
        };
    }

    private static CompressionAlgorithm? NegotiateAlgorithm(string acceptEncoding)
    {
        // Simple negotiation: prefer Brotli > Gzip > Deflate > Zstd
        if (acceptEncoding.Contains("br", StringComparison.OrdinalIgnoreCase))
            return CompressionAlgorithm.Brotli;
        if (acceptEncoding.Contains("gzip", StringComparison.OrdinalIgnoreCase))
            return CompressionAlgorithm.Gzip;
        if (acceptEncoding.Contains("deflate", StringComparison.OrdinalIgnoreCase))
            return CompressionAlgorithm.Deflate;
        if (acceptEncoding.Contains("zstd", StringComparison.OrdinalIgnoreCase))
            return CompressionAlgorithm.Zstd;

        return null;
    }
}

internal static class CompressionAlgorithmExtensions
{
    public static string GetEncodingName(this CompressionAlgorithm algo) => algo switch
    {
        CompressionAlgorithm.Brotli => "br",
        CompressionAlgorithm.Gzip => "gzip",
        CompressionAlgorithm.Zstd => "zstd",
        CompressionAlgorithm.Deflate => "deflate",
        _ => "gzip",
    };
}
