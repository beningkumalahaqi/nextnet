namespace NextNet.TemplateEngine.Tests.Variables;

using NextNet.TemplateEngine.Variables;
using Xunit;

public class VariableContextBuilderTests
{
    [Fact]
    public void Set_Should_StoreValue()
    {
        var ctx = VariableContext.CreateBuilder().Set("k", "v").Build();
        Assert.Equal("v", ctx.Get("k"));
    }

    [Fact]
    public void Set_Should_Throw_When_NameIsNull()
    {
        var builder = VariableContext.CreateBuilder();
        Assert.Throws<ArgumentException>(() => builder.Set(null!, "v"));
    }

    [Fact]
    public void Set_Should_Throw_When_NameIsEmpty()
    {
        var builder = VariableContext.CreateBuilder();
        Assert.Throws<ArgumentException>(() => builder.Set("", "v"));
    }

    [Fact]
    public void Set_Should_Throw_When_NameContainsBraces()
    {
        var builder = VariableContext.CreateBuilder();
        Assert.Throws<ArgumentException>(() => builder.Set("bad{name", "v"));
        Assert.Throws<ArgumentException>(() => builder.Set("bad}name", "v"));
    }

    [Fact]
    public void SetNested_Should_FlattenObjectProperties()
    {
        var ctx = VariableContext.CreateBuilder()
            .SetNested("project", new { name = "MyApp", version = "1.0" })
            .Build();
        Assert.Equal("MyApp", ctx.Get("project.name"));
        Assert.Equal("1.0", ctx.Get("project.version"));
    }

    [Fact]
    public void SetMany_Should_AddAllValues()
    {
        var ctx = VariableContext.CreateBuilder()
            .SetMany(new Dictionary<string, object?> { ["a"] = 1, ["b"] = 2 })
            .Build();
        Assert.Equal(1, ctx.Get("a"));
        Assert.Equal(2, ctx.Get("b"));
    }

    [Fact]
    public void Remove_Should_DeleteValue()
    {
        var ctx = VariableContext.CreateBuilder()
            .Set("k", "v")
            .Remove("k")
            .Build();
        Assert.False(ctx.Contains("k"));
    }

    [Fact]
    public void FluentChaining_Should_Work()
    {
        var ctx = VariableContext.CreateBuilder()
            .Set("a", 1)
            .Set("b", 2)
            .Set("c", 3)
            .Build();
        Assert.Equal(3, ctx.Keys.Count());
    }
}
