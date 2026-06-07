namespace NextNet.Components;

/// <summary>
/// Defines a layout component that wraps child content with a shared shell (e.g. header, footer, navigation).
/// Layouts can be nested to create complex page structures.
/// </summary>
public interface ILayout
{
    /// <summary>
    /// Renders the layout wrapping the provided <paramref name="children"/> content.
    /// </summary>
    /// <param name="children">The inner content to render inside the layout.</param>
    /// <returns>A task representing the asynchronous render operation, with the combined HTML content.</returns>
    Task<IHtmlContent> Render(IHtmlContent children);

    /// <summary>
    /// Renders the opening/shell portion of the layout (doctype, html open, head, header open, etc.).
    /// Used by the streaming renderer to yield content progressively before the page content is ready.
    /// Default implementation returns empty content — override to enable progressive streaming.
    /// </summary>
    /// <returns>A task representing the asynchronous render operation, with the shell HTML content.</returns>
    Task<IHtmlContent> RenderShell() => Task.FromResult<IHtmlContent>(new RawHtmlContent(""));

    /// <summary>
    /// Renders the closing/footer portion of the layout (footer, scripts, closing tags, etc.).
    /// Used by the streaming renderer to yield content after the page content has been flushed.
    /// Default implementation returns empty content — override to enable progressive streaming.
    /// </summary>
    /// <returns>A task representing the asynchronous render operation, with the footer HTML content.</returns>
    Task<IHtmlContent> RenderFooter() => Task.FromResult<IHtmlContent>(new RawHtmlContent(""));
}
