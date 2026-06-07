using NextNet.Cli.Templates;
using Xunit;

namespace NextNet.Cli.Tests.Templates;

public class TemplateManagerTests
{
    [Fact]
    public void GetAvailableTemplates_ReturnsAllTemplates()
    {
        var templates = TemplateManager.GetAvailableTemplates();
        Assert.Contains("fullstack", templates);
        Assert.Contains("minimal", templates);
        Assert.Contains("api", templates);
        Assert.Contains("empty", templates);
    }

    [Fact]
    public void IsKnownTemplate_ReturnsTrue_ForKnownTemplates()
    {
        Assert.True(TemplateManager.IsKnownTemplate("fullstack"));
        Assert.True(TemplateManager.IsKnownTemplate("minimal"));
        Assert.True(TemplateManager.IsKnownTemplate("api"));
        Assert.True(TemplateManager.IsKnownTemplate("empty"));
    }

    [Fact]
    public void IsKnownTemplate_ReturnsFalse_ForUnknown()
    {
        Assert.False(TemplateManager.IsKnownTemplate("nonexistent"));
    }

    [Fact]
    public void IsKnownTemplate_IsCaseInsensitive()
    {
        Assert.True(TemplateManager.IsKnownTemplate("FULLSTACK"));
        Assert.True(TemplateManager.IsKnownTemplate("Minimal"));
    }

    [Fact]
    public void DetectValues_CreatesPascalCase()
    {
        var values = TemplateManager.DetectValues("my-app");
        Assert.Equal("my-app", values.ProjectName);
        Assert.Equal("MyApp", values.ProjectNamePascal);
        Assert.Equal("MyApp", values.Namespace);
    }

    [Fact]
    public void DetectValues_SingleWord()
    {
        var values = TemplateManager.DetectValues("app");
        Assert.Equal("app", values.ProjectName);
        Assert.Equal("App", values.ProjectNamePascal);
    }

    [Fact]
    public void DetectValues_ComplexName()
    {
        var values = TemplateManager.DetectValues("my-cool-project-2");
        Assert.Equal("my-cool-project-2", values.ProjectName);
        Assert.Equal("MyCoolProject2", values.ProjectNamePascal);
    }

    [Fact]
    public void DetectValues_Version_Defaults()
    {
        var values = TemplateManager.DetectValues("test");
        Assert.Equal("1.0.0", values.Version);
        Assert.NotEmpty(values.Date);
    }

    [Fact]
    public void Scaffold_Throws_ForUnknownTemplate()
    {
        var values = TemplateManager.DetectValues("test");
        Assert.Throws<ArgumentException>(() =>
            TemplateManager.Scaffold("nonexistent", "/tmp", values));
    }

    [Fact]
    public void ListFiles_DryRun_DoesNotCreateFiles()
    {
        var testDir = Path.Combine(Path.GetTempPath(), "NextNetCliTests", Guid.NewGuid().ToString());
        try
        {
            var values = TemplateManager.DetectValues("test-app");
            var files = TemplateManager.ListFiles("minimal", testDir, values);
            Assert.NotEmpty(files);
            Assert.False(Directory.Exists(testDir));
        }
        finally
        {
            try { Directory.Delete(testDir, recursive: true); } catch { }
        }
    }
}
