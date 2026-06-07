using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NextNet.Edge.Streaming;
using NextNet.Routing;
using Xunit;

namespace NextNet.Edge.Tests.Streaming;

public class EdgeStreamingHtmlRendererTests
{
    [Fact]
    public void Constructor_NullInnerRenderer_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new EdgeStreamingHtmlRenderer(null!, new EdgeOptions()));
    }

    [Fact]
    public void Constructor_NullOptions_Throws()
    {
        var innerRenderer = new NextNet.Rendering.Streaming.StreamingHtmlRenderer(
            new NextNet.Rendering.SsrRenderer(
                new ServiceCollection().BuildServiceProvider(),
                RouteManifest.Empty),
            null);

        Assert.Throws<ArgumentNullException>(() =>
            new EdgeStreamingHtmlRenderer(innerRenderer, null!));
    }

    [Fact]
    public async Task RenderToStreamAsync_NullRoute_Throws()
    {
        var innerRenderer = new NextNet.Rendering.Streaming.StreamingHtmlRenderer(
            new NextNet.Rendering.SsrRenderer(
                new ServiceCollection().BuildServiceProvider(),
                RouteManifest.Empty),
            null);
        var renderer = new EdgeStreamingHtmlRenderer(innerRenderer, new EdgeOptions());
        var writer = new EdgeStreamWriter(new MemoryStream(), new EdgeOptions());

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            renderer.RenderToStreamAsync(null!,
                new NextNet.Components.ComponentContext(new DefaultHttpContext()),
                writer));
    }

    [Fact]
    public async Task RenderToStreamAsync_NullContext_Throws()
    {
        var innerRenderer = new NextNet.Rendering.Streaming.StreamingHtmlRenderer(
            new NextNet.Rendering.SsrRenderer(
                new ServiceCollection().BuildServiceProvider(),
                RouteManifest.Empty),
            null);
        var renderer = new EdgeStreamingHtmlRenderer(innerRenderer, new EdgeOptions());
        var writer = new EdgeStreamWriter(new MemoryStream(), new EdgeOptions());

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            renderer.RenderToStreamAsync("/", null!, writer));
    }

    [Fact]
    public async Task RenderToStreamAsync_NullWriter_Throws()
    {
        var innerRenderer = new NextNet.Rendering.Streaming.StreamingHtmlRenderer(
            new NextNet.Rendering.SsrRenderer(
                new ServiceCollection().BuildServiceProvider(),
                RouteManifest.Empty),
            null);
        var renderer = new EdgeStreamingHtmlRenderer(innerRenderer, new EdgeOptions());

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            renderer.RenderToStreamAsync("/",
                new NextNet.Components.ComponentContext(new DefaultHttpContext()), null!));
    }
}
