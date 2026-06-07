using Microsoft.AspNetCore.Http;

namespace NextNet.Edge.Streaming;

/// <summary>
/// Default implementation of <see cref="IEdgeStreamWriter"/> for writing streaming responses.
/// Wraps an ASP.NET Core <see cref="HttpResponse"/> stream or provides a buffered fallback
/// for edge environments that don't support true streaming.
/// </summary>
public class EdgeStreamWriter : IEdgeStreamWriter
{
    private readonly Stream _responseStream;
    private readonly EdgeOptions _options;
    private bool _isCompleted;

    /// <summary>
    /// Initializes a new instance of <see cref="EdgeStreamWriter"/>.
    /// </summary>
    /// <param name="responseStream">The underlying response stream to write to.</param>
    /// <param name="options">Edge configuration options.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="responseStream"/> or <paramref name="options"/> is null.</exception>
    public EdgeStreamWriter(Stream responseStream, EdgeOptions options)
    {
        _responseStream = responseStream ?? throw new ArgumentNullException(nameof(responseStream));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Initializes a new instance of <see cref="EdgeStreamWriter"/> from an ASP.NET Core <see cref="HttpResponse"/>.
    /// </summary>
    /// <param name="httpResponse">The ASP.NET Core HTTP response.</param>
    /// <param name="options">Edge configuration options.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="httpResponse"/> is null.</exception>
    public EdgeStreamWriter(HttpResponse httpResponse, EdgeOptions options)
        : this(httpResponse?.Body ?? throw new ArgumentNullException(nameof(httpResponse)), options)
    {
    }

    /// <inheritdoc />
    public bool SupportsStreaming => true;

    /// <inheritdoc />
    public async Task WriteAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        EnsureNotCompleted();
        cancellationToken.ThrowIfCancellationRequested();
        await _responseStream.WriteAsync(data, cancellationToken);
    }

    /// <inheritdoc />
    public async Task WriteAsync(string text, CancellationToken cancellationToken = default)
    {
        EnsureNotCompleted();
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrEmpty(text))
            return;

        var bytes = System.Text.Encoding.UTF8.GetBytes(text);
        await _responseStream.WriteAsync(bytes, cancellationToken);
    }

    /// <inheritdoc />
    public async Task FlushAsync(CancellationToken cancellationToken = default)
    {
        EnsureNotCompleted();
        cancellationToken.ThrowIfCancellationRequested();
        await _responseStream.FlushAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task CompleteAsync(CancellationToken cancellationToken = default)
    {
        EnsureNotCompleted();
        cancellationToken.ThrowIfCancellationRequested();
        _isCompleted = true;
        return Task.CompletedTask;
    }

    private void EnsureNotCompleted()
    {
        if (_isCompleted)
            throw new InvalidOperationException("The edge stream has already been completed. No more data can be written.");
    }
}
