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

    [Fact]
    public void Build_Should_AutoDeriveNamespaceName_When_ProjectNameSet_And_NamespaceNameNotSet()
    {
        var ctx = VariableContext.CreateBuilder()
            .Set("projectName", "my-blog")
            .Build();

        var namespaceName = ctx.Get<string>("namespaceName");
        Assert.Equal("MyBlog", namespaceName);
    }

    [Fact]
    public void Build_Should_NotOverrideNamespaceName_When_ExplicitlySet()
    {
        var ctx = VariableContext.CreateBuilder()
            .Set("projectName", "my-blog")
            .Set("namespaceName", "CustomNamespace")
            .Build();

        var namespaceName = ctx.Get<string>("namespaceName");
        Assert.Equal("CustomNamespace", namespaceName);
    }

    [Fact]
    public void Build_Should_DeriveNamespaceName_FromSingleSegmentProjectName()
    {
        var ctx = VariableContext.CreateBuilder()
            .Set("projectName", "MyBlog")
            .Build();

        var namespaceName = ctx.Get<string>("namespaceName");
        Assert.Equal("MyBlog", namespaceName);
    }

    [Fact]
    public void Build_Should_HandleEmptyProjectName_Gracefully()
    {
        var ctx = VariableContext.CreateBuilder()
            .Set("projectName", "")
            .Build();

        var namespaceName = ctx.Get<string>("namespaceName");
        Assert.Equal("App", namespaceName);

        // Also verify the namespaceName IS set (auto-derived)
        Assert.True(ctx.Contains("namespaceName"));
    }

    [Fact]
    public void Build_Should_HandleNullProjectName_Gracefully()
    {
        var ctx = VariableContext.CreateBuilder()
            .Set("projectName", null)
            .Build();

        // null → projectName is null, so the condition `projectName is string pn` is false
        // namespaceName should NOT be set
        var hasNamespace = ctx.TryGet<string>("namespaceName", out _);
        Assert.False(hasNamespace);
    }
}
