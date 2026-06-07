namespace NextNet.Templates.Official.Tests.Saas;

using NextNet.TemplateEngine;
using NextNet.TemplateEngine.Variables;
using NextNet.Templates.Exceptions;
using NextNet.Templates.Models;
using NextNet.Templates.Official.Saas;
using NextNet.IO;
using Xunit;
using System.Text;

/// <summary>
/// Tests for the <see cref="SaasTemplateProvider"/> class and the SaaS template generation pipeline.
/// </summary>
public class SaasTemplateProviderTests
{
    /// <summary>
    /// Verifies that <see cref="SaasTemplateProvider.Name"/> returns <c>"saasOfficial"</c>.
    /// </summary>
    [Fact]
    public void Name_Should_Return_SaasOfficial()
    {
        var provider = new SaasTemplateProvider();
        Assert.Equal("saasOfficial", provider.Name);
    }

    /// <summary>
    /// Verifies that <see cref="SaasTemplateProvider.ExistsAsync"/> returns <c>true</c>
    /// when the template name is <c>"saas"</c>.
    /// </summary>
    [Fact]
    public async Task ExistsAsync_Should_ReturnTrue_When_NameIsSaas()
    {
        var provider = new SaasTemplateProvider();
        Assert.True(await provider.ExistsAsync("saas"));
    }

    /// <summary>
    /// Verifies that <see cref="SaasTemplateProvider.ExistsAsync"/> returns <c>false</c>
    /// when the template name is not <c>"saas"</c>.
    /// </summary>
    [Fact]
    public async Task ExistsAsync_Should_ReturnFalse_When_NameIsNotSaas()
    {
        var provider = new SaasTemplateProvider();
        Assert.False(await provider.ExistsAsync("notsaas"));
    }

    /// <summary>
    /// Verifies that <see cref="SaasTemplateProvider.GetManifestAsync"/> returns
    /// a valid SaaS manifest when the template name is <c>"saas"</c>.
    /// </summary>
    [Fact]
    public async Task GetManifestAsync_Should_ReturnSaasManifest()
    {
        var provider = new SaasTemplateProvider();
        var manifest = await provider.GetManifestAsync("saas");

        Assert.Equal("saas", manifest.Name);
        Assert.Equal("1.0.0", manifest.Version);
        Assert.NotNull(manifest.Variables);
    }

    /// <summary>
    /// Verifies that <see cref="SaasTemplateProvider.GetManifestAsync"/> throws
    /// <see cref="TemplateNotFoundException"/> when the template name is not <c>"saas"</c>.
    /// </summary>
    [Fact]
    public async Task GetManifestAsync_Should_Throw_When_NameIsNotSaas()
    {
        var provider = new SaasTemplateProvider();
        await Assert.ThrowsAsync<TemplateNotFoundException>(() => provider.GetManifestAsync("notsaas"));
    }

    /// <summary>
    /// Verifies that <see cref="SaasTemplateProvider.GetFilesAsync"/> returns a non-empty
    /// dictionary containing the expected template files.
    /// </summary>
    [Fact]
    public async Task GetFilesAsync_Should_ReturnNonEmptyFiles()
    {
        var provider = new SaasTemplateProvider();
        var manifest = await provider.GetManifestAsync("saas");
        var files = await provider.GetFilesAsync(manifest);

        Assert.NotEmpty(files);
        Assert.Contains("app/Program.cs", files.Keys);
    }

    /// <summary>
    /// Verifies that the BillingService file is included in the template files.
    /// </summary>
    [Fact]
    public async Task GetFilesAsync_Should_IncludeBillingService()
    {
        var provider = new SaasTemplateProvider();
        var manifest = await provider.GetManifestAsync("saas");
        var files = await provider.GetFilesAsync(manifest);
        Assert.Contains("app/Billing/BillingService.cs", files.Keys);
    }

    /// <summary>
    /// Verifies that manifests tags contain expected values.
    /// </summary>
    [Fact]
    public async Task Manifest_Should_HaveExpectedTags()
    {
        var provider = new SaasTemplateProvider();
        var manifest = await provider.GetManifestAsync("saas");

        Assert.NotNull(manifest.Tags);
        Assert.Contains("saas", manifest.Tags);
        Assert.Contains("multi-tenant", manifest.Tags);
        Assert.Contains("auth", manifest.Tags);
        Assert.Contains("billing", manifest.Tags);
    }

    /// <summary>
    /// Verifies manifest contains the expected template variables.
    /// </summary>
    [Fact]
    public async Task Manifest_Should_HaveExpectedVariables()
    {
        var provider = new SaasTemplateProvider();
        var manifest = await provider.GetManifestAsync("saas");

        Assert.NotNull(manifest.Variables);
        Assert.NotEmpty(manifest.Variables);
        Assert.Contains(manifest.Variables, v => v.Name == "projectName" && v.Required);
        Assert.Contains(manifest.Variables, v => v.Name == "namespaceName" && !v.Required);
        Assert.Contains(manifest.Variables, v => v.Name == "database");
        Assert.Contains(manifest.Variables, v => v.Name == "includeBilling");
    }

    /// <summary>
    /// Verifies the billing feature is declared in the manifest.
    /// </summary>
    [Fact]
    public async Task Manifest_Should_HaveBillingFeature()
    {
        var provider = new SaasTemplateProvider();
        var manifest = await provider.GetManifestAsync("saas");

        Assert.NotNull(manifest.Features);
        Assert.Contains(manifest.Features, f => f.Name == "billing");
    }

