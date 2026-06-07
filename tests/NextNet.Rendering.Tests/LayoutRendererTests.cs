using Microsoft.Extensions.DependencyInjection;
using NextNet.Components;
using NextNet.Rendering.Tests.Fixtures.SampleApp.app;
using NextNet.Rendering.Tests.Fixtures.SampleApp.app.blog;
using Xunit;

namespace NextNet.Rendering.Tests;

public class LayoutRendererTests
{
    // ─── Test doubles ─────────────────────────────────────────────────────

    private sealed class PassthroughLayout : ILayout
    {
        public string Name { get; }

        public PassthroughLayout(string name)
        {
            Name = name;
        }

        public Task<IHtmlContent> Render(IHtmlContent children)
        {
            return Task.FromResult(HtmlHelper.Fragment(
                new RawHtmlContent($"<!--{Name}:start-->"),
                children,
                new RawHtmlContent($"<!--{Name}:end-->")
            ));
        }
    }

    private sealed class WrappingLayout : ILayout
    {
        public Task<IHtmlContent> Render(IHtmlContent children)
        {
            return Task.FromResult(
                HtmlHelper.Element("div",
                    new Dictionary<string, string> { ["class"] = "wrapper" },
                    content: children)
            );
        }
    }

    private sealed class EmptyPage : IPage
    {
        public IReadOnlyDictionary<string, object> Props { get; } = new Dictionary<string, object>();
        public Task<IHtmlContent> Render() => Task.FromResult(HtmlHelper.Text("Hello, World!"));
    }

    // ─── Helpers ──────────────────────────────────────────────────────────

    private static LayoutRenderer CreateLayoutRenderer(
        IReadOnlyDictionary<string, Type>? layoutMap = null)
    {
        layoutMap ??= new Dictionary<string, Type>();
        var resolver = new ConventionRouteComponentResolver(
            new Dictionary<string, Type>(), layoutMap);
        return new LayoutRenderer(resolver);
    }

    private static IServiceProvider CreateServiceProvider(
        Action<ServiceCollection>? configure = null)
    {
        var services = new ServiceCollection();
        configure?.Invoke(services);
        return services.BuildServiceProvider();
    }

    // ─── Tests ────────────────────────────────────────────────────────────

    [Fact]
    public async Task RenderAsync_WithEmptyLayoutChain_ReturnsBareContent()
    {
        var renderer = CreateLayoutRenderer();
        var services = CreateServiceProvider();
        var content = new RawHtmlContent("<p>bare</p>");

        var result = await renderer.RenderAsync(content, Array.Empty<string>(), services);

        Assert.Equal("<p>bare</p>", result.ToHtml());
    }

