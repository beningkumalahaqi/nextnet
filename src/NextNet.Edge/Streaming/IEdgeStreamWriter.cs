namespace NextNet.Edge.Streaming;

/// <summary>
/// Defines the contract for writing streaming responses at the edge.
/// Edge providers have varying support for streaming (Cloudflare Workers supports it,
/// Lambda@Edge has limited support), so this abstraction allows graceful degradation.
/// </summary>
public interface IEdgeStreamWriter
{
    /// <summary>
    /// Gets whether the current edge provider supports streaming responses.
    /// </summary>
    bool SupportsStreaming { get; }

    /// <summary>
    /// Writes a chunk of data to the streaming response.
    /// </summary>
    /// <param name="data">The data chunk to write.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous write operation.</returns>
    Task WriteAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes a string chunk to the streaming response.
    /// </summary>
    /// <param name="text">The text chunk to write.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous write operation.</returns>
    Task WriteAsync(string text, CancellationToken cancellationToken = default);

    /// <summary>
    /// Flushes the current buffer to the client.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous flush operation.</returns>
    Task FlushAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Completes the streaming response. No more data can be written after this.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous complete operation.</returns>
    Task CompleteAsync(CancellationToken cancellationToken = default);
}
