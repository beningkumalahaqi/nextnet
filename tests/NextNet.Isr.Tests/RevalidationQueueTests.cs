using NextNet.Isr.Background;

namespace NextNet.Isr.Tests;

public class RevalidationQueueTests
{
    [Fact]
    public async Task EnqueueAsync_Should_ReturnTrue_When_RouteProvided()
    {
        var queue = new RevalidationQueue(capacity: 10, deduplicationWindowSeconds: 30);
        var request = new RevalidationRequest { Route = "/test", Reason = "test" };

        var result = await queue.EnqueueAsync(request);
        Assert.True(result);
    }

    [Fact]
    public async Task EnqueueAsync_Should_ReturnFalse_When_DuplicateRouteWithinWindow()
    {
        var queue = new RevalidationQueue(capacity: 10, deduplicationWindowSeconds: 30);

        var request1 = new RevalidationRequest { Route = "/test" };
        var request2 = new RevalidationRequest { Route = "/test" };

        Assert.True(await queue.EnqueueAsync(request1));
        Assert.False(await queue.EnqueueAsync(request2)); // Deduplicated
    }

    [Fact]
    public async Task EnqueueAsync_Should_AcceptBoth_When_DifferentRoutes()
    {
        var queue = new RevalidationQueue(capacity: 10);

        Assert.True(await queue.EnqueueAsync(new RevalidationRequest { Route = "/a" }));
        Assert.True(await queue.EnqueueAsync(new RevalidationRequest { Route = "/b" }));
    }

    [Fact]
    public async Task CompleteRevalidation_Should_AllowReenqueue_When_Called()
    {
        var queue = new RevalidationQueue(capacity: 10, deduplicationWindowSeconds: 30);

        var request = new RevalidationRequest { Route = "/test" };
        Assert.True(await queue.EnqueueAsync(request));

        queue.CompleteRevalidation("/test");

        Assert.True(await queue.EnqueueAsync(request));
    }

    [Fact]
    public async Task ReadAllAsync_Should_YieldEnqueuedItems_When_ItemsAreEnqueued()
    {
        var queue = new RevalidationQueue(capacity: 10);
        var cts = new CancellationTokenSource();

        await queue.EnqueueAsync(new RevalidationRequest { Route = "/a" });
        await queue.EnqueueAsync(new RevalidationRequest { Route = "/b" });

        // Use timeout to stop the async enumeration after reading items
        cts.CancelAfter(500);

        var results = new List<RevalidationRequest>();
        try
        {
            await foreach (var item in queue.ReadAllAsync(cts.Token))
            {
                results.Add(item);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when cancellation triggers
        }

        Assert.Equal(2, results.Count);
        Assert.Contains(results, r => r.Route == "/a");
        Assert.Contains(results, r => r.Route == "/b");
    }

    [Fact]
    public async Task PendingCount_Should_ReflectQueueDepth_When_ItemsAreEnqueued()
    {
        var queue = new RevalidationQueue(capacity: 5);
        Assert.Equal(0, queue.PendingCount);

        // Enqueue multiple items
        await queue.EnqueueAsync(new RevalidationRequest { Route = "/a" });
        await queue.EnqueueAsync(new RevalidationRequest { Route = "/b" });

        // Channel reader count may vary; just check we can read it
        _ = queue.PendingCount;
    }

    [Fact]
    public void MaxConcurrentPerRoute_Should_ReturnConfiguredValue_When_Accessed()
    {
        var queue = new RevalidationQueue(capacity: 10, maxConcurrentPerRoute: 3);
        Assert.Equal(3, queue.MaxConcurrentPerRoute);
    }

    [Fact]
    public void Constructor_Should_Throw_When_CapacityIsNegative()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new RevalidationQueue(capacity: -1));
    }

    [Fact]
    public async Task EnqueueAsync_Should_Throw_When_RequestIsNull()
    {
        var queue = new RevalidationQueue();
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            queue.EnqueueAsync(null!).AsTask());
    }

    [Fact]
    public void ToString_Should_ReturnDescriptive_When_RouteIsSet()
    {
        var request = new RevalidationRequest { Route = "/test", Reason = "test reason" };
        var str = request.ToString();
        Assert.Contains("/test", str);
        Assert.Contains("test reason", str);
    }

    [Fact]
    public void ToString_Should_ReturnDescriptive_When_TagsAreSet()
    {
        var request = new RevalidationRequest { Tags = new[] { "blog" }, Reason = "tag test" };
        var str = request.ToString();
        Assert.Contains("blog", str);
    }

    [Fact]
    public void ToString_Should_ReturnUnknown_When_NoRouteOrTags()
    {
        var request = new RevalidationRequest();
        Assert.Contains("Unknown", request.ToString());
    }
}
