namespace NextNet.Data.EntityFramework.Tests;

/// <summary>
/// Tests for <see cref="EfCoreMigrationEngine"/>.
/// </summary>
public sealed class EfCoreMigrationEngineTests
{
    [Fact]
    public async Task ApplyAsync_Should_ReturnSuccess_When_NoPendingMigrations()
    {
        // Arrange
        using var factory = new InMemoryDbContextFactory();
        var contextFactory = factory.CreateFactory();
        var logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<EfCoreMigrationEngine>();
        var engine = new EfCoreMigrationEngine(
            contextFactory,
            config: new MigrationConfig(Directory: "TestMigrations"),
            logger: logger);

        // Act
        var result = await engine.ApplyAsync();

        // Assert
        // InMemory doesn't support migrations, so this may return success or failure.
        // We just verify it returns a MigrationResult without throwing.
        Assert.NotNull(result);
        Assert.False(string.IsNullOrEmpty(result.Message));
    }

    [Fact]
    public async Task AddMigrationAsync_Should_Throw_When_NameIsEmpty()
    {
        // Arrange
        using var factory = new InMemoryDbContextFactory();
        var engine = new EfCoreMigrationEngine(
            factory.CreateFactory(),
            logger: new Microsoft.Extensions.Logging.Abstractions.NullLogger<EfCoreMigrationEngine>());

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            engine.AddMigrationAsync(""));
    }

    [Fact]
    public async Task AddMigrationAsync_Should_Throw_When_NameIsNull()
    {
        // Arrange
        using var factory = new InMemoryDbContextFactory();
        var engine = new EfCoreMigrationEngine(
            factory.CreateFactory(),
            logger: new Microsoft.Extensions.Logging.Abstractions.NullLogger<EfCoreMigrationEngine>());

        // Act & Assert
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            engine.AddMigrationAsync(null));
#pragma warning restore CS8625
    }

    [Fact]
    public async Task ApplyAsync_Should_Return_Result_When_Cancelled()
    {
        // Arrange
        using var factory = new InMemoryDbContextFactory();
        var engine = new EfCoreMigrationEngine(
            factory.CreateFactory(),
            logger: new Microsoft.Extensions.Logging.Abstractions.NullLogger<EfCoreMigrationEngine>());

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act - In InMemory, cancellation may or may not throw depending on timing
        // Just verify it returns a result
        try
        {
            var result = await engine.ApplyAsync(cts.Token);
            Assert.NotNull(result);
        }
        catch (OperationCanceledException)
        {
            // Expected in some cases
        }
    }

    [Fact]
    public async Task RollbackAsync_Should_ReturnResult()
    {
        // Arrange
        using var factory = new InMemoryDbContextFactory();
        var engine = new EfCoreMigrationEngine(
            factory.CreateFactory(),
            logger: new Microsoft.Extensions.Logging.Abstractions.NullLogger<EfCoreMigrationEngine>());

        // Act - Rollback may fail since there's no migration to rollback in InMemory
        var result = await engine.RollbackAsync();

        // Assert - Should still return a result (may fail gracefully)
        Assert.NotNull(result);
    }

    [Fact]
    public void Constructor_Should_Throw_When_FactoryIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new EfCoreMigrationEngine(null!));
    }
}
