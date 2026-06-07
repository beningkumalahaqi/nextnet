using NextNet.Cli.Commands.Data;
using Xunit;

namespace NextNet.Cli.Tests.Commands.Data;

public class DbMigrationAddCommandTests
{
    [Fact]
    public void Create_ReturnsCommand()
    {
        var command = DbMigrationAddCommand.Create();
        Assert.NotNull(command);
        Assert.Equal("add", command.Name);
    }

    [Fact]
    public void Create_HasNameArgument()
    {
        var command = DbMigrationAddCommand.Create();
        var arg = command.Arguments.FirstOrDefault(a => a.Name == "name");
        Assert.NotNull(arg);
    }

    [Fact]
    public void Create_HasProviderOption()
    {
        var command = DbMigrationAddCommand.Create();
        var opt = command.Options.FirstOrDefault(o => o.Name == "provider");
        Assert.NotNull(opt);
    }

    [Fact]
    public void Create_HasOutputDirOption()
    {
        var command = DbMigrationAddCommand.Create();
        var opt = command.Options.FirstOrDefault(o => o.Name == "output-dir");
        Assert.NotNull(opt);
    }

    [Fact]
    public void Create_HasDryRunOption()
    {
        var command = DbMigrationAddCommand.Create();
        var opt = command.Options.FirstOrDefault(o => o.Name == "dry-run");
        Assert.NotNull(opt);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public async Task ExecuteAsync_EmptyName_ReturnsInputError(string? name)
    {
        var exitCode = await DbMigrationAddCommand.ExecuteAsync(name);
        Assert.Equal(2, exitCode);
    }

    [Fact]
    public async Task ExecuteAsync_DryRun_ReturnsSuccess()
    {
        var exitCode = await DbMigrationAddCommand.ExecuteAsync("AddUserTable", dryRun: true);
        Assert.Equal(0, exitCode);
    }

    [Fact]
    public async Task ExecuteAsync_ValidNameWithoutConfig_ReturnsInputError()
    {
        // Without a nextnet.config.json in the test directory, this should return
        // an input error since no project config is found
        var exitCode = await DbMigrationAddCommand.ExecuteAsync("AddUserTable");
        Assert.Equal(2, exitCode);
    }
}
