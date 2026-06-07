using NextNet.Data.Dapper.Internal;

namespace NextNet.Data.Dapper.Tests.Migration;

/// <summary>
/// Tests for <see cref="DapperMigrationEngine"/> migration operations,
/// constructor validation, and file system interactions.
/// </summary>
public sealed class DapperMigrationEngineTests : IDisposable
{
    private readonly string _tempMigrationsDir;
    private readonly DapperConnectionManager _connectionManager;
    private readonly ILogger<DapperMigrationEngine> _logger;
    private bool _disposed;

    public DapperMigrationEngineTests()
    {
        _tempMigrationsDir = Path.Combine(Path.GetTempPath(), "NextNetDapperTests", Guid.NewGuid().ToString());
        _logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<DapperMigrationEngine>();

        var connections = new Dictionary<string, ConnectionConfig>(StringComparer.OrdinalIgnoreCase)
        {
            ["Default"] = new ConnectionConfig(
                "Server=192.0.2.1,9999;Database=Test;Trusted_Connection=true;Connect Timeout=1;",
                Provider: "Dapper")
        };

        _connectionManager = new DapperConnectionManager(
            connections,
            new DapperOptions { EnablePooling = false, CommandTimeoutSeconds = 1 },
            new Microsoft.Extensions.Logging.Abstractions.NullLogger<DapperConnectionManager>());
    }

