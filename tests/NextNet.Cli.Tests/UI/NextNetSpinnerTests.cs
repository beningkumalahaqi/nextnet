using NextNet.Cli.UI;
using Xunit;

namespace NextNet.Cli.Tests.UI;

public class NextNetSpinnerTests
{
    [Fact]
    public async Task StartAsync_PlainMode_ExecutesAction()
    {
        var spinner = new NextNetSpinner("Loading...", OutputMode.Plain);
        var executed = false;
        await spinner.StartAsync(() =>
        {
            executed = true;
            return Task.CompletedTask;
        });
        Assert.True(executed);
    }

    [Fact]
    public void Dispose_DoesNotThrow()
    {
        var spinner = new NextNetSpinner("test", OutputMode.Plain);
        spinner.Dispose();
        spinner.Dispose(); // Multiple disposals should not throw
    }
}
