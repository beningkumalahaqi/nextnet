using System.Runtime.CompilerServices;
using NextNet.Components;

namespace NextNet.Rendering.Streaming;

/// <summary>
/// An <see cref="IHtmlContent"/> wrapper around an <see cref="IAsyncEnumerable{T}"/> of string chunks.
/// Enables streaming HTML content to be used where <see cref="IHtmlContent"/> is expected.
/// </summary>
public class ChunkedHtmlWriter : IHtmlContent
{
    private readonly IAsyncEnumerable<string> _chunks;
    private readonly int _bufferSize;

    /// <summary>
    /// Initializes a new instance of <see cref="ChunkedHtmlWriter"/>.
    /// </summary>
    /// <param name="chunks">The async enumerable of HTML string chunks.</param>
    /// <param name="bufferSize">The buffer size hint for writing.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="chunks"/> is null.</exception>
    public ChunkedHtmlWriter(IAsyncEnumerable<string> chunks, int bufferSize = 8192)
    {
        _chunks = chunks ?? throw new ArgumentNullException(nameof(chunks));
        _bufferSize = bufferSize;
    }

    /// <inheritdoc />
    public async Task WriteToAsync(TextWriter writer)
    {
        if (writer == null) throw new ArgumentNullException(nameof(writer));

        await foreach (var chunk in _chunks)
        {
            await writer.WriteAsync(chunk);
        }
    }

    /// <inheritdoc />
    /// <remarks>
    /// For streaming content, <see cref="ToHtml"/> may block until all chunks are consumed.
    /// Prefer <see cref="WriteToAsync"/> for non-blocking usage.
    /// </remarks>
    public string ToHtml()
    {
        // This is inherently synchronous; for streaming scenarios, use WriteToAsync instead.
        var sb = new System.Text.StringBuilder();
        var enumerator = _chunks.GetAsyncEnumerator();
        try
        {
            while (enumerator.MoveNextAsync().AsTask().GetAwaiter().GetResult())
            {
                sb.Append(enumerator.Current);
            }
        }
        finally
        {
            enumerator.DisposeAsync().AsTask().GetAwaiter().GetResult();
        }
        return sb.ToString();
    }
}
