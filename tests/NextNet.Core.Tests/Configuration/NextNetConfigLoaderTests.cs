using NextNet.Conventions;
using NextNet.Configuration;
using NextNet.Exceptions;
using NextNet.IO;
using Xunit;

namespace NextNet.Core.Tests.Configuration;

public class NextNetConfigLoaderTests
{
    private static readonly string ConfigFileName = NextNetConventions.ConfigFileName;

    [Fact]
    public void Load_WhenNoConfigFileExists_ReturnsDefaultConfig()
    {
        // Arrange
        using var tempDir = new TempDirectory();
        var loader = new NextNetConfigLoader();

        // Act
        var config = loader.Load(tempDir.Path);

        // Assert
        Assert.NotNull(config);
        Assert.Equal("app", config.AppDir);
        Assert.Equal("dist", config.OutputDir);
        Assert.Equal(3000, config.DevPort);
        Assert.True(config.Ssr);
        Assert.False(config.Ssg);
        Assert.True(config.Streaming);
        Assert.True(config.ServerActions);
        Assert.NotNull(config.Rendering);
        Assert.False(config.Rendering.PrettyPrint);
        Assert.Equal(128, config.Rendering.MaxRecursionDepth);
        Assert.True(config.Rendering.Minify);
    }

    [Fact]
    public void Load_WhenConfigFileExistsInDirectory_LoadsConfig()
    {
        // Arrange
        using var tempDir = new TempDirectory();
        var configPath = Path.Combine(tempDir.Path, ConfigFileName);
        File.WriteAllText(configPath, """
            {
                "appDir": "src/app",
                "outputDir": "build",
                "devPort": 5000,
                "ssr": false,
                "ssg": true,
                "streaming": false,
                "serverActions": false,
                "rendering": {
                    "prettyPrint": true,
                    "maxRecursionDepth": 64,
                    "minify": false
                }
            }
            """);

        var loader = new NextNetConfigLoader();

        // Act
        var config = loader.Load(tempDir.Path);

        // Assert
        Assert.NotNull(config);
        Assert.Equal("src/app", config.AppDir);
        Assert.Equal("build", config.OutputDir);
        Assert.Equal(5000, config.DevPort);
        Assert.False(config.Ssr);
        Assert.True(config.Ssg);
        Assert.False(config.Streaming);
        Assert.False(config.ServerActions);
        Assert.True(config.Rendering.PrettyPrint);
        Assert.Equal(64, config.Rendering.MaxRecursionDepth);
        Assert.False(config.Rendering.Minify);
    }

    [Fact]
    public void Load_WhenConfigFileExistsInParentDirectory_LoadsConfig()
    {
        // Arrange
        using var tempDir = new TempDirectory();
        var configPath = Path.Combine(tempDir.Path, ConfigFileName);
        File.WriteAllText(configPath, @"{ ""appDir"": ""custom/app"" }");

        var subDir = Path.Combine(tempDir.Path, "sub", "dir");
        Directory.CreateDirectory(subDir);

        var loader = new NextNetConfigLoader();

        // Act
        var config = loader.Load(subDir);

        // Assert
        Assert.NotNull(config);
        Assert.Equal("custom/app", config.AppDir);
    }

    [Fact]
    public void Load_WhenConfigFileContainsInvalidJson_ThrowsConfigurationException()
    {
        // Arrange
        using var tempDir = new TempDirectory();
        var configPath = Path.Combine(tempDir.Path, ConfigFileName);
        File.WriteAllText(configPath, "{ invalid json }");

        var loader = new NextNetConfigLoader();

        // Act & Assert
        var ex = Assert.Throws<NextNetConfigurationException>(() => loader.Load(tempDir.Path));
        Assert.Contains(configPath, ex.Message);
    }

    [Fact]
    public void Load_WhenStartDirectoryIsNull_UsesCurrentDirectory()
    {
        // This test verifies the default parameter doesn't throw.
        var loader = new NextNetConfigLoader();
        var config = loader.Load(); // uses CWD
        Assert.NotNull(config);
    }

    [Fact]
    public void FindConfigFile_ReturnsNullWhenNoConfigExists()
    {
        using var tempDir = new TempDirectory();
        var loader = new NextNetConfigLoader();
        var result = loader.FindConfigFile(tempDir.Path);
        Assert.Null(result);
    }

