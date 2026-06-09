using Microsoft.Extensions.DependencyInjection;
using NextNet.Build.StaticGeneration;
using NextNet.Components;
using NextNet.Conventions;
using NextNet.Rendering;
using NextNet.Routing;
using NextNet.Routing.Models;
using Xunit;

namespace NextNet.Build.Tests.StaticGeneration;

public class StaticParamsResolverTests
{
    private sealed class TestPageWithParams : IPage, IStaticPathProvider
    {
        public IReadOnlyDictionary<string, object> Props { get; } = new Dictionary<string, object>();

        public Task<IHtmlContent> Render()
            => Task.FromResult(HtmlHelper.Text("test"));

        public async Task<IReadOnlyList<Dictionary<string, string>>> GetStaticPathsAsync()
        {
            await Task.CompletedTask;
            return new List<Dictionary<string, string>>
            {
                new() { ["slug"] = "hello-world" },
                new() { ["slug"] = "getting-started" },
            };
        }
    }

    private sealed class TestPageWithoutParams : IPage
    {
        public IReadOnlyDictionary<string, object> Props { get; } = new Dictionary<string, object>();
        public Task<IHtmlContent> Render()
            => Task.FromResult(HtmlHelper.Text("test"));
    }

    [Fact]
    public async Task ResolveAsync_Should_ReturnParams_When_PageImplementsIStaticPathProvider()
    {
        var services = new ServiceCollection();
        services.AddScoped<IPage>(_ => new TestPageWithParams());
        services.AddScoped<IStaticPathProvider>(_ => new TestPageWithParams());
        var sp = services.BuildServiceProvider();

        var resolver = new ComponentTypeResolver(typeof(TestPageWithParams), null);
        var paramResolver = new StaticParamsResolver(resolver, sp);

        var entry = new RouteEntry("/blog/{slug}", "app/blog/[slug]/page.cs", RouteType.Page, RouteSegmentKind.Dynamic);

        var result = await paramResolver.ResolveAsync(entry);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("hello-world", result[0]["slug"]);
        Assert.Equal("getting-started", result[1]["slug"]);
    }

    [Fact]
    public async Task ResolveAsync_Should_ReturnNull_When_PageLacksIStaticPathProvider()
    {
        var services = new ServiceCollection();
        services.AddScoped<IPage>(_ => new TestPageWithoutParams());
        var sp = services.BuildServiceProvider();

        var resolver = new ComponentTypeResolver(typeof(TestPageWithoutParams), null);
        var paramResolver = new StaticParamsResolver(resolver, sp);

        var entry = new RouteEntry("/about", "app/about/page.cs", RouteType.Page, RouteSegmentKind.Static);
        var result = await paramResolver.ResolveAsync(entry);

        Assert.Null(result);
    }

    [Fact]
    public void ClearCache_Should_Invalidate_When_Called()
    {
        var services = new ServiceCollection().BuildServiceProvider();
        var resolver = new ComponentTypeResolver(typeof(TestPageWithParams), null);
        var paramResolver = new StaticParamsResolver(resolver, services);

        paramResolver.ClearCache();
    }

    [Fact]
    public async Task ResolveAsync_Should_ReturnParams_When_StaticParamsJsonExists()
    {
        using var tempDir = new TempDirectory();
        var pageDir = System.IO.Path.Combine(tempDir.Path, "app", "blog", "[slug]");
        Directory.CreateDirectory(pageDir);

        var jsonPath = System.IO.Path.Combine(pageDir, "staticparams.json");
        var json = @"[
            { ""slug"": ""hello-world"" },
            { ""slug"": ""getting-started"" },
            { ""slug"": ""third-post"" }
        ]";
        await File.WriteAllTextAsync(jsonPath, json);

        var services = new ServiceCollection().BuildServiceProvider();

        var pageEntryPath = System.IO.Path.Combine(pageDir, "page.cs");
        var resolver = new ComponentTypeResolver(null, null);
        var fileSystem = new NextNet.IO.DefaultSharpFileSystem();
        var paramResolver = new StaticParamsResolver(resolver, services, fileSystem);

        var entry = new RouteEntry("/blog/{slug}", pageEntryPath, RouteType.Page, RouteSegmentKind.Dynamic);

