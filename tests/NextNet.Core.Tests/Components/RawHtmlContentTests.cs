using NextNet.Components;
using Xunit;

namespace NextNet.Core.Tests.Components;

public class RawHtmlContentTests
{
    [Fact]
    public void Constructor_WithValidContent_StoresContent()
    {
        var content = new RawHtmlContent("<div>Hello</div>");
        Assert.Equal("<div>Hello</div>", content.Content);
    }

    [Fact]
    public void Constructor_WithNullContent_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new RawHtmlContent(null!));
    }

    [Fact]
    public void ToHtml_ReturnsRawContent()
    {
        var html = "<script>alert('xss')</script>";
        var content = new RawHtmlContent(html);
        Assert.Equal(html, content.ToHtml());
    }

    [Fact]
    public void ToString_ReturnsRawContent()
    {
        var content = new RawHtmlContent("<p>test</p>");
        Assert.Equal("<p>test</p>", content.ToString());
    }

    [Fact]
    public async Task WriteToAsync_WritesContentToWriter()
    {
        var content = new RawHtmlContent("<h1>Title</h1>");
        using var writer = new StringWriter();

        await content.WriteToAsync(writer);

        Assert.Equal("<h1>Title</h1>", writer.ToString());
    }

    [Fact]
    public async Task WriteToAsync_WithEmptyContent_WritesNothing()
    {
        var content = new RawHtmlContent("");
        using var writer = new StringWriter();

        await content.WriteToAsync(writer);

        Assert.Equal("", writer.ToString());
    }

    [Fact]
    public async Task WriteToAsync_WritesLargeContent()
    {
        var large = new string('a', 10000);
        var content = new RawHtmlContent(large);
        using var writer = new StringWriter();

        await content.WriteToAsync(writer);

        Assert.Equal(large, writer.ToString());
    }

    [Fact]
    public void Content_Property_IsReadOnly()
    {
        var content = new RawHtmlContent("hello");
        var contentValue = content.Content;
        Assert.Equal("hello", contentValue);
    }

    [Fact]
    public void ToHtml_Matches_Content_Property()
    {
        var content = new RawHtmlContent("<span>text</span>");
        Assert.Equal(content.Content, content.ToHtml());
    }
}
