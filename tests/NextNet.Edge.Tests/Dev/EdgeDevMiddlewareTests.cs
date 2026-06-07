using Microsoft.AspNetCore.Http;
using NextNet.Edge.Compatibility;
using NextNet.Edge.Dev;
using Xunit;

namespace NextNet.Edge.Tests.Dev;

public class EdgeDevMiddlewareTests
{
    private static EdgeDevMiddleware CreateMiddleware(
        EdgeOptions? options = null,
        RequestDelegate? next = null)
    {
        options ??= new EdgeOptions { Enabled = true, Provider = "cloudflare" };
        var whitelist = new EdgeApiWhitelist();
        var checker = new EdgeCompatibilityChecker(whitelist, options);
        next ??= ctx => Task.CompletedTask;

        return new EdgeDevMiddleware(next, options, checker);
    }

    [Fact]
    public async Task InvokeAsync_AddsPreviewHeaders()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = new DefaultHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(context.Response.Headers.ContainsKey("x-edge-preview"));
        Assert.Equal("true", context.Response.Headers["x-edge-preview"]);
        Assert.Equal("cloudflare", context.Response.Headers["x-edge-provider"]);
    }

    [Fact]
    public async Task InvokeAsync_CompatibilityEndpoint_ReturnsJson()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = new DefaultHttpContext();
        context.Request.Path = "/__edge/compatibility";
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal("application/json", context.Response.ContentType);
        context.Response.Body.Position = 0;
        var reader = new StreamReader(context.Response.Body);
        var json = await reader.ReadToEndAsync();
        Assert.Contains("cloudflare", json);
        Assert.Contains("maxBundleSize", json);
    }

    [Fact]
    public async Task InvokeAsync_NormalPath_CallsNext()
    {
        // Arrange
        var nextCalled = false;
        var middleware = CreateMiddleware(next: ctx =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });
        var context = new DefaultHttpContext();
        context.Request.Path = "/my-page";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_NullContext_Throws()
    {
        var middleware = CreateMiddleware();
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            middleware.InvokeAsync(null!));
    }

    [Fact]
    public void Constructor_NullNext_Throws()
    {
        var options = new EdgeOptions();
        var whitelist = new EdgeApiWhitelist();
        var checker = new EdgeCompatibilityChecker(whitelist, options);

        Assert.Throws<ArgumentNullException>(() =>
            new EdgeDevMiddleware(null!, options, checker));
    }

    [Fact]
    public void Constructor_NullOptions_Throws()
    {
        var whitelist = new EdgeApiWhitelist();
        var checker = new EdgeCompatibilityChecker(whitelist);

        Assert.Throws<ArgumentNullException>(() =>
            new EdgeDevMiddleware(_ => Task.CompletedTask, null!, checker));
    }

    [Fact]
    public void Constructor_NullChecker_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new EdgeDevMiddleware(_ => Task.CompletedTask, new EdgeOptions(), null!));
    }
}
