using NextNet.Cli.Commands;
using NextNet.Cli.Commands.Template;
using System.CommandLine;
using Xunit;

namespace NextNet.Cli.Tests.Commands;

/// <summary>
/// Tests for the V3 <see cref="NewCommand"/> and related template commands.
/// These tests verify command wire-up and basic structure.
/// Full template generation tests belong in the NextNet.TemplateEngine.Tests project.
/// </summary>
public class NewCommandTests
{
    [Fact]
    public void NewCommand_Should_HaveCorrectName()
    {
        var command = new NewCommand();
        Assert.Equal("new", command.Name);
        Assert.Equal("Create a new NextNet project from a template", command.Description);
    }

    [Fact]
    public void NewCommand_Should_HavePositionalArguments()
    {
        var command = new NewCommand();
        var templateArg = command.Arguments.FirstOrDefault(a => a.Name == "template");
        var nameArg = command.Arguments.FirstOrDefault(a => a.Name == "name");
        Assert.NotNull(templateArg);
        Assert.NotNull(nameArg);
    }

    [Fact]
    public void NewCommand_Should_HaveOptions()
    {
        var command = new NewCommand();
        Assert.Contains(command.Options, o => o.Name == "output");
        Assert.Contains(command.Options, o => o.Name == "no-restore");
    }

    [Fact]
    public void TemplateCommand_Should_HaveCorrectName()
    {
        var command = new TemplateCommand();
        Assert.Equal("template", command.Name);
        Assert.Equal("Manage templates", command.Description);
    }

    [Fact]
    public void TemplateCommand_Should_HaveSubcommands()
    {
        var command = new TemplateCommand();
        Assert.Contains(command.Subcommands, c => c.Name == "list");
        Assert.Contains(command.Subcommands, c => c.Name == "info");
    }

    [Fact]
    public void TemplateListCommand_Should_HaveCorrectName()
    {
        var command = new TemplateListCommand();
        Assert.Equal("list", command.Name);
    }

    [Fact]
    public void TemplateInfoCommand_Should_HaveCorrectName()
    {
        var command = new TemplateInfoCommand();
        Assert.Equal("info", command.Name);
    }

    [Fact]
    public void TemplateInfoCommand_Should_HaveNameArgument()
    {
        var command = new TemplateInfoCommand();
        Assert.Contains(command.Arguments, a => a.Name == "name");
    }

    [Fact]
    public void NextNetCli_Should_ContainNewCommand()
    {
        var root = NextNetCli.Create();
        Assert.Contains(root.Subcommands, c => c.Name == "new");
    }

    [Fact]
    public void NextNetCli_Should_ContainTemplateCommand()
    {
        var root = NextNetCli.Create();
        Assert.Contains(root.Subcommands, c => c.Name == "template");
    }
}
