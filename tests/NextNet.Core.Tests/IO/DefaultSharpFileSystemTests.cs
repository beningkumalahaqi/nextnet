using NextNet.IO;
using Xunit;

namespace NextNet.Core.Tests.IO;

public class DefaultSharpFileSystemTests
{
    private readonly DefaultSharpFileSystem _fs = new();

    [Fact]
    public void FileExists_Should_ReturnTrue_When_FileExists()
    {
        var path = Path.GetTempFileName();
        try
        {
            Assert.True(_fs.FileExists(path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void FileExists_Should_ReturnFalse_When_FileDoesNotExist()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".tmp");
        Assert.False(_fs.FileExists(path));
    }

    [Fact]
    public void ReadAllText_Should_Roundtrip_When_WriteAllTextCalled()
    {
        var path = Path.GetTempFileName();
        try
        {
            var expected = "Hello, NextNet!";
            _fs.WriteAllText(path, expected);
            var actual = _fs.ReadAllText(path);
            Assert.Equal(expected, actual);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task ReadAllTextAsync_Should_Roundtrip_When_WriteAllTextAsyncCalled()
    {
        var path = Path.GetTempFileName();
        try
        {
            var expected = "Async test content";
            await _fs.WriteAllTextAsync(path, expected);
            var actual = await _fs.ReadAllTextAsync(path);
            Assert.Equal(expected, actual);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void EnumerateFiles_Should_ReturnFiles_When_DirectoryHasFiles()
    {
        var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var file1 = Path.Combine(dir, "test1.txt");
            var file2 = Path.Combine(dir, "test2.txt");
            File.WriteAllText(file1, "a");
            File.WriteAllText(file2, "b");

            var files = _fs.EnumerateFiles(dir, "*.txt").ToList();
            Assert.Contains(file1, files);
            Assert.Contains(file2, files);
            Assert.Equal(2, files.Count);
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    [Fact]
    public void EnumerateDirectories_Should_ReturnSubdirectories_When_Invoked()
    {
        var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var sub1 = Path.Combine(dir, "sub1");
            var sub2 = Path.Combine(dir, "sub2");
            Directory.CreateDirectory(sub1);
            Directory.CreateDirectory(sub2);

            var dirs = _fs.EnumerateDirectories(dir).ToList();
            Assert.Contains(sub1, dirs);
            Assert.Contains(sub2, dirs);
            Assert.Equal(2, dirs.Count);
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    [Fact]
    public void DirectoryExists_Should_ReturnTrue_When_DirectoryExists()
    {
        var dir = Path.GetTempPath();
        Assert.True(_fs.DirectoryExists(dir));
    }

    [Fact]
    public void DirectoryExists_Should_ReturnFalse_When_DirectoryDoesNotExist()
    {
        var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Assert.False(_fs.DirectoryExists(dir));
    }

    [Fact]
    public void CreateDirectory_Should_CreateDirectory_When_Invoked()
    {
        var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        try
        {
            _fs.CreateDirectory(dir);
            Assert.True(Directory.Exists(dir));
        }
        finally
        {
            Directory.Delete(dir);
        }
    }

    [Fact]
    public void CreateDirectory_Should_CreateNestedDirectories_When_Invoked()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var dir = Path.Combine(root, "nested", "deep");
        try
        {
            _fs.CreateDirectory(dir);
            Assert.True(Directory.Exists(dir));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void GetFullPath_Should_ReturnAbsolutePath_When_Invoked()
    {
        var path = ".";
        var full = _fs.GetFullPath(path);
        Assert.True(Path.IsPathRooted(full));
    }

    [Fact]
    public void GetDirectoryName_Should_ReturnDirectoryPart_When_Invoked()
    {
        var dir = _fs.GetDirectoryName("/a/b/c.txt");
        Assert.Equal("/a/b", dir);
    }

    [Fact]
    public void GetFileName_Should_ReturnFileNamePart_When_Invoked()
    {
        var name = _fs.GetFileName("/a/b/file.txt");
        Assert.Equal("file.txt", name);
    }

    [Fact]
    public void GetFileNameWithoutExtension_Should_ReturnNameWithoutExtension_When_Invoked()
    {
        var name = _fs.GetFileNameWithoutExtension("/a/b/file.txt");
        Assert.Equal("file", name);
    }

    [Fact]
    public void Combine_Should_JoinPaths_When_Invoked()
    {
        var result = _fs.Combine("a", "b", "c.txt");
        var expected = Path.Combine("a", "b", "c.txt");
        Assert.Equal(expected, result);
    }
}
