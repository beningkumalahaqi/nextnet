using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NextNet.Components;
using NextNet.Rendering.Streaming;
using NextNet.Routing;
using NextNet.Routing.Models;
using Xunit;

namespace NextNet.Rendering.Tests;

public class StreamingHtmlRendererTests
{
    // ─── Test doubles ─────────────────────────────────────────────────────

    private sealed class SimplePage : IPage
    {
        public IReadOnlyDictionary<string, object> Props { get; } = new Dictionary<string, object>();

        public Task<IHtmlContent> Render() =>
            Task.FromResult(HtmlHelper.Element("p", content: HtmlHelper.Text("Hello from SSR")));
    }

    private sealed class LargePage : IPage
    {
        public IReadOnlyDictionary<string, object> Props { get; } = new Dictionary<string, object>();

        public Task<IHtmlContent> Render()
        {
            var items = string.Join("", Enumerable.Range(0, 1000)
                .Select(i => $"<li>{i}</li>"));
            return Task.FromResult(HtmlHelper.Raw(
                $"<ul>{items}</ul>"));
        }
    }

    private sealed class FailingPage : IPage
    {
        public IReadOnlyDictionary<string, object> Props { get; } = new Dictionary<string, object>();

        public Task<IHtmlContent> Render() =>
            throw new InvalidOperationException("Streaming render failure");
    }

    // ─── Test data ────────────────────────────────────────────────────────

    private static RouteManifest CreateManifest(params RouteEntry[] pages)
    {
        return new RouteManifest(
            pages,
            pages.Where(p => p.Type == RouteType.Page).ToList(),
            Array.Empty<RouteEntry>(),
            Array.Empty<RouteEntry>(),
            null,
            Array.Empty<RouteConflict>());
    }

    private static RouteEntry PageEntry(string route, string filePath,
        string[]? layoutChain = null)
    {
        var entry = new RouteEntry(route, filePath, RouteType.Page, RouteSegmentKind.Static);
        entry.LayoutChain = (IReadOnlyList<string>)(layoutChain ?? Array.Empty<string>());
        return entry;
    }

    private static (StreamingHtmlRenderer Renderer, SsrRenderer Ssr) CreateStreamingRenderer(
        RouteManifest manifest,
        Action<ServiceCollection>? configureServices = null,
        IReadOnlyDictionary<string, Type>? pageMap = null,
        IReadOnlyDictionary<string, Type>? layoutMap = null,
        int bufferSize = 256)
    {
        var services = new ServiceCollection();
        configureServices?.Invoke(services);
        var sp = services.BuildServiceProvider();

        var resolver = new ConventionRouteComponentResolver(
            pageMap ?? new Dictionary<string, Type>(),
            layoutMap ?? new Dictionary<string, Type>());

        var options = new SsrOptions
        {
            Streaming = true,
            BufferSize = bufferSize,
            RenderTimeout = TimeSpan.FromSeconds(5)
        };

        var ssr = new SsrRenderer(sp, manifest, options, resolver);
        var streaming = new StreamingHtmlRenderer(ssr, options);
        return (streaming, ssr);
    }

    private static ComponentContext CreateContext()
    {
        return new ComponentContext(new DefaultHttpContext());
    }

    // ─── Chunk size tests ─────────────────────────────────────────────────

    [Theory]
    [InlineData(64)]
    [InlineData(128)]
    [InlineData(256)]
    [InlineData(1024)]
    public async Task RenderAsyncEnumerable_Should_ProduceCompleteOutput_WithVariedBufferSizes(int bufferSize)
    {
        var manifest = CreateManifest(PageEntry("/", "app/page.cs"));
        var pageMap = new Dictionary<string, Type> { ["app/page.cs"] = typeof(SimplePage) };
        var (renderer, _) = CreateStreamingRenderer(manifest,
            s => s.AddScoped<SimplePage>(),
            pageMap: pageMap,
            bufferSize: bufferSize);

        var chunks = new List<string>();
        await foreach (var chunk in renderer.RenderAsyncEnumerable("/", CreateContext()))
        {
            chunks.Add(chunk);
        }

        var combined = string.Join("", chunks);
        Assert.Contains("Hello from SSR", combined);
        Assert.Contains("<p>", combined);

        // Verify chunk boundaries
        if (bufferSize < combined.Length)
        {
            Assert.True(chunks.Count > 1,
                $"Expected multiple chunks for buffer size {bufferSize}, got {chunks.Count}");
        }
    }

    [Fact]
    public async Task RenderAsyncEnumerable_Should_Return404_WhenRouteNotFound()
    {
        var manifest = CreateManifest();
        var (renderer, _) = CreateStreamingRenderer(manifest);

        var chunks = new List<string>();
        await foreach (var chunk in renderer.RenderAsyncEnumerable("/missing", CreateContext()))
        {
            chunks.Add(chunk);
        }

        var combined = string.Join("", chunks);
        Assert.Contains("404", combined);
    }

