namespace NextNet.TemplateMarketplace.Tests;

using Xunit;

public class MarketplaceDataCollectorTests
{
    [Fact]
    public async Task RecordInstall_Should_NotEnqueue_When_Disabled()
    {
        var options = new MarketplaceOptions { EnableDataCollection = false };
        await using var collector = new MarketplaceDataCollector(options);

        collector.RecordInstall("test", "1.0.0");

        Assert.Equal(0, collector.QueueSize);
    }

    [Fact]
    public async Task RecordInstall_Should_Enqueue_When_Enabled()
    {
        var options = new MarketplaceOptions { EnableDataCollection = true };
        await using var collector = new MarketplaceDataCollector(options);

        collector.RecordInstall("test", "1.0.0");

        Assert.Equal(1, collector.QueueSize);
    }

    [Fact]
    public async Task RecordGeneration_Should_NotEnqueue_When_Disabled()
    {
        var options = new MarketplaceOptions { EnableDataCollection = false };
        await using var collector = new MarketplaceDataCollector(options);

        collector.RecordGeneration("test", "1.0.0", true, TimeSpan.FromSeconds(2));

        Assert.Equal(0, collector.QueueSize);
    }

    [Fact]
    public async Task RecordGeneration_Should_Enqueue_When_Enabled()
    {
        var options = new MarketplaceOptions { EnableDataCollection = true };
        await using var collector = new MarketplaceDataCollector(options);

        collector.RecordGeneration("test", "1.0.0", true, TimeSpan.FromSeconds(2));

        Assert.Equal(1, collector.QueueSize);
    }

    [Fact]
    public async Task RecordError_Should_NotEnqueue_When_Disabled()
    {
        var options = new MarketplaceOptions { EnableDataCollection = false };
        await using var collector = new MarketplaceDataCollector(options);

        collector.RecordError("test", "1.0.0", "Something went wrong");

        Assert.Equal(0, collector.QueueSize);
    }

    [Fact]
    public async Task RecordError_Should_Enqueue_When_Enabled()
    {
        var options = new MarketplaceOptions { EnableDataCollection = true };
        await using var collector = new MarketplaceDataCollector(options);

        collector.RecordError("test", "1.0.0", "Something went wrong");

        Assert.Equal(1, collector.QueueSize);
    }

    [Fact]
    public async Task FlushAsync_Should_ClearQueue_When_Enabled()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var options = new MarketplaceOptions
        {
            EnableDataCollection = true,
            CacheDirectory = tempDir
        };

        await using var collector = new MarketplaceDataCollector(options);

        collector.RecordInstall("test", "1.0.0");
        collector.RecordGeneration("test", "1.0.0", true, TimeSpan.FromSeconds(1));
        Assert.Equal(2, collector.QueueSize);

        await collector.FlushAsync();

        Assert.Equal(0, collector.QueueSize);
    }
}
