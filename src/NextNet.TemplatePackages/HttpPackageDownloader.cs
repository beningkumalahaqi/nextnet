namespace NextNet.TemplatePackages;

/// <summary>
/// Downloads template package streams over HTTP(S) with optional progress reporting.
/// Uses <see cref="HttpCompletionOption.ResponseHeadersRead"/> for streaming downloads
/// so that large packages do not need to be buffered entirely in memory.
/// </summary>
public sealed class HttpPackageDownloader
{
    private readonly HttpClient _http;

    /// <summary>
    /// Initializes a new instance of <see cref="HttpPackageDownloader"/>.
    /// </summary>
    /// <param name="http">The <see cref="HttpClient"/> used for requests.</param>
    public HttpPackageDownloader(HttpClient http) => _http = http;

    /// <summary>
    /// Downloads a package as a <see cref="Stream"/> from the specified URL.
    /// When a progress callback and content-length are available, the returned
    /// stream tracks read progress and reports it through the <paramref name="progress"/>.
    /// </summary>
    /// <param name="url">The HTTP(S) URL of the package to download.</param>
    /// <param name="progress">Optional progress reporter for download progress.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A readable stream containing the downloaded package bytes.</returns>
    /// <exception cref="HttpRequestException">Thrown when the HTTP response is not successful.</exception>
    public async Task<Stream> DownloadAsync(
        string url,
        IProgress<DownloadProgress>? progress = null,
        CancellationToken ct = default)
    {
        var response = await _http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();

        var totalBytes = response.Content.Headers.ContentLength ?? -1;
        var contentStream = await response.Content.ReadAsStreamAsync(ct);

        if (progress is null || totalBytes < 0)
        {
            return contentStream;
        }

        return new ProgressTrackingStream(contentStream, totalBytes, progress);
    }
}

/// <summary>
/// A wrapping stream that reports read progress via <see cref="IProgress{T}"/>.
/// Throttles progress reports to at most every 100ms to avoid overwhelming the consumer.
/// </summary>
internal sealed class ProgressTrackingStream : Stream
{
    private readonly Stream _inner;
    private readonly long _totalBytes;
    private readonly IProgress<DownloadProgress> _progress;
    private long _bytesRead;
    private DateTime _lastReport = DateTime.MinValue;

    public ProgressTrackingStream(
        Stream inner,
        long totalBytes,
        IProgress<DownloadProgress> progress)
    {
        _inner = inner;
        _totalBytes = totalBytes;
        _progress = progress;
    }

    /// <inheritdoc />
    public override async Task<int> ReadAsync(
        byte[] buffer,
        int offset,
        int count,
        CancellationToken cancellationToken)
    {
        var read = await _inner.ReadAsync(buffer.AsMemory(offset, count), cancellationToken);
        ReportProgress(read);
        return read;
    }

    /// <inheritdoc />
    public override int Read(byte[] buffer, int offset, int count)
    {
        var read = _inner.Read(buffer, offset, count);
        ReportProgress(read);
        return read;
    }

    private void ReportProgress(int bytes)
    {
        _bytesRead += bytes;
        var now = DateTime.UtcNow;

        // Report at most every 100ms, but always report on completion
        if ((now - _lastReport).TotalMilliseconds >= 100 || _bytesRead >= _totalBytes)
        {
            _lastReport = now;
            _progress.Report(new DownloadProgress
            {
                BytesDownloaded = _bytesRead,
                TotalBytes = _totalBytes
            });
        }
    }

    /// <inheritdoc />
    public override bool CanRead => _inner.CanRead;

    /// <inheritdoc />
    public override bool CanSeek => false;

    /// <inheritdoc />
    public override bool CanWrite => false;

    /// <inheritdoc />
    public override long Length => throw new NotSupportedException();

    /// <inheritdoc />
    public override long Position
    {
        get => _bytesRead;
        set => throw new NotSupportedException();
    }

    /// <inheritdoc />
    public override void Flush() => _inner.Flush();

    /// <inheritdoc />
    public override long Seek(long offset, SeekOrigin origin) =>
        throw new NotSupportedException();

    /// <inheritdoc />
    public override void SetLength(long value) =>
        throw new NotSupportedException();

    /// <inheritdoc />
    public override void Write(byte[] buffer, int offset, int count) =>
        throw new NotSupportedException();
}
