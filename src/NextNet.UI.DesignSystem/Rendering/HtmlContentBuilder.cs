using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using NextNet.Components;

namespace NextNet.UI.DesignSystem.Rendering;

/// <summary>
/// Provides a fluent API for building component HTML content programmatically
/// with support for tag nesting, attributes, and child content.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="HtmlContentBuilder"/> uses a tag stack to track open elements.
/// Each call to <c>Element</c> or a convenience method like <c>Div</c> pushes
/// a new tag onto the stack. The <c>Close</c> method pops the most recently
/// opened tag and emits the closing tag. The <c>Build</c> method closes all
/// remaining open tags and returns the result as <see cref="IHtmlContent"/>.
/// </para>
/// <para>
/// The builder automatically handles void elements (like <c>img</c>, <c>input</c>)
/// that should never have closing tags.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var html = new HtmlContentBuilder()
///     .Div().Class("card")
///         .Div().Class("card-header").Text("Title").Close()
///         .Div().Class("card-body").Text("Body content").Close()
///     .Close()
///     .Build();
/// // Produces: &lt;div class="card"&gt;&lt;div class="card-header"&gt;Title&lt;/div&gt;&lt;div class="card-body"&gt;Body content&lt;/div&gt;&lt;/div&gt;
/// </code>
/// </example>
public sealed class HtmlContentBuilder
{
    private static readonly HashSet<string> VoidElements = new(StringComparer.OrdinalIgnoreCase)
    {
        "area", "base", "br", "col", "embed", "hr", "img", "input",
        "link", "meta", "param", "source", "track", "wbr"
    };

    private readonly StringBuilder _sb = new();
    private readonly Stack<string> _openTags = new();
    private bool _inOpenTag;

    /// <summary>
    /// Opens an HTML element with the specified tag name.
    /// </summary>
    /// <param name="tagName">The HTML tag name (e.g., "div", "span", "button"). Must not be null or empty.</param>
    /// <returns>This builder instance for chaining.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="tagName"/> is null or empty.</exception>
    public HtmlContentBuilder Element(string tagName)
    {
        if (string.IsNullOrWhiteSpace(tagName))
            throw new ArgumentException("Tag name cannot be null or empty.", nameof(tagName));

        FlushOpenTag();
        _openTags.Push(tagName);
        _sb.Append('<').Append(tagName);
        _inOpenTag = true;
        return this;
    }

    // --- Convenience element methods ---