    [Fact]
    public void FindConfigFile_FindsConfigInCurrentDirectory()
    {
        using var tempDir = new TempDirectory();
        var configPath = Path.Combine(tempDir.Path, ConfigFileName);
        File.WriteAllText(configPath, "{}");

        var loader = new NextNetConfigLoader();
        var result = loader.FindConfigFile(tempDir.Path);
        Assert.NotNull(result);
        Assert.Equal(configPath, result);
    }

    [Fact]
    public void FindConfigFile_SearchesUpwardWhenNotInCurrentDirectory()
    {
        using var tempDir = new TempDirectory();
        var configPath = Path.Combine(tempDir.Path, ConfigFileName);
        File.WriteAllText(configPath, "{}");

        var subDir = Path.Combine(tempDir.Path, "deeply", "nested", "path");
        Directory.CreateDirectory(subDir);

        var loader = new NextNetConfigLoader();
        var result = loader.FindConfigFile(subDir);
        Assert.NotNull(result);
        Assert.Equal(configPath, result);
    }

    [Fact]
    public void Constructor_WithCustomFileSystem_UsesIt()
    {
        // Arrange
        var fs = new TestFileSystem();
        fs.Files[NextNetConventions.ConfigFileName] = @"{ ""appDir"": ""custom"" }";

        var loader = new NextNetConfigLoader(fs);

        // Act
        var config = loader.Load(".");

        // Assert
        Assert.NotNull(config);
        Assert.Equal("custom", config.AppDir);
    }

    [Fact]
    public void Constructor_WithCustomFileSystem_WhenNoFile_ReturnsDefault()
    {
        // Arrange
        var fs = new TestFileSystem(); // no files
        var loader = new NextNetConfigLoader(fs);

        // Act
        var config = loader.Load(".");

        // Assert
        Assert.NotNull(config);
        Assert.Equal("app", config.AppDir); // default
    }

    [Fact]
    public void Constructor_WithNullFileSystem_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new NextNetConfigLoader(null!));
    }

    /// <summary>
    /// A test implementation of <see cref="ISharpFileSystem"/> that stores files in memory.
    /// </summary>
    internal sealed class TestFileSystem : global::NextNet.IO.ISharpFileSystem
    {
        public Dictionary<string, string> Files { get; } = new(StringComparer.OrdinalIgnoreCase);

        public bool FileExists(string path) =>
            Files.ContainsKey(GetFileName(path));

        public string ReadAllText(string path) =>
            Files.TryGetValue(GetFileName(path), out var content) ? content : throw new FileNotFoundException();

        public Task<string> ReadAllTextAsync(string path) =>
            Task.FromResult(ReadAllText(path));

        public void WriteAllText(string path, string content) =>
            Files[GetFileName(path)] = content;

        public Task WriteAllTextAsync(string path, string content)
        {
            WriteAllText(path, content);
            return Task.CompletedTask;
        }

        public IEnumerable<string> EnumerateFiles(string directory, string pattern) =>
            Files.Keys.Where(f => f.EndsWith(pattern.TrimStart('*')));

        public IEnumerable<string> EnumerateDirectories(string directory) =>
            Enumerable.Empty<string>();

        public bool DirectoryExists(string path) => true;

        public void CreateDirectory(string path) { }

        public string GetFullPath(string path) =>
            System.IO.Path.GetFullPath(path);

        public string? GetDirectoryName(string? path) =>
            path == null ? null : System.IO.Path.GetDirectoryName(path);

        public string GetFileName(string path) =>
            System.IO.Path.GetFileName(path);

        public string GetFileNameWithoutExtension(string path) =>
            System.IO.Path.GetFileNameWithoutExtension(path);

        public string Combine(params string[] paths) =>
            System.IO.Path.Combine(paths);

        public Task WriteAllBytesAsync(string path, byte[] content, CancellationToken cancellationToken = default)
        {
            WriteAllText(path, System.Text.Encoding.UTF8.GetString(content));
            return Task.CompletedTask;
        }

        public void DeleteDirectory(string path, bool recursive = true) { }
    }

    /// <summary>
    /// Helper that creates a temporary directory and cleans it up on dispose.
    /// </summary>
    internal sealed class TempDirectory : IDisposable
    {
        public string Path { get; }

        public TempDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "NextNetTest_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public void Dispose()
        {
            try { Directory.Delete(Path, recursive: true); }
            catch { /* best effort cleanup */ }
        }
    }
}
