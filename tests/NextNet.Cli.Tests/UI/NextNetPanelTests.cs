using NextNet.Cli.UI;
using Xunit;

namespace NextNet.Cli.Tests.UI;

public class NextNetPanelTests
{
    [Fact]
    public void CreatePanel_HasCorrectTitle()
    {
        var panel = new NextNetPanel("Test Panel");
        Assert.NotNull(panel);
        Assert.NotNull(panel.GetSpectrePanel());
    }

    [Fact]
    public void CreatePanel_PlainMode_Works()
    {
        var panel = new NextNetPanel("Test Panel", OutputMode.Plain);
        Assert.NotNull(panel);
    }

    [Fact]
    public void SetContent_String_DoesNotThrow()
    {
        var panel = new NextNetPanel("Test");
        panel.SetContent("Hello World");
    }

    [Fact]
    public void SetContent_Renderable_DoesNotThrow()
    {
        var panel = new NextNetPanel("Test");
        panel.SetContent(new Spectre.Console.Text("Hello"));
    }

    [Fact]
    public void SetContent_Factory_DoesNotThrow()
    {
        var panel = new NextNetPanel("Test");
        panel.SetContent(() => new Spectre.Console.Text("Hello"));
    }
}
