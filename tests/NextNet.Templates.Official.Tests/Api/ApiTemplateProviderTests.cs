namespace NextNet.Templates.Official.Tests.Api;

using NextNet.TemplateEngine;
using NextNet.TemplateEngine.Variables;
using NextNet.Templates.Exceptions;
using NextNet.Templates.Models;
using NextNet.Templates.Official.Api;
using NextNet.IO;
using Xunit;
using System.Text;

/// <summary>
/// Tests for the <see cref="ApiTemplateProvider"/> class and the API template
/// generation pipeline.
/// </summary>
public class ApiTemplateProviderTests
{
    /// <summary>
    /// Verifies that <see cref="ApiTemplateProvider.GetManifestAsync"/> returns
    /// a valid API manifest when the template name is <c>"api"</c>.
    /// </summary>
    [Fact]
    public async Task GetManifestAsync_Should_ReturnApiManifest_When_NameIsApi()
    {
        var provider = new ApiTemplateProvider();
        var manifest = await provider.GetManifestAsync("api");

        Assert.Equal("api", manifest.Name);
        Assert.Contains("openapi", manifest.Tags!);
    }

    /// <summary>
    /// Verifies that <see cref="ApiTemplateProvider.GetManifestAsync"/> throws
    /// <see cref="TemplateNotFoundException"/> when the template name is not <c>"api"</c>.
    /// </summary>
    [Fact]
    public async Task GetManifestAsync_Should_Throw_When_NameIsNotApi()
    {
        var provider = new ApiTemplateProvider();
        await Assert.ThrowsAsync<TemplateNotFoundException>(() => provider.GetManifestAsync("blog"));
    }

    /// <summary>
    /// Verifies that <see cref="ApiTemplateProvider.ExistsAsync"/> returns <c>true</c>
    /// when the template name is <c>"api"</c>.
    /// </summary>
    [Fact]
    public async Task ExistsAsync_Should_ReturnTrue_When_NameIsApi()
    {
        var provider = new ApiTemplateProvider();
        Assert.True(await provider.ExistsAsync("api"));
    }

    /// <summary>
    /// Verifies that <see cref="ApiTemplateProvider.ExistsAsync"/> returns <c>false</c>
    /// when the template name is not <c>"api"</c>.
    /// </summary>
    [Fact]
    public async Task ExistsAsync_Should_ReturnFalse_When_NameIsNotApi()
    {
        var provider = new ApiTemplateProvider();
        Assert.False(await provider.ExistsAsync("blog"));
    }

    /// <summary>
    /// Verifies that <see cref="ApiTemplateProvider.GetFilesAsync"/> returns a dictionary
    /// that includes the expected authentication controller file.
    /// </summary>
    [Fact]
    public async Task GetFilesAsync_Should_IncludeAuthController()
    {
        var provider = new ApiTemplateProvider();
        var manifest = await provider.GetManifestAsync("api");
        var files = await provider.GetFilesAsync(manifest);

        Assert.Contains("app/Auth/AuthController.cs", files.Keys);
    }

    /// <summary>
    /// Verifies that the manifest includes auth files unconditionally (conditions removed).
    /// </summary>
    [Fact]
    public async Task GetManifestAsync_Should_IncludeAuthFiles_Unconditionally()
    {
        var provider = new ApiTemplateProvider();
        var manifest = await provider.GetManifestAsync("api");

        var authFile = manifest.Files!.FirstOrDefault(f => f.SourcePath.Contains("AuthController"));
        Assert.NotNull(authFile);
        Assert.Null(authFile.Condition);
    }

    /// <summary>
    /// Verifies that <see cref="ApiTemplateProvider.GetFilesAsync"/> returns all 14 files.
    /// </summary>
    [Fact]
    public async Task GetFilesAsync_Should_Return_All_14_Files()
    {
        var provider = new ApiTemplateProvider();
        var manifest = await provider.GetManifestAsync("api");
        var files = await provider.GetFilesAsync(manifest);
        Assert.Equal(14, files.Count);
    }

    /// <summary>
    /// Verifies that the AppDbContext file entry has no condition (unconditional).
    /// </summary>
    [Fact]
    public async Task GetManifestAsync_Should_Have_NoCondition_On_AppDbContext()
    {
        var provider = new ApiTemplateProvider();
        var manifest = await provider.GetManifestAsync("api");
        var dbFile = manifest.Files!.FirstOrDefault(f => f.SourcePath.Contains("AppDbContext"));
        Assert.NotNull(dbFile);
        Assert.Null(dbFile.Condition);
    }

    /// <summary>
    /// Verifies that the manifest has all required metadata fields.
    /// </summary>
    [Fact]
    public async Task GetManifestAsync_Should_Have_ManifestMetadata()
    {
        var provider = new ApiTemplateProvider();
        var manifest = await provider.GetManifestAsync("api");
        Assert.Equal("api", manifest.Name);
        Assert.Equal("1.0.0", manifest.Version);
        Assert.Equal(">=3.0.0", manifest.NextNetVersion);
        Assert.Equal("NextNet Team", manifest.Author);
        Assert.NotNull(manifest.Description);
    }

    /// <summary>
    /// Verifies that the database variable is an enum with the expected allowed values.
    /// </summary>
    [Fact]
    public async Task GetManifestAsync_Should_Have_DatabaseEnum_WithAllowedValues()
    {
        var provider = new ApiTemplateProvider();
        var manifest = await provider.GetManifestAsync("api");
        var dbVar = manifest.Variables!.FirstOrDefault(v => v.Name == "database");
        Assert.NotNull(dbVar);
        Assert.Equal("enum", dbVar.Type);
        Assert.NotNull(dbVar.AllowedValues);
        Assert.Contains("sqlite", dbVar.AllowedValues!);
        Assert.Contains("postgres", dbVar.AllowedValues!);
        Assert.Contains("none", dbVar.AllowedValues!);
    }

    /// <summary>
    /// Verifies that the provider name is "api-official".
    /// </summary>
    [Fact]
    public void Name_Should_Return_ApiOfficial()
    {
        var provider = new ApiTemplateProvider();
        Assert.Equal("api-official", provider.Name);
    }

    /// <summary>
    /// Integration test that exercises the full generation pipeline:
    /// provider → manifest → files → template engine → variable substitution → output.
    /// </summary>
    [Fact]
    public async Task FullPipeline_Should_GenerateApiProject()
    {
        var provider = new ApiTemplateProvider();
        var manifest = await provider.GetManifestAsync("api");
        var files = await provider.GetFilesAsync(manifest);
        var package = new TemplatePackage(manifest, files);

        var variables = VariableContext.CreateBuilder()
            .Set("projectName", "MyApi")
            .Set("includeAuth", false)
            .Set("database", "sqlite")
            .Build();

        var fs = new InMemoryFileSystem();
        var engine = new TemplateEngine(fs);
        var result = await engine.GenerateAsync(package, variables, "MyApi.App");

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
