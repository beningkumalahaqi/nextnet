using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NextNet.Middleware.BuiltIn;
using NextNet.Middleware.Attributes;
using NextNet.Middleware.Extensions;
using Xunit;

namespace NextNet.Middleware.Tests.Integration;

/// <summary>
/// Integration tests that verify the full middleware pipeline works end-to-end:
/// pipeline building, ordering, request execution, error handling, and
/// ASP.NET Core integration extension methods.
/// </summary>
public class MiddlewareIntegrationTests
{
    [Fact]
    public async Task FullPipeline_Should_ExecuteAllMiddlewareInCorrectOrder_When_Configured()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var sp = services.BuildServiceProvider();

        var pipeline = new MiddlewarePipeline();
        var executionLog = new List<string>();

        pipeline.Use(new LoggingActionMiddleware(executionLog, "Logging"), order: MiddlewareOrder.Logging);
        pipeline.Use(new LoggingActionMiddleware(executionLog, "StaticFile"), order: MiddlewareOrder.StaticFiles);
        pipeline.Use(new LoggingActionMiddleware(executionLog, "Compression"), order: MiddlewareOrder.Compression);
        pipeline.Use(new LoggingActionMiddleware(executionLog, "ErrorHandling"), order: MiddlewareOrder.ErrorHandling);

        var built = pipeline.Build(sp);
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = "GET";
        httpContext.Request.Path = "/test";

        // Act
        await built(httpContext);

