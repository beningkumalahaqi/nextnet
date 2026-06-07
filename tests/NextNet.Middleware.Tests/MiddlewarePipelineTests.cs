using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NextNet.Middleware.Attributes;
using Xunit;

namespace NextNet.Middleware.Tests;

public class MiddlewarePipelineTests
{
    private static (MiddlewarePipeline Pipeline, IServiceProvider Services) CreatePipeline()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        var sp = services.BuildServiceProvider();
        var pipeline = new MiddlewarePipeline();
        return (pipeline, sp);
    }

    [Fact]
    public void Use_ValidMiddleware_AddsRegistration()
    {
        // Arrange
        var (pipeline, _) = CreatePipeline();

        // Act
        pipeline.Use<TestMiddleware>();

        // Assert
        Assert.Single(pipeline.Registrations);
        Assert.Equal(typeof(TestMiddleware), pipeline.Registrations[0].MiddlewareType);
    }

    [Fact]
    public void Use_WithExplicitOrder_OverridesAttribute()
    {
        // Arrange
        var (pipeline, _) = CreatePipeline();

        // Act
        pipeline.Use<TestMiddleware>(order: 999);

        // Assert
        Assert.Equal(999, pipeline.Registrations[0].Order);
    }

    [Fact]
    public void Use_Instance_AddsRegistration()
    {
        // Arrange
        var (pipeline, _) = CreatePipeline();

        // Act
        pipeline.Use(new ActionMiddleware(_ => { }), order: 50);

        // Assert
        Assert.Single(pipeline.Registrations);
        Assert.NotNull(pipeline.Registrations[0].InstanceOverride);
        Assert.Equal(50, pipeline.Registrations[0].Order);
    }

    [Fact]
    public void Use_TypeNotImplementingIMiddleware_Throws()
    {
        // Arrange
        var (pipeline, _) = CreatePipeline();

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => pipeline.Use(typeof(string)));
        Assert.Contains("does not implement IMiddleware", ex.Message);
    }

    [Fact]
    public void Use_NullType_Throws()
    {
        // Arrange
        var (pipeline, _) = CreatePipeline();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => pipeline.Use((Type)null!));
    }

    [Fact]
    public void Use_NullInstance_Throws()
    {
        // Arrange
        var (pipeline, _) = CreatePipeline();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => pipeline.Use((IMiddleware)null!));
    }

    [Fact]
    public void UseWhen_PredicateMiddleware_AddsRegistration()
    {
        // Arrange
        var (pipeline, _) = CreatePipeline();

        // Act
        pipeline.UseWhen<TestMiddleware>(ctx => ctx.Request.Path.StartsWithSegments("/api"));

        // Assert
        Assert.Single(pipeline.Registrations);
        Assert.NotNull(pipeline.Registrations[0].Predicate);
    }

    [Fact]
    public void UseWhen_NullPredicate_Throws()
    {
        // Arrange
        var (pipeline, _) = CreatePipeline();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => pipeline.UseWhen<TestMiddleware>(null!));
    }

    [Fact]
    public void UseWhen_BranchPipeline_AddsRegistration()
    {
        // Arrange
        var (pipeline, _) = CreatePipeline();

        // Act
        pipeline.UseWhen(
            ctx => ctx.Request.Path.StartsWithSegments("/admin"),
            branch => branch.Use<TestMiddleware>());

        // Assert
        Assert.Single(pipeline.Registrations);
        Assert.NotNull(pipeline.Registrations[0].Branch);
        Assert.Null(pipeline.Registrations[0].MiddlewareType);
    }

    [Fact]
    public void UseWhen_BranchNullPredicate_Throws()
    {
        // Arrange
        var (pipeline, _) = CreatePipeline();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => pipeline.UseWhen(null!, branch => { }));
    }

    [Fact]
    public void UseWhen_BranchNullConfigure_Throws()
    {
        // Arrange
        var (pipeline, _) = CreatePipeline();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            pipeline.UseWhen(ctx => true, null!));
    }

    [Fact]
    public async Task Build_ExecutesMiddlewareInOrder()
    {
        // Arrange
        var (pipeline, sp) = CreatePipeline();
        var executionOrder = new List<int>();

        pipeline.Use(new OrderTrackingInstanceMiddleware(executionOrder, 100), order: 100);
        pipeline.Use(new OrderTrackingInstanceMiddleware(executionOrder, 0), order: 0);
        pipeline.Use(new OrderTrackingInstanceMiddleware(executionOrder, 50), order: 50);

        var built = pipeline.Build(sp);
        var ctx = new DefaultHttpContext();

        // Act
        await built(ctx);

        // Assert
        Assert.Equal(3, executionOrder.Count);
        Assert.Equal(0, executionOrder[0]);
        Assert.Equal(50, executionOrder[1]);
        Assert.Equal(100, executionOrder[2]);
    }

    [Fact]
    public async Task Build_MiddlewareCanShortCircuit()
    {
        // Arrange
        var (pipeline, sp) = CreatePipeline();
        var shortCircuitExecuted = false;
        var downstreamExecuted = false;

        pipeline.Use(new ShortCircuitInstanceMiddleware(() => shortCircuitExecuted = true), order: 0);
        pipeline.Use(new ActionMiddleware(_ => downstreamExecuted = true), order: 100);

        var built = pipeline.Build(sp);
        var ctx = new DefaultHttpContext();

        // Act
        await built(ctx);

        // Assert
        Assert.True(shortCircuitExecuted);
        Assert.False(downstreamExecuted);
    }

    [Fact]
    public async Task Build_ConditionalMiddleware_SkipsWhenPredicateFalse()
    {
        // Arrange
        var (pipeline, sp) = CreatePipeline();
        var middleware1Invoked = false;
        var conditionalInvoked = false;

        pipeline.Use(new ActionMiddleware(_ => middleware1Invoked = true), order: 0);
        pipeline.UseWhen<TestMiddleware>(ctx => false);
        // Need to set up TestMiddleware.OnInvoke to track invocation
        TestMiddleware.OnInvoke = (_, _) =>
        {
            conditionalInvoked = true;
            return Task.CompletedTask;
        };
        pipeline.Use(new ActionMiddleware(_ => { }), order: 200);

        var built = pipeline.Build(sp);
        var ctx = new DefaultHttpContext();

        // Act
        await built(ctx);

        // Assert
        Assert.True(middleware1Invoked);
        Assert.False(conditionalInvoked);
    }

    [Fact]
    public async Task Build_ConditionalMiddleware_ExecutesWhenPredicateTrue()
    {
        // Arrange
        var (pipeline, sp) = CreatePipeline();
        var conditionalInvoked = false;

        TestMiddleware.OnInvoke = (_, _) =>
        {
            conditionalInvoked = true;
            return Task.CompletedTask;
        };
        pipeline.UseWhen<TestMiddleware>(ctx => true);

        var built = pipeline.Build(sp);
        var ctx = new DefaultHttpContext();

        // Act
        await built(ctx);

        // Assert
        Assert.True(conditionalInvoked);
    }

    [Fact]
    public async Task Build_BranchPipeline_ExecutesOnPredicateMatch()
    {
        // Arrange
        var (pipeline, sp) = CreatePipeline();
        var branchExecuted = false;

        pipeline.UseWhen(
            ctx => true,
            branch => branch.Use(new ActionMiddleware(_ => branchExecuted = true)));

        var built = pipeline.Build(sp);
        var ctx = new DefaultHttpContext();

        // Act
        await built(ctx);

        // Assert
        Assert.True(branchExecuted);
    }

    [Fact]
    public async Task Build_BranchPipeline_SkipsOnPredicateFalse()
    {
        // Arrange
        var (pipeline, sp) = CreatePipeline();
        var branchExecuted = false;

        pipeline.UseWhen(
            ctx => false,
            branch => branch.Use(new ActionMiddleware(_ => branchExecuted = true)));

        var built = pipeline.Build(sp);
        var ctx = new DefaultHttpContext();

        // Act
        await built(ctx);

        // Assert
        Assert.False(branchExecuted);
    }

    [Fact]
    public async Task Build_PipelineInvokesTerminalDelegate()
    {
        // Arrange
        var (pipeline, sp) = CreatePipeline();
        var terminalInvoked = false;

        var built = pipeline.Build(sp, _ =>
        {
            terminalInvoked = true;
            return Task.CompletedTask;
        });

        var ctx = new DefaultHttpContext();

        // Act
        await built(ctx);

        // Assert
        Assert.True(terminalInvoked);
    }

    [Fact]
    public async Task Build_EmptyPipeline_PassesThrough()
    {
        // Arrange
        var (pipeline, sp) = CreatePipeline();
        var built = pipeline.Build(sp);
        var ctx = new DefaultHttpContext();

        // Act - should not throw
        await built(ctx);

        // Assert
        Assert.Equal(200, ctx.Response.StatusCode);
    }

    [Fact]
    public void Build_NullServiceProvider_Throws()
    {
        // Arrange
        var pipeline = new MiddlewarePipeline();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => pipeline.Build(null!));
    }

    [Fact]
    public void Build_CachesResult()
    {
        // Arrange
        var (pipeline, sp) = CreatePipeline();
        pipeline.Use<TestMiddleware>();

        // Act
        var first = pipeline.Build(sp);
        var second = pipeline.Build(sp);

        // Assert
        Assert.Same(first, second);
    }

    [Fact]
    public void Build_InvalidatesCache_WhenNewMiddlewareAdded()
    {
        // Arrange
        var (pipeline, sp) = CreatePipeline();
        pipeline.Use<TestMiddleware>(order: 0);
        var first = pipeline.Build(sp);

        // Act
        pipeline.Use<TestMiddleware>(order: 100);
        var second = pipeline.Build(sp);

        // Assert
        Assert.NotSame(first, second);
    }

    [Fact]
    public void Clone_CreatesIndependentCopy()
    {
        // Arrange
        var (pipeline, _) = CreatePipeline();
        pipeline.Use<TestMiddleware>(order: 10);

        // Act
        var clone = pipeline.Clone();
        clone.Use<TestMiddleware>(order: 20);

        // Assert
        Assert.Single(pipeline.Registrations);
        Assert.Equal(2, clone.Registrations.Count);
    }

    [Fact]
    public void Use_MultipleMiddleware_RegistersAll()
    {
        // Arrange
        var (pipeline, _) = CreatePipeline();

        // Act
        pipeline.Use<TestMiddleware>();
        pipeline.Use<AnotherTestMiddleware>();

        // Assert
        Assert.Equal(2, pipeline.Registrations.Count);
    }

    [Fact]
    public async Task Build_MiddlewareReceivesContext()
    {
        // Arrange
        var (pipeline, sp) = CreatePipeline();
        MiddlewareContext? capturedContext = null;

        pipeline.Use(new ContextCapturingMiddleware(ctx => capturedContext = ctx));

        var built = pipeline.Build(sp);
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/test";

        // Act
        await built(httpContext);

        // Assert
        Assert.NotNull(capturedContext);
        Assert.Same(httpContext, capturedContext.HttpContext);
        Assert.NotNull(capturedContext.Items);
        Assert.Same(pipeline, capturedContext.Pipeline);
    }

    [Fact]
    public async Task Build_MiddlewareItems_ShareData()
    {
        // Arrange
        var (pipeline, sp) = CreatePipeline();
        var itemValue = "";

        // Use HttpContext.Items for cross-middleware data sharing
        pipeline.Use(new ActionMiddleware(ctx => ctx.HttpContext.Items["key"] = "value"), order: 0);
        pipeline.Use(new ActionMiddleware(ctx => itemValue = ctx.HttpContext.Items["key"]?.ToString() ?? ""), order: 100);

        var built = pipeline.Build(sp);
        var httpContext = new DefaultHttpContext();

        // Act
        await built(httpContext);

        // Assert
        Assert.Equal("value", itemValue);
    }

    [Fact]
    public async Task Build_WithTerminalDelegate_NoMiddleware_ExecutesTerminal()
    {
        // Arrange
        var (pipeline, sp) = CreatePipeline();
        var terminalInvoked = false;

        var built = pipeline.Build(sp, _ =>
        {
            terminalInvoked = true;
            return Task.CompletedTask;
        });
        var ctx = new DefaultHttpContext();

        // Act
        await built(ctx);

        // Assert
        Assert.True(terminalInvoked);
    }
}

