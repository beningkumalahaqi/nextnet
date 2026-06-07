namespace NextNet.Data.MongoDB.Tests;

/// <summary>
/// Tests for <see cref="MongoDbMigrationEngine"/>.
/// </summary>
public sealed class MongoDbMigrationEngineTests
{
    [Fact]
    public void Constructor_ShouldThrow_WhenClientManagerNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new MongoDbMigrationEngine(null!));
    }

    [Fact]
    public async Task AddMigrationAsync_ShouldThrow_WhenNameNull()
    {
        var engine = CreateEngine();
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            engine.AddMigrationAsync(null!));
    }

    [Fact]
    public async Task AddMigrationAsync_ShouldThrow_WhenNameEmpty()
    {
        var engine = CreateEngine();
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            engine.AddMigrationAsync(string.Empty));
    }

    [Fact]
    public async Task AddMigrationAsync_ShouldCreateFile()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        try
        {
            var config = new MigrationConfig(Directory: tempDir);
            var manager = CreateEmptyManager();
            var engine = new MongoDbMigrationEngine(manager, config, logger: new TestLogger<MongoDbMigrationEngine>());

            var result = await engine.AddMigrationAsync("TestMigration");

            Assert.True(result.Success);
            Assert.Equal("TestMigration", result.MigrationName);
            Assert.True(Directory.GetFiles(tempDir, "*.json").Length > 0);
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task ApplyAsync_ShouldReturnSuccess_WhenNoMigrations()
    {
        var engine = CreateEngine();
        var result = await engine.ApplyAsync();
        Assert.True(result.Success);
        Assert.Equal(0, result.MigrationsApplied);
    }

    [Fact]
    public async Task RollbackAsync_ShouldReturnSuccess_WhenNoMigrations()
    {
        var engine = CreateEngine();
        var result = await engine.RollbackAsync();
        Assert.True(result.Success);
        Assert.Equal(0, result.MigrationsApplied);
    }

    private static MongoDbMigrationEngine CreateEngine()
    {
        var manager = CreateEmptyManager();
        return new MongoDbMigrationEngine(manager, logger: new TestLogger<MongoDbMigrationEngine>());
    }

    private static MongoClientManager CreateEmptyManager()
    {
        return new MongoClientManager(
            new Dictionary<string, ConnectionConfig>(),
            new MongoDbOptions(),
            logger: new TestLogger<MongoClientManager>());
    }
}
