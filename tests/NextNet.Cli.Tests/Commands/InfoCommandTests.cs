using NextNet.Cli.Commands;
using Xunit;

namespace NextNet.Cli.Tests.Commands;

public class InfoCommandTests
{
    [Fact]
    public void Execute_ReturnsSuccess()
    {
        var exitCode = InfoCommand.Execute();
        Assert.Equal(0, exitCode);
    }

    [Fact]
    public void Create_ReturnsCommand()
    {
        var command = InfoCommand.Create();
        Assert.NotNull(command);
        Assert.Equal("info", command.Name);
    }
}
