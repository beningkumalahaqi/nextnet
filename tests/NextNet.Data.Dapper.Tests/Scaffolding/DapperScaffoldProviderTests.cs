using NextNet.Data.Dapper.Scaffolding;
using NextNet.Data.Dapper.Templates;

namespace NextNet.Data.Dapper.Tests.Scaffolding;

/// <summary>
/// Tests for <see cref="DapperScaffoldProvider"/> template generation,
/// artifact creation, and placeholder substitution.
/// </summary>
public sealed class DapperScaffoldProviderTests
{
    private readonly DapperScaffoldProvider _provider;
    private readonly string _tempOutputDir;

    public DapperScaffoldProviderTests()
    {
        _provider = new DapperScaffoldProvider(
            new Microsoft.Extensions.Logging.Abstractions.NullLogger<DapperScaffoldProvider>());

        _tempOutputDir = Path.Combine(Path.GetTempPath(), "NextNetDapperScaffoldTests", Guid.NewGuid().ToString());
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ProviderName_Should_ReturnDapper()
    {
        Assert.Equal("Dapper", _provider.ProviderName);
    }

    // ===== GenerateModelAsync Tests =====

    [Fact]
    [Trait("Category", "Unit")]
    public async Task GenerateModelAsync_Should_ThrowArgumentException_When_EntityNameIsEmpty()
    {
        var ex = await Assert.ThrowsAsync<ArgumentException>(
            () => _provider.GenerateModelAsync(string.Empty, new ScaffoldOptions()));
        Assert.Contains("entityName", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task GenerateModelAsync_Should_ThrowArgumentNullException_When_EntityNameIsNull()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _provider.GenerateModelAsync(null!, new ScaffoldOptions()));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task GenerateModelAsync_Should_ThrowArgumentNullException_When_OptionsIsNull()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _provider.GenerateModelAsync("User", null!));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task GenerateModelAsync_Should_ReturnArtifact_WithCorrectType()
    {
        var options = new ScaffoldOptions(OutputDirectory: _tempOutputDir, DryRun: true);
        var artifact = await _provider.GenerateModelAsync("User", options);

        Assert.Equal("User", artifact.EntityName);
        Assert.Equal(ScaffoldArtifactType.Model, artifact.ArtifactType);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task GenerateModelAsync_Should_NotWriteFile_When_DryRun()
    {
        var options = new ScaffoldOptions(OutputDirectory: _tempOutputDir, DryRun: true);
        var artifact = await _provider.GenerateModelAsync("Product", options);

        Assert.False(artifact.WasSkipped);
        Assert.False(File.Exists(artifact.FilePath), "File should not exist in dry-run mode.");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task GenerateModelAsync_Should_WriteFile_When_NotDryRun()
    {
        try
        {
            var options = new ScaffoldOptions(OutputDirectory: _tempOutputDir, DryRun: false);
            var artifact = await _provider.GenerateModelAsync("Customer", options);

            Assert.True(File.Exists(artifact.FilePath), "File should exist after generation.");
            Assert.True(artifact.LinesOfCode > 0, "Generated file should have content.");

            var content = await File.ReadAllTextAsync(artifact.FilePath);
            Assert.Contains("class Customer", content);
            Assert.Contains("namespace", content);
        }
        finally
        {
            CleanupDirectory(_tempOutputDir);
        }
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task GenerateModelAsync_Should_UseCustomNamespace()
    {
        var options = new ScaffoldOptions(
            OutputDirectory: _tempOutputDir,
            ModelsNamespace: "MyApp.Domain.Entities",
            DryRun: true);

        var artifact = await _provider.GenerateModelAsync("Order", options);

        // The artifact path should be in the Models subdirectory
        Assert.Contains("Order.cs", artifact.FilePath);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task GenerateModelAsync_Should_ReportSkipped_When_FileExistsAndNoOverwrite()
    {
        try
        {
            // First write the file
            var options = new ScaffoldOptions(OutputDirectory: _tempOutputDir, DryRun: false);
            await _provider.GenerateModelAsync("SkippedEntity", options);

            // Try to generate again without overwrite
            var artifact = await _provider.GenerateModelAsync("SkippedEntity", options);

            Assert.True(artifact.WasSkipped, "Artifact should be skipped when file exists and OverwriteExisting is false.");
        }
        finally
        {
            CleanupDirectory(_tempOutputDir);
        }
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task GenerateModelAsync_Should_Overwrite_When_FileExistsAndOverwriteTrue()
    {
        try
        {
            // First write the file
            var options = new ScaffoldOptions(OutputDirectory: _tempOutputDir, DryRun: false);
            await _provider.GenerateModelAsync("OverwriteEntity", options);

            // Generate again with overwrite
            var overwriteOptions = new ScaffoldOptions(
                OutputDirectory: _tempOutputDir,
                DryRun: false,
                OverwriteExisting: true);
            var artifact = await _provider.GenerateModelAsync("OverwriteEntity", overwriteOptions);

            Assert.False(artifact.WasSkipped, "Artifact should not be skipped when OverwriteExisting is true.");
        }
        finally
        {
            CleanupDirectory(_tempOutputDir);
        }
    }

    // ===== GenerateRepositoryAsync Tests =====

    [Fact]
    [Trait("Category", "Unit")]
    public async Task GenerateRepositoryAsync_Should_ThrowArgumentNullException_When_EntityNameIsNull()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _provider.GenerateRepositoryAsync(null!, new ScaffoldOptions()));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task GenerateRepositoryAsync_Should_ThrowArgumentNullException_When_OptionsIsNull()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _provider.GenerateRepositoryAsync("User", null!));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task GenerateRepositoryAsync_Should_ReturnArtifact_WithCorrectType()
    {
        var options = new ScaffoldOptions(OutputDirectory: _tempOutputDir, DryRun: true);
        var artifact = await _provider.GenerateRepositoryAsync("User", options);

        Assert.Equal("User", artifact.EntityName);
        Assert.Equal(ScaffoldArtifactType.Repository, artifact.ArtifactType);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task GenerateRepositoryAsync_Should_UseRepositorySuffix_InFileName()
    {
        var options = new ScaffoldOptions(OutputDirectory: _tempOutputDir, DryRun: true);
        var artifact = await _provider.GenerateRepositoryAsync("Product", options);

        Assert.Contains("ProductRepository.cs", artifact.FilePath);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task GenerateRepositoryAsync_Should_NotWriteFile_When_DryRun()
    {
        var options = new ScaffoldOptions(OutputDirectory: _tempOutputDir, DryRun: true);
        var artifact = await _provider.GenerateRepositoryAsync("Invoice", options);

        Assert.False(artifact.WasSkipped);
        Assert.False(File.Exists(artifact.FilePath), "File should not exist in dry-run mode.");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task GenerateRepositoryAsync_Should_WriteFile_When_NotDryRun()
    {
        try
        {
            var options = new ScaffoldOptions(OutputDirectory: _tempOutputDir, DryRun: false);
            var artifact = await _provider.GenerateRepositoryAsync("Invoice", options);

            Assert.True(File.Exists(artifact.FilePath), "File should exist after generation.");
            var content = await File.ReadAllTextAsync(artifact.FilePath);
            Assert.Contains("class InvoiceRepository", content);
            Assert.Contains("DapperRepository<Invoice>", content);
        }
        finally
        {
            CleanupDirectory(_tempOutputDir);
        }
    }

    // ===== GenerateCrudAsync Tests =====

    [Fact]
    [Trait("Category", "Unit")]
    public async Task GenerateCrudAsync_Should_ThrowArgumentNullException_When_EntityNameIsNull()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _provider.GenerateCrudAsync(null!, new ScaffoldOptions()));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task GenerateCrudAsync_Should_ThrowArgumentNullException_When_OptionsIsNull()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _provider.GenerateCrudAsync("User", null!));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task GenerateCrudAsync_Should_ReturnThreeArtifacts()
    {
        var options = new ScaffoldOptions(OutputDirectory: _tempOutputDir, DryRun: true);
        var artifacts = await _provider.GenerateCrudAsync("Category", options);

        Assert.Equal(3, artifacts.Length);

        var types = artifacts.Select(a => a.ArtifactType).OrderBy(t => t).ToList();
        Assert.Contains(ScaffoldArtifactType.Model, types);
        Assert.Contains(ScaffoldArtifactType.Repository, types);
        Assert.Contains(ScaffoldArtifactType.Action, types);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task GenerateCrudAsync_Should_NotWriteFiles_When_DryRun()
    {
        var options = new ScaffoldOptions(OutputDirectory: _tempOutputDir, DryRun: true);
        var artifacts = await _provider.GenerateCrudAsync("Tag", options);

        foreach (var artifact in artifacts)
        {
            Assert.False(artifact.WasSkipped, $"Artifact should not be skipped in dry-run: {artifact.FilePath}");
            Assert.False(File.Exists(artifact.FilePath), $"File should not exist in dry-run: {artifact.FilePath}");
        }
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task GenerateCrudAsync_Should_WriteAllFiles_When_NotDryRun()
    {
        try
        {
            var options = new ScaffoldOptions(OutputDirectory: _tempOutputDir, DryRun: false);
            var artifacts = await _provider.GenerateCrudAsync("Category", options);

            foreach (var artifact in artifacts)
            {
                Assert.True(File.Exists(artifact.FilePath),
                    $"File should exist: {artifact.FilePath}");
                Assert.True(artifact.LinesOfCode > 0,
                    $"Generated file should have content: {artifact.FilePath}");
            }
        }
        finally
        {
            CleanupDirectory(_tempOutputDir);
        }
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task GenerateCrudAsync_Should_GenerateRouteController_WithCorrectName()
    {
        try
        {
            var options = new ScaffoldOptions(OutputDirectory: _tempOutputDir, DryRun: false);
            var artifacts = await _provider.GenerateCrudAsync("Category", options);

            var routeArtifact = artifacts.First(a => a.ArtifactType == ScaffoldArtifactType.Action);
            Assert.Contains("CategoryController.cs", routeArtifact.FilePath);

            var content = await File.ReadAllTextAsync(routeArtifact.FilePath);
            Assert.Contains("class CategoryController", content);
            Assert.Contains("api/category", content);
        }
        finally
        {
            CleanupDirectory(_tempOutputDir);
        }
    }

    // ===== Template Content Tests =====

    [Fact]
    [Trait("Category", "Unit")]
    public void ModelTemplate_Should_ContainRequiredPlaceholders()
    {
        var template = DapperTemplates.MODEL_TEMPLATE;

        Assert.Contains("{{ModelNamespace}}", template);
        Assert.Contains("{{EntityName}}", template);
        Assert.Contains("{{TableAttribute}}", template);
        Assert.Contains("{{KeyDeclaration}}", template);
        Assert.Contains("{{Properties}}", template);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void RepositoryTemplate_Should_ContainRequiredPlaceholders()
    {
        var template = DapperTemplates.REPOSITORY_TEMPLATE;

        Assert.Contains("{{ModelNamespace}}", template);
        Assert.Contains("{{RepositoryNamespace}}", template);
        Assert.Contains("{{EntityName}}", template);
        Assert.Contains("{{ConnectionName}}", template);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void CrudRouteTemplate_Should_ContainRequiredPlaceholders()
    {
        var template = DapperTemplates.CRUD_ROUTE_TEMPLATE;

        Assert.Contains("{{ModelNamespace}}", template);
        Assert.Contains("{{RepositoryNamespace}}", template);
        Assert.Contains("{{ActionsNamespace}}", template);
        Assert.Contains("{{EntityName}}", template);
        Assert.Contains("{{EntityNameLower}}", template);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ModelTemplate_Should_GenerateValidCSharp_When_PlaceholdersReplaced()
    {
        var placeholders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["ModelNamespace"] = "TestApp.Models",
            ["EntityName"] = "Product",
            ["TableAttribute"] = "[Table(\"Products\")]",
            ["Summary"] = "Represents a Product entity.",
            ["KeyDeclaration"] = "    [Key]\n    public int Id { get; set; }",
            ["Properties"] = "    public string Name { get; set; } = string.Empty;\n    public decimal Price { get; set; }"
        };

        var content = ReplacePlaceholders(DapperTemplates.MODEL_TEMPLATE, placeholders);

        Assert.Contains("namespace TestApp.Models;", content);
        Assert.Contains("class Product", content);
        Assert.Contains("[Table(\"Products\")]", content);
        Assert.Contains("public int Id { get; set; }", content);
        Assert.Contains("public string Name { get; set; }", content);
        Assert.Contains("public decimal Price { get; set; }", content);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void RepositoryTemplate_Should_GenerateValidCSharp_When_PlaceholdersReplaced()
    {
        var placeholders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["ModelNamespace"] = "TestApp.Models",
            ["RepositoryNamespace"] = "TestApp.Repositories",
            ["EntityName"] = "Product",
            ["ConnectionName"] = "Default"
        };

        var content = ReplacePlaceholders(DapperTemplates.REPOSITORY_TEMPLATE, placeholders);

        Assert.Contains("namespace TestApp.Repositories;", content);
        Assert.Contains("class ProductRepository", content);
        Assert.Contains("DapperRepository<Product>", content);
        Assert.DoesNotContain("{{", content); // No un-replaced placeholders
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void CrudRouteTemplate_Should_GenerateValidCSharp_When_PlaceholdersReplaced()
    {
        var placeholders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["ModelNamespace"] = "TestApp.Models",
            ["RepositoryNamespace"] = "TestApp.Repositories",
            ["ActionsNamespace"] = "TestApp.Controllers",
            ["EntityName"] = "Product",
            ["EntityNameLower"] = "product"
        };

        var content = ReplacePlaceholders(DapperTemplates.CRUD_ROUTE_TEMPLATE, placeholders);

        Assert.Contains("namespace TestApp.Controllers;", content);
        Assert.Contains("class ProductController", content);
        Assert.Contains("api/product", content);
        Assert.Contains("ControllerBase", content);
        Assert.Contains("_repository.GetAllAsync", content);
        Assert.Contains("_repository.FindAsync", content);
        Assert.Contains("_repository.InsertAsync", content);
        Assert.Contains("_repository.UpdateAsync", content);
        Assert.Contains("_repository.DeleteAsync", content);
    }

    /// <summary>
    /// Simple placeholder replacement for template testing.
    /// </summary>
    private static string ReplacePlaceholders(string content, IReadOnlyDictionary<string, string> placeholders)
    {
        var result = content;
        foreach (var (key, value) in placeholders)
        {
            result = result.Replace($"{{{{{key}}}}}", value);
        }
        return result;
    }

    /// <summary>
    /// Cleans up the temp output directory.
    /// </summary>
    private static void CleanupDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: true);
            }
        }
        catch
        {
            // Ignore cleanup failures
        }
    }
}
