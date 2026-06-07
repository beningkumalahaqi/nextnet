using NextNet.DevTools.Panels;
using NextNet.DevTools.UI;
using Xunit;

namespace NextNet.DevTools.Tests.Panels;

public class PerformanceProfilerTests
{
    [Fact]
    public void Panel_HasCorrectName()
    {
        var store = new DevToolsDataStore();
        var panel = new PerformanceProfilerPanel(store);

        Assert.Equal("Performance", panel.Name);
    }

    [Fact]
    public void Panel_HasIcon()
    {
        var store = new DevToolsDataStore();
        var panel = new PerformanceProfilerPanel(store);

        Assert.NotNull(panel.Icon);
        Assert.NotEmpty(panel.Icon);
    }

    [Fact]
    public void Render_WithEmptyStore_NoCrash()
    {
        var store = new DevToolsDataStore();
        var panel = new PerformanceProfilerPanel(store);
        var context = new TuiRenderContext(80);

        var output = CaptureConsoleOutput(() => panel.Render(context));
        Assert.Contains("Performance Profiler", output);
        Assert.Contains("No metrics collected yet", output);
    }

    [Fact]
    public void Render_WithMetrics_ShowsBars()
    {
        var store = new DevToolsDataStore();
        store.AddMetric(new PerformanceProfilerPanel.PerformanceMetric
        {
            Name = "Route discovery",
            DurationMs = 120,
            Category = "build",
            Timestamp = DateTime.UtcNow
        });
        store.AddMetric(new PerformanceProfilerPanel.PerformanceMetric
        {
            Name = "Render /about",
            DurationMs = 45,
            Category = "render",
            Timestamp = DateTime.UtcNow
        });

        var panel = new PerformanceProfilerPanel(store);
        var context = new TuiRenderContext(80);

        var output = CaptureConsoleOutput(() => panel.Render(context));
        Assert.Contains("Performance Profiler", output);
        Assert.Contains("Route discovery", output);
        Assert.Contains("120ms", output);
        Assert.Contains("45ms", output);
    }

    [Fact]
    public void HandleInput_P_TogglesPause()
    {
        var store = new DevToolsDataStore();
        var panel = new PerformanceProfilerPanel(store);

        // Should not throw
        panel.HandleInput(ConsoleKey.P);

        var context = new TuiRenderContext(80);
        var output = CaptureConsoleOutput(() => panel.Render(context));
        Assert.Contains("PAUSED", output);
    }

    [Fact]
    public void HandleInput_C_ClearsMetrics()
    {
        var store = new DevToolsDataStore();
        store.AddMetric(new PerformanceProfilerPanel.PerformanceMetric
        {
            Name = "Test",
            DurationMs = 100
        });

        var panel = new PerformanceProfilerPanel(store);

        Assert.Single(store.GetMetrics());
        panel.HandleInput(ConsoleKey.C);
        Assert.Empty(store.GetMetrics());
    }

    [Fact]
    public void HandleInput_S_TogglesSort()
    {
        var store = new DevToolsDataStore();
        var panel = new PerformanceProfilerPanel(store);

        panel.HandleInput(ConsoleKey.S);

        var context = new TuiRenderContext(80);
        var output = CaptureConsoleOutput(() => panel.Render(context));
        Assert.Contains("by duration", output);
    }

    /// <summary>
    /// Captures console output written during the action.
    /// </summary>
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
