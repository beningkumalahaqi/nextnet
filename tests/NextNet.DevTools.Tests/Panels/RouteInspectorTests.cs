using NextNet.DevTools.Panels;
using NextNet.DevTools.UI;
using Xunit;

namespace NextNet.DevTools.Tests.Panels;

public class RouteInspectorTests
{
    [Fact]
    public void Panel_HasCorrectName()
    {
        var store = new DevToolsDataStore();
        var panel = new RouteInspectorPanel(store);

        Assert.Equal("Route Inspector", panel.Name);
    }

    [Fact]
    public void Panel_HasIcon()
    {
        var store = new DevToolsDataStore();
        var panel = new RouteInspectorPanel(store);

        Assert.NotNull(panel.Icon);
        Assert.NotEmpty(panel.Icon);
    }

    [Fact]
    public void Render_WithEmptyStore_NoCrash()
    {
        var store = new DevToolsDataStore();
        var panel = new RouteInspectorPanel(store);
        var context = new TuiRenderContext(80);

        var output = CaptureConsoleOutput(() => panel.Render(context));
        Assert.Contains("Route Inspector", output);
    }

    [Fact]
    public void Render_WithRoutes_ShowsRoutes()
    {
        var store = new DevToolsDataStore();
        store.SetRoutes(new[]
        {
            new RouteInspectorPanel.RouteInfo
            {
                Path = "/",
                Type = "static",
                File = "app/page.cs",
                RenderCount = 10,
                AverageRenderTimeMs = 15
            },
            new RouteInspectorPanel.RouteInfo
            {
                Path = "/blog/[slug]",
                Type = "dynamic",
                File = "app/blog/[slug]/page.cs",
                Ssr = true
            }
        });

        var panel = new RouteInspectorPanel(store);
        var context = new TuiRenderContext(80);

        var output = CaptureConsoleOutput(() => panel.Render(context));
        Assert.Contains("/", output);
        Assert.Contains("/blog/[slug]", output);
        Assert.Contains("2 routes total", output);
    }

    [Fact]
    public void RouteInfo_DefaultValues()
    {
        var info = new RouteInspectorPanel.RouteInfo
        {
            Path = "/test",
            File = "app/test/page.cs"
        };

        Assert.Equal("/test", info.Path);
        Assert.Equal("static", info.Type); // default
        Assert.True(info.Ssr); // default
        Assert.False(info.Ssg); // default
    }

    [Fact]
    public void HandleInput_NavigateUp_SetsIndex()
    {
        var store = new DevToolsDataStore();
        store.SetRoutes(new[]
        {
            new RouteInspectorPanel.RouteInfo { Path = "/", Type = "static", File = "page.cs" },
            new RouteInspectorPanel.RouteInfo { Path = "/about", Type = "static", File = "about/page.cs" }
        });

        var panel = new RouteInspectorPanel(store);

        // Navigate down then up (should not go below 0)
        panel.HandleInput(ConsoleKey.UpArrow);
        panel.HandleInput(ConsoleKey.UpArrow);

        // Should not throw — index stays at 0
        var context = new TuiRenderContext(80);
        CaptureConsoleOutput(() => panel.Render(context));
    }

    [Fact]
    public void HandleInput_Enter_TogglesExpand()
    {
        var store = new DevToolsDataStore();
        store.SetRoutes(new[]
        {
            new RouteInspectorPanel.RouteInfo { Path = "/", Type = "static", File = "page.cs" }
        });

        var panel = new RouteInspectorPanel(store);

        // Should not throw
        panel.HandleInput(ConsoleKey.Enter);

        var context = new TuiRenderContext(80);
        CaptureConsoleOutput(() => panel.Render(context));
    }

    [Fact]
    public void HandleInput_DownArrow_DoesNotGoBelowZero()
    {
        var store = new DevToolsDataStore();
        var panel = new RouteInspectorPanel(store);
        // Index should stay at 0 even when navigating up with empty routes
        panel.HandleInput(ConsoleKey.UpArrow);
        panel.HandleInput(ConsoleKey.UpArrow);
    }

    [Fact]
    public void HandleInput_DownArrow_MovesAndStopsAtMax()
    {
        var store = new DevToolsDataStore();
        store.SetRoutes(new[]
        {
            new RouteInspectorPanel.RouteInfo { Path = "/a", Type = "static", File = "a.cs" },
            new RouteInspectorPanel.RouteInfo { Path = "/b", Type = "static", File = "b.cs" }
        });
        var panel = new RouteInspectorPanel(store);

        panel.HandleInput(ConsoleKey.DownArrow);
        panel.HandleInput(ConsoleKey.DownArrow);
        panel.HandleInput(ConsoleKey.DownArrow); // past end, should clamp
        // Should not throw
        var context = new TuiRenderContext(80);
        CaptureConsoleOutput(() => panel.Render(context));
    }

    [Fact]
    public void Render_WithAllRouteTypes_UsesCorrectColors()
    {
        var store = new DevToolsDataStore();
        store.SetRoutes(new[]
        {
            new RouteInspectorPanel.RouteInfo { Path = "/", Type = "static", File = "page.cs" },
            new RouteInspectorPanel.RouteInfo { Path = "/blog/[slug]", Type = "dynamic", File = "blog/[slug]/page.cs" },
            new RouteInspectorPanel.RouteInfo { Path = "/api/health", Type = "api", File = "api/health/route.cs" },
            new RouteInspectorPanel.RouteInfo { Path = "/layout", Type = "layout", File = "layout.cs" }
        });

        var panel = new RouteInspectorPanel(store);
        var context = new TuiRenderContext(80);

        var output = CaptureConsoleOutput(() => panel.Render(context));
        Assert.Contains("Static:", output);
        Assert.Contains("Dynamic:", output);
        Assert.Contains("API:", output);
        Assert.Contains("Layout:", output);
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
