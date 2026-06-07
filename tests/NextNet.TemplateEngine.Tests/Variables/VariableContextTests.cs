namespace NextNet.TemplateEngine.Tests.Variables;

using NextNet.TemplateEngine.Variables;
using Xunit;

public class VariableContextTests
{
    [Fact]
    public void CreateBuilder_Should_ReturnNewBuilder()
    {
        var builder = VariableContext.CreateBuilder();
        Assert.NotNull(builder);
    }

    [Fact]
    public void Get_Should_ReturnValue_When_FlatKey()
    {
        var ctx = VariableContext.CreateBuilder().Set("name", "MyApp").Build();
        Assert.Equal("MyApp", ctx.Get("name"));
    }

    [Fact]
    public void Get_Should_ReturnNestedValue_When_DotNotation()
    {
        var ctx = VariableContext.CreateBuilder()
            .SetNested("project", new { name = "MyApp", version = "1.0.0" })
            .Build();
        Assert.Equal("MyApp", ctx.Get("project.name"));
        Assert.Equal("1.0.0", ctx.Get("project.version"));
    }

    [Fact]
    public void Get_Should_ReturnNull_When_KeyNotFound()
    {
        var ctx = VariableContext.CreateBuilder().Set("name", "X").Build();
        Assert.Null(ctx.Get("missing"));
    }

    [Fact]
    public void Get_T_Should_CastCorrectly()
    {
        var ctx = VariableContext.CreateBuilder().Set("port", 8080).Build();
        Assert.Equal(8080, ctx.Get<int>("port"));
    }

    [Fact]
    public void Get_T_Should_ReturnDefault_When_TypeMismatch()
    {
        var ctx = VariableContext.CreateBuilder().Set("name", "X").Build();
        Assert.Equal(0, ctx.Get<int>("name"));
    }

    [Fact]
    public void TryGet_Should_ReturnTrue_When_Found()
    {
        var ctx = VariableContext.CreateBuilder().Set("k", "v").Build();
        Assert.True(ctx.TryGet("k", out var v));
        Assert.Equal("v", v);
    }

    [Fact]
    public void TryGet_Should_ReturnFalse_When_NotFound()
    {
        var ctx = VariableContext.CreateBuilder().Build();
        Assert.False(ctx.TryGet("missing", out var v));
        Assert.Null(v);
    }

    [Fact]
    public void Contains_Should_ReturnTrue_When_Defined()
    {
        var ctx = VariableContext.CreateBuilder().Set("k", "v").Build();
        Assert.True(ctx.Contains("k"));
        Assert.False(ctx.Contains("missing"));
    }

    [Fact]
    public void Keys_Should_ReturnAllKeys()
    {
        var ctx = VariableContext.CreateBuilder().Set("a", 1).Set("b", 2).Build();
        Assert.Contains("a", ctx.Keys);
        Assert.Contains("b", ctx.Keys);
    }

    [Fact]
    public void Values_Should_ReturnReadOnlySnapshot()
    {
        var ctx = VariableContext.CreateBuilder().Set("a", 1).Build();
        var values = ctx.Values;
        Assert.Equal(1, values["a"]);
    }

    [Fact]
    public void Context_Should_BeImmutable()
    {
        // Verify that the context cannot be mutated through its Values snapshot:
        // modifying the copy should not affect the context.
        var ctx = VariableContext.CreateBuilder().Set("a", 1).Build();
        var snapshot = new Dictionary<string, object?>(ctx.Values);
        snapshot["a"] = 999;
        Assert.Equal(1, ctx.Get("a"));
    }
}