    /// <summary>Opens a <c>&lt;div&gt;</c> element.</summary>
    public HtmlContentBuilder Div() => Element("div");
    /// <summary>Opens a <c>&lt;span&gt;</c> element.</summary>
    public HtmlContentBuilder Span() => Element("span");
    /// <summary>Opens a <c>&lt;button&gt;</c> element.</summary>
    public HtmlContentBuilder Button() => Element("button");
    /// <summary>Opens an <c>&lt;h1&gt;</c> element.</summary>
    public HtmlContentBuilder H1() => Element("h1");
    /// <summary>Opens an <c>&lt;h2&gt;</c> element.</summary>
    public HtmlContentBuilder H2() => Element("h2");
    /// <summary>Opens an <c>&lt;h3&gt;</c> element.</summary>
    public HtmlContentBuilder H3() => Element("h3");
    /// <summary>Opens an <c>&lt;h4&gt;</c> element.</summary>
    public HtmlContentBuilder H4() => Element("h4");
    /// <summary>Opens a <c>&lt;p&gt;</c> element.</summary>
    public HtmlContentBuilder P() => Element("p");
    /// <summary>Opens an <c>&lt;a&gt;</c> element.</summary>
    public HtmlContentBuilder A() => Element("a");
    /// <summary>Opens a <c>&lt;ul&gt;</c> element.</summary>
    public HtmlContentBuilder Ul() => Element("ul");
    /// <summary>Opens an <c>&lt;ol&gt;</c> element.</summary>
    public HtmlContentBuilder Ol() => Element("ol");
    /// <summary>Opens an <c>&lt;li&gt;</c> element.</summary>
    public HtmlContentBuilder Li() => Element("li");
    /// <summary>Opens a <c>&lt;table&gt;</c> element.</summary>
    public HtmlContentBuilder Table() => Element("table");
    /// <summary>Opens a <c>&lt;thead&gt;</c> element.</summary>
    public HtmlContentBuilder Thead() => Element("thead");
    /// <summary>Opens a <c>&lt;tbody&gt;</c> element.</summary>
    public HtmlContentBuilder Tbody() => Element("tbody");
    /// <summary>Opens a <c>&lt;tr&gt;</c> element.</summary>
    public HtmlContentBuilder Tr() => Element("tr");
    /// <summary>Opens a <c>&lt;th&gt;</c> element.</summary>
    public HtmlContentBuilder Th() => Element("th");
    /// <summary>Opens a <c>&lt;td&gt;</c> element.</summary>
    public HtmlContentBuilder Td() => Element("td");
    /// <summary>Opens a <c>&lt;label&gt;</c> element.</summary>
    public HtmlContentBuilder Label() => Element("label");
    /// <summary>Opens an <c>&lt;input&gt;</c> element.</summary>
    public HtmlContentBuilder Input() => Element("input");
    /// <summary>Opens a <c>&lt;select&gt;</c> element.</summary>
    public HtmlContentBuilder Select() => Element("select");
    /// <summary>Opens an <c>&lt;option&gt;</c> element.</summary>
    public HtmlContentBuilder Option() => Element("option");
    /// <summary>Opens a <c>&lt;textarea&gt;</c> element.</summary>
    public HtmlContentBuilder Textarea() => Element("textarea");
    /// <summary>Opens an <c>&lt;img&gt;</c> element.</summary>
    public HtmlContentBuilder Img() => Element("img");
    /// <summary>Opens a <c>&lt;form&gt;</c> element.</summary>
    public HtmlContentBuilder Form() => Element("form");
    /// <summary>Opens a <c>&lt;header&gt;</c> element.</summary>
    public HtmlContentBuilder Header() => Element("header");
    /// <summary>Opens a <c>&lt;footer&gt;</c> element.</summary>
    public HtmlContentBuilder Footer() => Element("footer");
    /// <summary>Opens a <c>&lt;nav&gt;</c> element.</summary>
    public HtmlContentBuilder Nav() => Element("nav");
    /// <summary>Opens a <c>&lt;section&gt;</c> element.</summary>
    public HtmlContentBuilder Section() => Element("section");
    /// <summary>Opens an <c>&lt;article&gt;</c> element.</summary>
    public HtmlContentBuilder Article() => Element("article");
    /// <summary>Opens an <c>&lt;aside&gt;</c> element.</summary>
    public HtmlContentBuilder Aside() => Element("aside");
    /// <summary>Opens a <c>&lt;br&gt;</c> element.</summary>
    public HtmlContentBuilder Br() => Element("br");
    /// <summary>Opens an <c>&lt;hr&gt;</c> element.</summary>
    public HtmlContentBuilder Hr() => Element("hr");

    // --- Attribute methods ---

    /// <summary>Adds a <c>class</c> attribute to the current open tag.</summary>
    public HtmlContentBuilder Class(string cls) => Attr("class", cls);
    /// <summary>Adds an <c>id</c> attribute to the current open tag.</summary>
    public HtmlContentBuilder Id(string id) => Attr("id", id);
    /// <summary>Adds a <c>style</c> attribute to the current open tag.</summary>
    public HtmlContentBuilder Style(string style) => Attr("style", style);
    /// <summary>Adds an <c>href</c> attribute to the current open tag.</summary>
    public HtmlContentBuilder Href(string href) => Attr("href", href);
    /// <summary>Adds a <c>src</c> attribute to the current open tag.</summary>
    public HtmlContentBuilder Src(string src) => Attr("src", src);
    /// <summary>Adds an <c>alt</c> attribute to the current open tag.</summary>
    public HtmlContentBuilder Alt(string alt) => Attr("alt", alt);
    /// <summary>Adds a <c>type</c> attribute to the current open tag.</summary>
    public HtmlContentBuilder Type(string type) => Attr("type", type);
    /// <summary>Adds a <c>name</c> attribute to the current open tag.</summary>
    public HtmlContentBuilder Name(string name) => Attr("name", name);
    /// <summary>Adds a <c>value</c> attribute to the current open tag.</summary>
    public HtmlContentBuilder Value(string value) => Attr("value", value);
    /// <summary>Adds a <c>placeholder</c> attribute to the current open tag.</summary>
    public HtmlContentBuilder Placeholder(string placeholder) => Attr("placeholder", placeholder);
    /// <summary>Adds a <c>role</c> attribute to the current open tag.</summary>
    public HtmlContentBuilder Role(string role) => Attr("role", role);
    /// <summary>Adds a <c>data-*</c> attribute to the current open tag.</summary>
    public HtmlContentBuilder Data(string key, string value) => Attr($"data-{key}", value);

