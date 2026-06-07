using NextNet.DevTools.Panels;
using NextNet.DevTools.UI;
using Xunit;

namespace NextNet.DevTools.Tests.Panels;

public class ConsolePanelTests
{
    [Fact]
    public void Panel_HasCorrectName()
    {
        var store = new DevToolsDataStore();
        var panel = new ConsolePanel(store);

        Assert.Equal("Console", panel.Name);
    }

    [Fact]
    public void Panel_HasIcon()
    {
        var store = new DevToolsDataStore();
        var panel = new ConsolePanel(store);
        Assert.NotNull(panel.Icon);
        Assert.NotEmpty(panel.Icon);
    }

    [Fact]
    public void Render_WithEmptyStore_NoCrash()
    {
        var store = new DevToolsDataStore();
        var panel = new ConsolePanel(store);
        var context = new TuiRenderContext(80);

        var output = CaptureConsoleOutput(() => panel.Render(context));
        Assert.Contains("Console Log", output);
        Assert.Contains("No console output yet", output);
    }

    [Fact]
    public void Render_WithLogs_ShowsContent()
    {
        var store = new DevToolsDataStore();
        store.AddConsoleLog("[INFO] Server started");
        store.AddConsoleLog("[WARN] Route conflict detected");
        store.AddConsoleLog("[ERROR] Build failed");
        store.AddConsoleLog("✓ HMR update applied");

        var panel = new ConsolePanel(store);
        var context = new TuiRenderContext(80);

        var output = CaptureConsoleOutput(() => panel.Render(context));
        Assert.Contains("Console Log", output);
        Assert.Contains("4 entries", output);
        Assert.Contains("Server started", output);
        Assert.Contains("Build failed", output);
    }

    [Fact]
    public void HandleInput_C_ClearsLogs()
    {
        var store = new DevToolsDataStore();
        store.AddConsoleLog("Test log");

        var panel = new ConsolePanel(store);
        Assert.Single(store.GetConsoleLogs());

        panel.HandleInput(ConsoleKey.C);
        Assert.Empty(store.GetConsoleLogs());
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
