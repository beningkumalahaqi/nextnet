namespace NextNet.Templates.Official.Tests.Dashboard;

using NextNet.TemplateEngine;
using NextNet.TemplateEngine.Variables;
using NextNet.Templates.Exceptions;
using NextNet.Templates.Models;
using NextNet.Templates.Official.Dashboard;
using NextNet.IO;
using Xunit;
using System.Text;

/// <summary>
/// Tests for the <see cref="DashboardTemplateProvider"/> class and the Dashboard template
/// generation pipeline.
/// </summary>
public class DashboardTemplateProviderTests
{
    /// <summary>
    /// Verifies that <see cref="DashboardTemplateProvider.Name"/> returns <c>"dashboard-official"</c>.
    /// </summary>
    [Fact]
    public void Name_Should_Return_DashboardOfficial()
    {
        var provider = new DashboardTemplateProvider();
        Assert.Equal("dashboard-official", provider.Name);
    }

    /// <summary>
    /// Verifies that <see cref="DashboardTemplateProvider.GetManifestAsync"/> returns
    /// a valid Dashboard manifest when the template name is <c>"dashboard"</c>.
    /// </summary>
    [Fact]
    public async Task GetManifestAsync_Should_ReturnDashboardManifest_When_NameIsDashboard()
    {
        var provider = new DashboardTemplateProvider();
        var manifest = await provider.GetManifestAsync("dashboard");

        Assert.Equal("dashboard", manifest.Name);
        Assert.Equal("1.0.0", manifest.Version);
        Assert.Equal(">=3.0.0", manifest.NextNetVersion);
        Assert.NotNull(manifest.Variables);
    }

    /// <summary>
    /// Verifies that <see cref="DashboardTemplateProvider.GetManifestAsync"/> throws
    /// <see cref="TemplateNotFoundException"/> when the template name is not <c>"dashboard"</c>.
    /// </summary>
    [Fact]
    public async Task GetManifestAsync_Should_Throw_When_NameIsNotDashboard()
    {
        var provider = new DashboardTemplateProvider();
        await Assert.ThrowsAsync<TemplateNotFoundException>(() => provider.GetManifestAsync("notdashboard"));
    }

    /// <summary>
    /// Verifies that <see cref="DashboardTemplateProvider.ExistsAsync"/> returns <c>true</c>
    /// when the template name is <c>"dashboard"</c>.
    /// </summary>
    [Fact]
    public async Task ExistsAsync_Should_ReturnTrue_When_NameIsDashboard()
    {
        var provider = new DashboardTemplateProvider();
        Assert.True(await provider.ExistsAsync("dashboard"));
    }

    /// <summary>
    /// Verifies that <see cref="DashboardTemplateProvider.ExistsAsync"/> returns <c>false</c>
    /// when the template name is not <c>"dashboard"</c>.
    /// </summary>
    [Fact]
    public async Task ExistsAsync_Should_ReturnFalse_When_NameIsNotDashboard()
    {
        var provider = new DashboardTemplateProvider();
        Assert.False(await provider.ExistsAsync("xxx"));
    }

    /// <summary>
    /// Verifies that <see cref="DashboardTemplateProvider.GetFilesAsync"/> returns all
    /// expected dashboard template files.
    /// </summary>
    [Fact]
    public async Task GetFilesAsync_Should_IncludeAllRequiredFiles()
    {
        var provider = new DashboardTemplateProvider();
        var manifest = await provider.GetManifestAsync("dashboard");
        var files = await provider.GetFilesAsync(manifest);

        Assert.Contains("app/Program.cs", files.Keys);
        Assert.Contains("app/Services/AuthService.cs", files.Keys);
        Assert.Contains("app/Services/IAuthService.cs", files.Keys);
        Assert.Contains("app/layouts/dashboard.cs", files.Keys);
        Assert.Contains("app/pages/dashboard/index.cs", files.Keys);
        Assert.Contains("app/pages/login.cs", files.Keys);
        Assert.Contains("app/pages/logout.cs", files.Keys);
        Assert.Contains("app/Models/User.cs", files.Keys);
        Assert.Contains("app/wwwroot/css/dashboard.css", files.Keys);
    }

