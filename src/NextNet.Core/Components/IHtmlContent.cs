namespace NextNet.Components;

/// <summary>
/// Represents HTML content that can be written asynchronously to a <see cref="TextWriter"/>
/// or rendered to a string via <see cref="ToHtml"/>.
/// This is a framework-level abstraction for representing HTML output.
/// </summary>
public interface IHtmlContent
{
    /// <summary>
    /// Writes the HTML content to the specified <paramref name="writer"/> asynchronously.
    /// </summary>
    /// <param name="writer">The text writer to write to.</param>
    /// <returns>A task representing the asynchronous write operation.</returns>
    Task WriteToAsync(TextWriter writer);

    /// <summary>
    /// Returns the HTML content as a string.
    /// </summary>
    /// <returns>A string containing the HTML output.</returns>
    string ToHtml();
}
