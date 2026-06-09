using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NextNet.Edge.Streaming;
using NextNet.Routing;
using Xunit;

namespace NextNet.Edge.Tests.Streaming;

public class EdgeStreamingHtmlRendererTests
{
    [Fact]
    public void Constructor_Should_Throw_When_InnerRendererIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new EdgeStreamingHtmlRenderer(null!, new EdgeOptions()));
    }

    [Fact]
    public void Constructor_Should_Throw_When_OptionsIsNull()
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
    public async Task RenderToStreamAsync_Should_Throw_When_RouteIsNull()
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
    public async Task RenderToStreamAsync_Should_Throw_When_ContextIsNull()
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
    public async Task RenderToStreamAsync_Should_Throw_When_WriterIsNull()
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
