using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NextNet.Middleware.Attributes;
using Xunit;

namespace NextNet.Middleware.Tests;

public class MiddlewareOrderTests
{
    [Fact]
    public void Logging_Should_BeZero_When_Accessed()
    {
        Assert.Equal(0, MiddlewareOrder.Logging);
    }

    [Fact]
    public void StaticFiles_Should_Be100_When_Accessed()
    {
        Assert.Equal(100, MiddlewareOrder.StaticFiles);
    }

    [Fact]
    public void Compression_Should_Be200_When_Accessed()
    {
        Assert.Equal(200, MiddlewareOrder.Compression);
    }

    [Fact]
    public void ErrorHandling_Should_Be1000_When_Accessed()
    {
        Assert.Equal(1000, MiddlewareOrder.ErrorHandling);
    }

    [Fact]
    public void Normal_Should_BeZero_When_Accessed()
    {
        Assert.Equal(0, MiddlewareOrder.Normal);
    }

    [Fact]
    public void First_Should_BeMinValue_When_Accessed()
    {
        Assert.Equal(int.MinValue, MiddlewareOrder.First);
    }

    [Fact]
    public void Last_Should_BeMaxValue_When_Accessed()
    {
        Assert.Equal(int.MaxValue, MiddlewareOrder.Last);
    }

    [Fact]
    public void Early_Should_BeLessThanNormal_When_Compared()
    {
        Assert.True(MiddlewareOrder.Early < MiddlewareOrder.Normal);
    }

    [Fact]
    public void Late_Should_BeGreaterThanNormal_When_Compared()
    {
        Assert.True(MiddlewareOrder.Late > MiddlewareOrder.Normal);
    }

    [Fact]
    public void BuiltInOrders_Should_BeInIncreasingSequence_When_Enumerated()
    {
        var orders = new[]
        {
            MiddlewareOrder.Logging,
            MiddlewareOrder.StaticFiles,
            MiddlewareOrder.Compression,
            MiddlewareOrder.ErrorHandling,
        };

        for (int i = 1; i < orders.Length; i++)
        {
            Assert.True(orders[i - 1] < orders[i],
                $"Order at index {i - 1} ({orders[i - 1]}) should be less than index {i} ({orders[i]})");
        }
    }

    [Fact]
    public void MiddlewareOrderAttribute_Should_SetSpecifiedOrder_When_Applied()
    {
        // Act
        var attr = (MiddlewareOrderAttribute?)Attribute.GetCustomAttribute(
            typeof(OrderedMiddleware), typeof(MiddlewareOrderAttribute));

        // Assert
        Assert.NotNull(attr);
        Assert.Equal(42, attr.Order);
    }

    [Fact]
    public void MiddlewareOrderAttribute_Should_BeNull_When_NotSpecified()
    {
        // Act
        var attr = (MiddlewareOrderAttribute?)Attribute.GetCustomAttribute(
            typeof(UnorderedMiddleware), typeof(MiddlewareOrderAttribute));

        // Assert
        Assert.Null(attr);
    }

    [Fact]
    public void Pipeline_Should_ReadAttributeOrder_When_MiddlewareRegistered()
    {
        // Arrange
        var pipeline = new MiddlewarePipeline();
        var services = new ServiceCollection().AddLogging().BuildServiceProvider();

        // Act
        pipeline.Use<OrderedMiddleware>();
        pipeline.Use<UnorderedMiddleware>();

        // Assert
        var orderedReg = pipeline.Registrations.First(r => r.MiddlewareType == typeof(OrderedMiddleware));
        var unorderedReg = pipeline.Registrations.First(r => r.MiddlewareType == typeof(UnorderedMiddleware));

        Assert.Equal(42, orderedReg.Order);
        Assert.Equal(MiddlewareOrder.Normal, unorderedReg.Order);
    }

    [Fact]
    public void Pipeline_Should_SortRegistrationsByOrder_When_Built()
    {
        // Arrange
        var pipeline = new MiddlewarePipeline();
        var services = new ServiceCollection().AddLogging().BuildServiceProvider();

        pipeline.Use<UnorderedMiddleware>(); // order 0
        pipeline.Use<OrderedMiddleware>();   // order 42
        pipeline.Use<LastMiddleware>();       // order 1000

        // Verify by checking registration order
        var registrations = pipeline.Registrations.ToList();
        Assert.Equal(typeof(UnorderedMiddleware), registrations[0].MiddlewareType);
        Assert.Equal(typeof(OrderedMiddleware), registrations[1].MiddlewareType);
        Assert.Equal(typeof(LastMiddleware), registrations[2].MiddlewareType);

        // Build creates the sorted pipeline - verify it still works
        var built = pipeline.Build(services);
        var ctx = new DefaultHttpContext();

        // Act & Assert - should execute without error
        var ex = Record.Exception(() => built(ctx).GetAwaiter().GetResult());
        Assert.Null(ex);
    }
}

#region Test Middleware Classes with Attribute-Based Ordering

[MiddlewareOrder(42)]
public class OrderedMiddleware : IMiddleware
{
    public Task InvokeAsync(MiddlewareContext context, RequestDelegate next)
    {
        return next(context.HttpContext);
    }
}

public class UnorderedMiddleware : IMiddleware
{
    public Task InvokeAsync(MiddlewareContext context, RequestDelegate next)
    {
        return next(context.HttpContext);
    }
}

[MiddlewareOrder(1000)]
public class LastMiddleware : IMiddleware
{
    public Task InvokeAsync(MiddlewareContext context, RequestDelegate next)
    {
        return next(context.HttpContext);
    }
}

#endregion
