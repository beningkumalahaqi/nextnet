using NextNet.IO;
using NextNet.Routing.Models;
using Xunit;

namespace NextNet.Routing.Tests;

/// <summary>
/// A test implementation of <see cref="ISharpFileSystem"/> that works with an in-memory
/// representation of the file system for testing.
/// Uses a dictionary-based lookup for O(1) directory file enumeration.
/// </summary>
internal class TestFileSystem : ISharpFileSystem
{
    private readonly HashSet<string> _files = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, List<string>> _filesByDir =
        new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, List<string>> _childDirs =
        new(StringComparer.OrdinalIgnoreCase);

    public void AddFile(string path)
    {
        var normalized = path.Replace('\\', '/');
        if (_files.Add(normalized))
        {
            var dir = Path.GetDirectoryName(normalized) ?? "";
            if (!_filesByDir.ContainsKey(dir))
                _filesByDir[dir] = new List<string>();
            _filesByDir[dir].Add(normalized);
        }

        // Ensure parent directories exist
        EnsureDir(Path.GetDirectoryName(normalized) ?? "");
    }

    private void EnsureDir(string dir)
    {
        var added = new List<string>();
        while (!string.IsNullOrEmpty(dir))
        {
            if (!_childDirs.ContainsKey(dir))
            {
                added.Add(dir);
                _childDirs[dir] = new List<string>();
            }

            var parent = Path.GetDirectoryName(dir);
            if (!string.IsNullOrEmpty(parent))
            {
                // Register this dir as a child of its parent
                if (!_childDirs.TryGetValue(parent, out var siblings))
                {
                    siblings = new List<string>();
                    _childDirs[parent] = siblings;
                }
                if (!siblings.Contains(dir))
                {
                    siblings.Add(dir);
                }
            }
            dir = parent ?? "";
        }
    }

    public void RemoveFile(string path)
    {
        var normalized = path.Replace('\\', '/');
        if (_files.Remove(normalized))
        {
            var dir = Path.GetDirectoryName(normalized) ?? "";
            if (_filesByDir.TryGetValue(dir, out var files))
            {
                files.Remove(normalized);
            }
        }
    }

    public void Clear()
    {
        _files.Clear();
        _filesByDir.Clear();
        _childDirs.Clear();
    }

    public bool FileExists(string path) => _files.Contains(path.Replace('\\', '/'));
    public virtual string ReadAllText(string path) => "";
    public virtual Task<string> ReadAllTextAsync(string path) => Task.FromResult("");
    public void WriteAllText(string path, string content) => AddFile(path);
    public Task WriteAllTextAsync(string path, string content) { AddFile(path); return Task.CompletedTask; }