        // Assert
        Assert.Equal(4, executionLog.Count);
        Assert.Equal("Logging", executionLog[0]);
        Assert.Equal("StaticFile", executionLog[1]);
        Assert.Equal("Compression", executionLog[2]);
        Assert.Equal("ErrorHandling", executionLog[3]);
    }

    [Fact]
    public async Task FullPipeline_Should_RespectRouteConditional_When_RequestMatchesAdmin()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var sp = services.BuildServiceProvider();

        var pipeline = new MiddlewarePipeline();
        var adminExecuted = false;
        var apiExecuted = false;

        pipeline.UseWhen(
            ctx => ctx.Request.Path.StartsWithSegments("/admin"),
            branch => branch.Use(new ActionMiddleware(_ => adminExecuted = true)));

        pipeline.UseWhen(
            ctx => ctx.Request.Path.StartsWithSegments("/api"),
            branch => branch.Use(new ActionMiddleware(_ => apiExecuted = true)));

        var built = pipeline.Build(sp);

        // Act - request to /admin
        var adminContext = new DefaultHttpContext();
        adminContext.Request.Path = "/admin/users";
        await built(adminContext);

        // Assert
        Assert.True(adminExecuted);
        Assert.False(apiExecuted);
    }

    [Fact]
    public async Task FullPipeline_Should_NotLeakConditions_When_RequestMatchesUnrelatedRoute()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var sp = services.BuildServiceProvider();

        var pipeline = new MiddlewarePipeline();
        var adminExecuted = false;
        var apiExecuted = false;

        pipeline.UseWhen(
            ctx => ctx.Request.Path.StartsWithSegments("/admin"),
            branch => branch.Use(new ActionMiddleware(_ => adminExecuted = true)));

        pipeline.UseWhen(
            ctx => ctx.Request.Path.StartsWithSegments("/api"),
            branch => branch.Use(new ActionMiddleware(_ => apiExecuted = true)));

        var built = pipeline.Build(sp);

        // Act - request to /public
        var publicContext = new DefaultHttpContext();
        publicContext.Request.Path = "/public/hello";
        await built(publicContext);

        // Assert
        Assert.False(adminExecuted);
        Assert.False(apiExecuted);
    }

    [Fact]
    public async Task FullPipeline_Should_CatchExceptions_When_ErrorHandlingMiddlewarePresent()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var sp = services.BuildServiceProvider();

        var pipeline = new MiddlewarePipeline();
        // ErrorHandling must wrap other middleware, so it gets a lower order
        pipeline.Use<ErrorHandlingMiddleware>(order: MiddlewareOrder.First);
        pipeline.Use(new ThrowingMiddleware(), order: MiddlewareOrder.Normal);

        var built = pipeline.Build(sp);
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();
        httpContext.Request.Method = "GET";
        httpContext.Request.Path = "/error";

        // Act
        await built(httpContext);

        // Assert
        Assert.Equal(500, httpContext.Response.StatusCode);
        Assert.Equal("application/json; charset=utf-8", httpContext.Response.ContentType);
    }

    [Fact]
    public async Task FullPipeline_Should_SetResponseHeaders_When_MiddlewareAddsThem()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var sp = services.BuildServiceProvider();

        var pipeline = new MiddlewarePipeline();
        pipeline.Use(new HeaderMiddleware("X-Custom", "test-value"), order: 0);
        pipeline.Use(new HeaderMiddleware("X-Another", "another-value"), order: 100);

        var built = pipeline.Build(sp);
        var httpContext = new DefaultHttpContext();

        // Act
        await built(httpContext);

        // Assert
        Assert.Equal("test-value", httpContext.Response.Headers["X-Custom"]);
        Assert.Equal("another-value", httpContext.Response.Headers["X-Another"]);
    }

    [Fact]
    public async Task FullPipeline_Should_FlowDataBetweenMiddleware_When_UsingItems()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var sp = services.BuildServiceProvider();

        var pipeline = new MiddlewarePipeline();
        var finalValue = "";

        // Use HttpContext.Items for cross-middleware data sharing
        pipeline.Use(new ActionMiddleware(ctx => ctx.HttpContext.Items["user"] = "john"), order: 0);
        pipeline.Use(new ActionMiddleware(ctx => ctx.HttpContext.Items["role"] = "admin"), order: 50);
        pipeline.Use(new ActionMiddleware(ctx =>
        {
            var user = ctx.HttpContext.Items["user"]?.ToString() ?? "";
            var role = ctx.HttpContext.Items["role"]?.ToString() ?? "";
            finalValue = $"{user}:{role}";
        }), order: 100);

        var built = pipeline.Build(sp);
        var httpContext = new DefaultHttpContext();

        // Act
        await built(httpContext);

        // Assert
        Assert.Equal("john:admin", finalValue);
    }

    [Fact]
    public async Task FullPipeline_Should_ChainTerminalDelegate_When_BuiltWithTerminal()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var sp = services.BuildServiceProvider();

        var pipeline = new MiddlewarePipeline();
        var middlewareExecuted = false;
        var terminalExecuted = false;

        pipeline.Use(new ActionMiddleware(_ => middlewareExecuted = true), order: 0);

        var built = pipeline.Build(sp, _ =>
        {
            terminalExecuted = true;
            return Task.CompletedTask;
        });

        var httpContext = new DefaultHttpContext();

        // Act
        await built(httpContext);

        // Assert
        Assert.True(middlewareExecuted);
        Assert.True(terminalExecuted);
    }

    [Fact]
    public void AddNextNetMiddleware_Should_RegisterBuiltInServices_When_Called()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddNextNetMiddleware();

        var sp = services.BuildServiceProvider();

        // Assert
        var pipeline = sp.GetService<MiddlewarePipeline>();
        Assert.NotNull(pipeline);
        Assert.Equal(6, pipeline.Registrations.Count); // 6 built-in middleware (Logging, Cors, SecurityHeaders, StaticFiles, Compression, ErrorHandling)
    }

    [Fact]
    public void AddNextNetMiddleware_Should_AddUserMiddleware_When_ConfigureCallbackProvided()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddNextNetMiddleware(pipeline =>
        {
            pipeline.Use<TestMiddleware>();
        });

        var sp = services.BuildServiceProvider();

        // Assert
        var pipeline = sp.GetService<MiddlewarePipeline>();
        Assert.NotNull(pipeline);
        Assert.Equal(7, pipeline.Registrations.Count); // 6 built-in + 1 user
    }

    [Fact]
    public void UseMiddlewareAttribute_Should_CaptureMetadata_When_AppliedToClass()
    {
        // Arrange
        var attributes = typeof(TestPage)
            .GetCustomAttributes(typeof(UseMiddlewareAttribute), inherit: true)
            .Cast<UseMiddlewareAttribute>()
            .ToList();

        // Assert
        Assert.Equal(2, attributes.Count);
        Assert.Contains(attributes, a => a.MiddlewareType == typeof(LoggingMiddleware));
        Assert.Contains(attributes, a => a.MiddlewareType == typeof(ErrorHandlingMiddleware));
    }

    [Fact]
    public void UseMiddlewareAttribute_Should_ThrowArgumentNullException_When_TypeIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new UseMiddlewareAttribute(null!));
    }

    [Fact]
    public void UseMiddlewareAttribute_Should_ThrowArgumentException_When_NonMiddlewareType()
    {
        var ex = Assert.Throws<ArgumentException>(() => new UseMiddlewareAttribute(typeof(string)));
        Assert.Contains("DS-700", ex.Message);
    }

    [Fact]
    public void UseMiddlewareAttribute_Should_DefaultToNormalOrder_When_NotSpecified()
    {
        // Arrange
        var attr = new UseMiddlewareAttribute(typeof(TestMiddleware));

        // Assert
        Assert.Equal(MiddlewareOrder.Normal, attr.Order);
    }

    [Fact]
    public void UseMiddlewareAttribute_Should_StoreRoutes_When_Set()
    {
        // Arrange
        var attr = new UseMiddlewareAttribute(typeof(TestMiddleware))
        {
            Routes = new[] { "/admin/*", "/api/*" }
        };

        // Assert
        Assert.NotNull(attr.Routes);
        Assert.Equal(2, attr.Routes.Length);
        Assert.Equal("/admin/*", attr.Routes[0]);
    }

    [Fact]
    public async Task Clone_Should_BuildIndependently_When_ClonedAndBuilt()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var sp = services.BuildServiceProvider();

        var original = new MiddlewarePipeline();
        original.Use<TestMiddleware>(order: 10);

        var clone = original.Clone();
        clone.Use<TestMiddleware>(order: 20);

        // Act
        var originalBuilt = original.Build(sp);
        var cloneBuilt = clone.Build(sp);

        // Assert - both should execute without error
        var ctx1 = new DefaultHttpContext();
        var ctx2 = new DefaultHttpContext();

        await originalBuilt(ctx1);
        await cloneBuilt(ctx2);

        Assert.Single(original.Registrations);
        Assert.Equal(2, clone.Registrations.Count);
    }

    [Fact]
    public async Task Build_Should_ReturnCachedPipeline_When_SameConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var sp = services.BuildServiceProvider();

        var pipeline = new MiddlewarePipeline();
        pipeline.Use<TestMiddleware>();

        // Act
        var first = pipeline.Build(sp);
        var second = pipeline.Build(sp);

        // Assert
        Assert.Same(first, second);

        // Both should work
        await first(new DefaultHttpContext());
        await second(new DefaultHttpContext());
    }
}

