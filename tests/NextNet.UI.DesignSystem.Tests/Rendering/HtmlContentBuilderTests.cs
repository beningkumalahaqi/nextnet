using System;
using NextNet.UI.DesignSystem.Rendering;
using Xunit;

namespace NextNet.UI.DesignSystem.Tests.Rendering;

public class HtmlContentBuilderTests
{
    [Fact]
    public void OpenClose_Should_GenerateElement()
    {
        var html = new HtmlContentBuilder()
            .Div()
            .Close()
            .Build()
            .ToHtml();

        Assert.Equal("<div></div>", html);
    }

    [Fact]
    public void Class_Should_AddClassAttribute()
    {
        var html = new HtmlContentBuilder()
            .Div().Class("container")
            .Close()
            .Build()
            .ToHtml();

        Assert.Contains("class=\"container\"", html);
    }

    [Fact]
    public void Text_Should_EncodeText()
    {
        var html = new HtmlContentBuilder()
            .Div()
            .Text("Hello & Welcome")
            .Close()
            .Build()
            .ToHtml();

        Assert.Contains("Hello &amp; Welcome", html);
    }

    [Fact]
    public void NestedElements_Should_GenerateProperHierarchy()
    {
        var html = new HtmlContentBuilder()
            .Div().Class("outer")
                .Div().Class("inner").Text("Content").Close()
            .Close()
            .Build()
            .ToHtml();

        Assert.Contains("<div class=\"outer\"><div class=\"inner\">Content</div></div>", html);
    }

    [Fact]
    public void MultipleAttributes_Should_AllBePresent()
    {
        var html = new HtmlContentBuilder()
            .Div().Class("card").Id("main").Style("color:red;")
            .Close()
            .Build()
            .ToHtml();

        Assert.Contains("class=\"card\"", html);
        Assert.Contains("id=\"main\"", html);
        Assert.Contains("style=\"color:red;\"", html);
    }

    [Fact]
    public void SelfClosingVoidElement_Should_NotRequireClose()
    {
        var html = new HtmlContentBuilder()
            .Input().Type("text").Name("username").Class("field")
            .Build()
            .ToHtml();

        Assert.Contains("<input type=\"text\" name=\"username\" class=\"field\" />", html);
    }

    [Fact]
    public void MultipleVoidElements_Should_ChainCorrectly()
    {
        var html = new HtmlContentBuilder()
            .Input().Type("text")
            .Br()
            .Input().Type("checkbox")
            .Build()
            .ToHtml();

        Assert.Contains("<input type=\"text\" />", html);
        Assert.Contains("<br />", html);
        Assert.Contains("<input type=\"checkbox\" />", html);
    }

    [Fact]
    public void Raw_Should_NotEncodeContent()
    {
        var html = new HtmlContentBuilder()
            .Div()
            .Raw("<strong>Bold</strong>")
            .Close()
            .Build()
            .ToHtml();

        Assert.Contains("<strong>Bold</strong>", html);
    }

    [Fact]
    public void Child_Should_AppendHtmlContent()
    {
        var childContent = NextNet.Components.HtmlHelper.Raw("<span>child</span>");
        var html = new HtmlContentBuilder()
            .Div()
            .Child(childContent)
            .Close()
            .Build()
            .ToHtml();

        Assert.Contains("<span>child</span>", html);
    }

    [Fact]
    public void ConvenienceMethods_Should_GenerateCorrectTags()
    {
        var html = new HtmlContentBuilder()
            .H1().Text("Title").Close()
            .P().Text("Paragraph").Close()
            .Build()
            .ToHtml();

        Assert.Contains("<h1>Title</h1>", html);
        Assert.Contains("<p>Paragraph</p>", html);
    }

    [Fact]
    public void CloseIfOpen_Should_NotThrow_WhenNoOpenTags()
    {
        var builder = new HtmlContentBuilder();
        var result = builder.CloseIfOpen().Build().ToHtml();

        Assert.Equal("", result);
    }

    [Fact]
    public void AttrIf_Should_OnlyAdd_WhenConditionTrue()
    {
        var html = new HtmlContentBuilder()
            .Div().AttrIf("hidden", "hidden", true).AttrIf("data-x", "y", false)
            .Close()
            .Build()
            .ToHtml();

        Assert.Contains("hidden=\"hidden\"", html);
        Assert.DoesNotContain("data-x", html);
    }

