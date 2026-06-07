namespace NextNet.TemplateEngine.Tests.Conditionals;

using NextNet.TemplateEngine.Conditionals;
using NextNet.TemplateEngine.Variables;
using NextNet.Templates.Models;
using Xunit;

public class ConditionalFileFilterTests
{
    private readonly ConditionalFileFilter _filter = new();

    [Fact]
    public void Filter_Should_IncludeFile_When_NoCondition()
    {
        var files = new[]
        {
            new TemplateFile("src/Program.cs", "Program.cs")
        };

        var ctx = VariableContext.CreateBuilder().Build();
        var result = _filter.Filter(files, ctx);

        Assert.Single(result.Included);
        Assert.Empty(result.Excluded);
    }

    [Fact]
    public void Filter_Should_IncludeFile_When_ConditionTrue()
    {
        var files = new[]
        {
            new TemplateFile("src/Api.cs", "Api.cs", "features.api")
        };

        var ctx = VariableContext.CreateBuilder()
            .SetNested("features", new { api = true })
            .Build();

        var result = _filter.Filter(files, ctx);

        Assert.Single(result.Included);
        Assert.Empty(result.Excluded);
    }

    [Fact]
    public void Filter_Should_ExcludeFile_When_ConditionFalse()
    {
        var files = new[]
        {
            new TemplateFile("src/Api.cs", "Api.cs", "features.api")
        };

        var ctx = VariableContext.CreateBuilder()
            .SetNested("features", new { api = false })
            .Build();

        var result = _filter.Filter(files, ctx);

        Assert.Empty(result.Included);
        Assert.Single(result.Excluded);
        Assert.Equal("features.api", result.Excluded[0].Condition);
    }

    [Fact]
    public void Filter_Should_HandleMultipleFiles()
    {
        var files = new[]
        {
            new TemplateFile("src/Api.cs", "Api.cs", "features.api"),
            new TemplateFile("src/Logging.cs", "Logging.cs", "features.logging"),
            new TemplateFile("src/Program.cs", "Program.cs") // no condition
        };

        var ctx = VariableContext.CreateBuilder()
            .SetNested("features", new { api = true, logging = false })
            .Build();

        var result = _filter.Filter(files, ctx);

        Assert.Equal(2, result.Included.Count); // Api.cs and Program.cs
        Assert.Single(result.Excluded); // Logging.cs
        Assert.Equal("features.logging", result.Excluded[0].Condition);
    }

    [Fact]
    public void Filter_Should_IncludeAllFiles_When_AllConditionsTrue()
    {
        var files = new[]
        {
            new TemplateFile("src/Api.cs", "Api.cs", "true"),
            new TemplateFile("src/Program.cs", "Program.cs"),
            new TemplateFile("src/Test.cs", "Test.cs", "1 == 1")
        };

        var ctx = VariableContext.CreateBuilder().Build();
        var result = _filter.Filter(files, ctx);

        Assert.Equal(3, result.Included.Count);
        Assert.Empty(result.Excluded);
    }

    [Fact]
    public void Filter_Should_ExcludeAllFiles_When_AllConditionsFalse()
    {
        var files = new[]
        {
            new TemplateFile("src/Api.cs", "Api.cs", "false"),
            new TemplateFile("src/Test.cs", "Test.cs", "1 == 2")
        };

        var ctx = VariableContext.CreateBuilder().Build();
        var result = _filter.Filter(files, ctx);

        Assert.Empty(result.Included);
        Assert.Equal(2, result.Excluded.Count);
    }

    [Fact]
    public void Filter_Should_IncludeParseErrorFiles_ByDefault()
    {
        var files = new[]
        {
            new TemplateFile("src/Broken.cs", "Broken.cs", "@@invalid@@"),
            new TemplateFile("src/Good.cs", "Good.cs", "true")
        };

        var ctx = VariableContext.CreateBuilder().Build();
        var result = _filter.Filter(files, ctx);

        Assert.Equal(2, result.Included.Count);
        Assert.Empty(result.Excluded);
    }

    [Fact]
    public void Filter_Should_Throw_When_FilesNull()
    {
        var ctx = VariableContext.CreateBuilder().Build();
        Assert.Throws<ArgumentNullException>(() => _filter.Filter(null!, ctx));
    }

    [Fact]
    public void Filter_Should_Throw_When_ContextNull()
    {
        var files = new[] { new TemplateFile("src/A.cs", "A.cs") };
        Assert.Throws<ArgumentNullException>(() => _filter.Filter(files, null!));
    }
}
