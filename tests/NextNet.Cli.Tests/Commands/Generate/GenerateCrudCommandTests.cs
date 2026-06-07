using NextNet.Cli.Commands.Generate;
using Xunit;

namespace NextNet.Cli.Tests.Commands.Generate;

public class GenerateCrudCommandTests
{
    [Fact]
    public void Create_ReturnsCommand()
    {
        var command = GenerateCrudCommand.Create();
        Assert.NotNull(command);
        Assert.Equal("crud", command.Name);
    }

    [Fact]
    public void Create_HasNameArgument()
    {
        var command = GenerateCrudCommand.Create();
        Assert.Contains(command.Arguments, a => a.Name == "name");
    }

    [Fact]
    public void Create_HasOutputOption()
    {
        var command = GenerateCrudCommand.Create();
        Assert.Contains(command.Options, o => o.Name == "output");
    }

    [Fact]
    public void Create_HasPropertyOption()
    {
        var command = GenerateCrudCommand.Create();
        Assert.Contains(command.Options, o => o.Name == "property");
    }

    [Fact]
    public void Create_HasForceOption()
    {
        var command = GenerateCrudCommand.Create();
        Assert.Contains(command.Options, o => o.Name == "force");
    }

    [Fact]
    public void Create_HasDryRunOption()
    {
        var command = GenerateCrudCommand.Create();
        Assert.Contains(command.Options, o => o.Name == "dry-run");
    }

    [Fact]
    public async Task Execute_EmptyName_ReturnsErrorExitCode()
    {
        var exitCode = await GenerateCrudCommand.ExecuteAsync("");
        Assert.Equal(2, exitCode);
    }
}