#region Test Middleware Classes

/// <summary>
/// Simple test middleware that does nothing.
/// </summary>
public class TestMiddleware : IMiddleware
{
    public static Func<MiddlewareContext, RequestDelegate, Task>? OnInvoke { get; set; }

    public Task InvokeAsync(MiddlewareContext context, RequestDelegate next)
    {
        if (OnInvoke != null)
            return OnInvoke(context, next);
        return next(context.HttpContext);
    }
}

/// <summary>
/// Test middleware that tracks execution order.
/// </summary>
public class OrderTrackingInstanceMiddleware : IMiddleware
{
    private readonly List<int> _executionOrder;
    private readonly int _order;

    public OrderTrackingInstanceMiddleware(List<int> executionOrder, int order)
    {
        _executionOrder = executionOrder;
        _order = order;
    }

    public Task InvokeAsync(MiddlewareContext context, RequestDelegate next)
    {
        _executionOrder.Add(_order);
        return next(context.HttpContext);
    }
}

/// <summary>
/// Middleware that short-circuits by not calling next.
/// </summary>
public class ShortCircuitInstanceMiddleware : IMiddleware
{
    private readonly Action _onInvoke;

    public ShortCircuitInstanceMiddleware(Action onInvoke)
    {
        _onInvoke = onInvoke;
    }

