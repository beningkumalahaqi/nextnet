using Microsoft.AspNetCore.Http;

namespace NextNet.Build.Production.Caching;

/// <summary>
/// Middleware that adds cache control headers to HTTP responses based on
/// file type and content-hashing strategy.
/// </summary>
public class CacheHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly CacheHeaderOptions _options;
    private readonly ContentHashGenerator _hashGenerator;

    /// <summary>
    /// Initializes a new instance of <see cref="CacheHeadersMiddleware"/>.
    /// </summary>
    public CacheHeadersMiddleware(
        RequestDelegate next,
        CacheHeaderOptions options,
        ContentHashGenerator hashGenerator)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _hashGenerator = hashGenerator ?? throw new ArgumentNullException(nameof(hashGenerator));
    }

    /// <summary>
    /// Processes the response and adds cache headers.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        if (!_options.EnableCaching)
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

        // Capture the response for header modification
        var originalBody = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        await _next(context);

        responseBody.Seek(0, SeekOrigin.Begin);

        // Only add cache headers for successful responses
        if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
        {
            var ext = Path.GetExtension(path);

            if (_options.ImmutableExtensions.Contains(ext))
            {
                // Content-hashed assets: immutable, long max-age
                var cacheValue = $"public,max-age={(int)_options.ImmutableMaxAge.TotalSeconds}";
                if (_options.SetImmutable)
                {
                    cacheValue += ",immutable";
                }
                context.Response.Headers["Cache-Control"] = cacheValue;

                // Add ETag for content-hashed assets
                if (_options.EnableETag)
                {
                    var content = await ReadStreamAsync(responseBody);
                    var etag = _hashGenerator.GenerateETag(content);
                    context.Response.Headers["ETag"] = etag;
                }
            }
            else
            {
                // Non-hashed content: shorter cache
                var cacheValue = $"public,max-age={(int)_options.DefaultMaxAge.TotalSeconds}";
                context.Response.Headers["Cache-Control"] = cacheValue;

                if (_options.EnableLastModified)
                {
                    context.Response.Headers["Last-Modified"] = DateTime.UtcNow.ToString("R");
                }
            }
        }

        responseBody.Seek(0, SeekOrigin.Begin);
        await responseBody.CopyToAsync(originalBody);
        context.Response.Body = originalBody;
    }

    private static async Task<byte[]> ReadStreamAsync(Stream stream)
    {
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        return ms.ToArray();
    }
}
