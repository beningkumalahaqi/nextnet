using NextNet.Data.Abstractions.Models;

namespace NextNet.Data.EntityFramework.Tests;

/// <summary>
/// Tests for <see cref="EfCoreScaffoldProvider"/> using the new API.
/// </summary>
public sealed class EfCoreScaffoldProviderTests : IDisposable
{
    private readonly string _tempDir;

    public EfCoreScaffoldProviderTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);
    }

    [Fact]
    public async Task GenerateModelAsync_Should_WriteModelFile()
    {
        // Arrange
        var provider = new EfCoreScaffoldProvider();
        var options = new ScaffoldOptions(
            OutputDirectory: _tempDir,
            ModelsNamespace: "TestApp.Models",
            ProjectNamespace: "TestApp");

        // Act
        var artifact = await provider.GenerateModelAsync("User", options);

        // Assert
        Assert.NotNull(artifact);
        Assert.False(artifact.WasSkipped);
        Assert.True(File.Exists(artifact.FilePath), $"Expected file '{artifact.FilePath}' to exist.");
        Assert.Equal("User", artifact.EntityName);
        Assert.Equal(ScaffoldArtifactType.Model, artifact.ArtifactType);

        var content = await File.ReadAllTextAsync(artifact.FilePath);
        Assert.Contains("namespace TestApp.Models;", content);
        Assert.Contains("class User", content);
        Assert.True(artifact.LinesOfCode > 0);
    }

    [Fact]
    public async Task GenerateModelAsync_Should_IncludeProperties_When_Provided()
    {
        // Arrange
        var provider = new EfCoreScaffoldProvider();
        var properties = new List<ScaffoldProperty>
        {
            new("Name", "string", IsRequired: true),
            new("Email", "string"),
            new("Price", "decimal")
        };
        var options = new ScaffoldOptions(
            OutputDirectory: _tempDir,
            ModelsNamespace: "TestApp.Models",
            ProjectNamespace: "TestApp",
            Properties: properties.AsReadOnly());

        // Act
        var artifact = await provider.GenerateModelAsync("Product", options);

        // Assert
        var content = await File.ReadAllTextAsync(artifact.FilePath);
        Assert.Contains("class Product", content);
        Assert.Contains("public string Name { get; set; }", content);
        Assert.Contains("public string? Email { get; set; }", content);
        Assert.Contains("public decimal? Price { get; set; }", content);
    }

    [Fact]
    public async Task GenerateModelAsync_Should_Throw_When_EntityNameIsEmpty()
    {
        // Arrange
        var provider = new EfCoreScaffoldProvider();
        var options = new ScaffoldOptions(OutputDirectory: _tempDir);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            provider.GenerateModelAsync("", options));
    }

    [Fact]
    public async Task GenerateModelAsync_DryRun_Should_NotWriteFile()
    {
        // Arrange
        var provider = new EfCoreScaffoldProvider();
        var options = new ScaffoldOptions(
            OutputDirectory: _tempDir,
            ModelsNamespace: "TestApp.Models",
            ProjectNamespace: "TestApp",
            DryRun: true);

        // Act
        var artifact = await provider.GenerateModelAsync("User", options);

        // Assert
        Assert.NotNull(artifact);
        Assert.False(File.Exists(artifact.FilePath));
        Assert.True(artifact.LinesOfCode > 0);
    }

    [Fact]
    public async Task GenerateModelAsync_Overwrite_ExistingFile()
    {
        // Arrange
        var provider = new EfCoreScaffoldProvider();
        var options = new ScaffoldOptions(
            OutputDirectory: _tempDir,
            ModelsNamespace: "TestApp.Models",
            ProjectNamespace: "TestApp",
            OverwriteExisting: false);

        var artifact1 = await provider.GenerateModelAsync("User", options);
        Assert.False(artifact1.WasSkipped);

        // Act - Run again without overwrite
        var artifact2 = await provider.GenerateModelAsync("User", options);

        // Assert
        Assert.True(artifact2.WasSkipped, "Second run without --force should skip.");

        // Act - Run again with overwrite
        var optionsForce = options with { OverwriteExisting = true };
        var artifact3 = await provider.GenerateModelAsync("User", optionsForce);

        Assert.False(artifact3.WasSkipped, "Third run with --force should overwrite.");
    }

    [Fact]
    public async Task GenerateRepositoryAsync_Should_WriteRepositoryFile()
    {
        // Arrange
        var provider = new EfCoreScaffoldProvider();
        var options = new ScaffoldOptions(
            OutputDirectory: _tempDir,
            ModelsNamespace: "TestApp.Models",
            RepositoriesNamespace: "TestApp.Repositories",
            ProjectNamespace: "TestApp");

        // Act
        var artifact = await provider.GenerateRepositoryAsync("User", options);

        // Assert
        Assert.NotNull(artifact);
        Assert.False(artifact.WasSkipped);
        Assert.True(File.Exists(artifact.FilePath));
        Assert.Equal(ScaffoldArtifactType.Repository, artifact.ArtifactType);

        var content = await File.ReadAllTextAsync(artifact.FilePath);
        Assert.Contains("namespace TestApp.Repositories;", content);
        Assert.Contains("class UserRepository", content);
        Assert.Contains("IRepository<User>", content);
    }

    [Fact]
    public async Task GenerateCrudAsync_Should_CreateModelRepositoryAndRoute()
    {
        // Arrange
        var provider = new EfCoreScaffoldProvider();
        var options = new ScaffoldOptions(
            OutputDirectory: _tempDir,
            ModelsNamespace: "TestApp.Models",
            RepositoriesNamespace: "TestApp.Repositories",
            ActionsNamespace: "TestApp.Actions",
            ProjectNamespace: "TestApp");

        // Act
        var artifacts = await provider.GenerateCrudAsync("Product", options);

        // Assert - Should produce 3 artifacts: model, repository, route
        Assert.Equal(3, artifacts.Length);

        // Model
        Assert.Contains(artifacts, a => a.ArtifactType == ScaffoldArtifactType.Model);
        var modelFile = artifacts.First(a => a.ArtifactType == ScaffoldArtifactType.Model);
        Assert.True(File.Exists(modelFile.FilePath));
        var modelContent = await File.ReadAllTextAsync(modelFile.FilePath);
        Assert.Contains("class Product", modelContent);

        // Repository
        Assert.Contains(artifacts, a => a.ArtifactType == ScaffoldArtifactType.Repository);
        var repoFile = artifacts.First(a => a.ArtifactType == ScaffoldArtifactType.Repository);
        Assert.True(File.Exists(repoFile.FilePath));
        var repoContent = await File.ReadAllTextAsync(repoFile.FilePath);
        Assert.Contains("class ProductRepository", repoContent);

        // Route action
        Assert.Contains(artifacts, a => a.ArtifactType == ScaffoldArtifactType.Action);
        var routeFile = artifacts.First(a => a.ArtifactType == ScaffoldArtifactType.Action);
        Assert.True(File.Exists(routeFile.FilePath));
        var routeContent = await File.ReadAllTextAsync(routeFile.FilePath);
        Assert.Contains("class ProductApi", routeContent);
        Assert.Contains("GetAll", routeContent);
        Assert.Contains("GetById", routeContent);
        Assert.Contains("Create", routeContent);
        Assert.Contains("Update", routeContent);
        Assert.Contains("Delete", routeContent);
    }

    [Fact]
    public async Task GenerateCrudAsync_Should_Throw_When_EntityNameIsEmpty()
    {
        // Arrange
        var provider = new EfCoreScaffoldProvider();
        var options = new ScaffoldOptions(OutputDirectory: _tempDir);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            provider.GenerateCrudAsync("", options));
    }

    [Fact]
    public void ProviderName_Should_ReturnEntityFramework()
    {
        // Arrange
        var provider = new EfCoreScaffoldProvider();

        // Act & Assert
        Assert.Equal("EntityFramework", provider.ProviderName);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            try { Directory.Delete(_tempDir, recursive: true); } catch { }
        }
    }
}
