using NextNet.Build.Optimization;
using NextNet.Build.Tests.StaticGeneration;
using Xunit;

namespace NextNet.Build.Tests.Optimization;

public class AssetOptimizerTests
{
    [Fact]
    public void OptimizableExtensions_ContainsExpected()
    {
        Assert.Contains(".html", AssetOptimizer.OptimizableExtensions);
        Assert.Contains(".css", AssetOptimizer.OptimizableExtensions);
        Assert.Contains(".js", AssetOptimizer.OptimizableExtensions);
        Assert.Contains(".svg", AssetOptimizer.OptimizableExtensions);
        Assert.Contains(".xml", AssetOptimizer.OptimizableExtensions);
        Assert.Contains(".json", AssetOptimizer.OptimizableExtensions);
    }

    [Fact]
    public void OptimizableExtensions_NotContainsBinary()
    {
        Assert.DoesNotContain(".png", AssetOptimizer.OptimizableExtensions);
        Assert.DoesNotContain(".woff", AssetOptimizer.OptimizableExtensions);
        Assert.DoesNotContain(".ico", AssetOptimizer.OptimizableExtensions);
    }

    [Fact]
    public async Task OptimizeDirectoryAsync_NonExistent_ReturnsZero()
    {
        using var tempDir = new TempDirectory();
        var optimizer = new AssetOptimizer();
        var bytesSaved = await optimizer.OptimizeDirectoryAsync(
            System.IO.Path.Combine(tempDir.Path, "nonexistent"));

        Assert.Equal(0, bytesSaved);
    }

    [Fact]
    public async Task OptimizeDirectoryAsync_MinifiesHtml()
    {
        using var tempDir = new TempDirectory();
        var dir = System.IO.Path.Combine(tempDir.Path, "assets");
        Directory.CreateDirectory(dir);
        var file = System.IO.Path.Combine(dir, "test.html");
        await File.WriteAllTextAsync(file, "  <html>  <body>  Hello  </body>  </html>  ");

        var optimizer = new AssetOptimizer();
        var bytesSaved = await optimizer.OptimizeDirectoryAsync(dir);

        Assert.True(bytesSaved > 0);

        var content = await File.ReadAllTextAsync(file);
        Assert.DoesNotContain("  ", content); // No double spaces
        Assert.Contains("Hello", content); // Content preserved
    }

    [Fact]
    public async Task OptimizeDirectoryAsync_SkipsBinaryFiles()
    {
        using var tempDir = new TempDirectory();
        var dir = System.IO.Path.Combine(tempDir.Path, "assets");
        Directory.CreateDirectory(dir);
        var file = System.IO.Path.Combine(dir, "image.png");
        await File.WriteAllTextAsync(file, "fake-png-content");

        var optimizer = new AssetOptimizer();
        var bytesSaved = await optimizer.OptimizeDirectoryAsync(dir);

        Assert.Equal(0, bytesSaved);
    }

    [Fact]
    public async Task OptimizeDirectoryAsync_ProcessesNestedDirectories()
    {
        using var tempDir = new TempDirectory();
        var dir = System.IO.Path.Combine(tempDir.Path, "assets");
        var subDir = System.IO.Path.Combine(dir, "css");
        Directory.CreateDirectory(subDir);
        await File.WriteAllTextAsync(System.IO.Path.Combine(subDir, "styles.css"), "  body { color: red; }  ");

        var optimizer = new AssetOptimizer();
        var bytesSaved = await optimizer.OptimizeDirectoryAsync(dir);

        Assert.True(bytesSaved > 0);
        var content = await File.ReadAllTextAsync(System.IO.Path.Combine(subDir, "styles.css"));
        Assert.Contains("body", content);
    }
}
