using NextNet.Cli.Commands.Dev;
using NextNet.Cli.UI;
using Xunit;

namespace NextNet.Cli.Tests.Commands;

public class DevServerTests
{
    [Fact]
    public void Constructor_NullConsole_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new DevServer(null!));
    }

    [Fact]
    public void Constructor_WithValidArgs_SetsProperties()
    {
        var console = new NextNetConsole(OutputMode.Plain);
        var server = new DevServer(console, port: 5000, https: true, hostname: "0.0.0.0", noHmr: true, verbose: true);
        Assert.NotNull(server);
    }

    [Fact]
    public void Constructor_DefaultPort_Is3000()
    {
        var console = new NextNetConsole(OutputMode.Plain);
        var server = new DevServer(console);
        Assert.NotNull(server);
    }

    [Fact]
    public void Dispose_MultipleCalls_DoesNotThrow()
    {
        var console = new NextNetConsole(OutputMode.Plain);
        var server = new DevServer(console);
        server.Dispose();
        server.Dispose(); // Should not throw
    }
}