    /// <summary>
    /// Adds a <c>disabled</c> attribute to the current open tag when <paramref name="disabled"/> is <c>true</c>.
    /// </summary>
    public HtmlContentBuilder Disabled(bool disabled = true) => disabled ? Attr("disabled", "disabled") : this;

    /// <summary>
    /// Adds a <c>checked</c> attribute to the current open tag when <paramref name="checked"/><c>!</c> is <c>true</c>.
    /// </summary>
    public HtmlContentBuilder Checked(bool @checked = true) => @checked ? Attr("checked", "checked") : this;

    /// <summary>
    /// Adds a <c>hidden</c> attribute to the current open tag when <paramref name="hidden"/> is <c>true</c>.
    /// </summary>
    public HtmlContentBuilder Hidden(bool hidden = true) => hidden ? Attr("hidden", "hidden") : this;

    /// <summary>
    /// Adds a named attribute with the specified value to the current open tag.
    /// </summary>
    /// <param name="name">The attribute name (e.g., "class", "data-value").</param>
    /// <param name="value">The attribute value. Must not be null.</param>
    /// <returns>This builder instance for chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown if there is no currently open tag to attach an attribute to.</exception>
    public HtmlContentBuilder Attr(string name, string value)
    {
        if (!_inOpenTag)
            throw new InvalidOperationException("Cannot add attribute: no open tag. Call Element() first.");
        _sb.Append(' ').Append(name).Append("=\"").Append(HtmlEncoder.Default.Encode(value)).Append('"');
        return this;
    }

    /// <summary>
    /// Adds a named attribute with the specified value only if the condition is <c>true</c>.
    /// </summary>
    public HtmlContentBuilder AttrIf(string name, string value, bool condition)
        => condition ? Attr(name, value) : this;

    // --- Content methods ---

    /// <summary>
    /// Adds HTML-encoded text content. The text is encoded using <see cref="HtmlEncoder.Default"/>.
    /// </summary>
    /// <param name="text">The text content to add. Must not be null.</param>
    /// <returns>This builder instance for chaining.</returns>
    public HtmlContentBuilder Text(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        FlushOpenTag();
        _sb.Append(HtmlEncoder.Default.Encode(text));
        return this;
    }

    /// <summary>
    /// Adds child <see cref="IHtmlContent"/> as content within the current open tag.
    /// </summary>
    /// <param name="content">The child HTML content to add. Must not be null.</param>
    /// <returns>This builder instance for chaining.</returns>
    public HtmlContentBuilder Child(IHtmlContent content)
    {
        ArgumentNullException.ThrowIfNull(content);
        FlushOpenTag();
        _sb.Append(content.ToHtml());
        return this;
    }

    /// <summary>
    /// Adds raw (unencoded) HTML content. Use with extreme caution —
    /// <paramref name="html"/> is validated for XSS patterns
    /// <c>(&lt;script&gt;</c>, <c>javascript:</c>, <c>on*</c> event handlers).
    /// Throws <see cref="ArgumentException"/> if dangerous patterns are detected.
    /// </summary>
    /// <param name="html">The raw HTML string to add. Must not be null.</param>
    /// <returns>This builder instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="html"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="html"/> contains XSS patterns.</exception>
    public HtmlContentBuilder Raw(string html)
    {
        ArgumentNullException.ThrowIfNull(html);
        ValidateRawInput(html);
        FlushOpenTag();
        _sb.Append(html);
        return this;
    }

