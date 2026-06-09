using Microsoft.Extensions.DependencyInjection;
using NextNet.Components;
using NextNet.Exceptions;
using Xunit;

namespace NextNet.Layouts.Tests;

public class LayoutRendererTests
{
    // ─── Test layout doubles ──────────────────────────────────────────────

    /// <summary>
    /// Simple layout that wraps content with HTML comments for verification.
    /// </summary>
    private sealed class CommentWrapLayout : ILayout
    {
        public string Name { get; }

        public CommentWrapLayout()
        {
            Name = "unnamed";
        }

        public CommentWrapLayout(string name)
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

    /// <summary>
    /// Layout that wraps content in a div with a class.
    /// </summary>
    private sealed class DivWrapLayout : ILayout
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

    // ─── Helpers ──────────────────────────────────────────────────────────

    private static IServiceProvider CreateServiceProvider(
        Action<ServiceCollection>? configure = null)
    {
        var services = new ServiceCollection();
        configure?.Invoke(services);
        return services.BuildServiceProvider();
    }

    private static LayoutRenderer CreateRenderer()
    {
        return new LayoutRenderer();
    }

    // ─── Tests ────────────────────────────────────────────────────────────

    [Fact]
    public async Task RenderAsync_Should_ReturnBareContent_When_NoLayoutTypes()
    {
        var renderer = CreateRenderer();
        var services = CreateServiceProvider();
        var content = new RawHtmlContent("<p>bare</p>");

        var result = await renderer.RenderAsync(content, Array.Empty<Type>(), services);

        Assert.Equal("<p>bare</p>", result.ToHtml());
    }

    [Fact]
    public async Task RenderAsync_Should_ThrowArgumentNullException_When_PageContentIsNull()
    {
        var renderer = CreateRenderer();
        var services = CreateServiceProvider();

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            renderer.RenderAsync(null!, Array.Empty<Type>(), services));
    }

    [Fact]
    public async Task RenderAsync_Should_ThrowArgumentNullException_When_LayoutTypesIsNull()
    {
        var renderer = CreateRenderer();
        var services = CreateServiceProvider();

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            renderer.RenderAsync(new RawHtmlContent("x"), null!, services));
    }

    [Fact]
    public async Task RenderAsync_Should_ThrowArgumentNullException_When_ServiceProviderIsNull()
    {
        var renderer = CreateRenderer();

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            renderer.RenderAsync(new RawHtmlContent("x"), Array.Empty<Type>(), null!));
    }

    [Fact]
    public async Task RenderAsync_Should_WrapContent_When_SingleLayout()
    {
        var renderer = CreateRenderer();
        var services = CreateServiceProvider(s => s.AddScoped<DivWrapLayout>());
        var types = new[] { typeof(DivWrapLayout) };
        var content = new RawHtmlContent("<span>inner</span>");

        var result = await renderer.RenderAsync(content, types, services);

        Assert.Equal("<div class=\"wrapper\"><span>inner</span></div>", result.ToHtml());
    }

    [Fact]
    public async Task RenderAsync_Should_WrapInsideOut_When_TwoLayouts()
    {
        var renderer = CreateRenderer();
        var services = CreateServiceProvider(s =>
        {
            s.AddScoped<CommentWrapLayout>();
            s.AddScoped<DivWrapLayout>();
        });
        // Layout types ordered innermost → outermost
        var types = new[] { typeof(DivWrapLayout), typeof(CommentWrapLayout) };
        var content = new RawHtmlContent("CONTENT");

        var result = await renderer.RenderAsync(content, types, services);

        // DivWrapLayout wraps CONTENT first (innermost),
        // then CommentWrapLayout wraps the result (outermost)
        Assert.Equal("<!--unnamed:start--><div class=\"wrapper\">CONTENT</div><!--unnamed:end-->",
            result.ToHtml());
    }

    [Fact]
    public async Task RenderAsync_Should_ComposeCorrectly_When_ThreeLayouts()
    {
        var renderer = CreateRenderer();

        // Two distinct layout types
        var services = CreateServiceProvider(s =>
        {
            s.AddScoped<OuterLayout>();
            s.AddScoped<MiddleLayout>();
            s.AddScoped<InnerLayout>();
        });
        var types = new[] { typeof(InnerLayout), typeof(MiddleLayout), typeof(OuterLayout) };
        var content = new RawHtmlContent("PAGE");

        var result = await renderer.RenderAsync(content, types, services);

        // Inner wraps in <span>, Middle wraps in <div>, Outer wraps in comments
        var expected = "<!--outer:start--><div><span>PAGE</span></div><!--outer:end-->";
        Assert.Equal(expected, result.ToHtml());
    }

    [Fact]
    public async Task RenderAsync_Should_ThrowRenderException_When_LayoutNotRegistered()
    {
        var renderer = CreateRenderer();
        var services = CreateServiceProvider(); // DivWrapLayout not registered
        var types = new[] { typeof(DivWrapLayout) };

        var ex = await Assert.ThrowsAsync<RenderException>(() =>
            renderer.RenderAsync(new RawHtmlContent("test"), types, services));
        Assert.Contains("not registered", ex.Message);
    }

    [Fact]
    public async Task RenderAsync_Should_ThrowRenderException_When_LayoutTypeNotInDi()
    {
        var renderer = CreateRenderer();
        var services = CreateServiceProvider(); // no registrations
        var types = new[] { typeof(DivWrapLayout) }; // not registered in DI

        var ex = await Assert.ThrowsAsync<RenderException>(() =>
            renderer.RenderAsync(new RawHtmlContent("test"), types, services));
        Assert.Contains("not registered", ex.Message);
    }
}

// ─── Helper types for multi-level tests ─────────────────────────────────

file sealed class OuterLayout : ILayout
{
    public Task<IHtmlContent> Render(IHtmlContent children)
        => Task.FromResult(HtmlHelper.Fragment(
            new RawHtmlContent("<!--outer:start-->"),
            children,
            new RawHtmlContent("<!--outer:end-->")
        ));
}

file sealed class MiddleLayout : ILayout
{
    public Task<IHtmlContent> Render(IHtmlContent children)
        => Task.FromResult(HtmlHelper.Element("div", content: children));
}

file sealed class InnerLayout : ILayout
{
    public Task<IHtmlContent> Render(IHtmlContent children)
        => Task.FromResult(HtmlHelper.Element("span", content: children));
}
