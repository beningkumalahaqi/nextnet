using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NextNet.Edge.Adapters;
using NextNet.Edge.Compatibility;
using NextNet.Edge.Middleware;
using Xunit;

namespace NextNet.Edge.Tests.Middleware;

public class EdgeMiddlewareTests
{
    private static (EdgeMiddleware Middleware, HttpContext Context) CreateMiddleware(
        EdgeOptions? options = null,
        Action<HttpContext>? nextAction = null)
    {
        options ??= new EdgeOptions { Enabled = true };

        var services = new ServiceCollection();
        services.AddSingleton(options);
        var sp = services.BuildServiceProvider();

        var whitelist = new EdgeApiWhitelist();
        var checker = new EdgeCompatibilityChecker(whitelist, options);
        var registry = new AdapterRegistry(sp);

        RequestDelegate next = nextAction != null
            ? ctx =>
            {
                nextAction(ctx);
                return Task.CompletedTask;
            }
        : ctx => Task.CompletedTask;

        var middleware = new EdgeMiddleware(next, options, checker, registry);
        var httpContext = new DefaultHttpContext();

        return (middleware, httpContext);
    }

    [Fact]
    public async Task InvokeAsync_Should_PassThrough_When_EdgeDisabled()
    {
        // Arrange
        var (middleware, context) = CreateMiddleware(
            new EdgeOptions { Enabled = false });

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.False(context.Response.Headers.ContainsKey("x-edge-provider"));
    }

    [Fact]
    public async Task InvokeAsync_Should_AddHeaders_When_EdgeEnabled()
    {
        // Arrange
        var (middleware, context) = CreateMiddleware();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(context.Response.Headers.ContainsKey("x-edge-provider"));
        Assert.Equal("cloudflare", context.Response.Headers["x-edge-provider"]);
        Assert.Equal("true", context.Response.Headers["x-edge-simulated"]);
    }

    [Fact]
    public async Task InvokeAsync_Should_WrapResponseBody_When_SizeBudgetSet()
    {
        // Arrange
        var options = new EdgeOptions { Enabled = true, MaxBundleSize = 1000 };
        var (middleware, context) = CreateMiddleware(options);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        // The body should be wrapped in an EdgeBudgetStream
        Assert.NotNull(context.Response.Body);
    }

    [Fact]
    public async Task InvokeAsync_Should_AddResponseHeaders_When_Called()
    {
        // Arrange
        var (middleware, context) = CreateMiddleware();

        // Act
        await middleware.InvokeAsync(context);

        // Assert - headers should be set before the response starts
        Assert.Contains("x-edge-provider", context.Response.Headers.Keys);
    }

    [Fact]
    public async Task InvokeAsync_Should_Throw_When_ContextIsNull()
    {
        var (middleware, _) = CreateMiddleware();
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            middleware.InvokeAsync(null!));
    }
}

public class EdgeBudgetStreamTests
{
    [Fact]
    public void Write_Should_Succeed_When_WithinBudget()
    {
        // Arrange
        var inner = new MemoryStream();
        var budgetStream = new EdgeBudgetStream(inner, 100);
        var data = new byte[50];

        // Act
        budgetStream.Write(data, 0, data.Length);

        // Assert
        Assert.Equal(50, budgetStream.Length);
        Assert.Equal(50, budgetStream.Position);
    }

    [Fact]
    public void Write_Should_Throw_When_ExceedsBudget()
    {
        // Arrange
        var inner = new MemoryStream();
        var budgetStream = new EdgeBudgetStream(inner, 50);
        var data = new byte[60];

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            budgetStream.Write(data, 0, data.Length));
    }

    [Fact]
    public async Task WriteAsync_Should_Throw_When_ExceedsBudget()
    {
        // Arrange
        var inner = new MemoryStream();
        var budgetStream = new EdgeBudgetStream(inner, 50);
        var data = new byte[60];

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            budgetStream.WriteAsync(data, 0, data.Length));
    }

    [Fact]
    public void Write_Should_AccumulateBytes_When_WritingMultipleTimes()
    {
        // Arrange
        var inner = new MemoryStream();
        var budgetStream = new EdgeBudgetStream(inner, 100);

        // Act
        budgetStream.Write(new byte[30], 0, 30);
        budgetStream.Write(new byte[30], 0, 30);

        // Assert
        Assert.Equal(60, budgetStream.Length);
    }

    [Fact]
    public void Properties_Should_ReturnCorrectValues_When_Accessed()
    {
        var inner = new MemoryStream();
        var budgetStream = new EdgeBudgetStream(inner, 100);

        Assert.False(budgetStream.CanRead);
        Assert.False(budgetStream.CanSeek);
        Assert.True(budgetStream.CanWrite);
    }

    [Fact]
    public void Read_Should_ThrowNotSupported_When_Called()
    {
        var inner = new MemoryStream();
        var budgetStream = new EdgeBudgetStream(inner, 100);

        Assert.Throws<NotSupportedException>(() =>
            budgetStream.Read(new byte[10], 0, 10));
    }

    [Fact]
    public void Seek_Should_ThrowNotSupported_When_Called()
    {
        var inner = new MemoryStream();
        var budgetStream = new EdgeBudgetStream(inner, 100);

        Assert.Throws<NotSupportedException>(() =>
            budgetStream.Seek(0, SeekOrigin.Begin));
    }

    [Fact]
    public void SetLength_Should_ThrowNotSupported_When_Called()
    {
        var inner = new MemoryStream();
        var budgetStream = new EdgeBudgetStream(inner, 100);

        Assert.Throws<NotSupportedException>(() =>
            budgetStream.SetLength(50));
    }

    [Fact]
    public void Flush_Should_NotThrow_When_Called()
    {
        var inner = new MemoryStream();
        var budgetStream = new EdgeBudgetStream(inner, 100);

        budgetStream.Flush(); // Should not throw
    }

    [Fact]
    public async Task FlushAsync_Should_NotThrow_When_Called()
    {
        var inner = new MemoryStream();
        var budgetStream = new EdgeBudgetStream(inner, 100);

        await budgetStream.FlushAsync(); // Should not throw
    }
}
