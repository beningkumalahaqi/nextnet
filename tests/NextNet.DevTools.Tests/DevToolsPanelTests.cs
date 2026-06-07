using NextNet.DevTools.UI;
using Xunit;

namespace NextNet.DevTools.Tests;

public class DevToolsPanelTests
{
    [Fact]
    public void Create_ReturnsBorderedString()
    {
        var result = DevToolsPanel.Create("Test Title", "Content line 1\nContent line 2", 40);
        Assert.Contains("Test Title", result);
        Assert.Contains("Content line 1", result);
        Assert.Contains("Content line 2", result);
        Assert.StartsWith("┌", result);
        Assert.Contains("┐", result);
        Assert.Contains("└", result);
        Assert.Contains("┘", result);
    }

    [Fact]
    public void Create_WithEmptyContent_Works()
    {
        var result = DevToolsPanel.Create("Empty", "", 30);
        Assert.Contains("Empty", result);
    }

    [Fact]
    public void Render_WritesToConsole()
    {
        var output = CaptureConsoleOutput(() => DevToolsPanel.Render("Title", "Content", 40));
        Assert.Contains("Title", output);
        Assert.Contains("Content", output);
    }

    [Fact]
    public void CreateStatusBar_ReturnsBorderedString()
    {
        var result = DevToolsPanel.CreateStatusBar("Status text", 40);
        Assert.Contains("Status text", result);
        Assert.Contains("├", result);
        Assert.Contains("┤", result);
    }

    [Fact]
    public void CreateStatusBar_EmptyText_Works()
    {
        var result = DevToolsPanel.CreateStatusBar("", 30);
        Assert.NotNull(result);
        Assert.NotEmpty(result);
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
