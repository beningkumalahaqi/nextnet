using NextNet.Cli.Commands.Generate;
using Xunit;

namespace NextNet.Cli.Tests.Commands.Generate;

public class GenerateRepositoryCommandTests
{
    [Fact]
    public void Create_ReturnsCommand()
    {
        var command = GenerateRepositoryCommand.Create();
        Assert.NotNull(command);
        Assert.Equal("repository", command.Name);
    }

    [Fact]
    public void Create_HasNameArgument()
    {
        var command = GenerateRepositoryCommand.Create();
        Assert.Contains(command.Arguments, a => a.Name == "name");
    }

    [Fact]
    public void Create_HasOutputOption()
    {
        var command = GenerateRepositoryCommand.Create();
        Assert.Contains(command.Options, o => o.Name == "output");
    }

    [Fact]
    public void Create_HasNamespaceOption()
    {
        var command = GenerateRepositoryCommand.Create();
        Assert.Contains(command.Options, o => o.Name == "namespace");
    }

    [Fact]
    public void Create_HasForceOption()
    {
        var command = GenerateRepositoryCommand.Create();
        Assert.Contains(command.Options, o => o.Name == "force");
    }

    [Fact]
    public void Create_HasDryRunOption()
    {
        var command = GenerateRepositoryCommand.Create();
        Assert.Contains(command.Options, o => o.Name == "dry-run");
    }

    [Fact]
    public async Task Execute_EmptyName_ReturnsErrorExitCode()
    {
        var exitCode = await GenerateRepositoryCommand.ExecuteAsync("");
        Assert.Equal(2, exitCode);
    }
}
