using Microsoft.AspNetCore.Http;
using Moq;
using NextNet.Isr.Endpoints;
using NextNet.Isr.Middleware;
using NextNet.Isr.Revalidation;

namespace NextNet.Isr.Tests;

public class IsrRevalidationMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_Should_DelegateToEndpoint_When_RevalidationPath()
    {
        var manager = new Mock<IIsrRevalidationManager>(MockBehavior.Strict);
        var revalidator = new OnDemandRevalidator(manager.Object, new IsrGlobalOptions());
        var endpoint = new IsrRevalidationEndpoint(revalidator);

        var nextCalled = false;
        var middleware = new IsrRevalidationMiddleware(
            ctx =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            },
            endpoint);

        var context = new DefaultHttpContext();
        context.Request.Method = "POST";
        context.Request.Path = "/_isr/revalidate";

        await middleware.InvokeAsync(context);

        // Should not call next middleware since it matched the revalidation path
        Assert.False(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_Should_ForwardToNext_When_NonRevalidationPath()
    {
        var manager = new Mock<IIsrRevalidationManager>(MockBehavior.Strict);
        var revalidator = new OnDemandRevalidator(manager.Object, new IsrGlobalOptions());
        var endpoint = new IsrRevalidationEndpoint(revalidator);

        var nextCalled = false;
        var middleware = new IsrRevalidationMiddleware(
            ctx =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            },
            endpoint);

        var context = new DefaultHttpContext();
        context.Request.Path = "/about";

        await middleware.InvokeAsync(context);

        Assert.True(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_Should_ForwardToNext_When_SubPathOfRevalidation()
    {
        var manager = new Mock<IIsrRevalidationManager>(MockBehavior.Strict);
        var revalidator = new OnDemandRevalidator(manager.Object, new IsrGlobalOptions());
        var endpoint = new IsrRevalidationEndpoint(revalidator);

        var nextCalled = false;
        var middleware = new IsrRevalidationMiddleware(
            ctx =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            },
            endpoint);

        var context = new DefaultHttpContext();
        context.Request.Path = "/_isr/revalidate/extra";

        await middleware.InvokeAsync(context);

        Assert.True(nextCalled);
    }
}
