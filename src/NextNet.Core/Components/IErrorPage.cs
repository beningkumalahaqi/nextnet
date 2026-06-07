namespace NextNet.Components;

/// <summary>
/// Defines a page that renders when an error occurs during request processing.
/// Error pages are discovered by convention (e.g. <c>app/error.cs</c>) and
/// can be scoped to specific route segments.
/// </summary>
public interface IErrorPage
{
    /// <summary>
    /// Renders the error page with information about the exception.
    /// </summary>
    /// <param name="exception">The exception that caused the error.</param>
    /// <returns>A task representing the asynchronous render operation, with the HTML content.</returns>
    Task<IHtmlContent> Render(Exception exception);
}
