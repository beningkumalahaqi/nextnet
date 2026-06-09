using Microsoft.Extensions.DependencyInjection;
using NextNet.Build.StaticGeneration;
using NextNet.Rendering;
using Xunit;

namespace NextNet.Build.Tests.StaticGeneration;

/// <summary>
/// Tests for BuildPipeline orchestration.
/// We validate that the pipeline constructs correctly and fails gracefully
/// when the app directory doesn't exist.
/// </summary>
public class BuildPipelineTests
{
    [Fact]
    public void Constructor_Should_ThrowArgumentNullException_When_OptionsIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new BuildPipeline(null!, new ServiceCollection().BuildServiceProvider()));
    }

    [Fact]
    public void Constructor_Should_ThrowArgumentNullException_When_ServiceProviderIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new BuildPipeline(new SsgOptions(), null!));
    }

    [Fact]
    public async Task ExecuteAsync_Should_ReturnEmptyResult_When_AppDirMissing()
    {
        using var tempDir = new TempDirectory();
        var nonExistentApp = System.IO.Path.Combine(tempDir.Path, "nonexistent-app");

        var services = new ServiceCollection();
        var sp = services.BuildServiceProvider();

        var options = new SsgOptions
        {
            OutputDirectory = System.IO.Path.Combine(tempDir.Path, "dist"),
            MinifyHtml = false,
            CompressGzip = false,
            GenerateBuildManifest = false,
            CopyAssets = false,
            CleanOutput = false,
        };

        var pipeline = new BuildPipeline(
            options,
            sp,
            appDir: nonExistentApp,
            publicDir: System.IO.Path.Combine(tempDir.Path, "public"));

        var result = await pipeline.ExecuteAsync();

        // With no pages, generation should succeed with empty results
        Assert.True(result.Success);
        Assert.Empty(result.GeneratedFiles);
        Assert.Equal(0, result.PageCount);
    }

    [Fact]
    public async Task ExecuteAsync_Should_RespectSettings_When_CustomOptions()
    {
        using var tempDir = new TempDirectory();
        var appDir = System.IO.Path.Combine(tempDir.Path, "app");
        var outputDir = System.IO.Path.Combine(tempDir.Path, "dist");
        Directory.CreateDirectory(appDir);

        var services = new ServiceCollection().BuildServiceProvider();

        var options = new SsgOptions
        {
            OutputDirectory = outputDir,
            MinifyHtml = false,
            CompressGzip = false,
            GenerateBuildManifest = false,
            CopyAssets = false,
            CleanOutput = true,
        };

        var pipeline = new BuildPipeline(options, services, appDir: appDir);

        var result = await pipeline.ExecuteAsync();

        Assert.True(result.Success);
        Assert.Empty(result.GeneratedFiles);
    }

    [Fact]
    public void Constructor_Should_NotThrow_When_AppDirIsNull()
    {
        var options = new SsgOptions();
        var sp = new ServiceCollection().BuildServiceProvider();
        var pipeline = new BuildPipeline(options, sp, appDir: null);
        Assert.NotNull(pipeline);
    }

    [Fact]
    public void Constructor_Should_NotThrow_When_PublicDirIsNull()
    {
        var options = new SsgOptions();
        var sp = new ServiceCollection().BuildServiceProvider();
        var pipeline = new BuildPipeline(options, sp, publicDir: null);
        Assert.NotNull(pipeline);
    }

    [Fact]
    public async Task ExecuteAsync_Should_StillSucceed_When_EmptyAppDir()
    {
        using var tempDir = new TempDirectory();
        var appDir = System.IO.Path.Combine(tempDir.Path, "app");
        var outputDir = System.IO.Path.Combine(tempDir.Path, "dist");
        Directory.CreateDirectory(appDir);

        var services = new ServiceCollection().BuildServiceProvider();

        var options = new SsgOptions
        {
            OutputDirectory = outputDir,
            MinifyHtml = false,
            CompressGzip = false,
            GenerateBuildManifest = false,
            CopyAssets = false,
            CleanOutput = true,
        };

        var pipeline = new BuildPipeline(options, services, appDir: appDir);

        var result = await pipeline.ExecuteAsync();

        // With no pages found, it should still succeed with empty results
        Assert.True(result.Success);
        Assert.Empty(result.GeneratedFiles);
    }

    [Fact]
    public async Task ExecuteAsync_Should_CleanOutputFirst_When_CleanEnabled()
    {
        using var tempDir = new TempDirectory();
        var appDir = System.IO.Path.Combine(tempDir.Path, "app");
        var outputDir = System.IO.Path.Combine(tempDir.Path, "dist");
        Directory.CreateDirectory(appDir);
        Directory.CreateDirectory(outputDir);

        // Create a pre-existing file in output
        await File.WriteAllTextAsync(System.IO.Path.Combine(outputDir, "old.txt"), "old content");

        var services = new ServiceCollection().BuildServiceProvider();

        var options = new SsgOptions
        {
            OutputDirectory = outputDir,
            MinifyHtml = false,
            CompressGzip = false,
            GenerateBuildManifest = false,
            CopyAssets = false,
            CleanOutput = true,
        };

        var pipeline = new BuildPipeline(options, services, appDir: appDir);
        var result = await pipeline.ExecuteAsync();

        Assert.True(result.Success);

        // Old file should be gone after clean
        Assert.False(File.Exists(System.IO.Path.Combine(outputDir, "old.txt")));
    }

    [Fact]
    public async Task ExecuteAsync_Should_HandleCancellationGracefully_When_Cancelled()
    {
        using var tempDir = new TempDirectory();
        var appDir = System.IO.Path.Combine(tempDir.Path, "app");
        var outputDir = System.IO.Path.Combine(tempDir.Path, "dist");
        Directory.CreateDirectory(appDir);

        var services = new ServiceCollection().BuildServiceProvider();

        var options = new SsgOptions
        {
            OutputDirectory = outputDir,
            MinifyHtml = false,
            CompressGzip = false,
            GenerateBuildManifest = false,
            CopyAssets = false,
            CleanOutput = false,
        };

        var pipeline = new BuildPipeline(options, services, appDir: appDir);

        using var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately

        // Pipeline catches cancellation and returns error result
        var result = await pipeline.ExecuteAsync(cts.Token);
        Assert.False(result.Success);
        Assert.NotEmpty(result.Errors);
    }
}
