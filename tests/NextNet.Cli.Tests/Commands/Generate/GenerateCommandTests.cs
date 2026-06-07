using NextNet.Cli.Commands.Generate;
using Xunit;

namespace NextNet.Cli.Tests.Commands.Generate;

public class GenerateCommandTests
{
    [Fact]
    public void Create_ReturnsCommand()
    {
        var command = GenerateCommand.Create();
        Assert.NotNull(command);
        Assert.Equal("generate", command.Name);
    }

    [Fact]
    public void Create_HasModelSubcommand()
    {
        var command = GenerateCommand.Create();
        Assert.Contains(command.Subcommands, c => c.Name == "model");
    }

    [Fact]
    public void Create_HasRepositorySubcommand()
    {
        var command = GenerateCommand.Create();
        Assert.Contains(command.Subcommands, c => c.Name == "repository");
    }

    [Fact]
    public void Create_HasCrudSubcommand()
    {
        var command = GenerateCommand.Create();
        Assert.Contains(command.Subcommands, c => c.Name == "crud");
    }
}