#region Integration Test Helpers

/// <summary>
/// Middleware that logs its name to a shared list for order verification.
/// </summary>
public class LoggingActionMiddleware : IMiddleware
{
    private readonly List<string> _log;
    private readonly string _name;

    public LoggingActionMiddleware(List<string> log, string name)
    {
        _log = log;
        _name = name;
    }

    public Task InvokeAsync(MiddlewareContext context, RequestDelegate next)
    {
        _log.Add(_name);
        return next(context.HttpContext);
    }
}

/// <summary>
/// Middleware that always throws an exception.
/// </summary>
public class ThrowingMiddleware : IMiddleware
{
    public Task InvokeAsync(MiddlewareContext context, RequestDelegate next)
    {
        throw new InvalidOperationException("Test exception from middleware");
    }
}

/// <summary>
/// Middleware that sets a response header.
/// </summary>
public class HeaderMiddleware : IMiddleware
{
    private readonly string _headerName;
    private readonly string _headerValue;

    public HeaderMiddleware(string headerName, string headerValue)
    {
        _headerName = headerName;
        _headerValue = headerValue;
    }

    public Task InvokeAsync(MiddlewareContext context, RequestDelegate next)
    {
        context.HttpContext.Response.Headers[_headerName] = _headerValue;
        return next(context.HttpContext);
    }
}

/// <summary>
/// Test page class with UseMiddleware attributes for testing attribute metadata.
/// </summary>
[UseMiddleware(typeof(LoggingMiddleware))]
[UseMiddleware(typeof(ErrorHandlingMiddleware))]
public class TestPage
{
    // No actual page logic needed for attribute tests
}

#endregion
