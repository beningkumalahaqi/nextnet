using NextNet.TemplateSdk;
using Xunit;

namespace NextNet.TemplateSdk.Tests.Packaging;

/// <summary>
/// Tests for the <see cref="TemplatePackager"/> class.
/// </summary>
public class TemplatePackagerTests
{
    /// <summary>
    /// Verifies that <see cref="TemplatePackager.PackageAsync"/> creates a valid
    /// <c>.nntemplate</c> ZIP file with a checksum and non-zero size.
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task PackageAsync_Should_CreateZipFile()
    {
        var srcDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var outFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.nntemplate");

        Directory.CreateDirectory(srcDir);
        await File.WriteAllTextAsync(Path.Combine(srcDir, "template.json"),
            "{\"name\":\"x\",\"version\":\"1.0.0\",\"nextnetVersion\":\">=3.0.0\"}");
        await File.WriteAllTextAsync(Path.Combine(srcDir, "test.txt"), "hello");

        var packager = new TemplatePackager();
        var result = await packager.PackageAsync(srcDir, outFile);

        Assert.True(File.Exists(outFile));
        Assert.NotEmpty(result.ChecksumSha256);
        Assert.True(result.SizeBytes > 0);

        // Cleanup
        File.Delete(outFile);
        Directory.Delete(srcDir, recursive: true);
    }

    /// <summary>
    /// Verifies that <see cref="TemplatePackager.PackageAsync"/> throws when the
    /// source directory does not exist.
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task PackageAsync_Should_Throw_When_SourceDirNotFound()
    {
        var packager = new TemplatePackager();
        await Assert.ThrowsAsync<DirectoryNotFoundException>(() =>
            packager.PackageAsync("/nonexistent/path", "/tmp/out.nntemplate"));
    }

    /// <summary>
    /// Verifies that <see cref="TemplatePackager.PackageAsync"/> throws when
    /// <c>template.json</c> is missing.
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task PackageAsync_Should_Throw_When_MissingTemplateJson()
    {
        var srcDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(srcDir);
        var outFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.nntemplate");

        var packager = new TemplatePackager();
        await Assert.ThrowsAsync<FileNotFoundException>(() =>
            packager.PackageAsync(srcDir, outFile));

        // Cleanup
        Directory.Delete(srcDir, recursive: true);
    }

    /// <summary>
    /// Verifies that <see cref="TemplatePackager.PackageAsync"/> produces a consistent
    /// checksum for the same input.
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task PackageAsync_Should_ProduceConsistentChecksum()
    {
        var srcDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var outFile1 = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.nntemplate");
        var outFile2 = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.nntemplate");

        Directory.CreateDirectory(srcDir);
        await File.WriteAllTextAsync(Path.Combine(srcDir, "template.json"),
            "{\"name\":\"x\",\"version\":\"1.0.0\",\"nextnetVersion\":\">=3.0.0\"}");
        await File.WriteAllTextAsync(Path.Combine(srcDir, "file.txt"), "content");

        var packager = new TemplatePackager();
        var result1 = await packager.PackageAsync(srcDir, outFile1);
        var result2 = await packager.PackageAsync(srcDir, outFile2);

        Assert.Equal(result1.ChecksumSha256, result2.ChecksumSha256);

        // Cleanup
        File.Delete(outFile1);
        File.Delete(outFile2);
        Directory.Delete(srcDir, recursive: true);
    }

    /// <summary>
    /// Verifies that the package archive contains all files from the source directory.
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task PackageAsync_Should_IncludeAllFiles()
    {
        var srcDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var outFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.nntemplate");

        Directory.CreateDirectory(Path.Combine(srcDir, "subdir"));
        await File.WriteAllTextAsync(Path.Combine(srcDir, "template.json"),
            "{\"name\":\"x\",\"version\":\"1.0.0\",\"nextnetVersion\":\">=3.0.0\"}");
        await File.WriteAllTextAsync(Path.Combine(srcDir, "file1.txt"), "one");
        await File.WriteAllTextAsync(Path.Combine(srcDir, "subdir", "file2.txt"), "two");

        var packager = new TemplatePackager();
        var result = await packager.PackageAsync(srcDir, outFile);

        Assert.True(File.Exists(outFile));
        Assert.True(result.SizeBytes > 0);

        // Verify contents
        using var archive = System.IO.Compression.ZipFile.OpenRead(outFile);
        var entryNames = archive.Entries.Select(e => e.FullName).ToHashSet();
        Assert.Contains("template.json", entryNames);
        Assert.Contains("file1.txt", entryNames);
        Assert.Contains("subdir/file2.txt", entryNames);

        // Cleanup
        File.Delete(outFile);
        Directory.Delete(srcDir, recursive: true);
    }
}
