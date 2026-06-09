namespace NextNet.Templates.Official.Tests.Blog;

using NextNet.TemplateEngine;
using NextNet.TemplateEngine.Variables;
using NextNet.Templates.Abstractions;
using NextNet.Templates.Exceptions;
using NextNet.Templates.Models;
using NextNet.Templates.Official.Blog;
using NextNet.IO;
using Xunit;
using System.Text;

/// <summary>
/// Tests for the <see cref="BlogTemplateProvider"/> class and the Blog template
/// generation pipeline.
/// </summary>
public class BlogTemplateProviderTests
{
    /// <summary>
    /// Verifies that <see cref="BlogTemplateProvider.GetManifestAsync"/> returns
    /// a valid Blog manifest when the template name is <c>"blog"</c>.
    /// </summary>
    [Fact]
    public async Task GetManifestAsync_Should_ReturnBlogManifest_When_NameIsBlog()
    {
        var provider = new BlogTemplateProvider();
        var manifest = await provider.GetManifestAsync("blog");

        Assert.Equal("blog", manifest.Name);
        Assert.Equal("1.0.0", manifest.Version);
        Assert.NotNull(manifest.Variables);
    }

    /// <summary>
    /// Verifies that <see cref="BlogTemplateProvider.GetManifestAsync"/> throws
    /// <see cref="TemplateNotFoundException"/> when the template name is not <c>"blog"</c>.
    /// </summary>
    [Fact]
    public async Task GetManifestAsync_Should_Throw_When_NameIsNotBlog()
    {
        var provider = new BlogTemplateProvider();
        await Assert.ThrowsAsync<TemplateNotFoundException>(() => provider.GetManifestAsync("notblog"));
    }

    /// <summary>
    /// Verifies that <see cref="BlogTemplateProvider.ExistsAsync"/> returns <c>true</c>
    /// when the template name is <c>"blog"</c>.
    /// </summary>
    [Fact]
    public async Task ExistsAsync_Should_ReturnTrue_When_NameIsBlog()
    {
        var provider = new BlogTemplateProvider();
        Assert.True(await provider.ExistsAsync("blog"));
    }

    /// <summary>
    /// Verifies that <see cref="BlogTemplateProvider.ExistsAsync"/> returns <c>false</c>
    /// when the template name is not <c>"blog"</c>.
    /// </summary>
    [Fact]
    public async Task ExistsAsync_Should_ReturnFalse_When_NameIsNotBlog()
    {
        var provider = new BlogTemplateProvider();
        Assert.False(await provider.ExistsAsync("xxx"));
    }

    /// <summary>
    /// Verifies that <see cref="BlogTemplateProvider.GetFilesAsync"/> returns a non-empty
    /// dictionary containing the expected template files.
    /// </summary>
    [Fact]
    public async Task GetFilesAsync_Should_ReturnNonEmptyFiles_When_NameIsBlog()
    {
        var provider = new BlogTemplateProvider();
        var manifest = await provider.GetManifestAsync("blog");
        var files = await provider.GetFilesAsync(manifest);

        Assert.NotEmpty(files);
        Assert.Contains("app/Program.cs", files.Keys);
    }

    /// <summary>
    /// Integration test that exercises the full generation pipeline:
    /// provider → manifest → files → template engine → variable substitution → output.
    /// </summary>
    [Fact]
    public async Task FullPipeline_Should_GenerateBlogProject()
    {
        var provider = new BlogTemplateProvider();
        var manifest = await provider.GetManifestAsync("blog");
        var files = await provider.GetFilesAsync(manifest);
        var package = new TemplatePackage(manifest, files);

        var variables = VariableContext.CreateBuilder()
            .Set("projectName", "MyBlog")
            .Set("namespaceName", "MyBlog")
            .Set("authorName", "Jane Doe")
            .Set("baseUrl", "http://localhost:5000")
            .Build();

        var fs = new InMemoryFileSystem();
        var engine = new TemplateEngine(fs);
        var result = await engine.GenerateAsync(package, variables, "MyBlog.App");

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
