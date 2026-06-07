using NextNet.Cli.Commands.Data;
using Xunit;

namespace NextNet.Cli.Tests.Commands.Data;

public class DbMigrationCommandTests
{
    [Fact]
    public void Create_ReturnsCommand()
    {
        var command = DbMigrationCommand.Create();
        Assert.NotNull(command);
        Assert.Equal("migration", command.Name);
    }

    [Fact]
    public void Create_HasDescription()
    {
        var command = DbMigrationCommand.Create();
        Assert.Contains("migration", command.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Create_HasAddSubcommand()
    {
        var command = DbMigrationCommand.Create();
        Assert.Contains(command.Subcommands, c => c.Name == "add");
    }

    [Fact]
    public void Create_HasStatusSubcommand()
    {
        var command = DbMigrationCommand.Create();
        Assert.Contains(command.Subcommands, c => c.Name == "status");
    }
}
