namespace NextNet.TemplatePackages.Tests;

using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using Xunit;

public class TemplatePackageExtractorTests
{
    [Fact]
    public async Task ExtractAsync_Should_ExtractFiles()
    {
        var targetDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var ms = new MemoryStream();

        using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            var entry = archive.CreateEntry("test.txt");
            await using var es = entry.Open();
            await es.WriteAsync(Encoding.UTF8.GetBytes("hello"));
        }

        ms.Position = 0;

        var extractor = new TemplatePackageExtractor();
        var result = await extractor.ExtractAsync(ms, targetDir);

        try
        {
            Assert.True(result.FileCount > 0);
            Assert.True(File.Exists(Path.Combine(targetDir, "test.txt")));
            Assert.Single(result.ExtractedFiles);
            Assert.Equal(targetDir, result.TargetDirectory);
        }
        finally
        {
            Directory.Delete(targetDir, recursive: true);
        }
    }

    [Fact]
    public async Task ExtractAsync_Should_RejectPathTraversal()
    {
        var ms = new MemoryStream();

        using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            var entry = archive.CreateEntry("../../../etc/passwd");
            await using var es = entry.Open();
            await es.WriteAsync(Encoding.UTF8.GetBytes("malicious"));
        }

        ms.Position = 0;

        var targetDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var extractor = new TemplatePackageExtractor();

        await Assert.ThrowsAsync<InvalidDataException>(
            () => extractor.ExtractAsync(ms, targetDir));
    }

    [Fact]
    public async Task ExtractAsync_Should_CreateDirectoryEntries()
    {
        var targetDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var ms = new MemoryStream();

        using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            archive.CreateEntry("mydir/");
            var entry = archive.CreateEntry("mydir/file.txt");
            await using var es = entry.Open();
            await es.WriteAsync(Encoding.UTF8.GetBytes("nested"));
        }

        ms.Position = 0;

        var extractor = new TemplatePackageExtractor();
        var result = await extractor.ExtractAsync(ms, targetDir);

        try
        {
            Assert.True(File.Exists(Path.Combine(targetDir, "mydir", "file.txt")));
            Assert.Single(result.ExtractedFiles);
        }
        finally
        {
            Directory.Delete(targetDir, recursive: true);
        }
    }

    [Fact]
    public async Task ExtractAsync_Should_VerifyChecksum()
    {
        var targetDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var ms = new MemoryStream();

        using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            var entry = archive.CreateEntry("file.txt");
            await using var es = entry.Open();
            await es.WriteAsync(Encoding.UTF8.GetBytes("content"));
        }

        ms.Position = 0;

        // Compute the actual checksum
        var checksum = await ComputeSha256(ms);
        ms.Position = 0;

        var extractor = new TemplatePackageExtractor();
        var result = await extractor.ExtractAsync(ms, targetDir, expectedChecksum: checksum);

        try
        {
            Assert.True(result.FileCount > 0);
        }
        finally
        {
            Directory.Delete(targetDir, recursive: true);
        }
    }

    [Fact]
    public async Task ExtractAsync_Should_ThrowOnChecksumMismatch()
    {
        var targetDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var ms = new MemoryStream();

        using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            var entry = archive.CreateEntry("file.txt");
            await using var es = entry.Open();
            await es.WriteAsync(Encoding.UTF8.GetBytes("content"));
        }

        ms.Position = 0;

        var extractor = new TemplatePackageExtractor();

        await Assert.ThrowsAsync<InvalidDataException>(
            () => extractor.ExtractAsync(ms, targetDir, expectedChecksum: "badchecksum123"));
    }

    [Fact]
    public async Task ExtractAsync_Should_ThrowOnNullStream()
    {
        var extractor = new TemplatePackageExtractor();
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => extractor.ExtractAsync(null!, "some-dir"));
    }

    [Fact]
    public async Task ExtractAsync_Should_ThrowOnEmptyTargetDir()
    {
        var extractor = new TemplatePackageExtractor();
        await Assert.ThrowsAsync<ArgumentException>(
            () => extractor.ExtractAsync(new MemoryStream(), " "));
    }

    [Fact]
    public async Task ExtractAsync_Should_ExtractNestedDirectories()
    {
        var targetDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var ms = new MemoryStream();

        using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            archive.CreateEntry("a/b/c/");
            var entry = archive.CreateEntry("a/b/c/deep.txt");
            await using var es = entry.Open();
            await es.WriteAsync(Encoding.UTF8.GetBytes("deep"));
        }

        ms.Position = 0;

        var extractor = new TemplatePackageExtractor();
        var result = await extractor.ExtractAsync(ms, targetDir);

        try
        {
            Assert.True(File.Exists(Path.Combine(targetDir, "a", "b", "c", "deep.txt")));
            Assert.Single(result.ExtractedFiles);
        }
        finally
        {
            Directory.Delete(targetDir, recursive: true);
        }
    }

    [Fact]
    public async Task ExtractAsync_Should_RejectAbsolutePath()
    {
        var ms = new MemoryStream();

        using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            var entry = archive.CreateEntry("/etc/passwd");
            await using var es = entry.Open();
            await es.WriteAsync(Encoding.UTF8.GetBytes("malicious"));
        }

        ms.Position = 0;

        var targetDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var extractor = new TemplatePackageExtractor();

        await Assert.ThrowsAsync<InvalidDataException>(
            () => extractor.ExtractAsync(ms, targetDir));
    }

    [Fact]
    public async Task ExtractAsync_Should_AllowBackslashAsSeparator()
    {
        var targetDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var ms = new MemoryStream();

        using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            var entry = archive.CreateEntry(@"subdir\file.txt");
            await using var es = entry.Open();
            await es.WriteAsync(Encoding.UTF8.GetBytes("backslash"));
        }

        ms.Position = 0;

        var extractor = new TemplatePackageExtractor();
        var result = await extractor.ExtractAsync(ms, targetDir);

        try
        {
            Assert.True(File.Exists(Path.Combine(targetDir, "subdir", "file.txt")));
            Assert.Single(result.ExtractedFiles);
        }
        finally
        {
            Directory.Delete(targetDir, recursive: true);
        }
    }

    private static async Task<string> ComputeSha256(Stream stream)
    {
        using var sha = SHA256.Create();
        var hash = await sha.ComputeHashAsync(stream);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
