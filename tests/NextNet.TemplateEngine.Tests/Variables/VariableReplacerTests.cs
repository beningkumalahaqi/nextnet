namespace NextNet.TemplateEngine.Tests.Variables;

using NextNet.TemplateEngine.Variables;
using Xunit;

public class VariableReplacerTests
{
    [Fact]
    public async Task ReplaceAsync_Should_ReplaceSimpleVariable()
    {
        var replacer = new VariableReplacer();
        var ctx = VariableContext.CreateBuilder().Set("name", "MyApp").Build();
        var result = await replacer.ReplaceAsync("Hello {{name}}", ctx);
        Assert.Equal("Hello MyApp", result);
    }

    [Fact]
    public async Task ReplaceAsync_Should_ReplaceMultipleVariables()
    {
        var replacer = new VariableReplacer();
        var ctx = VariableContext.CreateBuilder()
            .Set("greeting", "Hello")
            .Set("name", "World")
            .Build();
        var result = await replacer.ReplaceAsync("{{greeting}} {{name}}!", ctx);
        Assert.Equal("Hello World!", result);
    }

    [Fact]
    public async Task ReplaceAsync_Should_HandleNestedVariables()
    {
        var replacer = new VariableReplacer();
        var ctx = VariableContext.CreateBuilder()
            .SetNested("project", new { name = "MyApp" })
            .Build();
        var result = await replacer.ReplaceAsync("Project: {{project.name}}", ctx);
        Assert.Equal("Project: MyApp", result);
    }

    [Fact]
    public async Task ReplaceAsync_Should_PreserveEscapedDelimiters()
    {
        var replacer = new VariableReplacer();
        var ctx = VariableContext.CreateBuilder().Build();
        var result = await replacer.ReplaceAsync(@"This is \{{literal}} text", ctx);
        Assert.Equal("This is {{literal}} text", result);
    }

    [Fact]
    public async Task ReplaceAsync_Should_ReplaceUndefinedWithEmptyString_ByDefault()
    {
        var replacer = new VariableReplacer();
        var ctx = VariableContext.CreateBuilder().Build();
        var result = await replacer.ReplaceAsync("X={{missing}}", ctx);
        Assert.Equal("X=", result);
    }

    [Fact]
    public async Task ReplaceAsync_Should_Throw_When_UndefinedAndThrowOptionSet()
    {
        var replacer = new VariableReplacer(new VariableReplacementOptions { ThrowOnUndefinedVariable = true });
        var ctx = VariableContext.CreateBuilder().Build();
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            replacer.ReplaceAsync("{{undefined}}", ctx));
    }

    [Fact]
    public async Task ReplaceAsync_Should_HandleEmptyContent()
    {
        var replacer = new VariableReplacer();
        var ctx = VariableContext.CreateBuilder().Build();
        var result = await replacer.ReplaceAsync("", ctx);
        Assert.Equal("", result);
    }

    [Fact]
    public async Task ReplaceAsync_Should_HandleContentWithNoVariables()
    {
        var replacer = new VariableReplacer();
        var ctx = VariableContext.CreateBuilder().Build();
        var result = await replacer.ReplaceAsync("just plain text", ctx);
        Assert.Equal("just plain text", result);
    }

    [Fact]
    public async Task ReplaceAsync_Should_BeCaseSensitive_ByDefault()
    {
        var replacer = new VariableReplacer();
        var ctx = VariableContext.CreateBuilder().Set("name", "MyApp").Build();
        var result = await replacer.ReplaceAsync("{{NAME}}", ctx);
        // "NAME" != "name" when case-sensitive, so the variable is undefined
        // and replaced with empty string by default
        Assert.Equal("", result);
    }

    [Fact]
    public async Task ReplaceAsync_Should_SupportCustomDelimiters()
    {
        var replacer = new VariableReplacer(new VariableReplacementOptions
        {
            OpenDelimiter = "${",
            CloseDelimiter = "}"
        });
        var ctx = VariableContext.CreateBuilder().Set("name", "MyApp").Build();
        var result = await replacer.ReplaceAsync("Hello ${name}", ctx);
        Assert.Equal("Hello MyApp", result);
    }

    [Fact]
    public async Task ReplaceAsync_Should_HandleLargeContent()
    {
        var replacer = new VariableReplacer();
        var ctx = VariableContext.CreateBuilder().Set("x", "REPLACED").Build();
        var content = string.Concat(Enumerable.Repeat("{{x}} ", 1000));
        var result = await replacer.ReplaceAsync(content, ctx);
        Assert.DoesNotContain("{{x}}", result);
        Assert.Contains("REPLACED", result);
    }
}
