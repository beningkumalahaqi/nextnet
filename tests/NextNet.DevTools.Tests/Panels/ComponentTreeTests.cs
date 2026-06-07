using NextNet.DevTools.Panels;
using NextNet.DevTools.UI;
using Xunit;

namespace NextNet.DevTools.Tests.Panels;

public class ComponentTreeTests
{
    [Fact]
    public void Panel_HasCorrectName()
    {
        var store = new DevToolsDataStore();
        var panel = new ComponentTreePanel(store);

        Assert.Equal("Component Tree", panel.Name);
    }

    [Fact]
    public void Panel_HasIcon()
    {
        var store = new DevToolsDataStore();
        var panel = new ComponentTreePanel(store);

        Assert.NotNull(panel.Icon);
        Assert.NotEmpty(panel.Icon);
    }

    [Fact]
    public void Render_WithEmptyStore_NoCrash()
    {
        var store = new DevToolsDataStore();
        var panel = new ComponentTreePanel(store);
        var context = new TuiRenderContext(80);

        var output = CaptureConsoleOutput(() => panel.Render(context));
        Assert.Contains("Component Tree", output);
    }

    [Fact]
    public void Render_WithComponents_ShowsComponents()
    {
        var store = new DevToolsDataStore();
        store.SetComponents(new[]
        {
            new ComponentTreePanel.ComponentInfo
            {
                Name = "Layout",
                Type = "layout",
                File = "app/layout.cs",
                RenderCount = 5
            },
            new ComponentTreePanel.ComponentInfo
            {
                Name = "HomePage",
                Type = "page",
                File = "app/page.cs",
                Parent = "Layout",
                RenderCount = 3
            }
        });

        var panel = new ComponentTreePanel(store);
        var context = new TuiRenderContext(80);

        var output = CaptureConsoleOutput(() => panel.Render(context));
        Assert.Contains("Component Tree", output);
    }

    [Fact]
    public void HandleInput_G_TogglesTreeMode()
    {
        var store = new DevToolsDataStore();
        var panel = new ComponentTreePanel(store);

        panel.HandleInput(ConsoleKey.G);

        // Should not throw
        var context = new TuiRenderContext(80);
        var output = CaptureConsoleOutput(() => panel.Render(context));
        Assert.Contains("LIST", output);
    }

    [Fact]
    public void HandleInput_Enter_TogglesDetails()
    {
        var store = new DevToolsDataStore();
        store.SetComponents(new[]
        {
            new ComponentTreePanel.ComponentInfo { Name = "Test", Type = "component" }
        });

        var panel = new ComponentTreePanel(store);

        panel.HandleInput(ConsoleKey.Enter);

        var context = new TuiRenderContext(80);
        CaptureConsoleOutput(() => panel.Render(context));
    }

    [Fact]
    public void Render_ListMode_ShowsTable()
    {
        var store = new DevToolsDataStore();
        store.SetComponents(new[]
        {
            new ComponentTreePanel.ComponentInfo
            {
                Name = "Layout",
                Type = "layout",
                File = "app/layout.cs",
                RenderCount = 5,
                AverageRenderTimeMs = 10
            },
            new ComponentTreePanel.ComponentInfo
            {
                Name = "Page",
                Type = "page",
                File = "app/page.cs",
                Parent = "Layout",
                RenderCount = 3,
                AverageRenderTimeMs = 8
            }
        });

        var panel = new ComponentTreePanel(store);
        panel.HandleInput(ConsoleKey.G); // Switch to list mode

        var context = new TuiRenderContext(80);
        var output = CaptureConsoleOutput(() => panel.Render(context));
        Assert.Contains("Layout", output);
        Assert.Contains("Page", output);
        Assert.Contains("5", output);
        Assert.Contains("3", output);
    }

    [Fact]
    public void HandleInput_DownArrow_WithEmptyStore()
    {
        var store = new DevToolsDataStore();
        var panel = new ComponentTreePanel(store);

        // Should not throw with empty store
        panel.HandleInput(ConsoleKey.DownArrow);
        panel.HandleInput(ConsoleKey.DownArrow);
    }

    [Fact]
    public void Render_TreeMode_WithNestedComponents()
    {
        var store = new DevToolsDataStore();
        store.SetComponents(new[]
        {
            new ComponentTreePanel.ComponentInfo { Name = "Root", Type = "layout" },
            new ComponentTreePanel.ComponentInfo { Name = "Child1", Type = "component", Parent = "Root" },
            new ComponentTreePanel.ComponentInfo { Name = "Child2", Type = "component", Parent = "Root" },
            new ComponentTreePanel.ComponentInfo { Name = "Grandchild", Type = "component", Parent = "Child1" }
        });

        var panel = new ComponentTreePanel(store);
        var context = new TuiRenderContext(80);
        var output = CaptureConsoleOutput(() => panel.Render(context));
        Assert.Contains("Component Tree", output);
    }

    [Fact]
    public void ComponentInfo_WithChildren()
    {
        var info = new ComponentTreePanel.ComponentInfo
        {
            Name = "Parent",
            Type = "layout",
            Children = new[] { "Child1", "Child2" }
        };

        Assert.Equal(2, info.Children.Count);
        Assert.Contains("Child1", info.Children);
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

    [Fact]
    public void ComponentInfo_DefaultValues()
    {
        var info = new ComponentTreePanel.ComponentInfo
        {
            Name = "TestComp"
        };

        Assert.Equal("TestComp", info.Name);
        Assert.Equal("component", info.Type); // default
        Assert.Empty(info.Children);
    }
}
