using NextNet.TemplateSdk;
using Xunit;

namespace NextNet.TemplateSdk.Tests.Scaffolding;

/// <summary>
/// Tests for the <see cref="TemplateScaffolder"/> class.
/// </summary>
public class TemplateScaffolderTests
{
    /// <summary>
    /// Verifies that <see cref="TemplateScaffolder.ScaffoldAsync"/> creates a valid
    /// <c>template.json</c> manifest file with the correct template name.
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task ScaffoldAsync_Should_CreateTemplateJson_When_ValidOptionsProvided()
    {
        var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var scaffolder = new TemplateScaffolder();

        await scaffolder.ScaffoldAsync(dir, new ScaffoldOptions
        {
            Name = "my-template",
            Author = "tester",
            Description = "Test template"
        });

        var manifestPath = Path.Combine(dir, "template.json");
        Assert.True(File.Exists(manifestPath));
        var content = await File.ReadAllTextAsync(manifestPath);
        Assert.Contains("my-template", content);

        // Cleanup
        Directory.Delete(dir, recursive: true);
    }

    /// <summary>
    /// Verifies that <see cref="TemplateScaffolder.ScaffoldAsync"/> creates the
    /// sample <c>files/hello.txt</c> file with a placeholder variable.
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task ScaffoldAsync_Should_CreateSampleFile_When_ScaffoldingTemplate()
    {
        var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var scaffolder = new TemplateScaffolder();

        await scaffolder.ScaffoldAsync(dir, new ScaffoldOptions { Name = "test" });

        var sample = Path.Combine(dir, "files", "hello.txt");
        Assert.True(File.Exists(sample));
        var content = await File.ReadAllTextAsync(sample);
        Assert.Contains("{{projectName}}", content);

        // Cleanup
        Directory.Delete(dir, recursive: true);
    }

    /// <summary>
    /// Verifies that <see cref="TemplateScaffolder.ScaffoldAsync"/> creates a
    /// README.md file with the template name.
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task ScaffoldAsync_Should_CreateReadme_When_ScaffoldingTemplate()
    {
        var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var scaffolder = new TemplateScaffolder();

        await scaffolder.ScaffoldAsync(dir, new ScaffoldOptions
        {
            Name = "my-template",
            Description = "A test template"
        });

        var readmePath = Path.Combine(dir, "README.md");
        Assert.True(File.Exists(readmePath));
        var content = await File.ReadAllTextAsync(readmePath);
        Assert.Contains("my-template", content);
        Assert.Contains("A test template", content);

        // Cleanup
        Directory.Delete(dir, recursive: true);
    }

    /// <summary>
    /// Verifies that <see cref="TemplateScaffolder.ScaffoldAsync"/> throws
    /// when the directory is null.
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task ScaffoldAsync_Should_ThrowArgumentNullException_When_DirectoryIsNull()
    {
        var scaffolder = new TemplateScaffolder();
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            scaffolder.ScaffoldAsync(null!, new ScaffoldOptions { Name = "test" }));
    }

    /// <summary>
    /// Verifies that <see cref="TemplateScaffolder.ScaffoldAsync"/> throws
    /// when options is null.
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task ScaffoldAsync_Should_ThrowArgumentNullException_When_OptionsIsNull()
    {
        var scaffolder = new TemplateScaffolder();
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            scaffolder.ScaffoldAsync("/tmp/test", null!));
    }

    /// <summary>
    /// Verifies that the generated <c>template.json</c> includes the tags array
    /// when provided.
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task ScaffoldAsync_Should_IncludeTags_When_TagsProvided()
    {
        var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var scaffolder = new TemplateScaffolder();

        await scaffolder.ScaffoldAsync(dir, new ScaffoldOptions
        {
            Name = "tagged-template",
            Tags = new[] { "web", "api", "minimal" }
        });

        var content = await File.ReadAllTextAsync(Path.Combine(dir, "template.json"));
        Assert.Contains("web", content);
        Assert.Contains("api", content);

        // Cleanup
        Directory.Delete(dir, recursive: true);
    }
}
