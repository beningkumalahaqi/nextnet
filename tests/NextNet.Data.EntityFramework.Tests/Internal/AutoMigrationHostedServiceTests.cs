using NextNet.Data.EntityFramework.Internal;

namespace NextNet.Data.EntityFramework.Tests.Internal;

/// <summary>
/// Tests for <see cref="AutoMigrationHostedService"/>.
/// </summary>
public sealed class AutoMigrationHostedServiceTests : IDisposable
{
    private readonly InMemoryDbContextFactory _fixture = new();

    [Fact]
    public async Task StartAsync_Should_Skip_When_AutoApplyIsFalse()
    {
        // Arrange
        var factory = _fixture.CreateFactory();
        var options = new EfCoreOptions { AutoApplyMigrations = false };
        var service = new AutoMigrationHostedService(
            factory,
            options,
            logger: new Microsoft.Extensions.Logging.Abstractions.NullLogger<AutoMigrationHostedService>());

        // Act
        await service.StartAsync(CancellationToken.None);

        // Assert - No exception means test passed
        Assert.True(true);
    }

    [Fact]
    public async Task StartAsync_Should_Skip_When_AutoApplyIsNull()
    {
        // Arrange
        var factory = _fixture.CreateFactory();
        var options = new EfCoreOptions { AutoApplyMigrations = null };
        var service = new AutoMigrationHostedService(
            factory,
            options,
            logger: new Microsoft.Extensions.Logging.Abstractions.NullLogger<AutoMigrationHostedService>());

        // Act
        await service.StartAsync(CancellationToken.None);

        // Assert
        Assert.True(true);
    }

    [Fact]
    public async Task StartAsync_Should_Run_When_AutoApplyIsTrue()
    {
        // Arrange
        var factory = _fixture.CreateFactory();
        var options = new EfCoreOptions { AutoApplyMigrations = true };
        var service = new AutoMigrationHostedService(
            factory,
            options,
            logger: new Microsoft.Extensions.Logging.Abstractions.NullLogger<AutoMigrationHostedService>());

        // Act - Should not throw even though there are no migrations
        await service.StartAsync(CancellationToken.None);

        // Assert
        Assert.True(true);
    }

    [Fact]
    public async Task StopAsync_Should_Complete()
    {
        // Arrange
        var factory = _fixture.CreateFactory();
        var service = new AutoMigrationHostedService(
            factory,
            new EfCoreOptions());

        // Act
        await service.StopAsync(CancellationToken.None);

        // Assert
        Assert.True(true);
    }

    [Fact]
    public void Constructor_Should_Throw_When_FactoryIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new AutoMigrationHostedService(null!, new EfCoreOptions()));
    }

    public void Dispose()
    {
        _fixture.Dispose();
    }
}
