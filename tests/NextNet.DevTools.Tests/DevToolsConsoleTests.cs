using NextNet.DevTools.UI;
using Xunit;

namespace NextNet.DevTools.Tests;

public class DevToolsConsoleTests
{
    [Fact]
    public void WriteLog_Should_WriteMessage_When_Called()
    {
        var output = CaptureConsoleOutput(() => DevToolsConsole.WriteLog("test message"));
        Assert.Contains("test message", output);
        // Should contain a timestamp in brackets (format may vary by culture)
        Assert.Matches(@"\[\d{2}.\d{2}.\d{2}\]", output);
    }

    [Fact]
    public void WriteSuccess_Should_WriteMessage_When_Called()
    {
        var output = CaptureConsoleOutput(() => DevToolsConsole.WriteSuccess("success!"));
        Assert.Contains("success!", output);
    }

    [Fact]
    public void WriteWarning_Should_WriteMessage_When_Called()
    {
        var output = CaptureConsoleOutput(() => DevToolsConsole.WriteWarning("warning!"));
        Assert.Contains("warning!", output);
    }

    [Fact]
    public void WriteError_Should_WriteMessage_When_Called()
    {
        var output = CaptureConsoleOutput(() => DevToolsConsole.WriteError("error!"));
        Assert.Contains("error!", output);
    }

    [Fact]
    public void WriteInfo_Should_WriteMessage_When_Called()
    {
        var output = CaptureConsoleOutput(() => DevToolsConsole.WriteInfo("info!"));
        Assert.Contains("info!", output);
    }

    [Fact]
    public void WriteHeading_Should_WriteMessage_When_Called()
    {
        var output = CaptureConsoleOutput(() => DevToolsConsole.WriteHeading("Heading"));
        Assert.Contains("Heading", output);
        Assert.Contains("=", output); // underline
    }

    [Fact]
    public void WriteMuted_Should_WriteMessage_When_Called()
    {
        var output = CaptureConsoleOutput(() => DevToolsConsole.WriteMuted("muted text"));
        Assert.Contains("muted text", output);
    }

    private static string CaptureConsoleOutput(Action action)
    {
        var original = System.Console.Out;
        try
        {
            using var writer = new StringWriter();
            System.Console.SetOut(writer);
            action();
            return writer.ToString();
        }
        finally
        {
            System.Console.SetOut(original);
        }
    }
}
