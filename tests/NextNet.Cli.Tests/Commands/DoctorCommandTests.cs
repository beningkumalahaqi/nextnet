using NextNet.Cli.Commands;
using Xunit;

namespace NextNet.Cli.Tests.Commands;

public class DoctorCommandTests
{
    [Fact]
    public void Execute_Completes()
    {
        // Doctor may return 0 or 1 depending on environment
        var exitCode = DoctorCommand.Execute();
        Assert.InRange(exitCode, 0, 1);
    }

    [Fact]
    public void Execute_WithFix_Completes()
    {
        var exitCode = DoctorCommand.Execute(fix: true);
        Assert.InRange(exitCode, 0, 1);
    }

    [Fact]
    public void Create_ReturnsCommand()
    {
        var command = DoctorCommand.Create();
        Assert.NotNull(command);
        Assert.Equal("doctor", command.Name);
    }
}
