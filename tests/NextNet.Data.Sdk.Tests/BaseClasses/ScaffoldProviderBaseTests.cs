using NextNet.Data.Abstractions.Abstractions;
using NextNet.Data.Abstractions.Configuration;
using NextNet.Data.Abstractions.Models;
using NextNet.Data.Sdk.Base;
using Microsoft.Extensions.Options;

namespace NextNet.Data.Sdk.Tests.BaseClasses;

/// <summary>
/// Tests for <see cref="ScaffoldProviderBase"/>.
/// </summary>
public class ScaffoldProviderBaseTests
{
    [Fact]
    public async Task GenerateModelAsync_Should_Throw_WhenEntityNameIsNull()
    {
        var provider = CreateProvider();
        var options = new ScaffoldOptions();

        await Assert.ThrowsAsync<ArgumentNullException>(() => provider.GenerateModelAsync(null!, options));
    }

    [Fact]
    public async Task GenerateModelAsync_Should_Throw_WhenOptionsIsNull()
    {
        var provider = CreateProvider();

        await Assert.ThrowsAsync<ArgumentNullException>(() => provider.GenerateModelAsync("Test", null!));
    }

    [Fact]
    public async Task GenerateModelAsync_Should_ReturnArtifact()
    {
        var provider = CreateProvider();
        var options = new ScaffoldOptions(OutputDirectory: Path.GetTempPath());

        var result = await provider.GenerateModelAsync("User", options);

        Assert.NotNull(result);
        Assert.Equal("User", result.EntityName);
        Assert.Equal(ScaffoldArtifactType.Model, result.ArtifactType);
    }

    [Fact]
    public async Task GenerateRepositoryAsync_Should_Throw_WhenEntityNameIsNull()
    {
        var provider = CreateProvider();
        var options = new ScaffoldOptions();

        await Assert.ThrowsAsync<ArgumentNullException>(() => provider.GenerateRepositoryAsync(null!, options));
    }

    [Fact]
    public async Task GenerateRepositoryAsync_Should_ReturnArtifact()
    {
        var provider = CreateProvider();
        var options = new ScaffoldOptions(OutputDirectory: Path.GetTempPath());

        var result = await provider.GenerateRepositoryAsync("User", options);

        Assert.NotNull(result);
        Assert.Equal("User", result.EntityName);
        Assert.Equal(ScaffoldArtifactType.Repository, result.ArtifactType);
    }

    [Fact]
    public async Task GenerateCrudAsync_Should_Throw_WhenEntityNameIsNull()
    {
        var provider = CreateProvider();
        var options = new ScaffoldOptions();

        await Assert.ThrowsAsync<ArgumentNullException>(() => provider.GenerateCrudAsync(null!, options));
    }

    [Fact]
    public async Task GenerateCrudAsync_Should_ReturnArtifacts()
    {
        var provider = CreateProvider();
        var options = new ScaffoldOptions(OutputDirectory: Path.GetTempPath());

        var results = await provider.GenerateCrudAsync("User", options);

        Assert.NotNull(results);
        Assert.NotEmpty(results);
    }

    [Fact]
    public void RenderTemplate_Should_ReplacePlaceholders()
    {
        var provider = CreateProvider();
        var template = "Hello {{Name}}, you are {{Age}} years old.";
        var values = new Dictionary<string, string>
        {
            ["Name"] = "Alice",
            ["Age"] = "30"
        };

        var result = provider.ExposedRenderTemplate(template, values);

        Assert.Equal("Hello Alice, you are 30 years old.", result);
    }

    [Fact]
    public void RenderTemplate_Should_ReturnOriginal_WhenNoPlaceholders()
    {
        var provider = CreateProvider();
        var template = "No placeholders here.";

        var result = provider.ExposedRenderTemplate(template, new Dictionary<string, string>());

        Assert.Equal(template, result);
    }

    [Fact]
    public async Task WriteFileAsync_Should_CreateFile()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var filePath = Path.Combine(tempDir, "test.txt");

        try
        {
            var provider = CreateProvider();
            var writtenPath = await provider.ExposedWriteFileAsync(filePath, "test content", CancellationToken.None);

            Assert.Equal(filePath, writtenPath);
            Assert.True(File.Exists(filePath));
            Assert.Equal("test content", await File.ReadAllTextAsync(filePath));
        }
        finally
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }

    private static TestScaffoldProvider CreateProvider()
    {
        var config = Options.Create(new ScaffoldingConfig());
        return new TestScaffoldProvider(config);
    }

    /// <summary>
    /// Test scaffold provider implementation.
    /// </summary>
    private sealed class TestScaffoldProvider : ScaffoldProviderBase
    {
        public override string ProviderName => "Test";

        public TestScaffoldProvider(IOptions<ScaffoldingConfig> config, ILogger? logger = null)
            : base(config, logger) { }

        public string ExposedRenderTemplate(string template, IReadOnlyDictionary<string, string> values)
            => RenderTemplate(template, values);

        public Task<string> ExposedWriteFileAsync(string filePath, string content, CancellationToken ct)
            => WriteFileAsync(filePath, content, ct);

        protected override Task<ScaffoldArtifact> GenerateModelCoreAsync(
            string entityName, string outputPath, CancellationToken cancellationToken)
        {
            return Task.FromResult(new ScaffoldArtifact(
                outputPath,
                $"Models/{entityName}.cs",
                ScaffoldArtifactType.Model,
                entityName,
                20));
        }

        protected override Task<ScaffoldArtifact> GenerateRepositoryCoreAsync(
            string entityName, string outputPath, CancellationToken cancellationToken)
        {
            return Task.FromResult(new ScaffoldArtifact(
                outputPath,
                $"Repositories/{entityName}Repository.cs",
                ScaffoldArtifactType.Repository,
                entityName,
                30));
        }

        protected override Task<ScaffoldArtifact[]> GenerateCrudCoreAsync(
            string entityName, string outputPath, CancellationToken cancellationToken)
        {
            return Task.FromResult(new[]
            {
                new ScaffoldArtifact(
                    Path.Combine(outputPath, "Create.cs"),
                    $"Actions/{entityName}/Create.cs",
                    ScaffoldArtifactType.Action,
                    entityName,
                    15)
            });
        }
    }
}
