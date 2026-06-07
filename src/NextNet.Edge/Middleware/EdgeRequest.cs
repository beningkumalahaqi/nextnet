using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace NextNet.Edge.Middleware;

/// <summary>
/// Represents an edge-compatible HTTP request.
/// Wraps ASP.NET Core's <see cref="HttpRequest"/> or provides a standalone implementation
/// for edge environments (Cloudflare Workers, Deno Deploy).
/// </summary>
public class EdgeRequest
{
    /// <summary>
    /// Gets the HTTP method (GET, POST, etc.).
    /// </summary>
    public string Method { get; }

    /// <summary>
    /// Gets the request scheme (http or https).
    /// </summary>
    public string Scheme { get; }

    /// <summary>
    /// Gets the request host.
    /// </summary>
    public HostString Host { get; }

    /// <summary>
    /// Gets the request path.
    /// </summary>
    public PathString Path { get; }

    /// <summary>
    /// Gets the query string.
    /// </summary>
    public QueryString QueryString { get; }

    /// <summary>
    /// Gets the full URL of the request.
    /// </summary>
    public string Url => $"{Scheme}://{Host}{Path}{QueryString}";

    /// <summary>
    /// Gets the request headers as a read-only dictionary.
    /// </summary>
    public IReadOnlyDictionary<string, string> Headers { get; }

    /// <summary>
    /// Gets the request body stream, if present.
    /// </summary>
    public Stream? Body { get; }

    /// <summary>
    /// Gets the content type of the request body.
    /// </summary>
    public string? ContentType { get; }

    /// <summary>
    /// Gets the content length of the request body.
    /// </summary>
    public long? ContentLength { get; }

    /// <summary>
    /// Gets query parameters parsed from the query string.
    /// </summary>
    public IReadOnlyDictionary<string, string> Query { get; }

    /// <summary>
    /// Gets the underlying ASP.NET Core <see cref="HttpRequest"/>, if available.
    /// Null when running on a pure edge runtime.
    /// </summary>
    public HttpRequest? AspNetCoreRequest { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="EdgeRequest"/> from an ASP.NET Core <see cref="HttpRequest"/>.
    /// </summary>
    /// <param name="httpRequest">The ASP.NET Core HTTP request.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="httpRequest"/> is null.</exception>
    public EdgeRequest(HttpRequest httpRequest)
    {
        if (httpRequest == null) throw new ArgumentNullException(nameof(httpRequest));

        AspNetCoreRequest = httpRequest;
        Method = httpRequest.Method;
        Scheme = httpRequest.Scheme;
        Host = httpRequest.Host;
        Path = httpRequest.Path;
        QueryString = httpRequest.QueryString;
        Body = httpRequest.Body;
        ContentType = httpRequest.ContentType;
        ContentLength = httpRequest.ContentLength;

        Headers = httpRequest.Headers
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString(), StringComparer.OrdinalIgnoreCase);

        Query = httpRequest.Query
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString(), StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Initializes a new instance of <see cref="EdgeRequest"/> from raw request data (edge runtime).
    /// </summary>
    /// <param name="method">The HTTP method.</param>
    /// <param name="url">The full request URL.</param>
    /// <param name="headers">Optional request headers.</param>
    /// <param name="body">Optional request body stream.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="method"/> or <paramref name="url"/> is null.</exception>
    public EdgeRequest(string method, string url, IReadOnlyDictionary<string, string>? headers = null, Stream? body = null)
    {
        if (method == null) throw new ArgumentNullException(nameof(method));
        if (url == null) throw new ArgumentNullException(nameof(url));

        Method = method;
        Body = body;
        Headers = headers ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        Query = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // Parse URL
        // On Unix, absolute paths like "/about" are parsed as file:// URIs,
        // so we check for that and treat them as relative paths.
        if (Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
            !string.Equals(uri.Scheme, "file", StringComparison.OrdinalIgnoreCase))
        {
            Scheme = uri.Scheme;
            var hostPort = uri.Port > 0 ? uri.Port : -1;
            Host = hostPort > 0
                ? new HostString(uri.Host, hostPort)
                : new HostString(uri.Host);
            Path = new PathString(uri.AbsolutePath);
            QueryString = new QueryString(uri.Query);

            // Parse query parameters
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
            var queryDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var key in query.AllKeys)
            {
                if (key != null)
                    queryDict[key] = query[key] ?? string.Empty;
            }
            Query = queryDict;
        }
        else
        {
            // Fallback: treat as relative URL
            Scheme = "https";
            Host = new HostString("localhost");
            Path = new PathString(url);
            QueryString = QueryString.Empty;
        }
    }

    /// <summary>
    /// Converts this edge request to an <see cref="Adapters.IEdgeRequest"/> for use with edge adapters.
    /// </summary>
    /// <returns>An adapter-compatible edge request.</returns>
    public Adapters.IEdgeRequest ToAdapterRequest()
    {
        return new EdgeAdapterRequest(
            Method,
            Url,
            new Dictionary<string, string>(Headers),
            Body);
    }

    /// <summary>
    /// Internal implementation of <see cref="Adapters.IEdgeRequest"/> for bridging.
    /// </summary>
    private sealed class EdgeAdapterRequest : Adapters.IEdgeRequest
    {
        public string Method { get; }
        public string Url { get; }
        public IReadOnlyDictionary<string, string> Headers { get; }
        public Stream? Body { get; }

        public EdgeAdapterRequest(string method, string url, IReadOnlyDictionary<string, string> headers, Stream? body)
        {
            Method = method;
            Url = url;
            Headers = headers;
            Body = body;
        }
    }
}
