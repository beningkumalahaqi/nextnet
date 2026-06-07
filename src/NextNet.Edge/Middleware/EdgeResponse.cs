using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace NextNet.Edge.Middleware;

/// <summary>
/// Represents an edge-compatible HTTP response.
/// Wraps ASP.NET Core's <see cref="HttpResponse"/> or provides a standalone implementation
/// for edge environments (Cloudflare Workers, Deno Deploy).
/// </summary>
public class EdgeResponse
{
    /// <summary>
    /// Gets or sets the HTTP status code.
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Gets the response headers.
    /// </summary>
    public IDictionary<string, string> Headers { get; }

    /// <summary>
    /// Gets or sets the response body stream.
    /// </summary>
    public Stream Body { get; set; }

    /// <summary>
    /// Gets or sets the content type.
    /// </summary>
    public string? ContentType
    {
        get => Headers.TryGetValue("Content-Type", out var ct) ? ct : null;
        set
        {
            if (value != null)
                Headers["Content-Type"] = value;
            else
                Headers.Remove("Content-Type");
        }
    }

    /// <summary>
    /// Gets or sets the content length.
    /// </summary>
    public long? ContentLength
    {
        get => Headers.TryGetValue("Content-Length", out var cl) && long.TryParse(cl, out var len) ? len : null;
        set
        {
            if (value.HasValue)
                Headers["Content-Length"] = value.Value.ToString();
            else
                Headers.Remove("Content-Length");
        }
    }

    /// <summary>
    /// Gets the underlying ASP.NET Core <see cref="HttpResponse"/>, if available.
    /// Null when running on a pure edge runtime.
    /// </summary>
    public HttpResponse? AspNetCoreResponse { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="EdgeResponse"/> from an ASP.NET Core <see cref="HttpResponse"/>.
    /// </summary>
    /// <param name="httpResponse">The ASP.NET Core HTTP response.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="httpResponse"/> is null.</exception>
    public EdgeResponse(HttpResponse httpResponse)
    {
        if (httpResponse == null) throw new ArgumentNullException(nameof(httpResponse));

        AspNetCoreResponse = httpResponse;
        StatusCode = httpResponse.StatusCode;
        Body = httpResponse.Body;
        Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var header in httpResponse.Headers)
        {
            Headers[header.Key] = header.Value.ToString();
        }
    }

    /// <summary>
    /// Initializes a new instance of <see cref="EdgeResponse"/> with standalone response data (edge runtime).
    /// </summary>
    /// <param name="statusCode">The HTTP status code. Defaults to 200.</param>
    /// <param name="headers">Optional response headers.</param>
    /// <param name="body">Optional response body stream. If null, a new MemoryStream is used.</param>
    public EdgeResponse(int statusCode = 200, IReadOnlyDictionary<string, string>? headers = null, Stream? body = null)
    {
        StatusCode = statusCode;
        Body = body ?? new MemoryStream();
        Headers = headers != null
            ? new Dictionary<string, string>(headers, StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Converts this edge response to an <see cref="Adapters.IEdgeResponse"/> for use with edge adapters.
    /// </summary>
    /// <returns>An adapter-compatible edge response.</returns>
    public Adapters.IEdgeResponse ToAdapterResponse()
    {
        return new EdgeAdapterResponse(StatusCode, new Dictionary<string, string>(Headers), Body);
    }

    /// <summary>
    /// Writes a string to the response body.
    /// </summary>
    /// <param name="content">The string content to write.</param>
    /// <param name="encoding">Optional encoding. Defaults to UTF-8.</param>
    public async Task WriteAsync(string content, System.Text.Encoding? encoding = null)
    {
        encoding ??= System.Text.Encoding.UTF8;
        var bytes = encoding.GetBytes(content);
        await Body.WriteAsync(bytes);
    }

    /// <summary>
    /// Sets a response header value.
    /// </summary>
    /// <param name="key">The header name.</param>
    /// <param name="value">The header value.</param>
    public void SetHeader(string key, string value)
    {
        Headers[key] = value;
    }

    /// <summary>
    /// Internal implementation of <see cref="Adapters.IEdgeResponse"/> for bridging.
    /// </summary>
    private sealed class EdgeAdapterResponse : Adapters.IEdgeResponse
    {
        public int StatusCode { get; }
        public IReadOnlyDictionary<string, string> Headers { get; }
        public Stream Body { get; }

        public EdgeAdapterResponse(int statusCode, IReadOnlyDictionary<string, string> headers, Stream body)
        {
            StatusCode = statusCode;
            Headers = headers;
            Body = body;
        }
    }
}
