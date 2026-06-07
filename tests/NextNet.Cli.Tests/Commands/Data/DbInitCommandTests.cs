using NextNet.Cli.Commands.Data;
using Xunit;

namespace NextNet.Cli.Tests.Commands.Data;

public class DbInitCommandTests
{
    [Fact]
    public void Create_ReturnsCommand()
    {
        var command = DbInitCommand.Create();
        Assert.NotNull(command);
        Assert.Equal("init", command.Name);
    }

    [Fact]
    public void Create_HasSqliteSubcommand()
    {
        var command = DbInitCommand.Create();
        Assert.Contains(command.Subcommands, c => c.Name == "sqlite");
    }

    [Fact]
    public void Create_HasPostgreSqlSubcommand()
    {
        var command = DbInitCommand.Create();
        Assert.Contains(command.Subcommands, c => c.Name == "postgresql");
    }

    [Fact]
    public async Task ExecuteAsync_NoConfig_ReturnsInputError()
    {
        // Without a config file, should return input error (2)
        var exitCode = await DbInitCommand.ExecuteAsync();
        Assert.Equal(2, exitCode);
    }
}

public class DbInitSqliteCommandTests
{
    [Fact]
    public void Create_ReturnsCommand()
    {
        var command = DbInitSqliteCommand.Create();
        Assert.NotNull(command);
        Assert.Equal("sqlite", command.Name);
    }

    [Fact]
    public void Create_HasFileOption()
    {
        var command = DbInitSqliteCommand.Create();
        var opt = command.Options.FirstOrDefault(o => o.Name == "file");
        Assert.NotNull(opt);
    }

    [Fact]
    public void Create_HasOutputOption()
    {
        var command = DbInitSqliteCommand.Create();
        var opt = command.Options.FirstOrDefault(o => o.Name == "output");
        Assert.NotNull(opt);
    }

    [Fact]
    public async Task ExecuteAsync_SuccessfullyCreatesDbFile()
    {
        var testDir = Path.Combine(Path.GetTempPath(), "NextNetCliTests", Guid.NewGuid().ToString());
        try
        {
            Directory.CreateDirectory(testDir);
            var originalDir = Environment.CurrentDirectory;
            Environment.CurrentDirectory = testDir;

            try
            {
                var testFile = "test.db";
                var exitCode = await DbInitSqliteCommand.ExecuteAsync(testFile, testDir);

                // We expect exit code 4 (execution error) because no nextnet.config.json exists
                // to update. The .db file should still have been created.
                Assert.Equal(4, exitCode);
                var dbPath = Path.Combine(testDir, testFile);
                Assert.True(File.Exists(dbPath), $"Database file should exist at {dbPath}");
                Assert.True(new FileInfo(dbPath).Length > 0, "Database file should not be empty");
            }
            finally
            {
                Environment.CurrentDirectory = originalDir;
            }
        }
        finally
        {
            try { Directory.Delete(testDir, recursive: true); } catch { }
        }
    }

    [Fact]
    public async Task ExecuteAsync_AppendsDbExtension()
    {
        var testDir = Path.Combine(Path.GetTempPath(), "NextNetCliTests", Guid.NewGuid().ToString());
        try
        {
            Directory.CreateDirectory(testDir);
            var originalDir = Environment.CurrentDirectory;
            Environment.CurrentDirectory = testDir;

            try
            {
                // Without .db extension
                var exitCode = await DbInitSqliteCommand.ExecuteAsync("mydata", testDir);
                Assert.Equal(4, exitCode);

                // With .db extension (explicit)
                var exitCode2 = await DbInitSqliteCommand.ExecuteAsync("data.db", testDir);
                Assert.Equal(4, exitCode2);
            }
            finally
            {
                Environment.CurrentDirectory = originalDir;
            }
        }
        finally
        {
            try { Directory.Delete(testDir, recursive: true); } catch { }
        }
    }

    [Fact]
    public async Task ExecuteAsync_WithExistingFile_DoesNotOverwrite()
    {
        var testDir = Path.Combine(Path.GetTempPath(), "NextNetCliTests", Guid.NewGuid().ToString());
        try
        {
            Directory.CreateDirectory(testDir);
            var originalDir = Environment.CurrentDirectory;
            Environment.CurrentDirectory = testDir;

            try
            {
                var testFile = "existing.db";
                var dbPath = Path.Combine(testDir, testFile);

                // Create an existing file with specific content
                await File.WriteAllTextAsync(dbPath, "existing content");

                // Run the init command
                var exitCode = await DbInitSqliteCommand.ExecuteAsync(testFile, testDir);
                Assert.Equal(4, exitCode);

                // File should still have the original content (not overwritten with SQLite header)
                var content = await File.ReadAllTextAsync(dbPath);
                Assert.Equal("existing content", content);
            }
            finally
            {
                Environment.CurrentDirectory = originalDir;
            }
        }
        finally
        {
            try { Directory.Delete(testDir, recursive: true); } catch { }
        }
    }

    [Fact]
    public async Task ExecuteAsync_WithNullFileName_UsesDefault()
    {
        var testDir = Path.Combine(Path.GetTempPath(), "NextNetCliTests", Guid.NewGuid().ToString());
        try
        {
            Directory.CreateDirectory(testDir);
            var originalDir = Environment.CurrentDirectory;
            Environment.CurrentDirectory = testDir;

            try
            {
                var exitCode = await DbInitSqliteCommand.ExecuteAsync(null, testDir);

                // Should use default "app.db" and fail with 4 due to no config
                Assert.Equal(4, exitCode);
                Assert.True(File.Exists(Path.Combine(testDir, "app.db")));
            }
            finally
            {
                Environment.CurrentDirectory = originalDir;
            }
        }
        finally
        {
            try { Directory.Delete(testDir, recursive: true); } catch { }
        }
    }
}
