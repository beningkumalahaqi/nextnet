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
    public async Task InvokeAsync_Should_AddPreviewHeaders_When_Called()
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
    public async Task InvokeAsync_Should_ReturnJson_When_CompatibilityEndpoint()
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
    public async Task InvokeAsync_Should_CallNext_When_NormalPath()
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
    public async Task InvokeAsync_Should_Throw_When_ContextIsNull()
    {
        var middleware = CreateMiddleware();
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            middleware.InvokeAsync(null!));
    }

    [Fact]
    public void Constructor_Should_Throw_When_NextIsNull()
    {
        var options = new EdgeOptions();
        var whitelist = new EdgeApiWhitelist();
        var checker = new EdgeCompatibilityChecker(whitelist, options);

        Assert.Throws<ArgumentNullException>(() =>
            new EdgeDevMiddleware(null!, options, checker));
    }

    [Fact]
    public void Constructor_Should_Throw_When_OptionsIsNull()
    {
        var whitelist = new EdgeApiWhitelist();
        var checker = new EdgeCompatibilityChecker(whitelist);

        Assert.Throws<ArgumentNullException>(() =>
            new EdgeDevMiddleware(_ => Task.CompletedTask, null!, checker));
    }

    [Fact]
    public void Constructor_Should_Throw_When_CheckerIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new EdgeDevMiddleware(_ => Task.CompletedTask, new EdgeOptions(), null!));
    }
}
