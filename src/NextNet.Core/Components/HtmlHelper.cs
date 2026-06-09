using System.Text;
using System.Text.Encodings.Web;

namespace NextNet.Components;

/// <summary>
/// Provides static helper methods for building HTML content programmatically.
/// </summary>
/// <example>
/// <code>
/// // Build an anchor element with attributes and encoded text content:
/// var link = HtmlHelper.Element("a",
///     new Dictionary&lt;string, string&gt; { ["href"] = "https://example.com" },
///     HtmlHelper.Text("Click here"));
///
/// // Use raw HTML sparingly (only with trusted input):
/// var raw = HtmlHelper.Raw("&lt;em&gt;emphasis&lt;/em&gt;");
/// </code>
/// </example>
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

    // =============================================================
    // Semantic HTML Element Helpers
    // =============================================================

    /// <summary>
    /// Creates an H1 heading element.
    /// </summary>
    /// <param name="content">The content inside the heading.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the H1 element.</returns>
    /// <example>
    /// <code>
    /// var h = HtmlHelper.H1(HtmlHelper.Text("Hello"));
    /// // Renders: &lt;h1&gt;Hello&lt;/h1&gt;
    /// </code>
    /// </example>
    public static IHtmlContent H1(params IHtmlContent[] content)
        => Element("h1", content: Combine(content));

    /// <summary>
    /// Creates an H1 heading element with custom attributes.
    /// </summary>
    /// <param name="attributes">Optional HTML attributes.</param>
    /// <param name="content">The content inside the heading.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the H1 element.</returns>
    /// <example>
    /// <code>
    /// var h = HtmlHelper.H1(new() { ["class"] = "title" }, HtmlHelper.Text("Hello"));
    /// // Renders: &lt;h1 class="title"&gt;Hello&lt;/h1&gt;
    /// </code>
    /// </example>
    public static IHtmlContent H1(Dictionary<string, string> attributes, params IHtmlContent[] content)
        => Element("h1", attributes, content: Combine(content));

    /// <summary>
    /// Creates an H2 heading element.
    /// </summary>
    /// <param name="content">The content inside the heading.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the H2 element.</returns>
    /// <example>
    /// <code>
    /// var h = HtmlHelper.H2(HtmlHelper.Text("Subtitle"));
    /// // Renders: &lt;h2&gt;Subtitle&lt;/h2&gt;
    /// </code>
    /// </example>
    public static IHtmlContent H2(params IHtmlContent[] content)
        => Element("h2", content: Combine(content));

    /// <summary>
    /// Creates an H2 heading element with custom attributes.
    /// </summary>
    /// <param name="attributes">Optional HTML attributes.</param>
    /// <param name="content">The content inside the heading.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the H2 element.</returns>
    public static IHtmlContent H2(Dictionary<string, string> attributes, params IHtmlContent[] content)
        => Element("h2", attributes, content: Combine(content));

    /// <summary>
    /// Creates an H3 heading element.
    /// </summary>
    /// <param name="content">The content inside the heading.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the H3 element.</returns>
    public static IHtmlContent H3(params IHtmlContent[] content)
        => Element("h3", content: Combine(content));

    /// <summary>
    /// Creates an H3 heading element with custom attributes.
    /// </summary>
    /// <param name="attributes">Optional HTML attributes.</param>
    /// <param name="content">The content inside the heading.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the H3 element.</returns>
    public static IHtmlContent H3(Dictionary<string, string> attributes, params IHtmlContent[] content)
        => Element("h3", attributes, content: Combine(content));

    /// <summary>
    /// Creates an H4 heading element.
    /// </summary>
    /// <param name="content">The content inside the heading.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the H4 element.</returns>
    public static IHtmlContent H4(params IHtmlContent[] content)
        => Element("h4", content: Combine(content));

    /// <summary>
    /// Creates an H4 heading element with custom attributes.
    /// </summary>
    /// <param name="attributes">Optional HTML attributes.</param>
    /// <param name="content">The content inside the heading.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the H4 element.</returns>
    public static IHtmlContent H4(Dictionary<string, string> attributes, params IHtmlContent[] content)
        => Element("h4", attributes, content: Combine(content));

    /// <summary>
    /// Creates an H5 heading element.
    /// </summary>
    /// <param name="content">The content inside the heading.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the H5 element.</returns>
    public static IHtmlContent H5(params IHtmlContent[] content)
        => Element("h5", content: Combine(content));

    /// <summary>
    /// Creates an H5 heading element with custom attributes.
    /// </summary>
    /// <param name="attributes">Optional HTML attributes.</param>
    /// <param name="content">The content inside the heading.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the H5 element.</returns>
    public static IHtmlContent H5(Dictionary<string, string> attributes, params IHtmlContent[] content)
        => Element("h5", attributes, content: Combine(content));

    /// <summary>
    /// Creates an H6 heading element.
    /// </summary>
    /// <param name="content">The content inside the heading.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the H6 element.</returns>
    public static IHtmlContent H6(params IHtmlContent[] content)
        => Element("h6", content: Combine(content));

    /// <summary>
    /// Creates an H6 heading element with custom attributes.
    /// </summary>
    /// <param name="attributes">Optional HTML attributes.</param>
    /// <param name="content">The content inside the heading.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the H6 element.</returns>
    public static IHtmlContent H6(Dictionary<string, string> attributes, params IHtmlContent[] content)
        => Element("h6", attributes, content: Combine(content));

    /// <summary>
    /// Creates a paragraph element.
    /// </summary>
    /// <param name="content">The content inside the paragraph.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the paragraph element.</returns>
    /// <example>
    /// <code>
    /// var p = HtmlHelper.P(HtmlHelper.Text("Some text"));
    /// // Renders: &lt;p&gt;Some text&lt;/p&gt;
    /// </code>
    /// </example>
    public static IHtmlContent P(params IHtmlContent[] content)
        => Element("p", content: Combine(content));

    /// <summary>
    /// Creates a paragraph element with custom attributes.
    /// </summary>
    /// <param name="attributes">Optional HTML attributes.</param>
    /// <param name="content">The content inside the paragraph.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the paragraph element.</returns>
    public static IHtmlContent P(Dictionary<string, string> attributes, params IHtmlContent[] content)
        => Element("p", attributes, content: Combine(content));

    /// <summary>
    /// Creates a div element.
    /// </summary>
    /// <param name="content">The content inside the div.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the div element.</returns>
    /// <example>
    /// <code>
    /// var div = HtmlHelper.Div(HtmlHelper.P(HtmlHelper.Text("Hello")));
    /// // Renders: &lt;div&gt;&lt;p&gt;Hello&lt;/p&gt;&lt;/div&gt;
    /// </code>
    /// </example>
    public static IHtmlContent Div(params IHtmlContent[] content)
        => Element("div", content: Combine(content));

    /// <summary>
    /// Creates a div element with custom attributes.
    /// </summary>
    /// <param name="attributes">Optional HTML attributes.</param>
    /// <param name="content">The content inside the div.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the div element.</returns>
    public static IHtmlContent Div(Dictionary<string, string> attributes, params IHtmlContent[] content)
        => Element("div", attributes, content: Combine(content));

    /// <summary>
    /// Creates a span element.
    /// </summary>
    /// <param name="content">The content inside the span.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the span element.</returns>
    /// <example>
    /// <code>
    /// var span = HtmlHelper.Span(HtmlHelper.Text("Inline text"));
    /// // Renders: &lt;span&gt;Inline text&lt;/span&gt;
    /// </code>
    /// </example>
    public static IHtmlContent Span(params IHtmlContent[] content)
        => Element("span", content: Combine(content));

    /// <summary>
    /// Creates a span element with custom attributes.
    /// </summary>
    /// <param name="attributes">Optional HTML attributes.</param>
    /// <param name="content">The content inside the span.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the span element.</returns>
    public static IHtmlContent Span(Dictionary<string, string> attributes, params IHtmlContent[] content)
        => Element("span", attributes, content: Combine(content));

    /// <summary>
    /// Creates an unordered list (ul) element.
    /// </summary>
    /// <param name="content">The list items inside the unordered list.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the ul element.</returns>
    /// <example>
    /// <code>
    /// var ul = HtmlHelper.Ul(HtmlHelper.Li(HtmlHelper.Text("Item 1")), HtmlHelper.Li(HtmlHelper.Text("Item 2")));
    /// // Renders: &lt;ul&gt;&lt;li&gt;Item 1&lt;/li&gt;&lt;li&gt;Item 2&lt;/li&gt;&lt;/ul&gt;
    /// </code>
    /// </example>
    public static IHtmlContent Ul(params IHtmlContent[] content)
        => Element("ul", content: Combine(content));

    /// <summary>
    /// Creates an unordered list (ul) element with custom attributes.
    /// </summary>
    /// <param name="attributes">Optional HTML attributes.</param>
    /// <param name="content">The list items inside the unordered list.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the ul element.</returns>
    public static IHtmlContent Ul(Dictionary<string, string> attributes, params IHtmlContent[] content)
        => Element("ul", attributes, content: Combine(content));

    /// <summary>
    /// Creates an ordered list (ol) element.
    /// </summary>
    /// <param name="content">The list items inside the ordered list.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the ol element.</returns>
    /// <example>
    /// <code>
    /// var ol = HtmlHelper.Ol(HtmlHelper.Li(HtmlHelper.Text("First")), HtmlHelper.Li(HtmlHelper.Text("Second")));
    /// // Renders: &lt;ol&gt;&lt;li&gt;First&lt;/li&gt;&lt;li&gt;Second&lt;/li&gt;&lt;/ol&gt;
    /// </code>
    /// </example>
    public static IHtmlContent Ol(params IHtmlContent[] content)
        => Element("ol", content: Combine(content));

    /// <summary>
    /// Creates an ordered list (ol) element with custom attributes.
    /// </summary>
    /// <param name="attributes">Optional HTML attributes.</param>
    /// <param name="content">The list items inside the ordered list.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the ol element.</returns>
    public static IHtmlContent Ol(Dictionary<string, string> attributes, params IHtmlContent[] content)
        => Element("ol", attributes, content: Combine(content));

    /// <summary>
    /// Creates a list item (li) element.
    /// </summary>
    /// <param name="content">The content inside the list item.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the li element.</returns>
    /// <example>
    /// <code>
    /// var li = HtmlHelper.Li(HtmlHelper.Text("Item"));
    /// // Renders: &lt;li&gt;Item&lt;/li&gt;
    /// </code>
    /// </example>
    public static IHtmlContent Li(params IHtmlContent[] content)
        => Element("li", content: Combine(content));

    /// <summary>
    /// Creates a list item (li) element with custom attributes.
    /// </summary>
    /// <param name="attributes">Optional HTML attributes.</param>
    /// <param name="content">The content inside the list item.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the li element.</returns>
    public static IHtmlContent Li(Dictionary<string, string> attributes, params IHtmlContent[] content)
        => Element("li", attributes, content: Combine(content));

    /// <summary>
    /// Creates an anchor (a) element.
    /// </summary>
    /// <param name="content">The content inside the anchor.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the a element.</returns>
    /// <example>
    /// <code>
    /// var a = HtmlHelper.A(new() { ["href"] = "https://example.com" }, HtmlHelper.Text("Click here"));
    /// // Renders: &lt;a href="https://example.com"&gt;Click here&lt;/a&gt;
    /// </code>
    /// </example>
    public static IHtmlContent A(params IHtmlContent[] content)
        => Element("a", content: Combine(content));

    /// <summary>
    /// Creates an anchor (a) element with custom attributes.
    /// </summary>
    /// <param name="attributes">Optional HTML attributes (e.g. href, target).</param>
    /// <param name="content">The content inside the anchor.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the a element.</returns>
    public static IHtmlContent A(Dictionary<string, string> attributes, params IHtmlContent[] content)
        => Element("a", attributes, content: Combine(content));

    /// <summary>
    /// Creates an image (img) element. This is a void element and renders as self-closing.
    /// </summary>
    /// <param name="attributes">Attributes for the img element (e.g. src, alt).</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the img element.</returns>
    /// <example>
    /// <code>
    /// var img = HtmlHelper.Img(new() { ["src"] = "/logo.png", ["alt"] = "Logo" });
    /// // Renders: &lt;img src="/logo.png" alt="Logo" /&gt;
    /// </code>
    /// </example>
    public static IHtmlContent Img(Dictionary<string, string> attributes)
        => Element("img", attributes);

    /// <summary>
    /// Creates an input element. This is a void element and renders as self-closing.
    /// </summary>
    /// <param name="attributes">Attributes for the input element (e.g. type, name, value).</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the input element.</returns>
    /// <example>
    /// <code>
    /// var input = HtmlHelper.Input(new() { ["type"] = "text", ["name"] = "username" });
    /// // Renders: &lt;input type="text" name="username" /&gt;
    /// </code>
    /// </example>
    public static IHtmlContent Input(Dictionary<string, string> attributes)
        => Element("input", attributes);

    /// <summary>
    /// Creates a button element.
    /// </summary>
    /// <param name="content">The content inside the button.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the button element.</returns>
    /// <example>
    /// <code>
    /// var btn = HtmlHelper.Button(new() { ["type"] = "submit" }, HtmlHelper.Text("Submit"));
    /// // Renders: &lt;button type="submit"&gt;Submit&lt;/button&gt;
    /// </code>
    /// </example>
    public static IHtmlContent Button(params IHtmlContent[] content)
        => Element("button", content: Combine(content));

    /// <summary>
    /// Creates a button element with custom attributes.
    /// </summary>
    /// <param name="attributes">Optional HTML attributes (e.g. type, disabled).</param>
    /// <param name="content">The content inside the button.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the button element.</returns>
    public static IHtmlContent Button(Dictionary<string, string> attributes, params IHtmlContent[] content)
        => Element("button", attributes, content: Combine(content));

    /// <summary>
    /// Creates a form element.
    /// </summary>
    /// <param name="content">The content inside the form.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the form element.</returns>
    /// <example>
    /// <code>
    /// var form = HtmlHelper.Form(new() { ["action"] = "/submit", ["method"] = "post" },
    ///     HtmlHelper.Input(new() { ["type"] = "submit" }));
    /// // Renders: &lt;form action="/submit" method="post"&gt;&lt;input type="submit" /&gt;&lt;/form&gt;
    /// </code>
    /// </example>
    public static IHtmlContent Form(params IHtmlContent[] content)
        => Element("form", content: Combine(content));

    /// <summary>
    /// Creates a form element with custom attributes.
    /// </summary>
    /// <param name="attributes">Optional HTML attributes (e.g. action, method).</param>
    /// <param name="content">The content inside the form.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the form element.</returns>
    public static IHtmlContent Form(Dictionary<string, string> attributes, params IHtmlContent[] content)
        => Element("form", attributes, content: Combine(content));

    /// <summary>
    /// Creates a table element.
    /// </summary>
    /// <param name="content">The content inside the table.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the table element.</returns>
    /// <example>
    /// <code>
    /// var table = HtmlHelper.Table(HtmlHelper.Tr(HtmlHelper.Th(HtmlHelper.Text("Name"))));
    /// // Renders: &lt;table&gt;&lt;tr&gt;&lt;th&gt;Name&lt;/th&gt;&lt;/tr&gt;&lt;/table&gt;
    /// </code>
    /// </example>
    public static IHtmlContent Table(params IHtmlContent[] content)
        => Element("table", content: Combine(content));

    /// <summary>
    /// Creates a table element with custom attributes.
    /// </summary>
    /// <param name="attributes">Optional HTML attributes.</param>
    /// <param name="content">The content inside the table.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the table element.</returns>
    public static IHtmlContent Table(Dictionary<string, string> attributes, params IHtmlContent[] content)
        => Element("table", attributes, content: Combine(content));

    /// <summary>
    /// Creates a table head (thead) element.
    /// </summary>
    /// <param name="content">The content inside the thead.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the thead element.</returns>
    public static IHtmlContent Thead(params IHtmlContent[] content)
        => Element("thead", content: Combine(content));

    /// <summary>
    /// Creates a table head (thead) element with custom attributes.
    /// </summary>
    /// <param name="attributes">Optional HTML attributes.</param>
    /// <param name="content">The content inside the thead.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the thead element.</returns>
    public static IHtmlContent Thead(Dictionary<string, string> attributes, params IHtmlContent[] content)
        => Element("thead", attributes, content: Combine(content));

    /// <summary>
    /// Creates a table body (tbody) element.
    /// </summary>
    /// <param name="content">The content inside the tbody.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the tbody element.</returns>
    public static IHtmlContent Tbody(params IHtmlContent[] content)
        => Element("tbody", content: Combine(content));

    /// <summary>
    /// Creates a table body (tbody) element with custom attributes.
    /// </summary>
    /// <param name="attributes">Optional HTML attributes.</param>
    /// <param name="content">The content inside the tbody.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the tbody element.</returns>
    public static IHtmlContent Tbody(Dictionary<string, string> attributes, params IHtmlContent[] content)
        => Element("tbody", attributes, content: Combine(content));

    /// <summary>
    /// Creates a table row (tr) element.
    /// </summary>
    /// <param name="content">The content inside the row.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the tr element.</returns>
    public static IHtmlContent Tr(params IHtmlContent[] content)
        => Element("tr", content: Combine(content));

    /// <summary>
    /// Creates a table row (tr) element with custom attributes.
    /// </summary>
    /// <param name="attributes">Optional HTML attributes.</param>
    /// <param name="content">The content inside the row.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the tr element.</returns>
    public static IHtmlContent Tr(Dictionary<string, string> attributes, params IHtmlContent[] content)
        => Element("tr", attributes, content: Combine(content));

    /// <summary>
    /// Creates a table header cell (th) element.
    /// </summary>
    /// <param name="content">The content inside the header cell.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the th element.</returns>
    public static IHtmlContent Th(params IHtmlContent[] content)
        => Element("th", content: Combine(content));

    /// <summary>
    /// Creates a table header cell (th) element with custom attributes.
    /// </summary>
    /// <param name="attributes">Optional HTML attributes.</param>
    /// <param name="content">The content inside the header cell.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the th element.</returns>
    public static IHtmlContent Th(Dictionary<string, string> attributes, params IHtmlContent[] content)
        => Element("th", attributes, content: Combine(content));

    /// <summary>
    /// Creates a table data cell (td) element.
    /// </summary>
    /// <param name="content">The content inside the data cell.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the td element.</returns>
    public static IHtmlContent Td(params IHtmlContent[] content)
        => Element("td", content: Combine(content));

    /// <summary>
    /// Creates a table data cell (td) element with custom attributes.
    /// </summary>
    /// <param name="attributes">Optional HTML attributes.</param>
    /// <param name="content">The content inside the data cell.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the td element.</returns>
    public static IHtmlContent Td(Dictionary<string, string> attributes, params IHtmlContent[] content)
        => Element("td", attributes, content: Combine(content));

    /// <summary>
    /// Creates a section element.
    /// </summary>
    /// <param name="content">The content inside the section.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the section element.</returns>
    /// <example>
    /// <code>
    /// var section = HtmlHelper.Section(HtmlHelper.H2(HtmlHelper.Text("Section Title")), HtmlHelper.P(HtmlHelper.Text("Content")));
    /// // Renders: &lt;section&gt;&lt;h2&gt;Section Title&lt;/h2&gt;&lt;p&gt;Content&lt;/p&gt;&lt;/section&gt;
    /// </code>
    /// </example>
    public static IHtmlContent Section(params IHtmlContent[] content)
        => Element("section", content: Combine(content));

    /// <summary>
    /// Creates a section element with custom attributes.
    /// </summary>
    /// <param name="attributes">Optional HTML attributes.</param>
    /// <param name="content">The content inside the section.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the section element.</returns>
    public static IHtmlContent Section(Dictionary<string, string> attributes, params IHtmlContent[] content)
        => Element("section", attributes, content: Combine(content));

    /// <summary>
    /// Creates a header element.
    /// </summary>
    /// <param name="content">The content inside the header.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the header element.</returns>
    public static IHtmlContent Header(params IHtmlContent[] content)
        => Element("header", content: Combine(content));

    /// <summary>
    /// Creates a header element with custom attributes.
    /// </summary>
    /// <param name="attributes">Optional HTML attributes.</param>
    /// <param name="content">The content inside the header.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the header element.</returns>
    public static IHtmlContent Header(Dictionary<string, string> attributes, params IHtmlContent[] content)
        => Element("header", attributes, content: Combine(content));

    /// <summary>
    /// Creates a footer element.
    /// </summary>
    /// <param name="content">The content inside the footer.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the footer element.</returns>
    public static IHtmlContent Footer(params IHtmlContent[] content)
        => Element("footer", content: Combine(content));

    /// <summary>
    /// Creates a footer element with custom attributes.
    /// </summary>
    /// <param name="attributes">Optional HTML attributes.</param>
    /// <param name="content">The content inside the footer.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the footer element.</returns>
    public static IHtmlContent Footer(Dictionary<string, string> attributes, params IHtmlContent[] content)
        => Element("footer", attributes, content: Combine(content));

    /// <summary>
    /// Creates a nav element.
    /// </summary>
    /// <param name="content">The content inside the nav.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the nav element.</returns>
    public static IHtmlContent Nav(params IHtmlContent[] content)
        => Element("nav", content: Combine(content));

    /// <summary>
    /// Creates a nav element with custom attributes.
    /// </summary>
    /// <param name="attributes">Optional HTML attributes.</param>
    /// <param name="content">The content inside the nav.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the nav element.</returns>
    public static IHtmlContent Nav(Dictionary<string, string> attributes, params IHtmlContent[] content)
        => Element("nav", attributes, content: Combine(content));

    /// <summary>
    /// Creates a main element.
    /// </summary>
    /// <param name="content">The content inside the main element.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the main element.</returns>
    public static IHtmlContent Main(params IHtmlContent[] content)
        => Element("main", content: Combine(content));

    /// <summary>
    /// Creates a main element with custom attributes.
    /// </summary>
    /// <param name="attributes">Optional HTML attributes.</param>
    /// <param name="content">The content inside the main element.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the main element.</returns>
    public static IHtmlContent Main(Dictionary<string, string> attributes, params IHtmlContent[] content)
        => Element("main", attributes, content: Combine(content));

    /// <summary>
    /// Creates an article element.
    /// </summary>
    /// <param name="content">The content inside the article.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the article element.</returns>
    public static IHtmlContent Article(params IHtmlContent[] content)
        => Element("article", content: Combine(content));

    /// <summary>
    /// Creates an article element with custom attributes.
    /// </summary>
    /// <param name="attributes">Optional HTML attributes.</param>
    /// <param name="content">The content inside the article.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the article element.</returns>
    public static IHtmlContent Article(Dictionary<string, string> attributes, params IHtmlContent[] content)
        => Element("article", attributes, content: Combine(content));

    /// <summary>
    /// Creates an aside element.
    /// </summary>
    /// <param name="content">The content inside the aside.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the aside element.</returns>
    public static IHtmlContent Aside(params IHtmlContent[] content)
        => Element("aside", content: Combine(content));

    /// <summary>
    /// Creates an aside element with custom attributes.
    /// </summary>
    /// <param name="attributes">Optional HTML attributes.</param>
    /// <param name="content">The content inside the aside.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the aside element.</returns>
    public static IHtmlContent Aside(Dictionary<string, string> attributes, params IHtmlContent[] content)
        => Element("aside", attributes, content: Combine(content));

    /// <summary>
    /// Creates a pre (preformatted text) element.
    /// </summary>
    /// <param name="content">The content inside the pre element.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the pre element.</returns>
    public static IHtmlContent Pre(params IHtmlContent[] content)
        => Element("pre", content: Combine(content));

    /// <summary>
    /// Creates a pre (preformatted text) element with custom attributes.
    /// </summary>
    /// <param name="attributes">Optional HTML attributes.</param>
    /// <param name="content">The content inside the pre element.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the pre element.</returns>
    public static IHtmlContent Pre(Dictionary<string, string> attributes, params IHtmlContent[] content)
        => Element("pre", attributes, content: Combine(content));

    /// <summary>
    /// Creates a code element.
    /// </summary>
    /// <param name="content">The content inside the code element.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the code element.</returns>
    public static IHtmlContent Code(params IHtmlContent[] content)
        => Element("code", content: Combine(content));

    /// <summary>
    /// Creates a code element with custom attributes.
    /// </summary>
    /// <param name="attributes">Optional HTML attributes.</param>
    /// <param name="content">The content inside the code element.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the code element.</returns>
    public static IHtmlContent Code(Dictionary<string, string> attributes, params IHtmlContent[] content)
        => Element("code", attributes, content: Combine(content));

    /// <summary>
    /// Creates a blockquote element.
    /// </summary>
    /// <param name="content">The content inside the blockquote.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the blockquote element.</returns>
    public static IHtmlContent Blockquote(params IHtmlContent[] content)
        => Element("blockquote", content: Combine(content));

    /// <summary>
    /// Creates a blockquote element with custom attributes.
    /// </summary>
    /// <param name="attributes">Optional HTML attributes.</param>
    /// <param name="content">The content inside the blockquote.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the blockquote element.</returns>
    public static IHtmlContent Blockquote(Dictionary<string, string> attributes, params IHtmlContent[] content)
        => Element("blockquote", attributes, content: Combine(content));

    /// <summary>
    /// Creates a horizontal rule (hr) element. This is a void element and renders as self-closing.
    /// </summary>
    /// <returns>An <see cref="IHtmlContent"/> representing the hr element.</returns>
    /// <example>
    /// <code>
    /// var hr = HtmlHelper.Hr();
    /// // Renders: &lt;hr /&gt;
    /// </code>
    /// </example>
    public static IHtmlContent Hr()
        => Element("hr");

    /// <summary>
    /// Creates a horizontal rule (hr) element with custom attributes. This is a void element.
    /// </summary>
    /// <param name="attributes">Optional HTML attributes.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the hr element.</returns>
    public static IHtmlContent Hr(Dictionary<string, string> attributes)
        => Element("hr", attributes);

    /// <summary>
    /// Creates a line break (br) element. This is a void element and renders as self-closing.
    /// </summary>
    /// <returns>An <see cref="IHtmlContent"/> representing the br element.</returns>
    /// <example>
    /// <code>
    /// var br = HtmlHelper.Br();
    /// // Renders: &lt;br /&gt;
    /// </code>
    /// </example>
    public static IHtmlContent Br()
        => Element("br");

    /// <summary>
    /// Creates a line break (br) element with custom attributes. This is a void element.
    /// </summary>
    /// <param name="attributes">Optional HTML attributes.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the br element.</returns>
    public static IHtmlContent Br(Dictionary<string, string> attributes)
        => Element("br", attributes);

    // =============================================================
    // CSS Class Helpers
    // =============================================================

    /// <summary>
    /// Wraps the provided HTML content in a span that has the specified CSS class name.
    /// This is useful for adding a class to inline content without a dedicated container.
    /// </summary>
    /// <param name="content">The HTML content to wrap.</param>
    /// <param name="className">The CSS class name to apply.</param>
    /// <returns>An <see cref="IHtmlContent"/> with the CSS class applied.</returns>
    /// <example>
    /// <code>
    /// var c = HtmlHelper.WithClass(HtmlHelper.Text("Emphasized"), "highlight");
    /// // Renders: &lt;span class="highlight"&gt;Emphasized&lt;/span&gt;
    /// </code>
    /// </example>
    public static IHtmlContent WithClass(IHtmlContent content, string className)
        => Span(new Dictionary<string, string> { ["class"] = className }, content);

    /// <summary>
    /// Adds one or more CSS class names to an element by wrapping it in a span
    /// with the additional class(es). This is a convenience wrapper that does
    /// not modify the original content's attributes.
    /// </summary>
    /// <param name="content">The existing HTML content.</param>
    /// <param name="className">The CSS class name(s) to add.</param>
    /// <returns>An <see cref="IHtmlContent"/> with the added class name(s).</returns>
    /// <example>
    /// <code>
    /// var c = HtmlHelper.AddClass(HtmlHelper.Span(HtmlHelper.Text("text")), "bold");
    /// // Renders: &lt;span class="bold"&gt;&lt;span&gt;text&lt;/span&gt;&lt;/span&gt;
    /// </code>
    /// </example>
    public static IHtmlContent AddClass(IHtmlContent content, string className)
        => Span(new Dictionary<string, string> { ["class"] = className }, content);

    // =============================================================
    // Inline Style Helpers
    // =============================================================

    /// <summary>
    /// Wraps the provided HTML content in a span with the specified inline style.
    /// </summary>
    /// <param name="content">The HTML content to style.</param>
    /// <param name="style">The inline CSS style string (e.g. "color: red; font-weight: bold").</param>
    /// <returns>An <see cref="IHtmlContent"/> with the inline style applied.</returns>
    /// <example>
    /// <code>
    /// var c = HtmlHelper.WithStyle(HtmlHelper.Text("Red text"), "color: red;");
    /// // Renders: &lt;span style="color: red;"&gt;Red text&lt;/span&gt;
    /// </code>
    /// </example>
    public static IHtmlContent WithStyle(IHtmlContent content, string style)
        => Span(new Dictionary<string, string> { ["style"] = style }, content);

    // =============================================================
    // Data Attribute Helpers
    // =============================================================

    /// <summary>
    /// Wraps the provided HTML content in a span with the specified data attribute.
    /// </summary>
    /// <param name="content">The HTML content to annotate.</param>
    /// <param name="key">The data attribute name (without the "data-" prefix).</param>
    /// <param name="value">The value of the data attribute.</param>
    /// <returns>An <see cref="IHtmlContent"/> with the data attribute applied.</returns>
    /// <example>
    /// <code>
    /// var c = HtmlHelper.WithData(HtmlHelper.Text("Item"), "id", "123");
    /// // Renders: &lt;span data-id="123"&gt;Item&lt;/span&gt;
    /// </code>
    /// </example>
    public static IHtmlContent WithData(IHtmlContent content, string key, string value)
        => Span(new Dictionary<string, string> { [$"data-{key}"] = value }, content);

    // =============================================================
    // Internal Helpers
    // =============================================================

    /// <summary>
    /// Combines an array of <see cref="IHtmlContent"/> items into a single content item.
    /// Returns <c>null</c> for an empty array, the single item for a single-element array,
    /// and a <see cref="Fragment"/> for multiple items.
    /// </summary>
    private static IHtmlContent? Combine(params IHtmlContent[] contents)
    {
        if (contents == null || contents.Length == 0)
            return null;

        if (contents.Length == 1)
            return contents[0];

        var sb = new StringBuilder();
        foreach (var c in contents)
        {
            sb.Append(c.ToHtml());
        }
        return new RawHtmlContent(sb.ToString());
    }
}
