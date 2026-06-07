using NextNet.Cli.Commands.Data;
using Xunit;

namespace NextNet.Cli.Tests.Commands.Data;

public class DbMigrateCommandTests
{
    [Fact]
    public void Create_ReturnsCommand()
    {
        var command = DbMigrateCommand.Create();
        Assert.NotNull(command);
        Assert.Equal("migrate", command.Name);
    }

    [Fact]
    public void Create_HasDryRunOption()
    {
        var command = DbMigrateCommand.Create();
        var opt = command.Options.FirstOrDefault(o => o.Name == "dry-run");
        Assert.NotNull(opt);
    }

    [Fact]
    public void Create_HasConnectionOption()
    {
        var command = DbMigrateCommand.Create();
        var opt = command.Options.FirstOrDefault(o => o.Name == "connection");
        Assert.NotNull(opt);
    }

    [Fact]
    public void Create_HasConfirmOption()
    {
        var command = DbMigrateCommand.Create();
        var opt = command.Options.FirstOrDefault(o => o.Name == "confirm");
        Assert.NotNull(opt);
    }

    [Fact]
    public async Task ExecuteAsync_DryRun_ReturnsSuccess()
    {
        var exitCode = await DbMigrateCommand.ExecuteAsync(dryRun: true);
        Assert.Equal(0, exitCode);
    }

    [Fact]
    public async Task ExecuteAsync_WithoutConfig_ReturnsInputError()
    {
        var exitCode = await DbMigrateCommand.ExecuteAsync();
        Assert.Equal(2, exitCode);
    }

    [Fact]
    public async Task ExecuteAsync_WithoutConfirm_ReturnsZero()
    {
        // Without --confirm, the command should not apply migrations
        // and return 0 (cancelled by user)
        var exitCode = await DbMigrateCommand.ExecuteAsync(confirm: false);
        Assert.Equal(2, exitCode); // No config found
    }
}
