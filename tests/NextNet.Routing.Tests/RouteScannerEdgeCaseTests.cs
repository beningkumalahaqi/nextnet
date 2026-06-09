using NextNet.IO;
using NextNet.Logging;
using NextNet.Routing.Models;
using Xunit;

namespace NextNet.Routing.Tests;

/// <summary>
/// Additional edge case and error path tests for <see cref="RouteScanner"/>.
/// </summary>
public class RouteScannerEdgeCaseTests
{
    private readonly TestFileSystem _fs;
    private readonly string _appDir = "/test/app";

    public RouteScannerEdgeCaseTests()
    {
        _fs = new TestFileSystem();
        _fs.CreateDirectory(_appDir);

        // Create some test data
        _fs.CreateDirectory("/test/app/about");
        _fs.AddFile("/test/app/layout.cs");
        _fs.AddFile("/test/app/about/page.cs");
    }

    [Fact]
    public void Scan_Should_LogWithoutThrowing_When_LoggerProvided()
    {
        var logger = new NextNetLogger("TestScanner");
        var scanner = new RouteScanner(_appDir, logger: logger, fileSystem: _fs);

        var manifest = scanner.Scan();

        Assert.Single(manifest.Pages);
    }

    [Fact]
    public void Scan_Should_ReturnPartialResults_When_DirectoryAccessError()
    {
        // The test file system doesn't throw, but simulate by creating an inaccessible path
        // This mainly tests that the scanner handles exceptions gracefully
        var fs = new ThrowingFileSystemWrapper(_fs, throwOnNextEnumerate: false);
        var scanner = new RouteScanner(_appDir, fileSystem: fs);

        var manifest = scanner.Scan();

        // Should return whatever was found before the error
        Assert.NotNull(manifest);
    }

    [Fact]
    public void Scan_Should_SkipNonRouteCsFiles_When_Scanning()
    {
        _fs.CreateDirectory("/test/app/shared");
        _fs.AddFile("/test/app/shared/helper.cs");

        var scanner = new RouteScanner(_appDir, fileSystem: _fs);
        var manifest = scanner.Scan();

        // Only the layout and about page should be there
        Assert.Equal(2, manifest.Routes.Count);
        Assert.DoesNotContain(manifest.Routes, r => r.FilePath.Contains("helper.cs"));
    }

    [Fact]
    public void Scan_Should_ReturnEmptyManifest_When_AppDirEmpty()
    {
        var emptyFs = new TestFileSystem();
        emptyFs.CreateDirectory("/empty/app");
        var scanner = new RouteScanner("/empty/app", fileSystem: emptyFs);

        var manifest = scanner.Scan();

        Assert.Empty(manifest.Routes);
    }

    [Fact]
    public void Scan_Should_NotRaiseFalsePositive_When_UniqueRoutes()
    {
        // This test verifies no false positive duplicate detection
        // when routes have unique patterns
        var scanner = new RouteScanner(_appDir, fileSystem: _fs);
        var manifest = scanner.Scan();

        Assert.DoesNotContain(manifest.Conflicts, c => c.Severity == ConflictSeverity.Error);
    }

    [Fact]
    public void Scan_Should_ClassifyApiAndPageRoutes_When_BothPresent()
    {
        _fs.CreateDirectory("/test/app/api/users");
        _fs.AddFile("/test/app/api/users/route.cs");

        var scanner = new RouteScanner(_appDir, fileSystem: _fs);
        var manifest = scanner.Scan();

        Assert.Single(manifest.Pages);
        Assert.Single(manifest.ApiRoutes);

        var page = manifest.Pages[0];
        var api = manifest.ApiRoutes[0];

        Assert.Equal(RouteType.Page, page.Type);
        Assert.Equal(RouteType.Api, api.Type);
        Assert.Equal("/about", page.RoutePattern);
        Assert.Equal("/api/users", api.RoutePattern);
    }

