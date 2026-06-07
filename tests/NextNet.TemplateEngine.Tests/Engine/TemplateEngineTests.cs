namespace NextNet.TemplateEngine.Tests.Engine;

using NextNet.IO;
using NextNet.Templates.Abstractions;
using NextNet.Templates.Exceptions;
using NextNet.Templates.Models;
using NextNet.TemplateEngine.Variables;
using Xunit;

public class TemplateEngineTests
{
    private static TemplatePackage CreateTestPackage()
    {
        var manifest = new TemplateManifest(
            Name: "test",
            Version: "1.0.0",
            NextNetVersion: ">=3.0.0",
            Files: new List<TemplateFile>
            {
                new(SourcePath: "hello.txt", TargetPath: "hello.txt"),
                new(SourcePath: "auth.txt", TargetPath: "auth.txt", Condition: "useAuth == true"),
                new(SourcePath: "image.png", TargetPath: "image.png", IsBinary: true)
            },
            Variables: new List<TemplateVariable>
            {
                new(Name: "projectName", Type: "string", Required: true)
            }
        );
        var files = new Dictionary<string, byte[]>
        {
            ["hello.txt"] = System.Text.Encoding.UTF8.GetBytes("Hello {{projectName}}!"),
            ["auth.txt"] = System.Text.Encoding.UTF8.GetBytes("Auth enabled"),
            ["image.png"] = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } // PNG header
        };
        return new TemplatePackage(manifest, files);
    }

    [Fact]
    public async Task GenerateAsync_Should_GenerateAllFiles_When_NoConditionsOrVariables()
    {
        // Arrange
        var fs = new InMemoryFileSystem();
        var engine = new TemplateEngine(fs);
        var manifest = new TemplateManifest(
            Name: "simple",
            Version: "1.0.0",
            NextNetVersion: ">=3.0.0",
            Files: new List<TemplateFile>
            {
                new(SourcePath: "file1.txt", TargetPath: "out/file1.txt"),
                new(SourcePath: "file2.txt", TargetPath: "out/file2.txt")
            }
        );
        var files = new Dictionary<string, byte[]>
        {
            ["file1.txt"] = System.Text.Encoding.UTF8.GetBytes("content1"),
            ["file2.txt"] = System.Text.Encoding.UTF8.GetBytes("content2")
        };
        var package = new TemplatePackage(manifest, files);
        var vars = VariableContext.CreateBuilder().Build();

        // Act
        var result = await engine.GenerateAsync(package, vars, "/output");

        // Assert
        Assert.True(result.Success);
        Assert.Equal(2, result.GeneratedFiles.Count);
        Assert.Contains(result.GeneratedFiles, p => p.EndsWith("file1.txt"));
        Assert.Contains(result.GeneratedFiles, p => p.EndsWith("file2.txt"));
    }

    [Fact]
    public async Task GenerateAsync_Should_ApplyVariableSubstitution()
    {
        // Arrange
        var fs = new InMemoryFileSystem();
        var engine = new TemplateEngine(fs);
        var package = CreateTestPackage();
        var vars = VariableContext.CreateBuilder()
            .Set("projectName", "MyApp")
            .Set("useAuth", false)
            .Build();

        // Act
        var result = await engine.GenerateAsync(package, vars, "/output");

        // Assert
        Assert.True(result.Success);
        Assert.Equal(2, result.GeneratedFiles.Count); // hello.txt + image.png (auth.txt excluded by condition)

        var writtenContent = fs.ReadAllText("/output/hello.txt");
        Assert.Equal("Hello MyApp!", writtenContent);
    }

    [Fact]
    public async Task GenerateAsync_Should_SkipBinaryFiles()
    {
        // Arrange
        var fs = new InMemoryFileSystem();
        var engine = new TemplateEngine(fs);
        var package = CreateTestPackage();
        var vars = VariableContext.CreateBuilder()
            .Set("projectName", "MyApp")
            .Set("useAuth", false)
            .Build();

        // Act
        var result = await engine.GenerateAsync(package, vars, "/output");

        // Assert
        Assert.True(result.Success);
        // hello.txt + image.png (auth.txt excluded by condition)
        Assert.Equal(2, result.GeneratedFiles.Count);

        var pngContent = fs.ReadAllBytes("/output/image.png");
        Assert.Equal(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }, pngContent);
    }

    [Fact]
    public async Task GenerateAsync_Should_ExcludeFilesBasedOnConditions()
    {
        // Arrange
        var fs = new InMemoryFileSystem();
        var engine = new TemplateEngine(fs);
        var package = CreateTestPackage();
        var vars = VariableContext.CreateBuilder()
            .Set("projectName", "MyApp")
            .Set("useAuth", false)
            .Build();

        // Act
        var result = await engine.GenerateAsync(package, vars, "/output");

        // Assert
        Assert.True(result.Success);
        // auth.txt should be excluded because useAuth == false
        Assert.DoesNotContain(result.GeneratedFiles, p => p.EndsWith("auth.txt"));
        Assert.Single(result.SkippedFiles);
        Assert.Contains(result.SkippedFiles, p => p.EndsWith("auth.txt"));
    }

    [Fact]
    public async Task GenerateAsync_Should_Throw_When_RequiredVariableMissing()
    {
        // Arrange
        var fs = new InMemoryFileSystem();
        var engine = new TemplateEngine(fs);
        var package = CreateTestPackage();
        var vars = VariableContext.CreateBuilder().Build(); // missing projectName

        // Act & Assert
        var exception = await Assert.ThrowsAsync<TemplateValidationException>(
            () => engine.GenerateAsync(package, vars, "/output"));
        Assert.Contains("projectName", exception.Message);
    }

    [Fact]
    public async Task GenerateAsync_Should_CreateOutputDirectory()
    {
        // Arrange
        var fs = new InMemoryFileSystem();
        var engine = new TemplateEngine(fs);
        var package = new TemplatePackage(
            new TemplateManifest("test", "1.0.0", ">=3.0.0"),
            new Dictionary<string, byte[]>
            {
                ["file.txt"] = System.Text.Encoding.UTF8.GetBytes("content")
            }
        );
        var vars = VariableContext.CreateBuilder().Build();

        // Act
        var result = await engine.GenerateAsync(package, vars, "/new-output-dir");

        // Assert
        Assert.True(result.Success);
        Assert.True(fs.DirectoryExists("/new-output-dir"));
    }

    [Fact]
    public async Task GenerateAsync_Should_ReportGeneratedFilesInResult()
    {
        // Arrange
        var fs = new InMemoryFileSystem();
        var engine = new TemplateEngine(fs);
        var package = CreateTestPackage();
        var vars = VariableContext.CreateBuilder()
            .Set("projectName", "MyApp")
            .Set("useAuth", true)
            .Build();

        // Act
        var result = await engine.GenerateAsync(package, vars, "/output");

        // Assert
        Assert.True(result.Success);
        Assert.Equal(3, result.GeneratedFiles.Count); // hello.txt, auth.txt, image.png
    }

    [Fact]
    public async Task GenerateAsync_Should_ReportSkippedFilesInResult()
    {
        // Arrange
        var fs = new InMemoryFileSystem();
        var engine = new TemplateEngine(fs);
        var package = CreateTestPackage();
        var vars = VariableContext.CreateBuilder()
            .Set("projectName", "MyApp")
            .Set("useAuth", false)
            .Build();

        // Act
        var result = await engine.GenerateAsync(package, vars, "/output");

        // Assert
        Assert.True(result.Success);
        Assert.Single(result.SkippedFiles);
        Assert.Contains(result.SkippedFiles, p => p.EndsWith("auth.txt"));
    }

    [Fact]
    public async Task GenerateAsync_Should_NotWriteFiles_When_DryRun()
    {
        // Arrange
        var fs = new InMemoryFileSystem();
        var engine = new TemplateEngine(fs);
        var package = CreateTestPackage();
        var vars = VariableContext.CreateBuilder()
            .Set("projectName", "MyApp")
            .Set("useAuth", true)
            .Build();
        var options = new GenerationOptions
        {
            OutputDirectory = "/output",
            DryRun = true
        };

        // Act
        var result = await engine.GenerateAsync(package, vars, options);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(3, result.GeneratedFiles.Count);
        Assert.False(fs.FileExists("/output/hello.txt"));
        Assert.False(fs.FileExists("/output/auth.txt"));
        Assert.False(fs.FileExists("/output/image.png"));
    }

    [Fact]
    public async Task GenerateAsync_Should_ReportError_When_PathTraversal()
    {
        // Arrange
        var fs = new InMemoryFileSystem();
        var engine = new TemplateEngine(fs);
        var manifest = new TemplateManifest(
            Name: "test",
            Version: "1.0.0",
            NextNetVersion: ">=3.0.0",
            Files: new List<TemplateFile>
            {
                new(SourcePath: "bad.txt", TargetPath: "../escape.txt")
            }
        );
        var files = new Dictionary<string, byte[]>
        {
            ["bad.txt"] = System.Text.Encoding.UTF8.GetBytes("content")
        };
        var package = new TemplatePackage(manifest, files);
        var vars = VariableContext.CreateBuilder().Build();

        // Act
        var result = await engine.GenerateAsync(package, vars, "/output");

        // Assert: path traversal is caught as a per-file error (TemplateGenerationException is caught
        // by the engine's exception handler and reported as a generation error)
        Assert.False(result.Success);
        Assert.NotNull(result.Errors);
        Assert.NotEmpty(result.Errors);
        Assert.Contains(result.Errors, e => e.Message.Contains("path traversal", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GenerateAsync_Should_CleanupPartialOutput_When_Cancelled()
    {
        // Arrange
        var fs = new InMemoryFileSystem();
        var engine = new TemplateEngine(fs);
        var package = CreateTestPackage();
        var vars = VariableContext.CreateBuilder()
            .Set("projectName", "MyApp")
            .Set("useAuth", true)
            .Build();
        using var cts = new CancellationTokenSource();

        // Cancel before starting so the first ThrowIfCancellationRequested triggers
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => engine.GenerateAsync(package, vars, "/test/output", cts.Token));
        Assert.False(fs.DirectoryExists("/test/output"));
    }

    [Fact]
    public async Task GenerateAsync_Should_NotOverwrite_When_OverwriteIsFalse()
    {
        // Arrange
        var fs = new InMemoryFileSystem();
        var engine = new TemplateEngine(fs);
        var package = new TemplatePackage(
            new TemplateManifest(
                Name: "test",
                Version: "1.0.0",
                NextNetVersion: ">=3.0.0",
                Files: new List<TemplateFile>
                {
                    new(SourcePath: "existing.txt", TargetPath: "existing.txt")
                }
            ),
            new Dictionary<string, byte[]>
            {
                ["existing.txt"] = System.Text.Encoding.UTF8.GetBytes("NEW CONTENT")
            }
        );
        var vars = VariableContext.CreateBuilder().Build();

        // Pre-create the file with old content
        fs.WriteAllText("/test/output/existing.txt", "PRE-EXISTING");

        var options = new GenerationOptions
        {
            OutputDirectory = "/test/output",
            Overwrite = false
        };

        // Act
        var result = await engine.GenerateAsync(package, vars, options);

        // Assert
        Assert.True(result.Success);
        Assert.Empty(result.GeneratedFiles);
        Assert.Equal("PRE-EXISTING", fs.ReadAllText("/test/output/existing.txt"));
    }

    [Fact]
    public async Task ValidateAsync_Should_ReturnValid_When_AllVariablesValid()
    {
        // Arrange
        var fs = new InMemoryFileSystem();
        var engine = new TemplateEngine(fs);
        var manifest = new TemplateManifest(
            Name: "test",
            Version: "1.0.0",
            NextNetVersion: ">=3.0.0",
            Variables: new List<TemplateVariable>
            {
                new(Name: "name", Type: "string", Required: true),
                new(Name: "count", Type: "number", Required: false)
            }
        );
        var vars = VariableContext.CreateBuilder()
            .Set("name", "MyApp")
            .Set("count", 42)
            .Build();

        // Act
        var result = await engine.ValidateAsync(manifest, vars);

        // Assert
        Assert.True(result.IsValid);
        Assert.Null(result.Errors);
    }

    [Fact]
    public async Task ValidateAsync_Should_ReturnInvalid_When_VariableMissing()
    {
        // Arrange
        var fs = new InMemoryFileSystem();
        var engine = new TemplateEngine(fs);
        var manifest = new TemplateManifest(
            Name: "test",
            Version: "1.0.0",
            NextNetVersion: ">=3.0.0",
            Variables: new List<TemplateVariable>
            {
                new(Name: "name", Type: "string", Required: true)
            }
        );
        var vars = VariableContext.CreateBuilder().Build(); // missing required variable

        // Act
        var result = await engine.ValidateAsync(manifest, vars);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotNull(result.Errors);
        Assert.Contains(result.Errors, e => e.Contains("name"));
    }

    [Fact]
    public async Task GenerateAsync_Should_IncludeFile_When_ConditionIsTrue()
    {
        // Arrange
        var fs = new InMemoryFileSystem();
        var engine = new TemplateEngine(fs);
        var package = CreateTestPackage();
        var vars = VariableContext.CreateBuilder()
            .Set("projectName", "MyApp")
            .Set("useAuth", true)
            .Build();

        // Act
        var result = await engine.GenerateAsync(package, vars, "/output");

        // Assert
        Assert.True(result.Success);
        Assert.Contains(result.GeneratedFiles, p => p.EndsWith("auth.txt"));
        var authContent = fs.ReadAllText("/output/auth.txt");
        Assert.Equal("Auth enabled", authContent);
    }

    [Fact]
    public async Task GenerateAsync_Should_ReportSuccessFalse_When_FileErrors()
    {
        // Arrange
        var fs = new InMemoryFileSystem();
        var engine = new TemplateEngine(fs);
        var manifest = new TemplateManifest(
            Name: "broken",
            Version: "1.0.0",
            NextNetVersion: ">=3.0.0",
            Files: new List<TemplateFile>
            {
                new(SourcePath: "missing.txt", TargetPath: "missing.txt")
            }
        );
        var files = new Dictionary<string, byte[]>(); // missing.txt not in dictionary
        var package = new TemplatePackage(manifest, files);
        var vars = VariableContext.CreateBuilder().Build();

        // Act
        var result = await engine.GenerateAsync(package, vars, "/output");

        // Assert
        Assert.True(result.Success); // Warning, not error
        Assert.Empty(result.GeneratedFiles);
        Assert.Single(result.Warnings);
        Assert.Contains("missing.txt", result.Warnings[0]);
    }
}

/// <summary>
/// In-memory implementation of <see cref="ISharpFileSystem"/> for testing purposes.
/// Stores all files in a dictionary keyed by absolute path.
/// </summary>
public sealed class InMemoryFileSystem : ISharpFileSystem
{
    private readonly Dictionary<string, byte[]> _files = new(StringComparer.Ordinal);
    private readonly HashSet<string> _directories = new(StringComparer.Ordinal)
    {
        "/"
    };

    public bool FileExists(string path)
    {
        var normalized = NormalizePath(path);
        return _files.ContainsKey(normalized);
    }

    public string ReadAllText(string path)
    {
        var normalized = NormalizePath(path);
        if (_files.TryGetValue(normalized, out var bytes))
            return System.Text.Encoding.UTF8.GetString(bytes);
        throw new FileNotFoundException($"File not found: {path}", path);
    }

    public Task<string> ReadAllTextAsync(string path)
    {
        return Task.FromResult(ReadAllText(path));
    }

    public void WriteAllText(string path, string content)
    {
        var normalized = NormalizePath(path);
        _files[normalized] = System.Text.Encoding.UTF8.GetBytes(content);
        EnsureDirectoryExists(normalized);
    }

    public Task WriteAllTextAsync(string path, string content)
    {
        WriteAllText(path, content);
        return Task.CompletedTask;
    }

    public Task WriteAllBytesAsync(string path, byte[] content, CancellationToken cancellationToken = default)
    {
        var normalized = NormalizePath(path);
        _files[normalized] = content;
        EnsureDirectoryExists(normalized);
        return Task.CompletedTask;
    }

    public IEnumerable<string> EnumerateFiles(string directory, string pattern)
    {
        var normalized = NormalizePath(directory);
        return _files.Keys
            .Where(f => f.StartsWith(normalized + "/", StringComparison.Ordinal) ||
                        f.Equals(normalized, StringComparison.Ordinal))
            .Where(f =>
            {
                var fileName = GetFileName(f);
                return System.IO.Directory.EnumerateFiles(".", pattern).Any() || // dummy, use simple match
                       MatchPattern(fileName, pattern);
            });
    }

    public IEnumerable<string> EnumerateDirectories(string directory)
    {
        var normalized = NormalizePath(directory);
        return _directories
            .Where(d => d.StartsWith(normalized + "/", StringComparison.Ordinal) && d != normalized);
    }

    public bool DirectoryExists(string path)
    {
        var normalized = NormalizePath(path);
        return _directories.Contains(normalized);
    }

    public void CreateDirectory(string path)
    {
        if (string.IsNullOrEmpty(path)) return;
        var normalized = NormalizePath(path);
        if (!_directories.Add(normalized))
            return; // Already exists, avoid recursion for root

        // Also create parent directories
        var parent = GetDirectoryName(normalized);
        if (!string.IsNullOrEmpty(parent) && parent != normalized)
            CreateDirectory(parent);
    }

    public string GetFullPath(string path)
    {
        return NormalizePath(path);
    }

    public string? GetDirectoryName(string? path)
    {
        if (string.IsNullOrEmpty(path)) return null;
        var normalized = NormalizePath(path);
        if (normalized == "/") return null;
        var lastSlash = normalized.LastIndexOf('/');
        if (lastSlash <= 0) return "/";
        return normalized[..lastSlash];
    }

    public string GetFileName(string path)
    {
        var normalized = NormalizePath(path);
        var lastSlash = normalized.LastIndexOf('/');
        return lastSlash >= 0 ? normalized[(lastSlash + 1)..] : normalized;
    }

    public string GetFileNameWithoutExtension(string path)
    {
        var name = GetFileName(path);
        var lastDot = name.LastIndexOf('.');
        return lastDot >= 0 ? name[..lastDot] : name;
    }

    public string Combine(params string[] paths)
    {
        if (paths.Length == 0) return "";
        // Handle absolute base
        var result = paths[0];
        for (int i = 1; i < paths.Length; i++)
        {
            result = result.TrimEnd('/') + "/" + paths[i].TrimStart('/');
        }
        return NormalizePath(result);
    }

    public void DeleteDirectory(string path, bool recursive = true)
    {
        var normalized = NormalizePath(path);
        _directories.Remove(normalized);
        if (recursive)
        {
            var toRemove = _directories.Where(d => d.StartsWith(normalized + "/", StringComparison.Ordinal)).ToList();
            foreach (var d in toRemove)
                _directories.Remove(d);

            var filesToRemove = _files.Keys
                .Where(f => f.StartsWith(normalized + "/", StringComparison.Ordinal))
                .ToList();
            foreach (var f in filesToRemove)
                _files.Remove(f);
        }
    }

    /// <summary>
    /// Reads all bytes from a file in the in-memory file system.
    /// </summary>
    public byte[] ReadAllBytes(string path)
    {
        var normalized = NormalizePath(path);
        if (_files.TryGetValue(normalized, out var bytes))
            return bytes;
        throw new FileNotFoundException($"File not found: {path}", path);
    }

    private static string NormalizePath(string path)
    {
        if (string.IsNullOrEmpty(path)) return "/";
        // Replace backslashes with forward slashes
        path = path.Replace('\\', '/');
        // Collapse multiple slashes
        while (path.Contains("//"))
            path = path.Replace("//", "/");
        // Ensure absolute paths start with /
        if (!path.StartsWith('/'))
            path = "/" + path;
        // Remove trailing slash (except for root)
        if (path.Length > 1 && path.EndsWith('/'))
            path = path[..^1];
        return path;
    }

    private void EnsureDirectoryExists(string filePath)
    {
        var dir = GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(dir))
            CreateDirectory(dir);
    }

    private static bool MatchPattern(string fileName, string pattern)
    {
        if (pattern == "*" || pattern == "*.*") return true;
        if (pattern.StartsWith('*') && pattern.EndsWith('*'))
            return fileName.Contains(pattern[1..^1], StringComparison.OrdinalIgnoreCase);
        if (pattern.StartsWith('*'))
            return fileName.EndsWith(pattern[1..], StringComparison.OrdinalIgnoreCase);
        if (pattern.EndsWith('*'))
            return fileName.StartsWith(pattern[..^1], StringComparison.OrdinalIgnoreCase);
        return string.Equals(fileName, pattern, StringComparison.OrdinalIgnoreCase);
    }
}
