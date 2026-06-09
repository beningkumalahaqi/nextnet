using System.Text;
using NextNet.Components;
using NextNet.UI.Rendering.Head;

namespace NextNet.UI.Rendering.Layouts;

/// <summary>
/// A theme-aware layout implementation that wraps child content with a
/// configurable shell including header, footer, and theme styling.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="UiLayout"/> implements <see cref="ILayout"/> and provides a
/// complete HTML shell with:
/// </para>
/// <list type="bullet">
///   <item><description>HTML document structure (<c>&lt;!DOCTYPE html&gt;</c>, <c>&lt;html&gt;</c>, <c>&lt;head&gt;</c>, <c>&lt;body&gt;</c>)</description></item>
///   <item><description>Theme CSS injection via <see cref="ThemeHeadInjector"/></description></item>
///   <item><description>Optional footer section</description></item>
/// </list>
/// <para>
/// When used in a layout chain, this layout wraps the page content with a
/// shared theme shell. The <see cref="Title"/> and <see cref="ThemeName"/>
/// properties control the appearance.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var layout = new UiLayout
/// {
///     Title = "My App",
///     ThemeName = "dark",
///     ShowFooter = true
/// };
/// var result = await layout.Render(pageContent);
/// </code>
/// </example>
public class UiLayout : ILayout
{
    /// <summary>
    /// Gets or sets the page title rendered in the <c>&lt;title&gt;</c> tag.
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// Gets or sets the theme name used for styling this layout.
    /// If null, the default theme is used.
    /// </summary>
    public string? ThemeName { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether a footer section should be rendered.
    /// </summary>
    public bool ShowFooter { get; init; }

    /// <summary>
    /// Gets or sets optional footer HTML content. If not set and <see cref="ShowFooter"/>
    /// is <c>true</c>, a default footer is rendered.
    /// </summary>
    public string? FooterContent { get; init; }

    /// <inheritdoc />
    public Task<IHtmlContent> Render(IHtmlContent children)
    {
        if (children == null) throw new ArgumentNullException(nameof(children));

        var injector = new ThemeHeadInjector();
        var themeStyle = injector.Inject(ThemeName);

        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang=\"en\">");
        sb.AppendLine("<head>");
        sb.AppendLine("<meta charset=\"utf-8\" />");
        sb.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\" />");

        if (!string.IsNullOrEmpty(Title))
        {
            sb.Append("<title>").Append(System.Net.WebUtility.HtmlEncode(Title)).AppendLine("</title>");
        }

        if (themeStyle != null)
        {
            sb.AppendLine(themeStyle.ToHtml());
        }

        sb.AppendLine("</head>");
        sb.AppendLine("<body>");

        // Render children
        sb.AppendLine(children.ToHtml());

        // Render footer
        if (ShowFooter)
        {
            sb.AppendLine("<footer style=\"padding: 1rem; text-align: center; font-size: 0.875rem;\">");
            if (!string.IsNullOrEmpty(FooterContent))
            {
                sb.AppendLine(FooterContent);
            }
            else
            {
                sb.AppendLine("<p>&copy; " + DateTime.Now.Year + " NextNet Application</p>");
            }
            sb.AppendLine("</footer>");
        }

        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return Task.FromResult<IHtmlContent>(new RawHtmlContent(sb.ToString()));
    }

    /// <inheritdoc />
    public Task<IHtmlContent> RenderShell()
    {
        var injector = new ThemeHeadInjector();
        var themeStyle = injector.Inject(ThemeName);

        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang=\"en\">");
        sb.AppendLine("<head>");
        sb.AppendLine("<meta charset=\"utf-8\" />");
        sb.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\" />");

        if (!string.IsNullOrEmpty(Title))
        {
            sb.Append("<title>").Append(System.Net.WebUtility.HtmlEncode(Title)).AppendLine("</title>");
        }

        if (themeStyle != null)
        {
            sb.AppendLine(themeStyle.ToHtml());
        }

        sb.AppendLine("</head>");
        sb.AppendLine("<body>");

        return Task.FromResult<IHtmlContent>(new RawHtmlContent(sb.ToString()));
    }

    /// <inheritdoc />
    public Task<IHtmlContent> RenderFooter()
    {
        if (!ShowFooter) return Task.FromResult<IHtmlContent>(new RawHtmlContent(""));

        var sb = new StringBuilder();
        sb.AppendLine("<footer style=\"padding: 1rem; text-align: center; font-size: 0.875rem;\">");
        if (!string.IsNullOrEmpty(FooterContent))
        {
            sb.AppendLine(FooterContent);
        }
        else
        {
            sb.AppendLine("<p>&copy; " + DateTime.Now.Year + " NextNet Application</p>");
        }
        sb.AppendLine("</footer>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return Task.FromResult<IHtmlContent>(new RawHtmlContent(sb.ToString()));
    }
}
