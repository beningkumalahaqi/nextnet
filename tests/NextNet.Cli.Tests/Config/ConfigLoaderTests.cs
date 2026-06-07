using System.Text.Json;
using NextNet.Cli.Config;
using Xunit;

namespace NextNet.Cli.Tests.Config;

public class ConfigLoaderTests : IDisposable
{
    private readonly string _testDir;

    public ConfigLoaderTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), "NextNetCliTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDir);
    }

    [Fact]
    public void Load_NoConfigFile_ReturnsNull()
    {
        var config = ConfigLoader.Load(_testDir);
        Assert.Null(config);
    }

    [Fact]
    public void Load_ValidConfig_ReturnsConfig()
    {
        var configPath = Path.Combine(_testDir, ConfigLoader.ConfigFileName);
        File.WriteAllText(configPath, """
        {
            "name": "my-app",
            "version": "1.0.0",
            "framework": { "ssr": true, "ssg": false, "streaming": true, "serverActions": true },
            "routing": { "dir": "app", "dynamicSegments": true, "catchAll": true },
            "build": { "output": "dist", "minify": true, "sourcemap": false, "target": "net10.0" },
            "dev": { "port": 3000, "https": false, "hmr": true, "openBrowser": true }
        }
        """);

        var config = ConfigLoader.Load(_testDir);
        Assert.NotNull(config);
        Assert.Equal("my-app", config.Name);
        Assert.Equal("1.0.0", config.Version);
        Assert.NotNull(config.Framework);
        Assert.True(config.Framework.Ssr);
        Assert.NotNull(config.Routing);
        Assert.Equal("app", config.Routing.Dir);
        Assert.NotNull(config.Build);
        Assert.Equal("dist", config.Build.Output);
        Assert.NotNull(config.Dev);
        Assert.Equal(3000, config.Dev.Port);
    }

    [Fact]
    public void Load_ConfigWithComments_Succeeds()
    {
        var configPath = Path.Combine(_testDir, ConfigLoader.ConfigFileName);
        File.WriteAllText(configPath, """
        {
            // This is a comment
            "name": "test-app",
            "version": "2.0.0"
        }
        """);

        var config = ConfigLoader.Load(_testDir);
        Assert.NotNull(config);
        Assert.Equal("test-app", config.Name);
        Assert.Equal("2.0.0", config.Version);
    }

    [Fact]
    public void Load_InvalidJson_ThrowsConfigException()
    {
        var configPath = Path.Combine(_testDir, ConfigLoader.ConfigFileName);
        File.WriteAllText(configPath, "{ invalid json }");

        var ex = Assert.Throws<NextNetConfigException>(() => ConfigLoader.Load(_testDir));
        Assert.Contains("NN-005", ex.ErrorEntry.Code);
    }

    [Fact]
    public void Load_DefaultDirectory_DoesNotThrow()
    {
        // Just ensure it doesn't throw when no config in cwd
        var exception = Record.Exception(() => ConfigLoader.Load());
        Assert.Null(exception);
    }

    [Fact]
    public void LoadRequired_NoConfig_Throws()
    {
        Assert.Throws<NextNetConfigException>(() => ConfigLoader.LoadRequired(_testDir));
    }

    [Fact]
    public void LoadRequired_ValidConfig_ReturnsConfig()
    {
        var configPath = Path.Combine(_testDir, ConfigLoader.ConfigFileName);
        File.WriteAllText(configPath, """{"name": "my-app"}""");

        var config = ConfigLoader.LoadRequired(_testDir);
        Assert.NotNull(config);
        Assert.Equal("my-app", config.Name);
    }

    public void Dispose()
    {
        try { Directory.Delete(_testDir, recursive: true); } catch { }
    }
}
