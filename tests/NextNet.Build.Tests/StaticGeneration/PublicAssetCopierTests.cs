using NextNet.Build.StaticGeneration;
using Xunit;

namespace NextNet.Build.Tests.StaticGeneration;

public class PublicAssetCopierTests
{
    [Fact]
    public async Task CopyAsync_WhenPublicDirDoesNotExist_ReturnsEmpty()
    {
        using var tempDir = new TempDirectory();
        var nonExistentPublic = System.IO.Path.Combine(tempDir.Path, "nonexistent-public");
        var outputDir = System.IO.Path.Combine(tempDir.Path, "dist");

        var copier = new PublicAssetCopier(nonExistentPublic, outputDir);
        var result = await copier.CopyAsync();

        Assert.Empty(result);
        Assert.Equal(0, copier.CopiedFileCount);
    }

    [Fact]
    public async Task CopyAsync_CopiesAllFiles()
    {
        using var tempDir = new TempDirectory();
        var publicDir = System.IO.Path.Combine(tempDir.Path, "public");
        var outputDir = System.IO.Path.Combine(tempDir.Path, "dist");

        Directory.CreateDirectory(publicDir);
        await File.WriteAllTextAsync(System.IO.Path.Combine(publicDir, "styles.css"), "body { color: red; }");
        await File.WriteAllTextAsync(System.IO.Path.Combine(publicDir, "app.js"), "console.log('hello');");

        var copier = new PublicAssetCopier(publicDir, outputDir);
        var result = await copier.CopyAsync();

        Assert.Equal(2, result.Count);
        Assert.Contains("styles.css", result);
        Assert.Contains("app.js", result);
        Assert.True(File.Exists(System.IO.Path.Combine(outputDir, "styles.css")));
        Assert.True(File.Exists(System.IO.Path.Combine(outputDir, "app.js")));
    }

    [Fact]
    public async Task CopyAsync_PreservesDirectoryStructure()
    {
        using var tempDir = new TempDirectory();
        var publicDir = System.IO.Path.Combine(tempDir.Path, "public");
        var outputDir = System.IO.Path.Combine(tempDir.Path, "dist");

        Directory.CreateDirectory(System.IO.Path.Combine(publicDir, "images"));
        Directory.CreateDirectory(System.IO.Path.Combine(publicDir, "fonts"));
        await File.WriteAllTextAsync(System.IO.Path.Combine(publicDir, "images", "logo.png"), "fake-png");
        await File.WriteAllTextAsync(System.IO.Path.Combine(publicDir, "fonts", "roboto.woff"), "fake-font");

        var copier = new PublicAssetCopier(publicDir, outputDir);
        var result = await copier.CopyAsync();

        Assert.Equal(2, result.Count);
        Assert.Contains("images/logo.png", result);
        Assert.Contains("fonts/roboto.woff", result);
        Assert.True(File.Exists(System.IO.Path.Combine(outputDir, "images", "logo.png")));
        Assert.True(File.Exists(System.IO.Path.Combine(outputDir, "fonts", "roboto.woff")));
    }

    [Fact]
    public async Task CopyAsync_OverwritesExistingFiles()
    {
        using var tempDir = new TempDirectory();
        var publicDir = System.IO.Path.Combine(tempDir.Path, "public");
        var outputDir = System.IO.Path.Combine(tempDir.Path, "dist");

        Directory.CreateDirectory(publicDir);
        Directory.CreateDirectory(outputDir);
        await File.WriteAllTextAsync(System.IO.Path.Combine(outputDir, "styles.css"), "old");
        await File.WriteAllTextAsync(System.IO.Path.Combine(publicDir, "styles.css"), "new");

        var copier = new PublicAssetCopier(publicDir, outputDir);
        await copier.CopyAsync();

        var content = await File.ReadAllTextAsync(System.IO.Path.Combine(outputDir, "styles.css"));
        Assert.Equal("new", content);
    }
}
