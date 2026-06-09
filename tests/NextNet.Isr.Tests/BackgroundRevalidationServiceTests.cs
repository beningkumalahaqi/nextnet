using Moq;
using NextNet.Isr.Background;
using NextNet.Isr.Revalidation;

namespace NextNet.Isr.Tests;

public class BackgroundRevalidationServiceTests
{
    [Fact]
    public async Task StartAndStop_Should_NotThrow_When_Called()
    {
        var queue = new RevalidationQueue(capacity: 10);
        var manager = new Mock<IIsrRevalidationManager>(MockBehavior.Strict);
        var globalOptions = new IsrGlobalOptions { MaxConcurrentRegenerations = 2 };

        var service = new BackgroundRevalidationService(queue, manager.Object, globalOptions);

        // Should not throw
        await service.StartAsync(CancellationToken.None);
        await service.StopAsync(CancellationToken.None);

        service.Dispose();
    }

    [Fact]
    public async Task BackgroundService_Should_ProcessQueuedRequests_When_Enqueued()
    {
        var queue = new RevalidationQueue(capacity: 10);
        var manager = new Mock<IIsrRevalidationManager>(MockBehavior.Strict);
        var globalOptions = new IsrGlobalOptions { MaxConcurrentRegenerations = 2 };

        manager.Setup(m => m.RevalidateAsync("/test", It.IsAny<CancellationToken>()))
            .ReturnsAsync(RevalidationResult.Ok("/test"));

        var service = new BackgroundRevalidationService(queue, manager.Object, globalOptions);
        await service.StartAsync(CancellationToken.None);

        // Enqueue a request
        await queue.EnqueueAsync(new RevalidationRequest { Route = "/test" });

        // Give the background service time to process
        await Task.Delay(500);

        await service.StopAsync(CancellationToken.None);
        service.Dispose();

        manager.Verify(m => m.RevalidateAsync("/test", It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public void Constructor_Should_Throw_When_QueueIsNull()
    {
        var manager = new Mock<IIsrRevalidationManager>(MockBehavior.Strict);
        var globalOptions = new IsrGlobalOptions();

        Assert.Throws<ArgumentNullException>(() =>
            new BackgroundRevalidationService(null!, manager.Object, globalOptions));
    }

    [Fact]
    public void Constructor_Should_Throw_When_ManagerIsNull()
    {
        var queue = new RevalidationQueue();
        var globalOptions = new IsrGlobalOptions();

        Assert.Throws<ArgumentNullException>(() =>
            new BackgroundRevalidationService(queue, null!, globalOptions));
    }

    [Fact]
    public void Constructor_Should_Throw_When_GlobalOptionsIsNull()
    {
        var queue = new RevalidationQueue();
        var manager = new Mock<IIsrRevalidationManager>(MockBehavior.Strict);

        Assert.Throws<ArgumentNullException>(() =>
            new BackgroundRevalidationService(queue, manager.Object, null!));
    }

    [Fact]
    public async Task Dispose_Should_NotThrow_When_CalledMultipleTimes()
    {
        var queue = new RevalidationQueue();
        var manager = new Mock<IIsrRevalidationManager>(MockBehavior.Strict);
        var globalOptions = new IsrGlobalOptions();

        var service = new BackgroundRevalidationService(queue, manager.Object, globalOptions);
        await service.StartAsync(CancellationToken.None);

        service.Dispose();
        service.Dispose(); // Should not throw
    }
}
