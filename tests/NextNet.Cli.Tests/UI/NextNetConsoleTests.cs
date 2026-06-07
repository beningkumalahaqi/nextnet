using NextNet.Cli.UI;
using Xunit;

namespace NextNet.Cli.Tests.UI;

public class NextNetConsoleTests
{
    [Fact]
    public void DetectMode_Plain_ReturnsPlain()
    {
        var mode = NextNetConsole.DetectMode(plain: true);
        Assert.Equal(OutputMode.Plain, mode);
    }

    [Fact]
    public void DetectMode_NoColor_ReturnsPlain()
    {
        var mode = NextNetConsole.DetectMode(noColor: true);
        Assert.Equal(OutputMode.Plain, mode);
    }

    [Fact]
    public void DetectMode_Default_ReturnsColor()
    {
        var mode = NextNetConsole.DetectMode();
        Assert.Equal(OutputMode.Color, mode);
    }

    [Fact]
    public void Create_PlainMode_IsPlainTrue()
    {
        var console = NextNetConsole.Create(plain: true);
        Assert.True(console.IsPlain);
        Assert.False(console.IsColor);
    }

    [Fact]
    public void Create_ColorMode_IsColorTrue()
    {
        var console = NextNetConsole.Create(plain: false);
        Assert.True(console.IsColor);
        Assert.False(console.IsPlain);
    }

    [Fact]
    public void Create_WithOutputMode_RespectsMode()
    {
        var console = new NextNetConsole(OutputMode.Plain);
        Assert.True(console.IsPlain);

        var console2 = new NextNetConsole(OutputMode.Color);
        Assert.True(console2.IsColor);

        var console3 = new NextNetConsole(OutputMode.Json);
        Assert.True(console3.IsJson);
    }

    [Fact]
    public void CreatePlain_WriteMethods_DoNotThrow()
    {
        var console = NextNetConsole.Create(plain: true);
        console.Write("test");
        console.WriteLine("test");
        console.WriteHeading("test");
        console.WriteSubheading("test");
        console.WriteMuted("test");
        console.WriteSuccess("test");
        console.WriteWarning("test");
        console.WriteError("test");
        console.WriteInfo("test");
        console.WriteCode("test");
        console.WriteLink("label", "https://example.com");
    }

    [Fact]
    public void CreateColor_WriteMethods_DoNotThrow()
    {
        var console = NextNetConsole.Create(plain: false);
        console.Write("test");
        console.WriteLine("test");
        console.WriteHeading("test");
        console.WriteSubheading("test");
        console.WriteMuted("test");
        console.WriteSuccess("test");
        console.WriteWarning("test");
        console.WriteError("test");
        console.WriteInfo("test");
        console.WriteCode("test");
        console.WriteLink("label", "https://example.com");
    }

    [Fact]
    public void Factory_CreatePanel_ReturnsPanel()
    {
        var console = NextNetConsole.Create(plain: true);
        var panel = console.CreatePanel("Test");
        Assert.NotNull(panel);
    }

    [Fact]
    public void Factory_CreateTable_ReturnsTable()
    {
        var console = NextNetConsole.Create(plain: true);
        var table = console.CreateTable("A", "B");
        Assert.NotNull(table);
    }

    [Fact]
    public void Factory_CreateTree_ReturnsTree()
    {
        var console = NextNetConsole.Create(plain: true);
        var tree = console.CreateTree("Root");
        Assert.NotNull(tree);
    }

    [Fact]
    public void Factory_CreateProgress_ReturnsProgress()
    {
        var console = NextNetConsole.Create(plain: true);
        var progress = console.CreateProgress();
        Assert.NotNull(progress);
    }

    [Fact]
    public void Factory_CreateSpinner_ReturnsSpinner()
    {
        var console = NextNetConsole.Create(plain: true);
        using var spinner = console.CreateSpinner("Loading...");
        Assert.NotNull(spinner);
    }
}