        var result = await paramResolver.ResolveAsync(entry);

        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.Equal("hello-world", result[0]["slug"]);
        Assert.Equal("getting-started", result[1]["slug"]);
        Assert.Equal("third-post", result[2]["slug"]);
    }

    [Fact]
    public async Task ResolveAsync_Should_ReturnNull_When_JsonArrayIsEmpty()
    {
        using var tempDir = new TempDirectory();
        var pageDir = System.IO.Path.Combine(tempDir.Path, "app", "blog", "[slug]");
        Directory.CreateDirectory(pageDir);

        var jsonPath = System.IO.Path.Combine(pageDir, "staticparams.json");
        await File.WriteAllTextAsync(jsonPath, "[]");

        var services = new ServiceCollection().BuildServiceProvider();
        var resolver = new ComponentTypeResolver(null, null);
        var fileSystem = new NextNet.IO.DefaultSharpFileSystem();
        var paramResolver = new StaticParamsResolver(resolver, services, fileSystem);

        var pageEntryPath = System.IO.Path.Combine(pageDir, "page.cs");
        var entry = new RouteEntry("/blog/{slug}", pageEntryPath, RouteType.Page, RouteSegmentKind.Dynamic);

        var result = await paramResolver.ResolveAsync(entry);

        Assert.Null(result);
    }

    [Fact]
    public async Task ResolveAsync_Should_ReturnNull_When_JsonIsInvalid()
    {
        using var tempDir = new TempDirectory();
        var pageDir = System.IO.Path.Combine(tempDir.Path, "app", "blog", "[slug]");
        Directory.CreateDirectory(pageDir);

        var jsonPath = System.IO.Path.Combine(pageDir, "staticparams.json");
        await File.WriteAllTextAsync(jsonPath, "not valid json");

        var services = new ServiceCollection().BuildServiceProvider();
        var resolver = new ComponentTypeResolver(null, null);
        var fileSystem = new NextNet.IO.DefaultSharpFileSystem();
        var paramResolver = new StaticParamsResolver(resolver, services, fileSystem);

        var pageEntryPath = System.IO.Path.Combine(pageDir, "page.cs");
        var entry = new RouteEntry("/blog/{slug}", pageEntryPath, RouteType.Page, RouteSegmentKind.Dynamic);

        var result = await paramResolver.ResolveAsync(entry);

        Assert.Null(result);
    }

    [Fact]
    public async Task ResolveAsync_Should_ReturnNull_When_NoJsonFileExists()
    {
        using var tempDir = new TempDirectory();
        var pageDir = System.IO.Path.Combine(tempDir.Path, "app", "about");
        Directory.CreateDirectory(pageDir);

        var services = new ServiceCollection().BuildServiceProvider();
        var resolver = new ComponentTypeResolver(null, null);
        var fileSystem = new NextNet.IO.DefaultSharpFileSystem();
        var paramResolver = new StaticParamsResolver(resolver, services, fileSystem);

        var pageEntryPath = System.IO.Path.Combine(pageDir, "page.cs");
        var entry = new RouteEntry("/about", pageEntryPath, RouteType.Page, RouteSegmentKind.Static);

        var result = await paramResolver.ResolveAsync(entry);

        Assert.Null(result);
    }

    [Fact]
    public async Task ResolveAsync_Should_NotFind_When_JsonInParentDir()
    {
        using var tempDir = new TempDirectory();
        var parentDir = System.IO.Path.Combine(tempDir.Path, "app", "blog");
        var pageDir = System.IO.Path.Combine(parentDir, "[slug]");
        Directory.CreateDirectory(pageDir);

        var jsonPath = System.IO.Path.Combine(parentDir, "staticparams.json");
        await File.WriteAllTextAsync(jsonPath, @"[{ ""slug"": ""test"" }]");

        var services = new ServiceCollection().BuildServiceProvider();
        var resolver = new ComponentTypeResolver(null, null);
        var fileSystem = new NextNet.IO.DefaultSharpFileSystem();
        var paramResolver = new StaticParamsResolver(resolver, services, fileSystem);

        var pageEntryPath = System.IO.Path.Combine(pageDir, "page.cs");
        var entry = new RouteEntry("/blog/{slug}", pageEntryPath, RouteType.Page, RouteSegmentKind.Dynamic);

        var result = await paramResolver.ResolveAsync(entry);

        Assert.Null(result);
    }

    [Fact]
    public async Task ResolveAsync_Should_FallbackToJson_When_ProviderReturnsNull()
    {
        using var tempDir = new TempDirectory();
        var pageDir = System.IO.Path.Combine(tempDir.Path, "app", "blog", "[slug]");
        Directory.CreateDirectory(pageDir);

        var jsonPath = System.IO.Path.Combine(pageDir, "staticparams.json");
        var json = @"[{ ""slug"": ""from-json"" }]";
        await File.WriteAllTextAsync(jsonPath, json);

        var services = new ServiceCollection();
        services.AddScoped<IStaticPathProvider>(_ => new NullReturningProvider());
        var sp = services.BuildServiceProvider();

        var resolver = new ComponentTypeResolver(typeof(NullReturningProvider), null);
        var fileSystem = new NextNet.IO.DefaultSharpFileSystem();
        var paramResolver = new StaticParamsResolver(resolver, sp, fileSystem);

        var pageEntryPath = System.IO.Path.Combine(pageDir, "page.cs");
        var entry = new RouteEntry("/blog/{slug}", pageEntryPath, RouteType.Page, RouteSegmentKind.Dynamic);

        var result = await paramResolver.ResolveAsync(entry);

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("from-json", result[0]["slug"]);
    }

    private sealed class NullReturningProvider : IStaticPathProvider
    {
        public Task<IReadOnlyList<Dictionary<string, string>>> GetStaticPathsAsync()
            => Task.FromResult<IReadOnlyList<Dictionary<string, string>>>(new List<Dictionary<string, string>>());
    }

    private sealed class ComponentTypeResolver : IRouteComponentResolver
    {
        private readonly Type? _pageType;
        private readonly Type? _layoutType;

        public ComponentTypeResolver(Type? pageType, Type? layoutType)
        {
            _pageType = pageType;
            _layoutType = layoutType;
        }

        public Type? GetPageType(RouteEntry entry) => _pageType;
        public Type? GetLayoutType(string layoutPath) => _layoutType;
    }
}
