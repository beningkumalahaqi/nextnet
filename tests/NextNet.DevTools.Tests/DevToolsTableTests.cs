using NextNet.DevTools.UI;
using Xunit;

namespace NextNet.DevTools.Tests;

public class DevToolsTableTests
{
    [Fact]
    public void Render_Should_ReturnFormattedTable_When_GivenHeaders()
    {
        var table = new DevToolsTable("Name", "Value");
        var result = table.Render();
        Assert.Contains("Name", result);
        Assert.Contains("Value", result);
    }

    [Fact]
    public void Render_Should_ShowData_When_RowsAdded()
    {
        var table = new DevToolsTable("A", "B");
        table.AddRow("x", "y");
        var result = table.Render();
        Assert.Contains("x", result);
        Assert.Contains("y", result);
    }

    [Fact]
    public void AddRow_Should_Throw_When_ColumnCountMismatch()
    {
        var table = new DevToolsTable("A", "B");
        Assert.Throws<ArgumentException>(() => table.AddRow("only one"));
    }

    [Fact]
    public void RenderToConsole_Should_WriteToConsole_When_Called()
    {
        var table = new DevToolsTable("H1", "H2");
        table.AddRow("v1", "v2");

        var output = CaptureConsoleOutput(() => table.RenderToConsole());
        Assert.Contains("H1", output);
        Assert.Contains("v1", output);
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
