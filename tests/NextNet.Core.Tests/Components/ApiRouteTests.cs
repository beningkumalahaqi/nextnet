using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NextNet.Components;
using Xunit;

namespace NextNet.Core.Tests.Components;

/// <summary>
/// Tests for the <see cref="ApiRoute"/> base class that API route handlers inherit from.
/// </summary>
public class ApiRouteTests
{
    [Fact]
    public void HttpContext_Should_BeNull_When_NotSet()
    {
        var route = new TestApiRoute();
        Assert.Null(route.HttpContext);
    }

    [Fact]
    public void HttpContext_Should_BeSet_When_Assigned()
    {
        var ctx = CreateMockHttpContext();
        var route = new TestApiRoute { HttpContext = ctx };
        Assert.Same(ctx, route.HttpContext);
    }

    [Fact]
    public void Get_Should_DelegateToHandle_When_NotOverridden()
    {
        var ctx = CreateMockHttpContext();
        var route = new TestApiRoute { HttpContext = ctx };
        var ignored = route.Get();

        Assert.True(route.HandleWasCalled);
        Assert.Same(ctx, route.HandleContext);
    }

    [Fact]
    public void Post_Should_DelegateToHandle_When_NotOverridden()
    {
        var ctx = CreateMockHttpContext();
        var route = new TestApiRoute { HttpContext = ctx };
        var ignored = route.Post();

        Assert.True(route.HandleWasCalled);
        Assert.Same(ctx, route.HandleContext);
    }

    [Fact]
    public void Put_Should_DelegateToHandle_When_NotOverridden()
    {
        var ctx = CreateMockHttpContext();
        var route = new TestApiRoute { HttpContext = ctx };
        var ignored = route.Put();

        Assert.True(route.HandleWasCalled);
        Assert.Same(ctx, route.HandleContext);
    }

    [Fact]
    public void Patch_Should_DelegateToHandle_When_NotOverridden()
    {
        var ctx = CreateMockHttpContext();
        var route = new TestApiRoute { HttpContext = ctx };
        var ignored = route.Patch();

        Assert.True(route.HandleWasCalled);
        Assert.Same(ctx, route.HandleContext);
    }

    [Fact]
    public void Delete_Should_DelegateToHandle_When_NotOverridden()
    {
        var ctx = CreateMockHttpContext();
        var route = new TestApiRoute { HttpContext = ctx };
        var ignored = route.Delete();

        Assert.True(route.HandleWasCalled);
        Assert.Same(ctx, route.HandleContext);
    }