    /// <summary>
    /// Adds HTML-encoded text content. Unlike <see cref="Raw"/>, this method
    /// encodes potentially dangerous characters using <see cref="HtmlEncoder.Default"/>,
    /// making it safe for untrusted input.
    /// </summary>
    /// <param name="text">The text content to add. Must not be null.</param>
    /// <returns>This builder instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="text"/> is null.</exception>
    public HtmlContentBuilder SafeRaw(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        FlushOpenTag();
        _sb.Append(HtmlEncoder.Default.Encode(text));
        return this;
    }

    /// <summary>
    /// Validates a raw HTML string for common XSS injection patterns.
    /// Throws <see cref="ArgumentException"/> if any dangerous pattern is detected.
    /// </summary>
    /// <param name="html">The raw HTML string to validate.</param>
    /// <exception cref="ArgumentException">Thrown if XSS patterns are detected.</exception>
    private static void ValidateRawInput(string html)
    {
        // Block <script> tags (case-insensitive)
        if (html.Contains("<script", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "Raw HTML content contains a <script> tag which is not allowed for XSS prevention.",
                nameof(html));
        }

        // Block javascript: URLs in attributes
        if (html.Contains("javascript:", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "Raw HTML content contains a 'javascript:' URL which is not allowed for XSS prevention.",
                nameof(html));
        }

        // Block event handler attributes (on* = "...")
        // Pattern: look for "on" followed by word chars, then optional whitespace and "="
        if (Regex.IsMatch(html, @"\bon\w+\s*=", RegexOptions.IgnoreCase | RegexOptions.Compiled, TimeSpan.FromMilliseconds(100)))
        {
            throw new ArgumentException(
                "Raw HTML content contains inline event handler attributes (on*) which are not allowed for XSS prevention.",
                nameof(html));
        }
    }

    /// <summary>
    /// Closes the most recently opened element. For void elements (e.g., <c>img</c>,
    /// <c>input</c>), this method does nothing since they are self-closing.
    /// </summary>
    /// <returns>This builder instance for chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown if there are no open tags to close.</exception>
    public HtmlContentBuilder Close()
    {
        if (_openTags.Count == 0)
            throw new InvalidOperationException("Cannot close tag: no open tags.");

        var tagName = _openTags.Pop();

        // If we're still in the open tag and it's a void element, close it as self-closing
        if (_inOpenTag && VoidElements.Contains(tagName))
        {
            _sb.Append(" />");
            _inOpenTag = false;
            return this;
        }

        FlushOpenTag();
        _sb.Append("</").Append(tagName).Append('>');
        return this;
    }

    /// <summary>
    /// Closes the most recently opened element only if there is one open.
    /// No exception is thrown if the stack is empty.
    /// </summary>
    /// <returns>This builder instance for chaining.</returns>
    public HtmlContentBuilder CloseIfOpen()
    {
        if (_openTags.Count > 0) Close();
        return this;
    }

    /// <summary>
    /// Builds the final HTML content. All remaining open tags are automatically closed.
    /// </summary>
    /// <returns>An <see cref="IHtmlContent"/> representing the complete HTML output.</returns>
    public IHtmlContent Build()
    {
        // Close all remaining open tags (in reverse order)
        while (_openTags.Count > 0)
        {
            Close();
        }

        return new RawHtmlContent(_sb.ToString());
    }

    /// <summary>
    /// Returns the accumulated HTML as a string without closing remaining tags.
    /// </summary>
    /// <returns>The raw HTML string built so far.</returns>
    public override string ToString() => _sb.ToString();

    private void FlushOpenTag()
    {
        if (!_inOpenTag) return;

        // If the currently open tag is a void element, close it as self-closing.
        // Also pop it from the stack since void elements can't have children.
        if (_openTags.Count > 0)
        {
            var tagName = _openTags.Peek();
            if (VoidElements.Contains(tagName))
            {
                _sb.Append(" />");
                _openTags.Pop();
                _inOpenTag = false;
                return;
            }
        }

        _sb.Append('>');
        _inOpenTag = false;
    }
}
