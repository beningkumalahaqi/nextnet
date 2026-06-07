using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NextNet.Components;
using Xunit;

namespace NextNet.Core.Tests;

/// <summary>
/// Integration tests verifying the full API route lifecycle:
/// base class behavior, HTTP method dispatch, HttpContext propagation, and DI integration.
/// </summary>
public class ApiRouteIntegrationTests
{
    [Fact]
    public async Task ApiRoute_Get_ReturnsOkResult()
    {
        var route = new GetOnlyRoute();
        var ctx = CreateMockHttpContext();
        route.HttpContext = ctx;

        var result = await route.Get();
        var statusCode = await ExecuteAndGetStatus(result);

        Assert.Equal(200, statusCode);
    }

    [Fact]
    public async Task ApiRoute_Post_CreatesResource()
    {
        var route = new PostRoute();
        var ctx = CreateMockHttpContext();
        route.HttpContext = ctx;

        var result = await route.Post();
        var statusCode = await ExecuteAndGetStatus(result);
        // Created returns 201
        Assert.Equal(201, statusCode);
    }

    [Fact]
    public async Task ApiRoute_Delete_ReturnsNoContent()
    {
        var route = new DeleteRoute();
        var ctx = CreateMockHttpContext();
        route.HttpContext = ctx;

        var result = await route.Delete();
        var statusCode = await ExecuteAndGetStatus(result);

        Assert.Equal(204, statusCode);
    }

    [Fact]
    public async Task ApiRoute_Handle_ReturnsFallbackForUnoverriddenMethod()
    {
        var route = new GetOnlyRoute(); // Only overrides Get()
        var ctx = CreateMockHttpContext();
        route.HttpContext = ctx;

        // Put is not overridden — should fallback to Handle()
        var result = await route.Put();
        var statusCode = await ExecuteAndGetStatus(result);

        Assert.Equal(200, statusCode);
    }

    [Fact]
    public async Task ApiRoute_Handle_WithHttpContext_PropagatesToHandle()
    {
        var route = new GetOnlyRoute();
        var ctx = CreateMockHttpContext();

        var result = await route.Handle(ctx);
        var statusCode = await ExecuteAndGetStatus(result);

        Assert.Equal(200, statusCode);
    }

    [Fact]
    public async Task ApiRoute_OverrideGet_ReturnsCustomResponse()
    {
        var route = new CustomResponseRoute();
        var ctx = CreateMockHttpContext();
        route.HttpContext = ctx;

        var result = await route.Get();
        var statusCode = await ExecuteAndGetStatus(result);

        Assert.Equal(200, statusCode);
    }

    [Fact]
    public void ApiRoute_MultipleRoutes_HttpContextIsIndependent()
    {
        var ctx1 = CreateMockHttpContext();
        var ctx2 = CreateMockHttpContext();

        var route1 = new GetOnlyRoute { HttpContext = ctx1 };
        var route2 = new GetOnlyRoute { HttpContext = ctx2 };

        Assert.Same(ctx1, route1.HttpContext);
        Assert.Same(ctx2, route2.HttpContext);
        Assert.NotSame(route1.HttpContext, route2.HttpContext);
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
    /// ApiRoute that only overrides GET.
    /// </summary>
    private class GetOnlyRoute : ApiRoute
    {
        public override Task<IResult> Handle(HttpContext context)
            => Task.FromResult(Results.Ok("default-handle"));

        public override Task<IResult> Get()
            => Task.FromResult(Results.Ok(new { message = "Hello from GET" }));
    }

    /// <summary>
    /// ApiRoute that handles POST with Created response.
    /// </summary>
    private class PostRoute : ApiRoute
    {
        public override Task<IResult> Handle(HttpContext context)
            => Task.FromResult(Results.Ok("default-handle"));

        public override Task<IResult> Post()
            => Task.FromResult(Results.Created("/api/users/1", new { id = 1 }));
    }

    /// <summary>
    /// ApiRoute that handles DELETE with NoContent.
    /// </summary>
    private class DeleteRoute : ApiRoute
    {
        public override Task<IResult> Handle(HttpContext context)
            => Task.FromResult(Results.Ok("default-handle"));

        public override Task<IResult> Delete()
            => Task.FromResult(Results.NoContent());
    }

    /// <summary>
    /// ApiRoute with custom JSON response on GET.
    /// </summary>
    private class CustomResponseRoute : ApiRoute
    {
        public override Task<IResult> Handle(HttpContext context)
            => Task.FromResult(Results.Ok("default-handle"));

        public override Task<IResult> Get()
            => Task.FromResult(Results.Json(new { data = "custom" }));
    }
}