    [Fact]
    public async Task Get_Should_Throw_When_HttpContextNotSet()
    {
        var route = new TestApiRoute();
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => route.Get());
        Assert.Contains("HttpContext is not set", ex.Message);
    }

    [Fact]
    public async Task Post_Should_Throw_When_HttpContextNotSet()
    {
        var route = new TestApiRoute();
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => route.Post());
        Assert.Contains("HttpContext is not set", ex.Message);
    }

    [Fact]
    public async Task Put_Should_Throw_When_HttpContextNotSet()
    {
        var route = new TestApiRoute();
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => route.Put());
        Assert.Contains("HttpContext is not set", ex.Message);
    }

    [Fact]
    public async Task Patch_Should_Throw_When_HttpContextNotSet()
    {
        var route = new TestApiRoute();
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => route.Patch());
        Assert.Contains("HttpContext is not set", ex.Message);
    }

    [Fact]
    public async Task Delete_Should_Throw_When_HttpContextNotSet()
    {
        var route = new TestApiRoute();
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => route.Delete());
        Assert.Contains("HttpContext is not set", ex.Message);
    }

    [Fact]
    public async Task Get_Should_NotCallHandle_When_Overridden()
    {
        var ctx = CreateMockHttpContext();
        var route = new TestApiRouteWithOverrides { HttpContext = ctx };
        var result = await route.Get();

        Assert.False(route.HandleWasCalled);
        var statusCode = await ExecuteAndGetStatus(result);
        Assert.Equal(200, statusCode);
    }

    [Fact]
    public async Task Post_Should_NotCallHandle_When_Overridden()
    {
        var ctx = CreateMockHttpContext();
        var route = new TestApiRouteWithOverrides { HttpContext = ctx };
        var result = await route.Post();

        Assert.False(route.HandleWasCalled);
        var statusCode = await ExecuteAndGetStatus(result);
        Assert.Equal(200, statusCode);
    }

    [Fact]
    public async Task Put_Should_NotCallHandle_When_Overridden()
    {
        var ctx = CreateMockHttpContext();
        var route = new TestApiRouteWithOverrides { HttpContext = ctx };
        var result = await route.Put();

        Assert.False(route.HandleWasCalled);
        var statusCode = await ExecuteAndGetStatus(result);
        Assert.Equal(200, statusCode);
    }

    [Fact]
    public async Task Patch_Should_NotCallHandle_When_Overridden()
    {
        var ctx = CreateMockHttpContext();
        var route = new TestApiRouteWithOverrides { HttpContext = ctx };
        var result = await route.Patch();

        Assert.False(route.HandleWasCalled);
        var statusCode = await ExecuteAndGetStatus(result);
        Assert.Equal(200, statusCode);
    }

    [Fact]
    public async Task Delete_Should_NotCallHandle_When_Overridden()
    {
        var ctx = CreateMockHttpContext();
        var route = new TestApiRouteWithOverrides { HttpContext = ctx };
        var result = await route.Delete();

        Assert.False(route.HandleWasCalled);
        var statusCode = await ExecuteAndGetStatus(result);
        Assert.Equal(204, statusCode);
    }

    private static async Task<int> ExecuteAndGetStatus(IResult result)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.RequestServices = CreateServiceProvider();
        httpContext.Response.Body = new MemoryStream();
        await result.ExecuteAsync(httpContext);
        return httpContext.Response.StatusCode;
    }

    private static HttpContext CreateMockHttpContext()
    {
        var ctx = new DefaultHttpContext();
        ctx.RequestServices = CreateServiceProvider();
        ctx.Features.Get<IHttpResponseFeature>()!.Headers = new HeaderDictionary();
        return ctx;
    }

    private static IServiceProvider CreateServiceProvider()
    {
        return new TestServiceProvider();
    }

    /// <summary>
    /// Minimal service provider for test execution.
    /// Provides ILoggerFactory via NullLoggerFactory.
    /// </summary>
    private class TestServiceProvider : IServiceProvider
    {
        private readonly ILoggerFactory _loggerFactory = NullLoggerFactory.Instance;

        public object? GetService(Type serviceType)
        {
            if (serviceType == typeof(ILoggerFactory))
                return _loggerFactory;
            if (serviceType == typeof(ILogger<>))
                return null;
            return null;
        }
    }

    /// <summary>
    /// Minimal ApiRoute implementation for testing base class behavior.
    /// </summary>
    private class TestApiRoute : ApiRoute
    {
        public bool HandleWasCalled { get; private set; }
        public HttpContext? HandleContext { get; private set; }

        public override Task<IResult> Handle(HttpContext context)
        {
            HandleWasCalled = true;
            HandleContext = context;
            return Task.FromResult(Results.Ok("handle-called"));
        }
    }

    /// <summary>
    /// ApiRoute implementation with specific HTTP method overrides.
    /// </summary>
    private class TestApiRouteWithOverrides : ApiRoute
    {
        public bool HandleWasCalled { get; private set; }

        public override Task<IResult> Handle(HttpContext context)
        {
            HandleWasCalled = true;
            return Task.FromResult(Results.Ok("handle-called"));
        }

        public override Task<IResult> Get()
            => Task.FromResult(Results.Ok("get-ok"));

        public override Task<IResult> Post()
            => Task.FromResult(Results.Ok("post-ok"));

        public override Task<IResult> Put()
            => Task.FromResult(Results.Ok("put-ok"));

        public override Task<IResult> Patch()
            => Task.FromResult(Results.Ok("patch-ok"));

        public override Task<IResult> Delete()
            => Task.FromResult(Results.NoContent());
    }
}
