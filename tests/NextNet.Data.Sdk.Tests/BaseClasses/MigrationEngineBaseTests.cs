using NextNet.Data.Abstractions.Abstractions;
using NextNet.Data.Abstractions.Configuration;
using NextNet.Data.Abstractions.Models;
using NextNet.Data.Sdk.Base;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace NextNet.Data.Sdk.Tests.BaseClasses;

/// <summary>
/// Tests for <see cref="MigrationEngineBase"/>.
/// </summary>
public class MigrationEngineBaseTests
{
    [Fact]
    public async Task AddMigrationAsync_Should_ReturnSuccess()
    {
        var engine = CreateEngine();

        var result = await engine.AddMigrationAsync("AddUserTable");

        Assert.True(result.Success);
        Assert.Equal("AddUserTable", result.MigrationName);
    }

    [Fact]
    public async Task AddMigrationAsync_Should_ReturnFailure_WhenNameIsEmpty()
    {
        var engine = CreateEngine();

        var result = await engine.AddMigrationAsync(string.Empty);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task ApplyAsync_Should_ApplyPendingMigrations()
    {
        // First add a migration, then apply
        var engine = CreateEngine();
        await engine.AddMigrationAsync("TestMigration");

        var result = await engine.ApplyAsync();

        Assert.True(result.Success);
        Assert.True(result.MigrationsApplied > 0);
    }

    [Fact]
    public async Task ApplyAsync_Should_ReturnZero_WhenNoPending()
    {
        var engine = CreateEngine();

        var result = await engine.ApplyAsync();

        Assert.True(result.Success);
        Assert.Equal(0, result.MigrationsApplied);
    }

    [Fact]
    public async Task RollbackAsync_Should_ReturnSuccess_WhenMigrationsApplied()
    {
        var engine = CreateEngine();
        await engine.AddMigrationAsync("TestMigration");
        await engine.ApplyAsync();

        var result = await engine.RollbackAsync();

        Assert.True(result.Success);
        // Migration name includes a timestamp prefix added by MigrationEngineBase
        Assert.Contains("TestMigration", result.MigrationName);
    }

    [Fact]
    public async Task RollbackAsync_Should_ReturnSuccess_WhenNoMigrations()
    {
        var engine = CreateEngine();

        var result = await engine.RollbackAsync();

        Assert.True(result.Success);
    }

    private static TestMigrationEngine CreateEngine()
    {
        var config = Options.Create(new MigrationConfig(
            Directory: Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"))));
        var dataConfig = Options.Create(new DataConfig(
            DefaultConnection: "Default",
            Connections: new Dictionary<string, ConnectionConfig>
            {
                ["Default"] = new ConnectionConfig("Server=test;")
            }));
        var connectionManager = new TestConnectionManager(dataConfig);
        var logger = new TestLogger();

        return new TestMigrationEngine(config, connectionManager, logger);
    }

    /// <summary>
    /// Test migration engine implementation.
    /// </summary>
    private sealed class TestMigrationEngine : MigrationEngineBase
    {
        private readonly List<string> _applied = new();

        public TestMigrationEngine(
            IOptions<MigrationConfig> config,
            ConnectionManagerBase connectionManager,
            ILogger? logger = null)
            : base(config, connectionManager, logger) { }

        protected override Task<bool> ExecuteMigrationAsync(string script, string migrationName, CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }

        protected override Task<IReadOnlyList<string>> GetAppliedMigrationNamesAsync(CancellationToken cancellationToken)
        {
            IReadOnlyList<string> result = _applied.ToList();
            return Task.FromResult(result);
        }

        protected override Task RecordMigrationAsync(string migrationName, CancellationToken cancellationToken)
        {
            _applied.Add(migrationName);
            return Task.CompletedTask;
        }

        protected override Task RemoveMigrationAsync(string migrationName, CancellationToken cancellationToken)
        {
            _applied.Remove(migrationName);
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Test connection manager.
    /// </summary>
    private sealed class TestConnectionManager : ConnectionManagerBase
    {
        public TestConnectionManager(IOptions<DataConfig> config, ILogger? logger = null)
            : base(config, logger) { }

        protected override object CreateConnectionCore(string connectionString, string name)
            => name;
    }

    /// <summary>
    /// Simple logger for testing.
    /// </summary>
    private sealed class TestLogger : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
    }
}
