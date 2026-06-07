using System.CommandLine;
using NextNet.Cli.Commands.Generate;
using Xunit;

namespace NextNet.Cli.Tests.Commands.Generate;

public class GenerateModelCommandTests
{
    [Fact]
    public void Create_ReturnsCommand()
    {
        var command = GenerateModelCommand.Create();
        Assert.NotNull(command);
        Assert.Equal("model", command.Name);
    }

    [Fact]
    public void Create_HasNameArgument()
    {
        var command = GenerateModelCommand.Create();
        Assert.Contains(command.Arguments, a => a.Name == "name");
    }

    [Fact]
    public void Create_HasOutputOption()
    {
        var command = GenerateModelCommand.Create();
        Assert.Contains(command.Options, o => o.Name == "output");
    }

    [Fact]
    public void Create_HasPropertyOption()
    {
        var command = GenerateModelCommand.Create();
        Assert.Contains(command.Options, o => o.Name == "property");
    }

    [Fact]
    public void Create_HasNamespaceOption()
    {
        var command = GenerateModelCommand.Create();
        Assert.Contains(command.Options, o => o.Name == "namespace");
    }

    [Fact]
    public void Create_HasForceOption()
    {
        var command = GenerateModelCommand.Create();
        Assert.Contains(command.Options, o => o.Name == "force");
    }

    [Fact]
    public void Create_HasDryRunOption()
    {
        var command = GenerateModelCommand.Create();
        Assert.Contains(command.Options, o => o.Name == "dry-run");
    }

    [Fact]
    public async Task Execute_EmptyName_ReturnsErrorExitCode()
    {
        var exitCode = await GenerateModelCommand.ExecuteAsync("");
        Assert.Equal(2, exitCode);
    }

    [Fact]
    public async Task Execute_NullName_ReturnsErrorExitCode()
    {
        var exitCode = await GenerateModelCommand.ExecuteAsync(null!);
        Assert.Equal(2, exitCode);
    }

    [Fact]
    public async Task Execute_InvalidName_ReturnsErrorExitCode()
    {
        var exitCode = await GenerateModelCommand.ExecuteAsync("123Invalid");
        Assert.Equal(2, exitCode);
    }
}
