namespace NextNet.Components;

/// <summary>
/// Defines a page component that can render itself to HTML.
/// Pages are the top-level routeable components in a NextNet application.
/// </summary>
public interface IPage
{
    /// <summary>
    /// Renders the page to HTML content.
    /// </summary>
    /// <returns>A task representing the asynchronous render operation, with the HTML content.</returns>
    Task<IHtmlContent> Render();

    /// <summary>
    /// Gets a read-only dictionary of properties or parameters for this page.
    /// These may include route parameters, query string values, or custom metadata.
    /// </summary>
    IReadOnlyDictionary<string, object> Props { get; }
}
