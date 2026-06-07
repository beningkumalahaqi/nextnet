using Microsoft.Extensions.DependencyInjection;
using NextNet.Build.Extensions;
using NextNet.Build.StaticGeneration;
using Xunit;

namespace NextNet.Build.Tests.Extensions;

public class BuildServiceCollectionExtensionsTests
{
    [Fact]
    public void AddNextNetBuild_RegistersServices()
    {
        var services = new ServiceCollection();

        services.AddNextNetBuild();

        var sp = services.BuildServiceProvider();

        Assert.NotNull(sp.GetService<SsgOptions>());
        Assert.NotNull(sp.GetService<BuildPipeline>());
        Assert.NotNull(sp.GetService<OutputWriter>());
    }

    [Fact]
    public void AddNextNetBuild_WithConfigure_CustomizesOptions()
    {
        var services = new ServiceCollection();

        services.AddNextNetBuild(options =>
        {
            options.OutputDirectory = "custom-dist";
            options.MinifyHtml = false;
        });

        var sp = services.BuildServiceProvider();
        var options = sp.GetRequiredService<SsgOptions>();

        Assert.Equal("custom-dist", options.OutputDirectory);
        Assert.False(options.MinifyHtml);
    }

    [Fact]
    public void AddNextNetBuild_WithDefaults_UsesDefaultOptions()
    {
        var services = new ServiceCollection();

        services.AddNextNetBuild();

        var sp = services.BuildServiceProvider();
        var options = sp.GetRequiredService<SsgOptions>();

        Assert.Equal("dist", options.OutputDirectory);
        Assert.True(options.MinifyHtml);
    }

    [Fact]
    public void AddNextNetBuild_NullServices_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            ((IServiceCollection)null!).AddNextNetBuild());
    }
}