    [Fact]
    public async Task RenderAsyncEnumerable_Should_ReturnErrorChunk_WhenPageFails()
    {
        var manifest = CreateManifest(PageEntry("/fail", "app/fail/page.cs"));
        var pageMap = new Dictionary<string, Type> { ["app/fail/page.cs"] = typeof(FailingPage) };
        var (renderer, _) = CreateStreamingRenderer(manifest,
            s => s.AddScoped<FailingPage>(),
            pageMap: pageMap);

        var chunks = new List<string>();
        await foreach (var chunk in renderer.RenderAsyncEnumerable("/fail", CreateContext()))
        {
            chunks.Add(chunk);
        }

        var combined = string.Join("", chunks);
        Assert.Contains("Internal Server Error", combined);
    }

    [Fact]
    public async Task RenderAsyncEnumerable_Should_YieldMultipleChunks_WhenPageIsLarge()
    {
        var manifest = CreateManifest(PageEntry("/large", "app/large/page.cs"));
        var pageMap = new Dictionary<string, Type> { ["app/large/page.cs"] = typeof(LargePage) };
        var (renderer, _) = CreateStreamingRenderer(manifest,
            s => s.AddScoped<LargePage>(),
            pageMap: pageMap,
            bufferSize: 512);

        var chunks = new List<string>();
        await foreach (var chunk in renderer.RenderAsyncEnumerable("/large", CreateContext()))
        {
            chunks.Add(chunk);
        }

        Assert.True(chunks.Count > 1, $"Expected multiple chunks, got {chunks.Count}");
        var combined = string.Join("", chunks);
        Assert.Contains("<li>", combined);
        Assert.Contains("</ul>", combined);
    }

    [Fact]
    public async Task RenderAsyncEnumerable_Should_Cancel_WhenCancellationTokenIsSet()
    {
        var manifest = CreateManifest(PageEntry("/large", "app/large/page.cs"));
        var pageMap = new Dictionary<string, Type> { ["app/large/page.cs"] = typeof(LargePage) };
        var (renderer, _) = CreateStreamingRenderer(manifest,
            s => s.AddScoped<LargePage>(),
            pageMap: pageMap,
            bufferSize: 128);

        using var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately

        var chunks = new List<string>();
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
        {
            await foreach (var chunk in renderer.RenderAsyncEnumerable(
                "/large", CreateContext(), cts.Token))
            {
                chunks.Add(chunk);
            }
        });
    }

    [Fact]
    public async Task RenderAsyncEnumerable_Should_ThrowArgumentNullException_WhenRouteIsNull()
    {
        var manifest = CreateManifest();
        var (renderer, _) = CreateStreamingRenderer(manifest);

        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await foreach (var chunk in renderer.RenderAsyncEnumerable(null!, CreateContext()))
            {
            }
        });
    }

    [Fact]
    public async Task RenderAsyncEnumerable_Should_ThrowArgumentNullException_WhenContextIsNull()
    {
        var manifest = CreateManifest();
        var (renderer, _) = CreateStreamingRenderer(manifest);

        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await foreach (var chunk in renderer.RenderAsyncEnumerable("/", null!))
            {
            }
        });
    }

    // ─── ChunkedHtmlWriter tests ──────────────────────────────────────────

    [Fact]
    public async Task ChunkedHtmlWriter_WriteToAsync_Should_WriteChunks_WhenCalled()
    {
        async IAsyncEnumerable<string> GetChunks()
        {
            yield return "Hello ";
            yield return "World";
            await Task.CompletedTask;
        }

        var writer = new ChunkedHtmlWriter(GetChunks());
        using var textWriter = new StringWriter();

        await writer.WriteToAsync(textWriter);

        Assert.Equal("Hello World", textWriter.ToString());
    }

    [Fact]
    public void ChunkedHtmlWriter_ToHtml_Should_ReturnFullContent_WhenCalled()
    {
        async IAsyncEnumerable<string> GetChunks()
        {
            await Task.CompletedTask;
            yield return "<div>";
            yield return "content";
            yield return "</div>";
        }

        var writer = new ChunkedHtmlWriter(GetChunks());
        var result = writer.ToHtml();

        Assert.Equal("<div>content</div>", result);
    }

    [Fact]
    public void ChunkedHtmlWriter_Constructor_Should_ThrowArgumentNullException_WhenChunksIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new ChunkedHtmlWriter(null!));
    }

    [Fact]
    public async Task ChunkedHtmlWriter_WriteToAsync_Should_ThrowArgumentNullException_WhenWriterIsNull()
    {
        var writer = new ChunkedHtmlWriter(EmptyChunks());
        await Assert.ThrowsAsync<ArgumentNullException>(() => writer.WriteToAsync(null!));
    }

    private static async IAsyncEnumerable<string> EmptyChunks()
    {
        await Task.CompletedTask;
        yield break;
    }
}