    [Fact]
    public void Disabled_Should_AddDisabledAttribute()
    {
        var html = new HtmlContentBuilder()
            .Button().Disabled().Text("Click").Close()
            .Build()
            .ToHtml();

        Assert.Contains("disabled=\"disabled\"", html);
    }

    [Fact]
    public void Checked_Should_AddCheckedAttribute()
    {
        var html = new HtmlContentBuilder()
            .Input().Type("checkbox").Checked()
            .Build()
            .ToHtml();

        Assert.Contains("checked=\"checked\"", html);
    }

    [Fact]
    public void Data_Should_AddDataAttribute()
    {
        var html = new HtmlContentBuilder()
            .Div().Data("value", "123")
            .Close()
            .Build()
            .ToHtml();

        Assert.Contains("data-value=\"123\"", html);
    }

    [Fact]
    public void Close_Should_Throw_WhenNoOpenTags()
    {
        var builder = new HtmlContentBuilder();

        Assert.Throws<InvalidOperationException>(() => builder.Close());
    }

    [Fact]
    public void Attr_Should_Throw_WhenNoOpenTag()
    {
        var builder = new HtmlContentBuilder();

        Assert.Throws<InvalidOperationException>(() => builder.Class("test"));
    }

    [Fact]
    public void Build_Should_CloseAllRemainingTags()
    {
        var html = new HtmlContentBuilder()
            .Div()
                .Span()
            .Build()
            .ToHtml();

        Assert.Equal("<div><span></span></div>", html);
    }

    [Fact]
    public void SelectAndOption_Should_GenerateCorrectly()
    {
        var html = new HtmlContentBuilder()
            .Select().Name("color")
                .Option().Value("red").Text("Red").Close()
                .Option().Value("blue").Text("Blue").Close()
            .Close()
            .Build()
            .ToHtml();

        Assert.Contains("<select name=\"color\">", html);
        Assert.Contains("<option value=\"red\">Red</option>", html);
        Assert.Contains("<option value=\"blue\">Blue</option>", html);
        Assert.Contains("</select>", html);
    }

    // --- XSS prevention tests ---

    [Fact]
    public void Raw_Should_AcceptSafeHtml()
    {
        var html = new HtmlContentBuilder()
            .Div()
            .Raw("<strong>Bold</strong>")
            .Close()
            .Build()
            .ToHtml();

        Assert.Contains("<strong>Bold</strong>", html);
    }

    [Fact]
    public void Raw_Should_RejectScriptTag()
    {
        var builder = new HtmlContentBuilder().Div();

        var ex = Assert.Throws<ArgumentException>(() =>
            builder.Raw("<script>alert(1)</script>"));
        Assert.Contains("script", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Raw_Should_RejectJavaScriptUrl()
    {
        var builder = new HtmlContentBuilder().Div();

        var ex = Assert.Throws<ArgumentException>(() =>
            builder.Raw("<a href=\"javascript:alert(1)\">click</a>"));
        Assert.Contains("javascript", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Raw_Should_RejectInlineEventHandler()
    {
        var builder = new HtmlContentBuilder().Div();

        var ex = Assert.Throws<ArgumentException>(() =>
            builder.Raw("<img src=x onerror=\"alert(1)\">"));
        Assert.Contains("event handler", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Raw_Should_RejectOnClickAttribute()
    {
        var builder = new HtmlContentBuilder().Div();

        var ex = Assert.Throws<ArgumentException>(() =>
            builder.Raw("<button onclick=\"evil()\">click</button>"));
        Assert.Contains("event handler", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void SafeRaw_Should_EncodeDangerousCharacters()
    {
        var html = new HtmlContentBuilder()
            .Div()
            .SafeRaw("<script>alert(1)</script>")
            .Close()
            .Build()
            .ToHtml();

        Assert.Contains("&lt;script&gt;", html);
        Assert.DoesNotContain("<script>", html);
    }

    [Fact]
    public void SafeRaw_Should_EncodeBasicHtml()
    {
        var html = new HtmlContentBuilder()
            .Div()
            .SafeRaw("<strong>Bold</strong>")
            .Close()
            .Build()
            .ToHtml();

        Assert.Contains("&lt;strong&gt;Bold&lt;/strong&gt;", html);
    }

    [Fact]
    public void SafeRaw_Should_Throw_WhenNull()
    {
        var builder = new HtmlContentBuilder();
        Assert.Throws<ArgumentNullException>(() => builder.SafeRaw(null!));
    }
}
