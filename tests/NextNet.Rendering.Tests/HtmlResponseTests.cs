using Microsoft.AspNetCore.Http;
using NextNet.Components;
using Xunit;

namespace NextNet.Rendering.Tests;

public class HtmlResponseTests
{
    [Fact]
    public void Constructor_WithContent_SetsProperties()
    {
        var content = new RawHtmlContent("<p>hello</p>");
        var response = new HtmlResponse(content, 200, "public, max-age=3600");

        Assert.Equal(200, response.StatusCode);
        Assert.Equal("public, max-age=3600", response.CacheControl);
        Assert.Same(content, response.Content);
    }

    [Fact]
    public void Constructor_WithDefaultStatusCode_Is200()
    {
        var response = new HtmlResponse(new RawHtmlContent("ok"));
        Assert.Equal(200, response.StatusCode);
    }

    [Fact]
    public void Constructor_WithNullContent_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new HtmlResponse(null!));
    }

    [Fact]
    public void ToString_ReturnsContentHtml()
    {
        var content = new RawHtmlContent("<div>test</div>");
        var response = new HtmlResponse(content);
        Assert.Equal("<div>test</div>", response.ToString());
    }

    [Fact]
    public void NotFound_Returns404WithNotFoundBody()
    {
        var response = HtmlResponse.NotFound();

        Assert.Equal(404, response.StatusCode);
        Assert.Equal("no-cache", response.CacheControl);
        Assert.Contains("404", response.ToString());
        Assert.Contains("Not Found", response.ToString());
    }

    [Fact]
    public void Redirect_Returns301WithLocation()
    {
        var response = HtmlResponse.Redirect("/new-location");

        Assert.Equal(301, response.StatusCode);
        Assert.Equal("no-cache", response.CacheControl);
        var html = response.ToString();
        Assert.Contains("/new-location", html);
        Assert.Contains("Redirecting", html);
    }

    [Fact]
    public async Task ExecuteAsync_SetsStatusCodeAndContentType()
    {
        var ctx = new DefaultHttpContext();
        ctx.Response.Body = new MemoryStream();
        var content = new RawHtmlContent("<h1>Hello</h1>");
        var response = new HtmlResponse(content, 201);

        await response.ExecuteAsync(ctx);

        Assert.Equal(201, ctx.Response.StatusCode);
        Assert.Equal("text/html; charset=utf-8", ctx.Response.ContentType);
    }

    [Fact]
    public async Task ExecuteAsync_WithoutCacheControl_OmitsHeader()
    {
        var ctx = new DefaultHttpContext();
        ctx.Response.Body = new MemoryStream();
        var response = new HtmlResponse(new RawHtmlContent("test"));

        await response.ExecuteAsync(ctx);

        Assert.True(string.IsNullOrEmpty(ctx.Response.Headers.CacheControl));
    }

    [Fact]
    public async Task ExecuteAsync_WithCacheControl_SetsHeader()
    {
        var ctx = new DefaultHttpContext();
        ctx.Response.Body = new MemoryStream();
        var response = new HtmlResponse(new RawHtmlContent("test"), cacheControl: "no-store");

        await response.ExecuteAsync(ctx);

        Assert.Equal("no-store", ctx.Response.Headers.CacheControl);
    }

    [Fact]
    public async Task ExecuteAsync_WritesContentToBody()
    {
        var ctx = new DefaultHttpContext();
        ctx.Response.Body = new MemoryStream();
        var response = new HtmlResponse(new RawHtmlContent("HTML content"));

        await response.ExecuteAsync(ctx);

        ctx.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = new StreamReader(ctx.Response.Body).ReadToEnd();
        Assert.Equal("HTML content", body);
    }

    [Fact]
    public async Task ExecuteAsync_ResponseWithHttpContext_WritesCorrectly()
    {
        var ctx = new DefaultHttpContext();
        ctx.Response.Body = new MemoryStream();

        var response = new HtmlResponse(new RawHtmlContent("<p>Hello, World!</p>"), 202, "private");

        await response.ExecuteAsync(ctx);

        Assert.Equal(202, ctx.Response.StatusCode);
        Assert.Equal("text/html; charset=utf-8", ctx.Response.ContentType);
        Assert.Equal("private", ctx.Response.Headers.CacheControl);

        ctx.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = new StreamReader(ctx.Response.Body).ReadToEnd();
        Assert.Equal("<p>Hello, World!</p>", body);
    }

    [Fact]
    public async Task ExecuteAsync_NullHttpContext_Throws()
    {
        var response = new HtmlResponse(new RawHtmlContent("test"));
        await Assert.ThrowsAsync<ArgumentNullException>(() => response.ExecuteAsync(null!));
    }

    [Fact]
    public void ToString_RichContent_ReturnsExpected()
    {
        var content = new RawHtmlContent("<script>alert('xss')</script>");
        var response = new HtmlResponse(content);
        Assert.Contains("<script>", response.ToString());
    }
}