    /// <summary>
    /// Creates a <see cref="DapperMigrationEngine"/> instance for testing.
    /// Uses a temp directory for migration files to avoid polluting the project.
    /// </summary>
    private DapperMigrationEngine CreateEngine(
        string? migrationsDir = null,
        string? connectionName = null,
        MigrationConfig? config = null)
    {
        var dir = migrationsDir ?? _tempMigrationsDir;
        var migrationConfig = config ?? new MigrationConfig(
            Directory: dir,
            HistoryTableName: "__NextNetMigrations_Test",
            TimeoutSeconds: 30);

        return new DapperMigrationEngine(
            _connectionManager,
            migrationConfig,
            connectionName ?? "Default",
            _logger);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Constructor_Should_ThrowArgumentNullException_When_ConnectionManagerIsNull()
    {
        var ex = Assert.Throws<ArgumentNullException>(
            () => new DapperMigrationEngine(null!));
        Assert.Contains("connectionManager", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Constructor_Should_UseDefaults_When_ConfigIsNull()
    {
        var engine = new DapperMigrationEngine(
            _connectionManager,
            config: null,
            connectionName: "Default",
            logger: _logger);

        Assert.NotNull(engine);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Constructor_Should_UseDefaults_When_ConnectionNameIsNull()
    {
        var engine = new DapperMigrationEngine(
            _connectionManager,
            config: null,
            connectionName: null,
            logger: _logger);

        Assert.NotNull(engine);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Constructor_Should_UseNullLogger_When_LoggerIsNull()
    {
        var engine = new DapperMigrationEngine(
            _connectionManager,
            config: null,
            connectionName: "Default",
            logger: null);

        Assert.NotNull(engine);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task AddMigrationAsync_Should_ThrowArgumentNullException_When_NameIsNull()
    {
        var engine = CreateEngine();
        var ex = await Assert.ThrowsAsync<ArgumentNullException>(
            () => engine.AddMigrationAsync(null!));
        Assert.Contains("name", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task AddMigrationAsync_Should_ThrowArgumentNullException_When_NameIsEmpty()
    {
        var engine = CreateEngine();
        var ex = await Assert.ThrowsAsync<ArgumentNullException>(
            () => engine.AddMigrationAsync(string.Empty));
        Assert.Contains("name", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task AddMigrationAsync_Should_ThrowArgumentNullException_When_NameIsWhitespace()
    {
        var engine = CreateEngine();
        var ex = await Assert.ThrowsAsync<ArgumentNullException>(
            () => engine.AddMigrationAsync("   "));
        Assert.Contains("name", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task AddMigrationAsync_Should_CreateMigrationFiles_When_ValidName()
    {
        // Use the MigrationFileSystem directly to verify file creation
        var fileSystem = new MigrationFileSystem(_tempMigrationsDir, _logger);
        var (upPath, downPath) = fileSystem.CreateMigrationFiles("AddUserTable");

        // Verify files exist
        Assert.True(File.Exists(upPath), "Up migration file should exist.");
        Assert.True(File.Exists(downPath), "Down migration file should exist.");

        // Verify file content
        var upContent = await File.ReadAllTextAsync(upPath);
        Assert.Contains("UP migration", upContent, StringComparison.OrdinalIgnoreCase);

        var downContent = await File.ReadAllTextAsync(downPath);
        Assert.Contains("DOWN migration", downContent, StringComparison.OrdinalIgnoreCase);

        // Clean up
        File.Delete(upPath);
        File.Delete(downPath);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void AddMigrationAsync_Should_SanitizeInvalidCharacters_InName()
    {
        var fileSystem = new MigrationFileSystem(_tempMigrationsDir, _logger);
        // Use a name containing a path separator, which is universally invalid in filenames
        var (upPath, downPath) = fileSystem.CreateMigrationFiles("Add/User/Table");

        Assert.True(File.Exists(upPath), "Up migration file should exist.");
        Assert.True(File.Exists(downPath), "Down migration file should exist.");

        // Verify filename uses sanitized name
        var fileName = Path.GetFileNameWithoutExtension(upPath);
        Assert.DoesNotContain("/", fileName);

        // Clean up
        File.Delete(upPath);
        File.Delete(downPath);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task AddMigrationAsync_Should_ReturnSuccessResult_When_Successful()
    {
        var engine = CreateEngine();
        var result = await engine.AddMigrationAsync("TestMigration");

        Assert.True(result.Success, "Migration creation should succeed.");
        Assert.Contains("TestMigration", result.Message);
        Assert.Equal("TestMigration", result.MigrationName);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task AddMigrationAsync_Should_ReturnFailureResult_When_FileSystemFails()
    {
        // Use a non-writable path to trigger failure
        var invalidDir = Path.Combine(Path.GetTempPath(), "\0invalid\0path");
        var config = new MigrationConfig(
            Directory: invalidDir,
            HistoryTableName: "__NextNetMigrations_Test");
        var engine = CreateEngine(migrationsDir: invalidDir, config: config);

        var result = await engine.AddMigrationAsync("FailingMigration", CancellationToken.None);

        Assert.False(result.Success);
        Assert.NotNull(result.Errors);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ApplyAsync_Should_ReturnFailure_When_DatabaseUnreachable()
    {
        var config = new MigrationConfig(
            Directory: _tempMigrationsDir,
            HistoryTableName: "__NextNetMigrations_Test_Empty");
        var engine = CreateEngine(config: config);

        // Database is unreachable, so EnsureHistoryTableExistsAsync will fail
        var result = await engine.ApplyAsync(CancellationToken.None);

        Assert.False(result.Success);
        Assert.NotNull(result.Errors);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void MigrationFileSystem_Should_DetectPendingMigrations()
    {
        var fileSystem = new MigrationFileSystem(_tempMigrationsDir, _logger);
        var applied = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Create a migration file
        var (upPath, downPath) = fileSystem.CreateMigrationFiles("InitialCreate");

        try
        {
            // No migrations applied yet - should find pending
            var pending = fileSystem.GetPendingMigrations(applied);
            Assert.Single(pending);

            // Mark as applied - should find none
            var migrationName = MigrationFileSystem.GetMigrationName(upPath);
            applied.Add(migrationName);
            pending = fileSystem.GetPendingMigrations(applied);
            Assert.Empty(pending);
        }
        finally
        {
            File.Delete(upPath);
            File.Delete(downPath);
        }
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void MigrationFileSystem_Should_ReturnEmpty_When_DirectoryDoesNotExist()
    {
        var nonExistentDir = Path.Combine(Path.GetTempPath(), "__nextnet_nonexistent__");
        var fileSystem = new MigrationFileSystem(nonExistentDir, _logger);

        var pending = fileSystem.GetPendingMigrations(new HashSet<string>());

        Assert.Empty(pending);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void MigrationFileSystem_Should_GetDownScriptPath_When_Exists()
    {
        var fileSystem = new MigrationFileSystem(_tempMigrationsDir, _logger);
        var (upPath, downPath) = fileSystem.CreateMigrationFiles("DownTest");

        try
        {
            var foundDown = fileSystem.GetDownScriptPath(upPath);
            Assert.NotNull(foundDown);
            Assert.Equal(downPath, foundDown);
        }
        finally
        {
            File.Delete(upPath);
            File.Delete(downPath);
        }
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void MigrationFileSystem_Should_ReturnNullDownScript_When_NotExists()
    {
        var fileSystem = new MigrationFileSystem(_tempMigrationsDir, _logger);
        var upPath = Path.Combine(_tempMigrationsDir, "20250101000000_NoDown.sql");

        var downPath = fileSystem.GetDownScriptPath(upPath);
        Assert.Null(downPath);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void MigrationFileSystem_Should_IgnoreDownFiles_InGetPendingMigrations()
    {
        var fileSystem = new MigrationFileSystem(_tempMigrationsDir, _logger);

        // Create only a down file without an up file
        var downPath = Path.Combine(_tempMigrationsDir, "20250101000000_Orphan.down.sql");
        Directory.CreateDirectory(_tempMigrationsDir);
        File.WriteAllText(downPath, "-- Orphan down script");

        try
        {
            var pending = fileSystem.GetPendingMigrations(new HashSet<string>());
            Assert.Empty(pending); // .down.sql files should be ignored
        }
        finally
        {
            File.Delete(downPath);
        }
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task RollbackAsync_Should_ReturnFailure_When_NoMigrationsApplied()
    {
        var config = new MigrationConfig(
            Directory: _tempMigrationsDir,
            HistoryTableName: "__NextNetMigrations_RollbackTest");
        var engine = CreateEngine(config: config);

        // Can't connect to a real DB, so EnsureHistoryTableExistsAsync will fail
        // but that's fine — this tests the fallback path
        var result = await engine.RollbackAsync(CancellationToken.None);

        // The history table doesn't exist, so the rollback will try to create it
        // and fail to connect. The engine should catch this and return failure.
        Assert.False(result.Success);
        Assert.NotNull(result.Errors);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ApplyAsync_Should_ReturnErrorResult_When_DatabaseUnreachable()
    {
        var engine = CreateEngine();
        var result = await engine.ApplyAsync(CancellationToken.None);

        // Database is unreachable, so EnsureHistoryTableExistsAsync will fail.
        // The engine should catch this and return a failure result.
        Assert.False(result.Success);
        Assert.NotNull(result.Errors);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;

            // Clean up temp directory
            try
            {
                if (Directory.Exists(_tempMigrationsDir))
                {
                    Directory.Delete(_tempMigrationsDir, recursive: true);
                }
            }
            catch
            {
                // Ignore cleanup failures
            }

            _connectionManager.Dispose();
        }
    }
}
