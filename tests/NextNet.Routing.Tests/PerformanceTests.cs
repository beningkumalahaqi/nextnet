using System.Diagnostics;
using NextNet.IO;
using NextNet.Routing.Models;
using Xunit;
using Xunit.Abstractions;

namespace NextNet.Routing.Tests;

/// <summary>
/// Performance tests for route scanning operations.
/// These tests verify that scanning meets the documented performance targets:
/// - Full scan (100 files): &lt; 50ms
/// - Full scan (1000 files): &lt; 100ms
/// - Incremental scan (1 file): &lt; 5ms
/// </summary>
[Trait("Category", "Performance")]
public class PerformanceTests
{
    private readonly ITestOutputHelper _output;

    public PerformanceTests(ITestOutputHelper output)
    {
        _output = output;
    }

    /// <summary>
    /// Creates a test file system with the specified number of route files.
    /// </summary>
    private static (TestFileSystem Fs, string AppDir) CreateFileSystem(int fileCount)
    {
        var fs = new TestFileSystem();
        var appDir = "/perf/app";
        fs.CreateDirectory(appDir);
        fs.AddFile($"{appDir}/layout.cs");

        for (int i = 0; i < fileCount; i++)
        {
            var category = $"category{i % 20}";
            var slug = $"slug{i}";
            var dir = $"{appDir}/{category}/[{slug}]";
            fs.CreateDirectory(dir);
            fs.AddFile($"{dir}/page.cs");
        }

        return (fs, appDir);
    }

    [Fact]
    public void FullScan_100Files_Under50ms()
    {
        var (fs, appDir) = CreateFileSystem(100);
        var scanner = new RouteScanner(appDir, fileSystem: fs);

        // Warmup
        scanner.Scan();

        var sw = Stopwatch.StartNew();
        var manifest = scanner.Scan();
        sw.Stop();

        _output.WriteLine($"Full scan of 100 files: {sw.ElapsedMilliseconds}ms");
        Assert.True(sw.ElapsedMilliseconds < 50,
            $"Expected < 50ms but was {sw.ElapsedMilliseconds}ms");
    }

    [Fact]
    public void FullScan_1000Files_Under100ms()
    {
        var (fs, appDir) = CreateFileSystem(1000);
        var scanner = new RouteScanner(appDir, fileSystem: fs);

        // Warmup
        scanner.Scan();

        var sw = Stopwatch.StartNew();
        var manifest = scanner.Scan();
        sw.Stop();

        _output.WriteLine($"Full scan of 1000 files: {sw.ElapsedMilliseconds}ms");
        Assert.True(sw.ElapsedMilliseconds < 100,
            $"Expected < 100ms but was {sw.ElapsedMilliseconds}ms");
    }

    [Fact]
    public void IncrementalScan_1File_Under5ms()
    {
        var (fs, appDir) = CreateFileSystem(100);
        var scanner = new RouteScanner(appDir, fileSystem: fs);

        // Initial full scan
        var manifest = scanner.Scan();

        // Add one new file
        var newDir = $"{appDir}/newcategory/[newslug]";
        fs.CreateDirectory(newDir);
        fs.AddFile($"{newDir}/page.cs");

        // Warmup incremental
        scanner.IncrementalScan(manifest, new HashSet<string> { $"{newDir}/page.cs" });

        var sw = Stopwatch.StartNew();
        var updated = scanner.IncrementalScan(manifest, new HashSet<string> { $"{newDir}/page.cs" });
        sw.Stop();

        _output.WriteLine($"Incremental scan of 1 file: {sw.ElapsedMilliseconds}ms");
        Assert.True(sw.ElapsedMilliseconds < 5,
            $"Expected < 5ms but was {sw.ElapsedMilliseconds}ms");
    }

    [Fact]
    public void RouteTreeBuilder_Performance_LargeManifest()
    {
        var (fs, appDir) = CreateFileSystem(500);
        var scanner = new RouteScanner(appDir, fileSystem: fs);
        var manifest = scanner.Scan();

        var builder = new RouteTreeBuilder();

        var sw = Stopwatch.StartNew();
        var tree = builder.BuildTree(manifest);
        sw.Stop();

        _output.WriteLine($"Building tree from 500 routes: {sw.ElapsedMilliseconds}ms");
        Assert.NotNull(tree);
    }

    [Fact]
    public void Scanner_BuildsCorrectNumberOfRoutes()
    {
        var count = 50;
        var (fs, appDir) = CreateFileSystem(count);
        var scanner = new RouteScanner(appDir, fileSystem: fs);

        var manifest = scanner.Scan();

        // Each file is a page, plus the root layout
        Assert.Equal(count + 1, manifest.Routes.Count);
        Assert.Single(manifest.Layouts);
        Assert.Equal(count, manifest.Pages.Count);
    }
}