    public Task InvokeAsync(MiddlewareContext context, RequestDelegate next)
    {
        _onInvoke();
        // Don't call next — short circuit
        return Task.CompletedTask;
    }
}

/// <summary>
/// Middleware that wraps an Action{MiddlewareContext} delegate.
/// </summary>
public class ActionMiddleware : IMiddleware
{
    private readonly Action<MiddlewareContext> _action;

    public ActionMiddleware(Action<MiddlewareContext> action)
    {
        _action = action;
    }

    public Task InvokeAsync(MiddlewareContext context, RequestDelegate next)
    {
        _action(context);
        return next(context.HttpContext);
    }
}

/// <summary>
/// Middleware that captures the MiddlewareContext for assertions.
/// </summary>
public class ContextCapturingMiddleware : IMiddleware
{
    private readonly Action<MiddlewareContext> _capture;

    public ContextCapturingMiddleware(Action<MiddlewareContext> capture)
    {
        _capture = capture;
    }

    public Task InvokeAsync(MiddlewareContext context, RequestDelegate next)
    {
        _capture(context);
        return next(context.HttpContext);
    }
}

/// <summary>
/// Another test middleware for multi-registration tests.
/// </summary>
public class AnotherTestMiddleware : IMiddleware
{
    public Task InvokeAsync(MiddlewareContext context, RequestDelegate next)
    {
        return next(context.HttpContext);
    }
}

#endregion
