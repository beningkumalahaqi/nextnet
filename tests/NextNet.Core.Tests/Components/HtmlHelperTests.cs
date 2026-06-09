using NextNet.Components;
using Xunit;

namespace NextNet.Core.Tests.Components;

public class HtmlHelperTests
{
    // ─── Element ────────────────────────────────────────────────────────────

    [Fact]
    public void Element_Should_CreateSelfClosingTag_When_OnlyTagProvided()
    {
        var result = HtmlHelper.Element("br");
        Assert.Equal("<br />", result.ToHtml());
    }

    [Fact]
    public void Element_Should_CreateElementWithContent_When_TagAndContentProvided()
    {
        var result = HtmlHelper.Element("div", content: HtmlHelper.Text("hello"));
        Assert.Equal("<div>hello</div>", result.ToHtml());
    }

    [Fact]
    public void Element_Should_RenderAttributes_When_Provided()
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
    public void Element_Should_RenderFullElement_When_AttributesAndContentProvided()
    {
        var attrs = new Dictionary<string, string> { ["href"] = "https://example.com" };
        var result = HtmlHelper.Element("a", attrs, HtmlHelper.Text("click"));
        Assert.Equal("<a href=\"https://example.com\">click</a>", result.ToHtml());
    }

    [Fact]
    public void Element_Should_ThrowArgumentNullException_When_TagIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => HtmlHelper.Element(null!));
    }

    [Fact]
    public void Element_Should_ThrowArgumentException_When_TagIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => HtmlHelper.Element(""));
    }

    [Fact]
    public void Element_Should_ThrowArgumentException_When_TagIsWhitespace()
    {
        Assert.Throws<ArgumentException>(() => HtmlHelper.Element("   "));
    }

    [Fact]
    public void Element_Should_EncodeAttributeValues_When_Rendered()
    {
        var attrs = new Dictionary<string, string> { ["onclick"] = "alert('xss')" };
        var result = HtmlHelper.Element("div", attrs);
        Assert.Contains("onclick", result.ToHtml());
        // Ensure attribute encoding works
        Assert.DoesNotContain("'", result.ToHtml().Replace("&#39;", "'"));
    }

    // ─── Text ──────────────────────────────────────────────────────────────

    [Fact]
    public void Text_Should_EncodeHtml_When_ContainsSpecialCharacters()
    {
        var result = HtmlHelper.Text("<b>bold</b>");
        Assert.Equal("&lt;b&gt;bold&lt;/b&gt;", result.ToHtml());
    }

    [Fact]
    public void Text_Should_ThrowArgumentNullException_When_InputIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => HtmlHelper.Text(null!));
    }

    [Fact]
    public void Text_Should_ReturnSameText_When_PlainTextProvided()
    {
        var result = HtmlHelper.Text("Hello, World!");
        Assert.Equal("Hello, World!", result.ToHtml());
    }

    // ─── Raw ────────────────────────────────────────────────────────────────

    [Fact]
    public void Raw_Should_ReturnUnencodedHtml_When_Invoked()
    {
        var result = HtmlHelper.Raw("<b>bold</b>");
        Assert.Equal("<b>bold</b>", result.ToHtml());
    }

    [Fact]
    public void Raw_Should_ThrowArgumentNullException_When_InputIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => HtmlHelper.Raw(null!));
    }

    // ─── Fragment ──────────────────────────────────────────────────────────

    [Fact]
    public void Fragment_Should_CombineMultipleContents_When_Invoked()
    {
        var result = HtmlHelper.Fragment(
            HtmlHelper.Text("Hello"),
            HtmlHelper.Raw("<br />"),
            HtmlHelper.Text("World")
        );
        Assert.Equal("Hello<br />World", result.ToHtml());
    }

    [Fact]
    public void Fragment_Should_ReturnSingleItem_When_OneContentProvided()
    {
        var result = HtmlHelper.Fragment(HtmlHelper.Text("only"));
        Assert.Equal("only", result.ToHtml());
    }

    [Fact]
    public void Fragment_Should_ThrowArgumentNullException_When_InputIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => HtmlHelper.Fragment(null!));
    }

    // ─── DocType ────────────────────────────────────────────────────────────

    [Fact]
    public void DocType_Should_ReturnHtml5Doctype_When_DefaultTypeUsed()
    {
        var result = HtmlHelper.DocType();
        Assert.Equal("<!DOCTYPE html>", result.ToHtml());
    }

    [Fact]
    public void DocType_Should_ReturnCustomDoctype_When_CustomTypeProvided()
    {
        var result = HtmlHelper.DocType("xml");
        Assert.Equal("<!DOCTYPE xml>", result.ToHtml());
    }

    [Fact]
    public void DocType_Should_ThrowArgumentNullException_When_TypeIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => HtmlHelper.DocType(null!));
    }

    // ─── Stylesheet ─────────────────────────────────────────────────────────

    [Fact]
    public void Stylesheet_Should_CreateLinkElement_When_Invoked()
    {
        var result = HtmlHelper.Stylesheet("/styles.css");
        var html = result.ToHtml();
        Assert.Contains("<link", html);
        Assert.Contains("rel=\"stylesheet\"", html);
        Assert.Contains("href=\"/styles.css\"", html);
    }

    // ─── Script ─────────────────────────────────────────────────────────────

    [Fact]
    public void Script_Should_CreateScriptElement_When_Invoked()
    {
        var result = HtmlHelper.Script("/app.js");
        var html = result.ToHtml();
        Assert.Contains("<script", html);
        Assert.Contains("src=\"/app.js\"", html);
    }
}
