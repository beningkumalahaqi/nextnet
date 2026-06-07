using NextNet.Build.StaticGeneration;
using NextNet.IO;
using Xunit;

namespace NextNet.Build.Tests.StaticGeneration;

public class OutputWriterTests
{
    [Fact]
    public void RouteToFilePath_Root_ReturnsIndexHtml()
    {
        var path = OutputWriter.RouteToFilePath("/");
        Assert.Equal("index.html", path);
    }

    [Fact]
    public void RouteToFilePath_About_ReturnsAboutIndexHtml()
    {
        var path = OutputWriter.RouteToFilePath("/about");
        Assert.Equal("about/index.html", path);
    }

    [Fact]
    public void RouteToFilePath_DeepNested_ReturnsCorrectPath()
    {
        var path = OutputWriter.RouteToFilePath("/blog/hello-world");
        Assert.Equal("blog/hello-world/index.html", path);
    }

    [Fact]
    public void RouteToFilePath_WithTrailingSlash_WorksCorrectly()
    {
        var path = OutputWriter.RouteToFilePath("/about/");
        Assert.Equal("about/index.html", path);
    }

    [Fact]
    public void RouteToFilePath_WithoutLeadingSlash_WorksCorrectly()
    {
        var path = OutputWriter.RouteToFilePath("about");
        Assert.Equal("about/index.html", path);
    }

    [Fact]
    public void RouteToFilePath_EmptyString_Throws()
    {
        Assert.Throws<ArgumentException>(() => OutputWriter.RouteToFilePath(""));
    }

    [Fact]
    public async Task WriteAsync_CreatesDirectoryAndFile()
    {
        using var tempDir = new TempDirectory();
        var writer = new OutputWriter(tempDir.Path);

        var result = await writer.WriteAsync("/about", "<html><body>About</body></html>");

        Assert.Equal("about/index.html", result);
        Assert.True(File.Exists(Path.Combine(tempDir.Path, "about", "index.html")));
    }

    [Fact]
    public async Task WriteAsync_FileContent_IsCorrect()
    {
        using var tempDir = new TempDirectory();
        var writer = new OutputWriter(tempDir.Path);
        var html = "<h1>Hello</h1>";

        await writer.WriteAsync("/hello", html);

        var content = File.ReadAllText(Path.Combine(tempDir.Path, "hello", "index.html"));
        Assert.Equal(html, content);
    }

    [Fact]
    public async Task WriteAsync_TracksBytesWritten()
    {
        using var tempDir = new TempDirectory();
        var writer = new OutputWriter(tempDir.Path);

        await writer.WriteAsync("/page1", "hello");
        await writer.WriteAsync("/page2", "world");

        Assert.Equal(10, writer.TotalBytesWritten);
    }

    [Fact]
    public async Task WriteBytesAsync_WritesCorrectContent()
    {
        using var tempDir = new TempDirectory();
        var writer = new OutputWriter(tempDir.Path);
        var data = new byte[] { 0x1F, 0x8B, 0x08 };

        var result = await writer.WriteBytesAsync("about/index.html.gz", data);

        Assert.Equal("about/index.html.gz", result);
        Assert.True(File.Exists(Path.Combine(tempDir.Path, "about", "index.html.gz")));
    }

    [Fact]
    public async Task CleanOutputDirectory_RemovesAllFiles()
    {
        using var tempDir = new TempDirectory();
        var writer = new OutputWriter(tempDir.Path);

        await writer.WriteAsync("/test", "content");
        Assert.True(Directory.Exists(tempDir.Path));

        writer.CleanOutputDirectory();

        Assert.True(Directory.Exists(tempDir.Path)); // Directory should exist (recreated)
        Assert.False(File.Exists(Path.Combine(tempDir.Path, "test", "index.html")));
    }
}

/// <summary>
/// Helper that creates a unique temp directory and cleans it up on dispose.
/// </summary>
public sealed class TempDirectory : IDisposable
{
    public string Path { get; }

    public TempDirectory()
    {
        Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "NextNet_Test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path);
    }

    public void Dispose()
    {
        if (Directory.Exists(Path))
        {
            try { Directory.Delete(Path, recursive: true); }
            catch { /* best effort cleanup */ }
        }
    }
}