    public IEnumerable<string> EnumerateFiles(string directory, string pattern)
    {
        var dir = directory.Replace('\\', '/').TrimEnd('/');
        var suffix = pattern.TrimStart('*');

        if (_filesByDir.TryGetValue(dir, out var files))
        {
            foreach (var f in files)
            {
                if (f.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                    yield return f;
            }
        }
    }

    public IEnumerable<string> EnumerateDirectories(string directory)
    {
        var dir = directory.Replace('\\', '/').TrimEnd('/');
        if (_childDirs.TryGetValue(dir, out var children))
        {
            foreach (var child in children)
            {
                yield return child;
            }
        }
    }

    public bool DirectoryExists(string path) => _childDirs.ContainsKey(path.Replace('\\', '/'));

    public void CreateDirectory(string path)
    {
        var normalized = path.Replace('\\', '/');
        EnsureDir(normalized);
    }
    public string GetFullPath(string path) => path.Replace('\\', '/');
    public string? GetDirectoryName(string? path) => Path.GetDirectoryName(path);
    public string GetFileName(string path) => Path.GetFileName(path);
    public string GetFileNameWithoutExtension(string path) => Path.GetFileNameWithoutExtension(path);
    public string Combine(params string[] paths) => Path.Combine(paths).Replace('\\', '/');
    public Task WriteAllBytesAsync(string path, byte[] content, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
    public void DeleteDirectory(string path, bool recursive = true) { }
}

public class RouteScannerTests
{
    private readonly TestFileSystem _fs;
    private readonly string _appDir = "/test/app";

    public RouteScannerTests()
    {
        _fs = new TestFileSystem();
        _fs.CreateDirectory(_appDir);
    }

    [Fact]
    public void Scan_Should_ReturnEmptyManifest_When_DirectoryEmpty()
    {
        var scanner = new RouteScanner(_appDir, fileSystem: _fs);
        var manifest = scanner.Scan();

        Assert.Empty(manifest.Routes);
        Assert.Empty(manifest.Pages);
        Assert.Empty(manifest.Layouts);
        Assert.Empty(manifest.ApiRoutes);
        Assert.Null(manifest.ErrorPage);
        Assert.Empty(manifest.Conflicts);
        Assert.False(manifest.HasConflicts);
    }

    [Fact]
    public void Scan_Should_ReturnManifestWithOnePage_When_SinglePageExists()
    {
        _fs.CreateDirectory("/test/app/about");
        _fs.AddFile("/test/app/about/page.cs");

        var scanner = new RouteScanner(_appDir, fileSystem: _fs);
        var manifest = scanner.Scan();

        Assert.Single(manifest.Routes);
        Assert.Single(manifest.Pages);
        Assert.Empty(manifest.Layouts);
        Assert.Empty(manifest.ApiRoutes);
        Assert.Null(manifest.ErrorPage);

        var page = manifest.Pages[0];
        Assert.Equal("/about", page.RoutePattern);
        Assert.Equal(RouteType.Page, page.Type);
        Assert.Equal(RouteSegmentKind.Static, page.SegmentKind);
    }

    [Fact]
    public void Scan_Should_SetLayoutChain_When_PageWithRootLayout()
    {
        _fs.CreateDirectory("/test/app/about");
        _fs.AddFile("/test/app/layout.cs");
        _fs.AddFile("/test/app/about/page.cs");

        var scanner = new RouteScanner(_appDir, fileSystem: _fs);
        var manifest = scanner.Scan();

        Assert.Equal(2, manifest.Routes.Count);
        Assert.Single(manifest.Layouts);
        Assert.Single(manifest.Pages);

        var page = manifest.Pages[0];
        Assert.Equal("/about", page.RoutePattern);

        // Root layout applies
        Assert.Contains(manifest.Layouts[0].FilePath, page.LayoutChain);
    }

    [Fact]
    public void Scan_Should_ResolveCorrectLayoutChain_When_PageWithNestedLayout()
    {
        _fs.CreateDirectory("/test/app/blog");
        _fs.AddFile("/test/app/layout.cs");
        _fs.AddFile("/test/app/blog/layout.cs");
        _fs.AddFile("/test/app/blog/[slug]/page.cs");

        var scanner = new RouteScanner(_appDir, fileSystem: _fs);
        var manifest = scanner.Scan();

        Assert.Equal(3, manifest.Routes.Count);
        Assert.Equal(2, manifest.Layouts.Count);
        Assert.Single(manifest.Pages);

        var page = manifest.Pages[0];
        Assert.Equal("/blog/{slug}", page.RoutePattern);
        Assert.Equal(2, page.LayoutChain.Count);
    }

    [Fact]
    public void Scan_Should_ReturnAllPages_When_MultiplePagesExist()
    {
        _fs.CreateDirectory("/test/app/about");
        _fs.CreateDirectory("/test/app/contact");
        _fs.CreateDirectory("/test/app/blog");
        _fs.AddFile("/test/app/about/page.cs");
        _fs.AddFile("/test/app/contact/page.cs");
        _fs.AddFile("/test/app/blog/[slug]/page.cs");

        var scanner = new RouteScanner(_appDir, fileSystem: _fs);
        var manifest = scanner.Scan();

        Assert.Equal(3, manifest.Routes.Count);
        Assert.Equal(3, manifest.Pages.Count);
    }

    [Fact]
    public void Scan_Should_ReturnApiRoutes_When_ApiRoutesExist()
    {
        _fs.CreateDirectory("/test/app/api/users");
        _fs.AddFile("/test/app/api/users/route.cs");

        var scanner = new RouteScanner(_appDir, fileSystem: _fs);
        var manifest = scanner.Scan();

        Assert.Single(manifest.ApiRoutes);
        Assert.Equal("/api/users", manifest.ApiRoutes[0].RoutePattern);
        Assert.Equal(RouteType.Api, manifest.ApiRoutes[0].Type);
    }

    [Fact]
    public void Scan_Should_DetectErrorPage_When_ErrorFileExists()
    {
        _fs.AddFile("/test/app/error.cs");

        var scanner = new RouteScanner(_appDir, fileSystem: _fs);
        var manifest = scanner.Scan();

        Assert.NotNull(manifest.ErrorPage);
        Assert.Equal("/", manifest.ErrorPage.RoutePattern);
        Assert.Equal(RouteType.Error, manifest.ErrorPage.Type);
    }

    [Fact]
    public void Scan_Should_WarnAboutMissingRootLayout_When_PagesExistWithoutRootLayout()
    {
        _fs.CreateDirectory("/test/app/about");
        _fs.AddFile("/test/app/about/page.cs");

        var scanner = new RouteScanner(_appDir, fileSystem: _fs);
        var manifest = scanner.Scan();

        Assert.Contains(manifest.Conflicts, c =>
            c.Message.Contains("No root layout found", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Scan_Should_WarnAboutOrphanedLayout_When_LayoutHasNoPages()
    {
        _fs.CreateDirectory("/test/app/blog");
        _fs.AddFile("/test/app/layout.cs");
        _fs.AddFile("/test/app/blog/layout.cs");
        // No pages under /blog

        var scanner = new RouteScanner(_appDir, fileSystem: _fs);
        var manifest = scanner.Scan();

        Assert.Contains(manifest.Conflicts, c =>
            c.Message.Contains("orphaned", StringComparison.OrdinalIgnoreCase) ||
            c.Message.Contains("no pages beneath", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Scan_Should_NotReportConflicts_When_RoutesAreUnique()
    {
        _fs.CreateDirectory("/test/app/about");
        _fs.AddFile("/test/app/about/page.cs");
        _fs.AddFile("/test/app/contact/page.cs");

        var scanner = new RouteScanner(_appDir, fileSystem: _fs);
        var manifest = scanner.Scan();

        Assert.Equal(2, manifest.Routes.Count);
        Assert.DoesNotContain(manifest.Conflicts, c => c.Severity == ConflictSeverity.Error);
    }

    [Fact]
    public void Scan_Should_ReturnEmptyManifest_When_DirectoryNotExists()
    {
        var scanner = new RouteScanner("/nonexistent/dir", fileSystem: _fs);
        var manifest = scanner.Scan();

        Assert.Empty(manifest.Routes);
        Assert.False(manifest.HasConflicts);
    }

    [Fact]
    public async Task ScanAsync_Should_ReturnSameAsScan_When_Called()
    {
        _fs.CreateDirectory("/test/app/about");
        _fs.AddFile("/test/app/layout.cs");
        _fs.AddFile("/test/app/about/page.cs");

        var scanner = new RouteScanner(_appDir, fileSystem: _fs);
        var manifest = await scanner.ScanAsync();

        Assert.Single(manifest.Pages);
        Assert.Single(manifest.Layouts);
    }

    [Fact]
    public async Task ScanAsync_Should_ThrowOperationCanceled_When_CancellationTokenCancelled()
    {
        var scanner = new RouteScanner(_appDir, fileSystem: _fs);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            scanner.ScanAsync(cts.Token));
    }

    [Fact]
    public void IncrementalScan_Should_IncludeNewRoute_When_FileAdded()
    {
        _fs.CreateDirectory("/test/app/about");
        _fs.AddFile("/test/app/layout.cs");
        _fs.AddFile("/test/app/about/page.cs");

        var scanner = new RouteScanner(_appDir, fileSystem: _fs);
        var manifest = scanner.Scan();

        Assert.Single(manifest.Pages);

        // Add a new page
        _fs.CreateDirectory("/test/app/contact");
        _fs.AddFile("/test/app/contact/page.cs");

        var updated = scanner.IncrementalScan(manifest, new HashSet<string>
        {
            "/test/app/contact/page.cs"
        });

        Assert.Equal(2, updated.Pages.Count);
    }

    [Fact]
    public void IncrementalScan_Should_RemoveRoute_When_FileDeleted()
    {
        _fs.CreateDirectory("/test/app/about");
        _fs.CreateDirectory("/test/app/contact");
        _fs.AddFile("/test/app/layout.cs");
        _fs.AddFile("/test/app/about/page.cs");
        _fs.AddFile("/test/app/contact/page.cs");

        var scanner = new RouteScanner(_appDir, fileSystem: _fs);
        var manifest = scanner.Scan();

        Assert.Equal(2, manifest.Pages.Count);

        // Delete a page
        _fs.RemoveFile("/test/app/contact/page.cs");

        var updated = scanner.IncrementalScan(manifest, new HashSet<string>
        {
            "/test/app/contact/page.cs"
        });

        Assert.Single(updated.Pages);
    }

    [Fact]
    public void IncrementalScan_Should_UpdateRoute_When_FileModified()
    {
        _fs.CreateDirectory("/test/app/about");
        _fs.AddFile("/test/app/layout.cs");
        _fs.AddFile("/test/app/about/page.cs");

        var scanner = new RouteScanner(_appDir, fileSystem: _fs);
        var manifest = scanner.Scan();

        var page = manifest.Pages[0];
        Assert.Equal("/about", page.RoutePattern);

        // "Modify" by re-adding the same file (content doesn't matter, path does)
        var updated = scanner.IncrementalScan(manifest, new HashSet<string>
        {
            "/test/app/about/page.cs"
        });

        Assert.Single(updated.Pages);
        Assert.Equal("/about", updated.Pages[0].RoutePattern);
    }

    [Fact]
    public void IncrementalScan_Should_ReturnSameManifest_When_NoChanges()
    {
        _fs.CreateDirectory("/test/app/about");
        _fs.AddFile("/test/app/layout.cs");
        _fs.AddFile("/test/app/about/page.cs");

        var scanner = new RouteScanner(_appDir, fileSystem: _fs);
        var manifest = scanner.Scan();

        var updated = scanner.IncrementalScan(manifest, new HashSet<string>());

        Assert.Same(manifest.Conflicts, updated.Conflicts);
    }

    [Fact]
    public void IncrementalScan_Should_ThrowArgumentNull_When_PreviousIsNull()
    {
        var scanner = new RouteScanner(_appDir, fileSystem: _fs);
        Assert.Throws<ArgumentNullException>(() =>
            scanner.IncrementalScan(null!, new HashSet<string> { "file.cs" }));
    }

    [Fact]
    public void IncrementalScan_Should_ThrowArgumentNull_When_ChangedFilesIsNull()
    {
        var scanner = new RouteScanner(_appDir, fileSystem: _fs);
        Assert.Throws<ArgumentNullException>(() =>
            scanner.IncrementalScan(RouteManifest.Empty, null!));
    }
}
