using NextNet.IO;
using NextNet.Routing.Models;
using Xunit;

namespace NextNet.Routing.Tests;

/// <summary>
/// Tests for the <see cref="ApiRouteScanner"/> that discovers API route files
/// in the <c>app/api/</c> directory.
/// </summary>
public class ApiRouteScannerTests
{
    private const string AppDir = "/app";

    private static TestFileSystem CreateFileSystem()
    {
        var fs = new TestFileSystem();
        // Add common directories
        fs.AddFile("/app/api/users/route.cs");
        fs.AddFile("/app/api/products/route.cs");
        fs.AddFile("/app/api/products/[id]/route.cs");
        fs.AddFile("/app/about/page.cs"); // Non-API route file
        return fs;
    }

    [Fact]
    public void Scan_NoApiDirectory_ReturnsEmpty()
    {
        var fs = new TestFileSystem();
        var scanner = new ApiRouteScanner(AppDir, fileSystem: fs);
        var results = scanner.Scan();

        Assert.Empty(results);
    }

    [Fact]
    public void Scan_WithApiRoutes_ReturnsEntries()
    {
        var fs = CreateFileSystem();
        var scanner = new ApiRouteScanner(AppDir, fileSystem: fs);
        var results = scanner.Scan();

        Assert.Equal(3, results.Count);
    }

    [Fact]
    public void Scan_ApiRoutes_HaveCorrectType()
    {
        var fs = CreateFileSystem();
        var scanner = new ApiRouteScanner(AppDir, fileSystem: fs);
        var results = scanner.Scan();

        Assert.All(results, entry => Assert.Equal(RouteType.Api, entry.Type));
    }

    [Fact]
    public void Scan_PathConversion_StaticRoute()
    {
        var fs = CreateFileSystem();
        var scanner = new ApiRouteScanner(AppDir, fileSystem: fs);

        // Need to make sure the app dir is set correctly for path parsing
        var results = scanner.Scan();

        var usersRoute = results.FirstOrDefault(r => r.FilePath.EndsWith("users/route.cs"));
        Assert.NotNull(usersRoute);
        Assert.Equal("/api/users", usersRoute.RoutePattern);
    }

    [Fact]
    public void Scan_PathConversion_DynamicSegment()
    {
        var fs = CreateFileSystem();
        var scanner = new ApiRouteScanner(AppDir, fileSystem: fs);
        var results = scanner.Scan();

        var productById = results.FirstOrDefault(r => r.FilePath.Contains("[id]"));
        Assert.NotNull(productById);
        Assert.Contains("{id}", productById.RoutePattern);
        Assert.Equal(RouteSegmentKind.Dynamic, productById.SegmentKind);
    }

    [Fact]
    public void Scan_ExcludesNonRouteFiles()
    {
        var fs = CreateFileSystem();
        var scanner = new ApiRouteScanner(AppDir, fileSystem: fs);
        var results = scanner.Scan();

        // Should not include about/page.cs
        Assert.DoesNotContain(results, r => r.FilePath.Contains("about"));
    }

    [Fact]
    public void Scan_WithMethodDetection_DetectsOverrides()
    {
        var fs = new ApiTestFileSystem();
        var scanner = new ApiRouteScanner(AppDir, fileSystem: fs);
        var results = scanner.Scan();

        var usersRoute = results.FirstOrDefault(r => r.FilePath.EndsWith("api/users/route.cs"));
        Assert.NotNull(usersRoute);
        Assert.Contains("GET", usersRoute.HttpMethods);
        Assert.Contains("POST", usersRoute.HttpMethods);
        Assert.DoesNotContain("PUT", usersRoute.HttpMethods);
    }

    [Fact]
    public async Task ScanAsync_ReturnsSameResults()
    {
        var fs = CreateFileSystem();
        var scanner = new ApiRouteScanner(AppDir, fileSystem: fs);
        var results = await scanner.ScanAsync();

        Assert.Equal(3, results.Count);
    }

    [Fact]
    public void NormalizeApiRoutePattern_AddsLeadingSlash()
    {
        Assert.Equal("/api/users", ApiRouteScanner.NormalizeApiRoutePattern("api/users"));
    }

    [Fact]
    public void NormalizeApiRoutePattern_TrimsTrailingSlash()
    {
        Assert.Equal("/api/users", ApiRouteScanner.NormalizeApiRoutePattern("/api/users/"));
    }

    [Fact]
    public void NormalizeApiRoutePattern_EmptyReturnsApi()
    {
        Assert.Equal("/api", ApiRouteScanner.NormalizeApiRoutePattern(""));
    }

    [Fact]
    public void NormalizeApiRoutePattern_NullReturnsApi()
    {
        Assert.Equal("/api", ApiRouteScanner.NormalizeApiRoutePattern(null!));
    }

    /// <summary>
    /// A test file system that includes source content for HTTP method detection.
    /// </summary>
    private class ApiTestFileSystem : TestFileSystem
    {
        private readonly Dictionary<string, string> _fileContents = new(StringComparer.OrdinalIgnoreCase);

        public ApiTestFileSystem()
        {
            AddFileWithContent(
                "/app/api/users/route.cs",
                @"using NextNet.Components;
using Microsoft.AspNetCore.Http;

public class UsersRoute : ApiRoute
{
    public override async Task<IResult> Get()
    {
        return Results.Ok(new { });
    }

    public override async Task<IResult> Post()
    {
        return Results.Ok(new { });
    }

    public override Task<IResult> Handle(HttpContext context)
    {
        return Task.FromResult(Results.Ok(""default""));
    }
}");

            AddFileWithContent(
                "/app/api/products/route.cs",
                @"using NextNet.Components;
using Microsoft.AspNetCore.Http;

public class ProductsRoute : ApiRoute
{
    public override Task<IResult> Handle(HttpContext context)
    {
        return Task.FromResult(Results.Ok(""default""));
    }
}");

            // This file has no methods at all (incomplete/placeholder)
            AddFileWithContent(
                "/app/api/products/[id]/route.cs",
                @"// Placeholder file");

            // Non-API route without methods
            AddFileWithContent(
                "/app/about/page.cs",
                @"public class AboutPage {}");
        }

        public void AddFileWithContent(string path, string content)
        {
            AddFile(path);
            _fileContents[path.Replace('\\', '/')] = content;
        }

        public override string ReadAllText(string path)
        {
            var normalized = path.Replace('\\', '/');
            return _fileContents.TryGetValue(normalized, out var content) ? content : string.Empty;
        }

        public override Task<string> ReadAllTextAsync(string path)
            => Task.FromResult(ReadAllText(path));
    }
}
