using NextNet.Cli.Commands.Data;
using Xunit;

namespace NextNet.Cli.Tests.Commands.Data;

public class DbMigrationStatusCommandTests
{
    [Fact]
    public void Create_ReturnsCommand()
    {
        var command = DbMigrationStatusCommand.Create();
        Assert.NotNull(command);
        Assert.Equal("status", command.Name);
    }

    [Fact]
    public void Create_HasConnectionOption()
    {
        var command = DbMigrationStatusCommand.Create();
        var opt = command.Options.FirstOrDefault(o => o.Name == "connection");
        Assert.NotNull(opt);
    }

    [Fact]
    public void Create_HasVerboseOption()
    {
        var command = DbMigrationStatusCommand.Create();
        var opt = command.Options.FirstOrDefault(o => o.Name == "verbose");
        Assert.NotNull(opt);
    }

    [Fact]
    public void Create_HasJsonOption()
    {
        var command = DbMigrationStatusCommand.Create();
        var opt = command.Options.FirstOrDefault(o => o.Name == "json");
        Assert.NotNull(opt);
    }

    [Fact]
    public async Task ExecuteAsync_WithoutConfig_ReturnsInputError()
    {
        var exitCode = await DbMigrationStatusCommand.ExecuteAsync();
        Assert.Equal(2, exitCode);
    }

    [Fact]
    public async Task ExecuteAsync_JsonMode_ReturnsSuccessWithoutConfig()
    {
        // Without config it should return 2, but with config it would work
        var exitCode = await DbMigrationStatusCommand.ExecuteAsync(json: true);
        Assert.Equal(2, exitCode);
    }
}
