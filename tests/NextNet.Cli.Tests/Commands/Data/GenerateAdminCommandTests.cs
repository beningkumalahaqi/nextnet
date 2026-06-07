using NextNet.Cli.Commands.Data;
using Xunit;

namespace NextNet.Cli.Tests.Commands.Data;

/// <summary>
/// Tests for the <c>nextnet generate admin &lt;entity&gt;</c> command.
/// </summary>
public class GenerateAdminCommandTests
{
    [Fact]
    public void Create_ReturnsCommand()
    {
        var command = GenerateAdminCommand.Create();
        Assert.NotNull(command);
        Assert.Equal("admin", command.Name);
    }

    [Fact]
    public void Create_HasEntityArgument()
    {
        var command = GenerateAdminCommand.Create();
        Assert.Contains(command.Arguments, a => a.Name == "entity");
    }

    [Fact]
    public void Create_HasOutputOption()
    {
        var command = GenerateAdminCommand.Create();
        Assert.Contains(command.Options, o => o.Name == "output");
    }

    [Fact]
    public void Create_HasPropertyOption()
    {
        var command = GenerateAdminCommand.Create();
        Assert.Contains(command.Options, o => o.Name == "property");
    }

    [Fact]
    public void Create_HasNamespaceOption()
    {
        var command = GenerateAdminCommand.Create();
        Assert.Contains(command.Options, o => o.Name == "namespace");
    }

    [Fact]
    public void Create_HasRoutePrefixOption()
    {
        var command = GenerateAdminCommand.Create();
        Assert.Contains(command.Options, o => o.Name == "route-prefix");
    }

    [Fact]
    public void Create_HasLayoutOption()
    {
        var command = GenerateAdminCommand.Create();
        Assert.Contains(command.Options, o => o.Name == "layout");
    }

    [Fact]
    public void Create_HasForceOption()
    {
        var command = GenerateAdminCommand.Create();
        Assert.Contains(command.Options, o => o.Name == "force");
    }

    [Fact]
    public void Create_HasDryRunOption()
    {
        var command = GenerateAdminCommand.Create();
        Assert.Contains(command.Options, o => o.Name == "dry-run");
    }

    [Fact]
    public async Task Execute_EmptyName_ReturnsErrorExitCode()
    {
        var exitCode = await GenerateAdminCommand.ExecuteAsync("");
        Assert.Equal(2, exitCode);
    }

    [Fact]
    public async Task Execute_InvalidName_ReturnsErrorExitCode()
    {
        var exitCode = await GenerateAdminCommand.ExecuteAsync("123invalid");
        Assert.Equal(2, exitCode);
    }
}
