using System.Text;
using System.Text.Encodings.Web;

namespace NextNet.UI.Rendering.Head;

/// <summary>
/// Accumulates <c>&lt;meta&gt;</c>, <c>&lt;link&gt;</c>, <c>&lt;style&gt;</c>,
/// and <c>&lt;script&gt;</c> elements for injection into the <c>&lt;head&gt;</c>
/// section of an HTML page.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="HeadContent"/> provides a mutable builder for collecting head elements
/// from multiple sources (theme, SEO, page metadata). Call <see cref="Render"/> to
/// produce the combined HTML string for the head section.
/// </para>
/// <para>
/// Elements are rendered in the order they are added: meta tags first, then links,
/// then the title, then styles and scripts.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var head = new HeadContent()
///     .AddMeta("description", "A NextNet page")
///     .AddMeta("og:title", "Hello World")
///     .AddLink("&lt;link rel=\"stylesheet\" href=\"/styles.css\" /&gt;")
///     .AddTitle("My Page")
///     .AddStyle("body { font-family: sans-serif; }");
/// var html = head.Render();
/// </code>
/// </example>
public sealed class HeadContent
{
    private readonly List<(string name, string content)> _metaTags = new();
    private readonly List<string> _links = new();
    private readonly List<string> _styles = new();
    private readonly List<string> _scripts = new();
    private string? _title;

    /// <summary>
    /// Gets the accumulated meta tags as a read-only list of (name, content) tuples.
    /// </summary>
    public IReadOnlyList<(string name, string content)> MetaTags => _metaTags.AsReadOnly();

    /// <summary>
    /// Gets the accumulated link elements as a read-only list of strings.
    /// </summary>
    public IReadOnlyList<string> Links => _links.AsReadOnly();

    /// <summary>
    /// Gets the accumulated style content as a read-only list of strings.
    /// </summary>
    public IReadOnlyList<string> Styles => _styles.AsReadOnly();

    /// <summary>
    /// Gets the accumulated script content as a read-only list of strings.
    /// </summary>
    public IReadOnlyList<string> Scripts => _scripts.AsReadOnly();

    /// <summary>
    /// Gets the title string, or <c>null</c> if not set.
    /// </summary>
    public string? Title => _title;

    /// <summary>
    /// Adds a <c>&lt;meta&gt;</c> tag with the specified name and content attributes.
    /// </summary>
    /// <param name="name">The value of the <c>name</c> attribute (e.g., "description", "og:title").</param>
    /// <param name="content">The value of the <c>content</c> attribute.</param>
    /// <returns>This instance for fluent chaining.</returns>
    public HeadContent AddMeta(string name, string content)
    {
        _metaTags.Add((name, content));
        return this;
    }

    /// <summary>
    /// Adds a <c>&lt;link&gt;</c> element as a raw HTML string.
    /// </summary>
    /// <param name="linkHtml">The complete <c>&lt;link&gt;</c> HTML element string.</param>
    /// <returns>This instance for fluent chaining.</returns>
    public HeadContent AddLink(string linkHtml)
    {
        _links.Add(linkHtml);
        return this;
    }

    /// <summary>
    /// Sets the page title rendered as a <c>&lt;title&gt;</c> element.
    /// </summary>
    /// <param name="title">The page title text. HTML-encoded automatically.</param>
    /// <returns>This instance for fluent chaining.</returns>
    public HeadContent AddTitle(string title)
    {
        _title = title;
        return this;
    }

    /// <summary>
    /// Adds raw CSS content wrapped in a <c>&lt;style&gt;</c> tag.
    /// </summary>
    /// <param name="cssContent">The CSS content to wrap in a <c>&lt;style&gt;</c> tag.</param>
    /// <returns>This instance for fluent chaining.</returns>
    public HeadContent AddStyle(string cssContent)
    {
        _styles.Add(cssContent);
        return this;
    }

    /// <summary>
    /// Adds raw JavaScript content wrapped in a <c>&lt;script&gt;</c> tag.
    /// </summary>
    /// <param name="scriptContent">The JavaScript content to wrap in a <c>&lt;script&gt;</c> tag.</param>
    /// <returns>This instance for fluent chaining.</returns>
    public HeadContent AddScript(string scriptContent)
    {
        _scripts.Add(scriptContent);
        return this;
    }

    /// <summary>
    /// Renders the accumulated head elements as an HTML string.
    /// </summary>
    /// <returns>A string containing all head elements, each on a separate line.</returns>
    public string Render()
    {
        var sb = new StringBuilder();

        // Render meta tags
        foreach (var (name, content) in _metaTags)
        {
            sb.Append("<meta name=\"")
              .Append(HtmlEncoder.Default.Encode(name))
              .Append("\" content=\"")
              .Append(HtmlEncoder.Default.Encode(content))
              .AppendLine("\" />");
        }

        // Render link elements
        foreach (var link in _links)
        {
            sb.AppendLine(link);
        }

        // Render title
        if (!string.IsNullOrEmpty(_title))
        {
            sb.Append("<title>")
              .Append(HtmlEncoder.Default.Encode(_title))
              .AppendLine("</title>");
        }

        // Render styles
        foreach (var style in _styles)
        {
            // If the style content already contains <style> tags, append as-is
            if (style.Contains("<style", StringComparison.OrdinalIgnoreCase))
            {
                sb.AppendLine(style);
            }
            else
            {
                sb.Append("<style>").AppendLine(style).AppendLine("</style>");
            }
        }

        // Render scripts
        foreach (var script in _scripts)
        {
            if (script.Contains("<script", StringComparison.OrdinalIgnoreCase))
            {
                sb.AppendLine(script);
            }
            else
            {
                sb.Append("<script>").AppendLine(script).AppendLine("</script>");
            }
        }

        return sb.ToString();
    }
}