    [Fact]
    public async Task RenderAsync_WithNullLayoutChain_ThrowsArgumentNullException()
    {
        var renderer = CreateLayoutRenderer();
        var services = CreateServiceProvider();

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            renderer.RenderAsync(new RawHtmlContent("x"), null!, services));
    }

    [Fact]
    public async Task RenderAsync_WithSingleLayout_WrapsContent()
    {
        var layoutMap = new Dictionary<string, Type>
        {
            ["root"] = typeof(WrappingLayout)
        };
        var renderer = CreateLayoutRenderer(layoutMap);
        var services = CreateServiceProvider(s => s.AddScoped<WrappingLayout>());
        var content = new RawHtmlContent("<span>inner</span>");

        var result = await renderer.RenderAsync(content, new[] { "root" }, services);

        Assert.Equal("<div class=\"wrapper\"><span>inner</span></div>", result.ToHtml());
    }

    [Fact]
    public async Task RenderAsync_WithTwoLayouts_WrapsNested()
    {
        var layoutMap = new Dictionary<string, Type>
        {
            ["outer"] = typeof(PassthroughLayout),
            ["inner"] = typeof(WrappingLayout)
        };
        // Need named instances for PassthroughLayout
        var services = CreateServiceProvider(s =>
        {
            s.AddScoped<WrappingLayout>();
            s.AddScoped(_ => new PassthroughLayout("outer"));
        });
        var renderer = CreateLayoutRenderer(layoutMap);

        // But the resolver only resolves by file path, and PassthroughLayout is registered
        // as itself. Let me use a different approach for two layouts.
        // Actually, each layout type needs a unique registration.
        // Let me use two separate types instead.

        var outerLayoutMap = new Dictionary<string, Type>
        {
            ["outer"] = typeof(OuterTestLayout),
            ["inner"] = typeof(InnerTestLayout)
        };
        var outerServices = CreateServiceProvider(s =>
        {
            s.AddScoped<OuterTestLayout>();
            s.AddScoped<InnerTestLayout>();
        });
        var outerRenderer = CreateLayoutRenderer(outerLayoutMap);
        var content = new RawHtmlContent("CONTENT");

        var result = await outerRenderer.RenderAsync(content, new[] { "inner", "outer" }, outerServices);

        Assert.Equal("<!--outer:start--><!--inner:start-->CONTENT<!--inner:end--><!--outer:end-->",
            result.ToHtml());
    }

    [Fact]
    public async Task RenderAsync_WithThreeLevels_ComposesCorrectly()
    {
        // Innermost wraps in <span>
        // Middle wraps in <div>
        // Outermost wraps in <!--markers-->
        var layoutMap = new Dictionary<string, Type>
        {
            ["outer"] = typeof(OuterTestLayout),
            ["middle"] = typeof(MiddleTestLayout),
            ["inner"] = typeof(InnerTestLayout)
        };
        var services = CreateServiceProvider(s =>
        {
            s.AddScoped<OuterTestLayout>();
            s.AddScoped<MiddleTestLayout>();
            s.AddScoped<InnerTestLayout>();
        });
        var resolver = new ConventionRouteComponentResolver(
            new Dictionary<string, Type>(), layoutMap);
        var renderer = new LayoutRenderer(resolver);
        var content = new RawHtmlContent("PAGE");

        var result = await renderer.RenderAsync(content, new[] { "inner", "middle", "outer" }, services);

        // inner wraps in <!--inner:start--><!--inner:end-->
        // middle wraps in <div>...</div>
        // outer wraps in <!--outer:start--><!--outer:end-->
        var expected = "<!--outer:start--><div><!--inner:start-->PAGE<!--inner:end--></div><!--outer:end-->";
        Assert.Equal(expected, result.ToHtml());
    }

    [Fact]
    public async Task RenderAsync_WithUnregisteredLayout_ThrowsRenderException()
    {
        var layoutMap = new Dictionary<string, Type>
        {
            ["missing"] = typeof(WrappingLayout)
        };
        var resolver = new ConventionRouteComponentResolver(
            new Dictionary<string, Type>(), layoutMap);
        var renderer = new LayoutRenderer(resolver);
        var services = CreateServiceProvider(); // WrappingLayout not registered

        var content = new RawHtmlContent("test");

        var ex = await Assert.ThrowsAsync<NextNet.Exceptions.RenderException>(() =>
            renderer.RenderAsync(content, new[] { "missing" }, services));
        Assert.Contains("not registered", ex.Message);
    }

    [Fact]
    public async Task RenderAsync_WithUnresolvableLayoutPath_ThrowsRenderException()
    {
        var resolver = new ConventionRouteComponentResolver(
            new Dictionary<string, Type>(), new Dictionary<string, Type>());
        var renderer = new LayoutRenderer(resolver);
        var services = CreateServiceProvider();

        var ex = await Assert.ThrowsAsync<NextNet.Exceptions.RenderException>(() =>
            renderer.RenderAsync(new RawHtmlContent("x"), new[] { "nonexistent" }, services));
        Assert.Contains("Cannot resolve layout type", ex.Message);
    }
}

// Helper layout types for multi-level tests
file sealed class OuterTestLayout : ILayout
{
    public Task<IHtmlContent> Render(IHtmlContent children)
        => Task.FromResult(HtmlHelper.Fragment(
            new RawHtmlContent("<!--outer:start-->"),
            children,
            new RawHtmlContent("<!--outer:end-->")
        ));
}

file sealed class MiddleTestLayout : ILayout
{
    public Task<IHtmlContent> Render(IHtmlContent children)
        => Task.FromResult(HtmlHelper.Element("div", content: children));
}

file sealed class InnerTestLayout : ILayout
{
    public Task<IHtmlContent> Render(IHtmlContent children)
        => Task.FromResult(HtmlHelper.Fragment(
            new RawHtmlContent("<!--inner:start-->"),
            children,
            new RawHtmlContent("<!--inner:end-->")
        ));
}
