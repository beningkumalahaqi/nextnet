namespace NextNet.Edge.Adapters;

/// <summary>
/// Represents a request received at the edge runtime.
/// Abstracts away provider-specific request objects.
/// </summary>
public interface IEdgeRequest
{
    /// <summary>
    /// Gets the HTTP method (GET, POST, etc.).
    /// </summary>
    string Method { get; }

    /// <summary>
    /// Gets the full request URL.
    /// </summary>
    string Url { get; }

    /// <summary>
    /// Gets the request headers as a read-only dictionary.
    /// </summary>
    IReadOnlyDictionary<string, string> Headers { get; }

    /// <summary>
    /// Gets the request body stream, if present.
    /// </summary>
    Stream? Body { get; }
}

/// <summary>
/// Represents a response to be returned from the edge runtime.
/// Abstracts away provider-specific response objects.
/// </summary>
public interface IEdgeResponse
{
    /// <summary>
    /// Gets the HTTP status code.
    /// </summary>
    int StatusCode { get; }

    /// <summary>
    /// Gets the response headers as a read-only dictionary.
    /// </summary>
    IReadOnlyDictionary<string, string> Headers { get; }

    /// <summary>
    /// Gets the response body stream.
    /// </summary>
    Stream Body { get; }
}

/// <summary>
/// Represents a static asset to be deployed alongside the edge function.
/// </summary>
public sealed record StaticAsset
{
    /// <summary>
    /// Gets the route path for the asset (e.g., "/styles/main.css").
    /// </summary>
    public string Route { get; }

    /// <summary>
    /// Gets the content type of the asset.
    /// </summary>
    public string ContentType { get; }

    /// <summary>
    /// Gets the size of the asset in bytes.
    /// </summary>
    public long Size { get; }

    /// <summary>
    /// Gets the local file path of the asset.
    /// </summary>
    public string FilePath { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="StaticAsset"/>.
    /// </summary>
    /// <param name="route">The route path.</param>
    /// <param name="contentType">The MIME content type.</param>
    /// <param name="size">The size in bytes.</param>
    /// <param name="filePath">The local file path.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="route"/>, <paramref name="contentType"/>, or <paramref name="filePath"/> is null.</exception>
    public StaticAsset(string route, string contentType, long size, string filePath)
    {
        Route = route ?? throw new ArgumentNullException(nameof(route));
        ContentType = contentType ?? throw new ArgumentNullException(nameof(contentType));
        Size = size;
        FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
    }
}

/// <summary>
/// Defines the contract for an edge runtime adapter.
/// Each edge provider (Cloudflare Workers, Vercel Edge, Deno Deploy, AWS Lambda@Edge)
/// implements this interface to handle edge requests and serve static assets.
/// </summary>
public interface IEdgeRuntimeAdapter
{
    /// <summary>
    /// Gets the human-readable provider name.
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Gets the provider identifier used in configuration.
    /// </summary>
    string ProviderId { get; }

    /// <summary>
    /// Handles an incoming edge request and returns an edge response.
    /// </summary>
    /// <param name="request">The incoming edge request.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The edge response.</returns>
    Task<IEdgeResponse> HandleRequestAsync(IEdgeRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the list of static assets to be deployed.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A read-only list of static assets.</returns>
    Task<IReadOnlyList<StaticAsset>> GetStaticAssetsAsync(CancellationToken cancellationToken = default);
}
