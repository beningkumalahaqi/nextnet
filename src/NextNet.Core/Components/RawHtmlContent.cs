namespace NextNet.Components;

/// <summary>
/// Wraps a raw HTML string as <see cref="IHtmlContent"/>.
/// The content is written as-is without any encoding.
/// </summary>
public class RawHtmlContent : IHtmlContent
{
    /// <summary>
    /// Gets the raw HTML content string.
    /// </summary>
    public string Content { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="RawHtmlContent"/> with the specified HTML content.
    /// </summary>
    /// <param name="content">The raw HTML content. Must not be <c>null</c>.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="content"/> is <c>null</c>.</exception>
    public RawHtmlContent(string content)
    {
        Content = content ?? throw new ArgumentNullException(nameof(content));
    }

    /// <summary>
    /// Writes the raw HTML content to the specified <paramref name="writer"/> asynchronously.
    /// </summary>
    /// <param name="writer">The text writer to write to.</param>
    /// <returns>A task representing the asynchronous write operation.</returns>
    public Task WriteToAsync(TextWriter writer)
    {
        return writer.WriteAsync(Content);
    }

    /// <summary>
    /// Returns the raw HTML content as a string.
    /// </summary>
    /// <returns>The HTML string.</returns>
    public string ToHtml() => Content;

    /// <summary>
    /// Returns the raw HTML content as a string.
    /// </summary>
    public override string ToString() => Content;
}