    /// <summary>
    /// Verifies that the manifest has all required metadata fields.
    /// </summary>
    [Fact]
    public async Task GetManifestAsync_Should_Have_ManifestMetadata()
    {
        var provider = new DashboardTemplateProvider();
        var manifest = await provider.GetManifestAsync("dashboard");

        Assert.Equal("dashboard", manifest.Name);
        Assert.Equal("1.0.0", manifest.Version);
        Assert.Equal(">=3.0.0", manifest.NextNetVersion);
        Assert.Equal("NextNet Team", manifest.Author);
        Assert.NotNull(manifest.Description);
        Assert.NotNull(manifest.Tags);
        Assert.Contains("dashboard", manifest.Tags!);
        Assert.Contains("auth", manifest.Tags!);
        Assert.Contains("admin", manifest.Tags!);
    }

    /// <summary>
    /// Verifies that the manifest includes the expected variables.
    /// </summary>
    [Fact]
    public async Task GetManifestAsync_Should_IncludeExpectedVariables()
    {
        var provider = new DashboardTemplateProvider();
        var manifest = await provider.GetManifestAsync("dashboard");

        Assert.NotNull(manifest.Variables);

        var projectNameVar = manifest.Variables!.FirstOrDefault(v => v.Name == "projectName");
        Assert.NotNull(projectNameVar);
        Assert.True(projectNameVar.Required);
        Assert.Equal("string", projectNameVar.Type);

        var appTitleVar = manifest.Variables!.FirstOrDefault(v => v.Name == "appTitle");
        Assert.NotNull(appTitleVar);
        Assert.False(appTitleVar.Required);
        Assert.Equal("string", appTitleVar.Type);

        var primaryColorVar = manifest.Variables!.FirstOrDefault(v => v.Name == "primaryColor");
        Assert.NotNull(primaryColorVar);
        Assert.Equal("enum", primaryColorVar.Type);
        Assert.NotNull(primaryColorVar.AllowedValues);
        Assert.Contains("blue", primaryColorVar.AllowedValues!);
        Assert.Contains("green", primaryColorVar.AllowedValues!);
        Assert.Contains("purple", primaryColorVar.AllowedValues!);
        Assert.Contains("red", primaryColorVar.AllowedValues!);
    }

    /// <summary>
    /// Verifies that <see cref="DashboardTemplateProvider.GetFilesAsync"/> returns exactly 11 files.
    /// </summary>
    [Fact]
    public async Task GetFilesAsync_Should_Return_11_Files()
    {
        var provider = new DashboardTemplateProvider();
        var manifest = await provider.GetManifestAsync("dashboard");
        var files = await provider.GetFilesAsync(manifest);
        Assert.Equal(11, files.Count);
    }

    /// <summary>
    /// Verifies that the template.json file content has the expected structure.
    /// </summary>
    [Fact]
    public async Task GetFilesAsync_Should_Have_ValidManifestJson()
    {
        var provider = new DashboardTemplateProvider();
        var manifest = await provider.GetManifestAsync("dashboard");
        var files = await provider.GetFilesAsync(manifest);

        Assert.True(files.ContainsKey("template.json"), "template.json should exist");
        var jsonContent = Encoding.UTF8.GetString(files["template.json"]);
        Assert.Contains("\"name\": \"dashboard\"", jsonContent);
        Assert.Contains("\"version\": \"1.0.0\"", jsonContent);
        Assert.Contains("\"nextnetVersion\": \">=3.0.0\"", jsonContent);
    }

    /// <summary>
    /// Verifies that the manifest file entries have no conditions (all unconditional).
    /// </summary>
    [Fact]
    public async Task GetManifestAsync_Should_Have_NoConditions_OnFiles()
    {
        var provider = new DashboardTemplateProvider();
        var manifest = await provider.GetManifestAsync("dashboard");

        Assert.NotNull(manifest.Files);
        foreach (var file in manifest.Files!)
        {
            Assert.Null(file.Condition);
        }
    }

    /// <summary>
    /// Integration test that exercises the full generation pipeline:
    /// provider → manifest → files → template engine → variable substitution → output.
    /// </summary>
    [Fact]
    public async Task FullPipeline_Should_GenerateDashboardProject()
    {
        var provider = new DashboardTemplateProvider();
        var manifest = await provider.GetManifestAsync("dashboard");
        var files = await provider.GetFilesAsync(manifest);
        var package = new TemplatePackage(manifest, files);

        var variables = VariableContext.CreateBuilder()
            .Set("projectName", "MyDashboard")
            .Set("appTitle", "Admin Panel")
            .Set("primaryColor", "purple")
            .Build();

        var fs = new InMemoryFileSystem();
        var engine = new TemplateEngine(fs);
        var result = await engine.GenerateAsync(package, variables, "MyDashboard.App");

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
