using NextNet.Cli.Commands;
using Xunit;

namespace NextNet.Cli.Tests.Commands;

public class DevCommandTests
{
    [Fact]
    public void Create_ReturnsCommand()
    {
        var command = DevCommand.Create();
        Assert.NotNull(command);
        Assert.Equal("dev", command.Name);
    }

    [Fact]
    public void Create_HasPortOption()
    {
        var command = DevCommand.Create();
        Assert.Contains(command.Options, o => o.Name == "port");
    }

    [Fact]
    public void Create_HasHostnameOption()
    {
        var command = DevCommand.Create();
        Assert.Contains(command.Options, o => o.Name == "hostname");
    }

    [Fact]
    public void Create_HasNoHmrOption()
    {
        var command = DevCommand.Create();
        Assert.Contains(command.Options, o => o.Name == "no-hmr");
    }

    [Fact]
    public void Create_HasHttpsOption()
    {
        var command = DevCommand.Create();
        Assert.Contains(command.Options, o => o.Name == "https");
    }

    [Fact]
    public void Create_HasPortAlias()
    {
        var command = DevCommand.Create();
        var portOption = command.Options.First(o => o.Name == "port");
        Assert.Contains(portOption.Aliases, a => a == "-p");
    }
}