    /// <summary>
    /// Verifies that billing file has a conditional and all core files are always present.
    /// </summary>
    [Fact]
    public async Task Manifest_CoreFiles_Should_BeAlwaysPresent()
    {
        var provider = new SaasTemplateProvider();
        var manifest = await provider.GetManifestAsync("saas");

        Assert.NotNull(manifest.Files);
        Assert.Contains(manifest.Files, f => f.SourcePath == "app/Program.cs" && string.IsNullOrEmpty(f.Condition));
        Assert.Contains(manifest.Files, f => f.SourcePath == "app/Models/User.cs" && string.IsNullOrEmpty(f.Condition));
        Assert.Contains(manifest.Files, f => f.SourcePath == "app/Models/Organization.cs" && string.IsNullOrEmpty(f.Condition));
        Assert.Contains(manifest.Files, f => f.SourcePath == "app/Models/Membership.cs" && string.IsNullOrEmpty(f.Condition));
    }

    /// <summary>
    /// Verifies that billing file has a condition.
    /// </summary>
    [Fact]
    public async Task Manifest_BillingService_Should_HaveCondition()
    {
        var provider = new SaasTemplateProvider();
        var manifest = await provider.GetManifestAsync("saas");

        Assert.NotNull(manifest.Files);
        var billingFile = manifest.Files.FirstOrDefault(f => f.SourcePath == "app/Billing/BillingService.cs");
        Assert.NotNull(billingFile);
        Assert.Equal("includeBilling == true", billingFile.Condition);
    }

    /// <summary>
    /// Integration test that exercises the full generation pipeline:
    /// provider → manifest → files → template engine → variable substitution → output.
    /// </summary>
    [Fact]
    public async Task FullPipeline_Should_GenerateSaasProject()
    {
        var provider = new SaasTemplateProvider();
        var manifest = await provider.GetManifestAsync("saas");
        var files = await provider.GetFilesAsync(manifest);
        var package = new TemplatePackage(manifest, files);

        var variables = VariableContext.CreateBuilder()
            .Set("projectName", "MyStartup")
            .Set("namespaceName", "MyStartup")
            .Set("database", "sqlite")
            .Set("includeBilling", false)
            .Build();

        var fs = new InMemoryFileSystem();
        var engine = new TemplateEngine(fs);
        var result = await engine.GenerateAsync(package, variables, "MyStartup.App");

        var errorMessages = result.Errors?.Select(e => $"[{e.Stage}] {e.File}: {e.Message}") ?? Enumerable.Empty<string>();
        Assert.True(result.Success, $"Generation failed: {string.Join(", ", errorMessages)}");
        Assert.NotEmpty(result.GeneratedFiles);
    }
}

/// <summary>
/// Minimal in-memory implementation of <see cref="ISharpFileSystem"/> for testing.
/// </summary>
internal sealed class InMemoryFileSystem : ISharpFileSystem
{
    private readonly Dictionary<string, byte[]> _files = new();
    private readonly HashSet<string> _dirs = new();

    /// <inheritdoc />
    public bool FileExists(string path) => _files.ContainsKey(path);

    /// <inheritdoc />
    public string ReadAllText(string path) => Encoding.UTF8.GetString(_files[path]);

    /// <inheritdoc />
    public Task<string> ReadAllTextAsync(string path) => Task.FromResult(ReadAllText(path));

    /// <inheritdoc />
    public void WriteAllText(string path, string content) => WriteAllBytes(path, Encoding.UTF8.GetBytes(content));

    /// <inheritdoc />
    public Task WriteAllTextAsync(string path, string content)
    {
        WriteAllText(path, content);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public IEnumerable<string> EnumerateFiles(string directory, string pattern)
    {
        // Normalize the directory to ensure it ends with '/'
        var prefix = directory.EndsWith('/') ? directory : directory + "/";
        return _files.Keys.Where(k => k.StartsWith(prefix));
    }

    /// <inheritdoc />
    public IEnumerable<string> EnumerateDirectories(string directory)
    {
        var prefix = directory.EndsWith('/') ? directory : directory + "/";
        return _dirs.Where(d => d.StartsWith(prefix));
    }

    /// <inheritdoc />
    public bool DirectoryExists(string path)
    {
        return _dirs.Contains(path) || _files.Keys.Any(k =>
        {
            var dir = System.IO.Path.GetDirectoryName(k);
            return dir == path || (dir?.StartsWith(path) ?? false);
        });
    }

    /// <inheritdoc />
    public void CreateDirectory(string path)
    {
        while (!string.IsNullOrEmpty(path))
        {
            _dirs.Add(path);
            path = System.IO.Path.GetDirectoryName(path) ?? "";
        }
    }

    /// <inheritdoc />
    public string GetFullPath(string path) => System.IO.Path.GetFullPath(path);

    /// <inheritdoc />
    public string? GetDirectoryName(string? path) => System.IO.Path.GetDirectoryName(path);

    /// <inheritdoc />
    public string GetFileName(string path) => System.IO.Path.GetFileName(path);

    /// <inheritdoc />
    public string GetFileNameWithoutExtension(string path) => System.IO.Path.GetFileNameWithoutExtension(path);

    /// <inheritdoc />
    public string Combine(params string[] paths) => System.IO.Path.Combine(paths);

    /// <inheritdoc />
    public Task WriteAllBytesAsync(string path, byte[] content, CancellationToken cancellationToken = default)
    {
        WriteAllBytes(path, content);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public void DeleteDirectory(string path, bool recursive = true)
    {
        _dirs.RemoveWhere(d => d.StartsWith(path));
        var keysToRemove = _files.Keys.Where(k => k.StartsWith(path)).ToList();
        foreach (var key in keysToRemove)
        {
            _files.Remove(key);
        }
    }

    private void WriteAllBytes(string path, byte[] bytes)
    {
        var dir = System.IO.Path.GetDirectoryName(path);
        if (dir != null)
        {
            _dirs.Add(dir);
        }
        _files[path] = bytes;
    }
}
