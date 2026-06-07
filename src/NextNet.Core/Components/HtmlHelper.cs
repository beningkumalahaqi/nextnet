using System.Text;
using System.Text.Encodings.Web;

namespace NextNet.Components;

/// <summary>
/// Provides static helper methods for building HTML content programmatically.
/// </summary>
public static class HtmlHelper
{
    /// <summary>
    /// Creates an HTML element with optional attributes and content.
    /// Self-closing tags are used when no content is provided.
    /// </summary>
    /// <param name="tagName">The HTML tag name (e.g. "div", "span", "img").</param>
    /// <param name="attributes">Optional dictionary of attribute key-value pairs.</param>
    /// <param name="content">Optional inner HTML content.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the element.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tagName"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="tagName"/> is empty or whitespace.</exception>
    public static IHtmlContent Element(
        string tagName,
        IReadOnlyDictionary<string, string>? attributes = null,
        IHtmlContent? content = null)
    {
        if (tagName == null) throw new ArgumentNullException(nameof(tagName));
        if (string.IsNullOrWhiteSpace(tagName))
            throw new ArgumentException("Tag name cannot be empty.", nameof(tagName));

        var sb = new StringBuilder();
        sb.Append('<');
        sb.Append(tagName);

        if (attributes != null)
        {
            foreach (var attr in attributes)
            {
                sb.Append(' ');
                sb.Append(HtmlEncoder.Default.Encode(attr.Key));
                sb.Append("=\"");
                sb.Append(HtmlEncoder.Default.Encode(attr.Value));
                sb.Append('"');
            }
        }

        if (content != null)
        {
            sb.Append('>');
            sb.Append(content.ToHtml());
            sb.Append("</");
            sb.Append(tagName);
            sb.Append('>');
        }
        else
        {
            sb.Append(" />");
        }

        return new RawHtmlContent(sb.ToString());
    }

    /// <summary>
    /// Creates an HTML text node with HTML-encoded content.
    /// </summary>
    /// <param name="text">The text content to encode.</param>
    /// <returns>An <see cref="IHtmlContent"/> with the encoded text.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="text"/> is <c>null</c>.</exception>
    public static IHtmlContent Text(string text)
    {
        if (text == null) throw new ArgumentNullException(nameof(text));
        return new RawHtmlContent(HtmlEncoder.Default.Encode(text));
    }

    /// <summary>
    /// Creates HTML content from a raw (unencoded) HTML string.
    /// Use with caution; ensure the input is trusted or properly sanitized.
    /// </summary>
    /// <param name="html">The raw HTML string.</param>
    /// <returns>An <see cref="IHtmlContent"/> that renders the raw HTML.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="html"/> is <c>null</c>.</exception>
    public static IHtmlContent Raw(string html)
    {
        if (html == null) throw new ArgumentNullException(nameof(html));
        return new RawHtmlContent(html);
    }

    /// <summary>
    /// Combines multiple HTML content items into a single fragment.
    /// </summary>
    /// <param name="contents">The HTML content items to combine.</param>
    /// <returns>An <see cref="IHtmlContent"/> that renders all items sequentially.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="contents"/> is <c>null</c>.</exception>
    public static IHtmlContent Fragment(params IHtmlContent[] contents)
    {
        if (contents == null) throw new ArgumentNullException(nameof(contents));

        var sb = new StringBuilder();
        foreach (var content in contents)
        {
            sb.Append(content.ToHtml());
        }
        return new RawHtmlContent(sb.ToString());
    }

    /// <summary>
    /// Creates a DOCTYPE declaration.
    /// </summary>
    /// <param name="type">The document type (defaults to <c>"html"</c>).</param>
    /// <returns>An <see cref="IHtmlContent"/> with the DOCTYPE declaration.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is <c>null</c>.</exception>
    public static IHtmlContent DocType(string type = "html")
    {
        if (type == null) throw new ArgumentNullException(nameof(type));
        return new RawHtmlContent($"<!DOCTYPE {type}>");
    }

    /// <summary>
    /// Creates a <c>&lt;link&gt;</c> element for stylesheet references.
    /// </summary>
    /// <param name="href">The stylesheet URL.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the link element.</returns>
    public static IHtmlContent Stylesheet(string href)
    {
        return Element("link", new Dictionary<string, string>
        {
            ["rel"] = "stylesheet",
            ["href"] = href,
        });
    }

    /// <summary>
    /// Creates a <c>&lt;script&gt;</c> element with a source URL.
    /// </summary>
    /// <param name="src">The script source URL.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the script element.</returns>
    public static IHtmlContent Script(string src)
    {
        return Element("script", new Dictionary<string, string>
        {
            ["src"] = src,
        });
    }
}
