using NextNet.Build.Production.Optimization.AssetOptimizer;
using Xunit;

namespace NextNet.Build.Tests.Production.Optimization.AssetOptimizer;

public class JavaScriptMinifierTests
{
    [Fact]
    public void Minify_RemovesSingleLineComments()
    {
        var js = "var x = 1; // this is a comment\nvar y = 2;";
        var result = JavaScriptMinifier.Minify(js);
        Assert.DoesNotContain("//", result);
    }

    [Fact]
    public void Minify_RemovesMultiLineComments()
    {
        var js = "/* comment block */ var x = 1;";
        var result = JavaScriptMinifier.Minify(js);
        Assert.DoesNotContain("/*", result);
    }

    [Fact]
    public void Minify_RemovesWhitespaceAroundOperators()
    {
        var js = "var x = 1 + 2;";
        var result = JavaScriptMinifier.Minify(js);
        Assert.Contains("1+2", result.Replace(" ", ""));
    }

    [Fact]
    public void Minify_RemovesUnnecessarySemicolons()
    {
        var js = "function f() { var x = 1; }";
        var result = JavaScriptMinifier.Minify(js);
        Assert.DoesNotContain(";}", result);
    }

    [Fact]
    public void Minify_CollapsesMultipleSpaces()
    {
        var js = "var    x    =    1;";
        var result = JavaScriptMinifier.Minify(js);
        Assert.True(result.Length < js.Length);
    }

    [Fact]
    public void Minify_NullOrEmpty_ReturnsInput()
    {
        Assert.Null(JavaScriptMinifier.Minify(null!));
        Assert.Equal("", JavaScriptMinifier.Minify(""));
    }

    [Fact]
    public void CanHandle_ReturnsTrueForJsExtensions()
    {
        var optimizer = new JavaScriptMinifier(new NextNet.IO.DefaultSharpFileSystem());
        Assert.True(optimizer.CanHandle(".js"));
        Assert.True(optimizer.CanHandle(".mjs"));
        Assert.True(optimizer.CanHandle(".cjs"));
        Assert.False(optimizer.CanHandle(".css"));
        Assert.False(optimizer.CanHandle(".html"));
    }
}
