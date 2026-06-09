namespace NextNet.Components;

/// <summary>
/// Represents HTML content that can be written asynchronously to a <see cref="TextWriter"/>
/// or rendered to a string via <see cref="ToHtml"/>.
/// This is a framework-level abstraction for representing HTML output.
/// </summary>
/// <example>
/// <code>
/// // Implementing custom HTML content:
/// public class AlertContent : IHtmlContent
/// {
///     private readonly string _message;
///
///     public AlertContent(string message) => _message = message;
///
///     public Task WriteToAsync(TextWriter writer)
///         => writer.WriteAsync($"&lt;div class=\"alert\"&gt;{_message}&lt;/div&gt;");
///
///     public string ToHtml() =&gt; $"&lt;div class=\"alert\"&gt;{_message}&lt;/div&gt;";
/// }
/// </code>
/// </example>
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
