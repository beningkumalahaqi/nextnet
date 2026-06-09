using NextNet.Build.StaticGeneration;
using NextNet.IO;
using Xunit;

namespace NextNet.Build.Tests.StaticGeneration;

public class OutputWriterTests
{
    [Fact]
    public void RouteToFilePath_Should_ReturnIndexHtml_When_Root()
    {
        var path = OutputWriter.RouteToFilePath("/");
        Assert.Equal("index.html", path);
    }

    [Fact]
    public void RouteToFilePath_Should_ReturnAboutIndexHtml_When_About()
    {
        var path = OutputWriter.RouteToFilePath("/about");
        Assert.Equal("about/index.html", path);
    }

    [Fact]
    public void RouteToFilePath_Should_ReturnCorrectPath_When_DeepNested()
    {
        var path = OutputWriter.RouteToFilePath("/blog/hello-world");
        Assert.Equal("blog/hello-world/index.html", path);
    }

    [Fact]
    public void RouteToFilePath_Should_WorkCorrectly_When_TrailingSlash()
    {
        var path = OutputWriter.RouteToFilePath("/about/");
        Assert.Equal("about/index.html", path);
    }

    [Fact]
    public void RouteToFilePath_Should_WorkCorrectly_When_WithoutLeadingSlash()
    {
        var path = OutputWriter.RouteToFilePath("about");
        Assert.Equal("about/index.html", path);
    }

    [Fact]
    public void RouteToFilePath_Should_ThrowArgumentException_When_EmptyString()
    {
        Assert.Throws<ArgumentException>(() => OutputWriter.RouteToFilePath(""));
    }

    [Fact]
    public async Task WriteAsync_Should_CreateDirectoryAndWriteFile()
    {
        using var tempDir = new TempDirectory();
        var writer = new OutputWriter(tempDir.Path);

        var result = await writer.WriteAsync("/about", "<html><body>About</body></html>");

        Assert.Equal("about/index.html", result);
        Assert.True(File.Exists(Path.Combine(tempDir.Path, "about", "index.html")));
    }

    [Fact]
    public async Task WriteAsync_Should_WriteCorrectContent_When_Provided()
    {
        using var tempDir = new TempDirectory();
        var writer = new OutputWriter(tempDir.Path);
        var html = "<h1>Hello</h1>";

        await writer.WriteAsync("/hello", html);

        var content = File.ReadAllText(Path.Combine(tempDir.Path, "hello", "index.html"));
        Assert.Equal(html, content);
    }

    [Fact]
    public async Task WriteAsync_Should_TrackBytesWritten_When_MultipleWrites()
    {
        using var tempDir = new TempDirectory();
        var writer = new OutputWriter(tempDir.Path);

        await writer.WriteAsync("/page1", "hello");
        await writer.WriteAsync("/page2", "world");

        Assert.Equal(10, writer.TotalBytesWritten);
    }

    [Fact]
    public async Task WriteBytesAsync_Should_WriteCorrectContent_When_Provided()
    {
        using var tempDir = new TempDirectory();
        var writer = new OutputWriter(tempDir.Path);
        var data = new byte[] { 0x1F, 0x8B, 0x08 };

        var result = await writer.WriteBytesAsync("about/index.html.gz", data);

        Assert.Equal("about/index.html.gz", result);
        Assert.True(File.Exists(Path.Combine(tempDir.Path, "about", "index.html.gz")));
    }

    [Fact]
    public async Task CleanOutputDirectory_Should_RemoveAllFiles_When_Called()
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