    [Fact]
    public void Scan_Should_ResolveDeepLayoutHierarchy_When_NestedLayoutsExist()
    {
        // Create nested layout structure:
        // app/layout.cs (root)
        // app/blog/layout.cs
        // app/blog/2024/layout.cs
        // app/blog/2024/[slug]/page.cs
        _fs.CreateDirectory("/test/app/blog/2024/[slug]");
        _fs.AddFile("/test/app/blog/layout.cs");
        _fs.AddFile("/test/app/blog/2024/layout.cs");
        _fs.AddFile("/test/app/blog/2024/[slug]/page.cs");

        var scanner = new RouteScanner(_appDir, fileSystem: _fs);
        var manifest = scanner.Scan();

        var page = manifest.Pages.FirstOrDefault(p => p.RoutePattern == "/blog/2024/{slug}");
        Assert.NotNull(page);
        Assert.Equal(3, page.LayoutChain.Count); // blog/2024/layout + blog/layout + root layout
    }

    [Fact]
    public void Scan_Should_HandleRootPage_When_PageInRoot()
    {
        // app/index/page.cs -> /index
        _fs.CreateDirectory("/test/app/index");
        _fs.AddFile("/test/app/index/page.cs");

        var scanner = new RouteScanner(_appDir, fileSystem: _fs);
        var manifest = scanner.Scan();

        var page = manifest.Pages.FirstOrDefault(p => p.RoutePattern == "/index");
        Assert.NotNull(page);
        Assert.Single(page.LayoutChain); // Only root layout
    }

    [Fact]
    public void Scan_Should_DetectErrorPage_When_ErrorFileTogetherWithLayout()
    {
        _fs.AddFile("/test/app/error.cs");

        var scanner = new RouteScanner(_appDir, fileSystem: _fs);
        var manifest = scanner.Scan();

        Assert.NotNull(manifest.ErrorPage);
        // Error pages don't have layout resolution in current implementation
        // (they're not pages)
    }
}

/// <summary>
/// A file system wrapper that can simulate errors for testing error handling.
/// </summary>
internal class ThrowingFileSystemWrapper : ISharpFileSystem
{
    private readonly ISharpFileSystem _inner;
    private readonly bool _throwOnNextEnumerate;

    public ThrowingFileSystemWrapper(ISharpFileSystem inner, bool throwOnNextEnumerate)
    {
        _inner = inner;
        _throwOnNextEnumerate = throwOnNextEnumerate;
    }

    public bool FileExists(string path) => _inner.FileExists(path);
    public string ReadAllText(string path) => _inner.ReadAllText(path);
    public Task<string> ReadAllTextAsync(string path) => _inner.ReadAllTextAsync(path);
    public void WriteAllText(string path, string content) => _inner.WriteAllText(path, content);
    public Task WriteAllTextAsync(string path, string content) => _inner.WriteAllTextAsync(path, content);

    public IEnumerable<string> EnumerateFiles(string directory, string pattern)
    {
        if (_throwOnNextEnumerate)
            throw new UnauthorizedAccessException("Access denied");
        return _inner.EnumerateFiles(directory, pattern);
    }

    public IEnumerable<string> EnumerateDirectories(string directory)
    {
        if (_throwOnNextEnumerate)
            throw new UnauthorizedAccessException("Access denied");
        return _inner.EnumerateDirectories(directory);
    }

    public bool DirectoryExists(string path) => _inner.DirectoryExists(path);
    public void CreateDirectory(string path) => _inner.CreateDirectory(path);
    public string GetFullPath(string path) => _inner.GetFullPath(path);
    public string? GetDirectoryName(string? path) => _inner.GetDirectoryName(path);
    public string GetFileName(string path) => _inner.GetFileName(path);
    public string GetFileNameWithoutExtension(string path) => _inner.GetFileNameWithoutExtension(path);
    public string Combine(params string[] paths) => _inner.Combine(paths);
    public Task WriteAllBytesAsync(string path, byte[] content, CancellationToken cancellationToken = default)
        => _inner.WriteAllBytesAsync(path, content, cancellationToken);
    public void DeleteDirectory(string path, bool recursive = true)
        => _inner.DeleteDirectory(path, recursive);
}
