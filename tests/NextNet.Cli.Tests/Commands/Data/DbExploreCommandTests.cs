using NextNet.Cli.Commands.Data;
using Xunit;

namespace NextNet.Cli.Tests.Commands.Data;

/// <summary>
/// Tests for the <c>nextnet db explore</c> command.
/// </summary>
public class DbExploreCommandTests
{
    [Fact]
    public void Create_ReturnsCommand()
    {
        var command = DbExploreCommand.Create();
        Assert.NotNull(command);
        Assert.Equal("explore", command.Name);
    }

    [Fact]
    public void Create_HasTableArgument()
    {
        var command = DbExploreCommand.Create();
        Assert.Contains(command.Arguments, a => a.Name == "table");
    }

    [Fact]
    public void Create_HasConnectionOption()
    {
        var command = DbExploreCommand.Create();
        Assert.Contains(command.Options, o => o.Name == "connection");
    }

    [Fact]
    public void Create_HasFormatOption()
    {
        var command = DbExploreCommand.Create();
        Assert.Contains(command.Options, o => o.Name == "format");
    }

    [Fact]
    public void Create_HasIncludeViewsOption()
    {
        var command = DbExploreCommand.Create();
        Assert.Contains(command.Options, o => o.Name == "include-views");
    }

    [Fact]
    public void Create_HasVerboseOption()
    {
        var command = DbExploreCommand.Create();
        Assert.Contains(command.Options, o => o.Name == "verbose");
    }

    [Fact]
    public void Create_HasInspectAlias()
    {
        var command = DbExploreCommand.Create();
        Assert.Contains(command.Aliases, a => a == "inspect");
    }

    [Fact]
    public async Task Execute_InvalidFormat_ReturnsErrorExitCode()
    {
        var exitCode = await DbExploreCommand.ExecuteAsync(format: "invalid-format");
        Assert.Equal(2, exitCode);
    }

    [Fact]
    public async Task Execute_EmptyTableName_ReturnsListModeExitCode()
    {
        // No database config available in test environment; expects config not found error
        var exitCode = await DbExploreCommand.ExecuteAsync(tableName: null);
        Assert.Equal(2, exitCode); // Config file not found
    }
}
