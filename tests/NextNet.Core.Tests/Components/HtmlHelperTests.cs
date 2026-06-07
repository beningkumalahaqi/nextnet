using NextNet.Components;
using Xunit;

namespace NextNet.Core.Tests.Components;

public class HtmlHelperTests
{
    // ─── Element ────────────────────────────────────────────────────────────

    [Fact]
    public void Element_WithTagOnly_CreatesSelfClosingTag()
    {
        var result = HtmlHelper.Element("br");
        Assert.Equal("<br />", result.ToHtml());
    }

    [Fact]
    public void Element_WithTagAndContent_CreatesElementWithContent()
    {
        var result = HtmlHelper.Element("div", content: HtmlHelper.Text("hello"));
        Assert.Equal("<div>hello</div>", result.ToHtml());
    }

    [Fact]
    public void Element_WithAttributes_RendersAttributes()
    {
        var attrs = new Dictionary<string, string>
        {
            ["class"] = "container",
            ["id"] = "main",
        };
        var result = HtmlHelper.Element("div", attributes: attrs);
        Assert.Equal("<div class=\"container\" id=\"main\" />", result.ToHtml());
    }

    [Fact]
    public void Element_WithAttributesAndContent_RendersFullElement()
    {
        var attrs = new Dictionary<string, string> { ["href"] = "https://example.com" };
        var result = HtmlHelper.Element("a", attrs, HtmlHelper.Text("click"));
        Assert.Equal("<a href=\"https://example.com\">click</a>", result.ToHtml());
    }

    [Fact]
    public void Element_WithNullTag_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => HtmlHelper.Element(null!));
    }

    [Fact]
    public void Element_WithEmptyTag_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => HtmlHelper.Element(""));
    }

    [Fact]
    public void Element_WithWhitespaceTag_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => HtmlHelper.Element("   "));
    }

    [Fact]
    public void Element_EncodesAttributeValues()
    {
        var attrs = new Dictionary<string, string> { ["onclick"] = "alert('xss')" };
        var result = HtmlHelper.Element("div", attrs);
        Assert.Contains("onclick", result.ToHtml());
        // Ensure attribute encoding works
        Assert.DoesNotContain("'", result.ToHtml().Replace("&#39;", "'"));
    }

    // ─── Text ──────────────────────────────────────────────────────────────

    [Fact]
    public void Text_EncodesHtml()
    {
        var result = HtmlHelper.Text("<b>bold</b>");
        Assert.Equal("&lt;b&gt;bold&lt;/b&gt;", result.ToHtml());
    }

    [Fact]
    public void Text_WithNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => HtmlHelper.Text(null!));
    }

    [Fact]
    public void Text_WithPlainText_ReturnsSameText()
    {
        var result = HtmlHelper.Text("Hello, World!");
        Assert.Equal("Hello, World!", result.ToHtml());
    }

    // ─── Raw ────────────────────────────────────────────────────────────────

    [Fact]
    public void Raw_ReturnsUnencodedHtml()
    {
        var result = HtmlHelper.Raw("<b>bold</b>");
        Assert.Equal("<b>bold</b>", result.ToHtml());
    }

    [Fact]
    public void Raw_WithNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => HtmlHelper.Raw(null!));
    }

    // ─── Fragment ──────────────────────────────────────────────────────────

    [Fact]
    public void Fragment_CombinesMultipleContents()
    {
        var result = HtmlHelper.Fragment(
            HtmlHelper.Text("Hello"),
            HtmlHelper.Raw("<br />"),
            HtmlHelper.Text("World")
        );
        Assert.Equal("Hello<br />World", result.ToHtml());
    }

    [Fact]
    public void Fragment_WithSingleItem_ReturnsThatItem()
    {
        var result = HtmlHelper.Fragment(HtmlHelper.Text("only"));
        Assert.Equal("only", result.ToHtml());
    }

    [Fact]
    public void Fragment_WithNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => HtmlHelper.Fragment(null!));
    }

    // ─── DocType ────────────────────────────────────────────────────────────

    [Fact]
    public void DocType_WithDefault_ReturnsHtml5Doctype()
    {
        var result = HtmlHelper.DocType();
        Assert.Equal("<!DOCTYPE html>", result.ToHtml());
    }

    [Fact]
    public void DocType_WithCustomType()
    {
        var result = HtmlHelper.DocType("xml");
        Assert.Equal("<!DOCTYPE xml>", result.ToHtml());
    }

    [Fact]
    public void DocType_WithNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => HtmlHelper.DocType(null!));
    }

    // ─── Stylesheet ─────────────────────────────────────────────────────────

    [Fact]
    public void Stylesheet_CreatesLinkElement()
    {
        var result = HtmlHelper.Stylesheet("/styles.css");
        var html = result.ToHtml();
        Assert.Contains("<link", html);
        Assert.Contains("rel=\"stylesheet\"", html);
        Assert.Contains("href=\"/styles.css\"", html);
    }

    // ─── Script ─────────────────────────────────────────────────────────────

    [Fact]
    public void Script_CreatesScriptElement()
    {
        var result = HtmlHelper.Script("/app.js");
        var html = result.ToHtml();
        Assert.Contains("<script", html);
        Assert.Contains("src=\"/app.js\"", html);
    }
}
