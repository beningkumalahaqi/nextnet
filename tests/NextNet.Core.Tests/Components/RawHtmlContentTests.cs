using NextNet.Components;
using Xunit;

namespace NextNet.Core.Tests.Components;

public class RawHtmlContentTests
{
    [Fact]
    public void Constructor_Should_StoreContent_When_ValidContentProvided()
    {
        var content = new RawHtmlContent("<div>Hello</div>");
        Assert.Equal("<div>Hello</div>", content.Content);
    }

    [Fact]
    public void Constructor_Should_ThrowArgumentNullException_When_ContentIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new RawHtmlContent(null!));
    }

    [Fact]
    public void ToHtml_Should_ReturnRawContent_When_Invoked()
    {
        var html = "<script>alert('xss')</script>";
        var content = new RawHtmlContent(html);
        Assert.Equal(html, content.ToHtml());
    }

    [Fact]
    public void ToString_Should_ReturnRawContent_When_Invoked()
    {
        var content = new RawHtmlContent("<p>test</p>");
        Assert.Equal("<p>test</p>", content.ToString());
    }

    [Fact]
    public async Task WriteToAsync_Should_WriteContentToWriter_When_Invoked()
    {
        var content = new RawHtmlContent("<h1>Title</h1>");
        using var writer = new StringWriter();

        await content.WriteToAsync(writer);

        Assert.Equal("<h1>Title</h1>", writer.ToString());
    }

    [Fact]
    public async Task WriteToAsync_Should_WriteNothing_When_ContentIsEmpty()
    {
        var content = new RawHtmlContent("");
        using var writer = new StringWriter();

        await content.WriteToAsync(writer);

        Assert.Equal("", writer.ToString());
    }

    [Fact]
    public async Task WriteToAsync_Should_WriteLargeContent_When_Invoked()
    {
        var large = new string('a', 10000);
        var content = new RawHtmlContent(large);
        using var writer = new StringWriter();

        await content.WriteToAsync(writer);

        Assert.Equal(large, writer.ToString());
    }

    [Fact]
    public void Content_Should_BeReadOnly_When_Accessed()
    {
        var content = new RawHtmlContent("hello");
        var contentValue = content.Content;
        Assert.Equal("hello", contentValue);
    }

    [Fact]
    public void ToHtml_Should_MatchContentProperty_When_Invoked()
    {
        var content = new RawHtmlContent("<span>text</span>");
        Assert.Equal(content.Content, content.ToHtml());
    }
}
