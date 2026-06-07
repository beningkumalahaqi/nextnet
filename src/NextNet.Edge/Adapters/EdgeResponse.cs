namespace NextNet.Edge.Adapters;

/// <summary>
/// Default implementation of <see cref="IEdgeResponse"/>.
/// Used internally by adapters and for testing.
/// </summary>
internal class EdgeResponse : IEdgeResponse
{
    /// <inheritdoc />
    public int StatusCode { get; }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, string> Headers { get; }

    /// <inheritdoc />
    public Stream Body { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="EdgeResponse"/>.
    /// </summary>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="headers">Optional response headers.</param>
    /// <param name="body">Optional response body stream. If null, an empty stream is used.</param>
    public EdgeResponse(
        int statusCode,
        IReadOnlyDictionary<string, string>? headers = null,
        Stream? body = null)
    {
        StatusCode = statusCode;
        Headers = headers ?? new Dictionary<string, string>();
        Body = body ?? new MemoryStream();
    }
}
