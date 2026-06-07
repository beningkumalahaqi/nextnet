using NextNet.Build.Production.Optimization;
using NextNet.IO;
using Xunit;

namespace NextNet.Build.Tests.Production.Optimization;

public class BundleAnalyzerTests
{
    [Fact]
    public async Task AnalyzeAsync_EmptyDirectory_ReturnsEmptyResult()
    {
        var fs = new DefaultSharpFileSystem();
        var analyzer = new BundleAnalyzer(fs);

        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        try
        {
            var result = await analyzer.AnalyzeAsync(tempDir);
            Assert.NotNull(result);
            Assert.Equal(0, result.TotalFiles);
            Assert.Equal(0, result.TotalSize);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task AnalyzeAsync_WithFiles_ReturnsCorrectCounts()
    {
        var fs = new DefaultSharpFileSystem();
        var analyzer = new BundleAnalyzer(fs);

        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        try
        {
            await File.WriteAllTextAsync(Path.Combine(tempDir, "test.js"), "var x = 1;");
            await File.WriteAllTextAsync(Path.Combine(tempDir, "test.css"), "body { }");
            await File.WriteAllBytesAsync(Path.Combine(tempDir, "image.png"), new byte[100]);

            var result = await analyzer.AnalyzeAsync(tempDir);

            Assert.Equal(3, result.TotalFiles);
            Assert.True(result.TotalSize > 0);
            Assert.Equal(3, result.ByExtension.Count);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task AnalyzeAsync_LargestFiles_OrderedBySize()
    {
        var fs = new DefaultSharpFileSystem();
        var analyzer = new BundleAnalyzer(fs);

        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        try
        {
            await File.WriteAllTextAsync(Path.Combine(tempDir, "small.js"), "x=1;");
            await File.WriteAllTextAsync(Path.Combine(tempDir, "medium.js"), new string('a', 500));
            await File.WriteAllTextAsync(Path.Combine(tempDir, "large.js"), new string('b', 2000));

            var result = await analyzer.AnalyzeAsync(tempDir);

            Assert.True(result.LargestFiles.Count > 0);
            Assert.Equal("large.js", result.LargestFiles[0].Path);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task AnalyzeAsync_InvalidDirectory_ReturnsEmpty()
    {
        var fs = new DefaultSharpFileSystem();
        var analyzer = new BundleAnalyzer(fs);

        var result = await analyzer.AnalyzeAsync("/nonexistent/path");
        Assert.NotNull(result);
        Assert.Equal(0, result.TotalFiles);
    }
}
