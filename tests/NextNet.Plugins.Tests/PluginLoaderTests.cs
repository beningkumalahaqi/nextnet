using NextNet.Logging;
using NextNet.Plugins.Tests.TestInfrastructure;
using Xunit;

namespace NextNet.Plugins.Tests;

public class PluginLoaderTests
{
    private readonly INextNetLogger _logger;

    public PluginLoaderTests()
    {
        _logger = new NextNetLogger("Test");
    }

    [Fact]
    public void Constructor_Should_ThrowArgumentNullException_When_LoggerIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new PluginLoader(null!));
    }

    [Fact]
    public void LoadAllFromDirectory_Should_ReturnEmptyList_When_DirectoryDoesNotExist()
    {
        var loader = new PluginLoader(_logger);
        var plugins = loader.LoadAllFromDirectory("/nonexistent/path/plugins");
        Assert.Empty(plugins);
    }

    [Fact]
    public void LoadAll_Should_ReturnEmptyList_When_BasePathHasNoPluginsDirectory()
    {
        var loader = new PluginLoader(_logger);
        var plugins = loader.LoadAll("/nonexistent/path");
        Assert.Empty(plugins);
    }

    [Fact]
    public void LoadFromAssembly_Should_ReturnNull_When_FileDoesNotExist()
    {
        var loader = new PluginLoader(_logger);
        var plugin = loader.LoadFromAssembly("/nonexistent/plugin.dll");
        Assert.Null(plugin);
    }

    [Fact]
    public void LoadFromAssemblyMetadata_Should_ReturnNull_When_AssemblyHasNoPluginAttribute()
    {
        var loader = new PluginLoader(_logger);
        var assembly = typeof(PluginLoaderTests).Assembly; // No [NextNetPlugin]
        var plugin = loader.LoadFromAssemblyMetadata(assembly);
        Assert.Null(plugin);
    }

    [Fact]
    public void LoadFromAssemblyMetadata_Should_ReturnPluginInstance_When_AssemblyHasValidAttribute()
    {
        // Build a dynamic assembly with the plugin attribute
        var assembly = TestPluginBuilder.BuildPluginAssembly(
            "TestPlugin",
            "1.0.0",
            typeof(TestPlugin));

        var loader = new PluginLoader(_logger);
        var plugin = loader.LoadFromAssemblyMetadata(assembly);

        Assert.NotNull(plugin);
        Assert.Equal("TestPlugin", plugin!.Name);
        Assert.Equal(new Version(1, 0, 0), plugin.Version);
    }

    [Fact]
    public void LoadFromAssemblyMetadata_Should_ResolveMultiplePlugins_When_GivenDifferentAssemblies()
    {
        var assembly1 = TestPluginBuilder.BuildPluginAssembly(
            "PluginA", "2.0.0", typeof(TestPlugin));

        var assembly2 = TestPluginBuilder.BuildPluginAssembly(
            "PluginB", "1.5.0", typeof(AnotherTestPlugin));

        var loader = new PluginLoader(_logger);

        var plugin1 = loader.LoadFromAssemblyMetadata(assembly1);
        var plugin2 = loader.LoadFromAssemblyMetadata(assembly2);

        Assert.NotNull(plugin1);
        Assert.NotNull(plugin2);
        // Name comes from the class, not the attribute
        Assert.Equal("TestPlugin", plugin1!.Name);
        Assert.Equal("AnotherTestPlugin", plugin2!.Name);
        Assert.Equal(new Version(1, 0, 0), plugin1.Version);
        Assert.Equal(new Version(2, 0, 0), plugin2.Version);
    }

    [Fact]
    public void LoadFromAssemblyMetadata_Should_ReturnNull_When_TypeDoesNotImplementINextNetPlugin()
    {
        var assembly = TestPluginBuilder.BuildPluginAssembly(
            "InvalidPlugin",
            "1.0.0",
            typeof(object)); // object does not implement INextNetPlugin

        var loader = new PluginLoader(_logger);
        var plugin = loader.LoadFromAssemblyMetadata(assembly);

        Assert.Null(plugin);
    }

    [Fact]
    public void LoadAllFromDirectory_Should_ReturnEmptyList_When_DirectoryContainsNoDlls()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            var loader = new PluginLoader(_logger);
            var plugins = loader.LoadAllFromDirectory(tempDir);
            Assert.Empty(plugins);
        }
        finally
        {
            Directory.Delete(tempDir);
        }
    }
}
