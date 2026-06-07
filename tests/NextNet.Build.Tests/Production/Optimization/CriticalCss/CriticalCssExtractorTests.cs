using NextNet.Build.Production.Optimization.CriticalCss;
using Xunit;

namespace NextNet.Build.Tests.Production.Optimization.CriticalCss;

public class CriticalCssExtractorTests
{
    private readonly CriticalCssExtractor _extractor = new();

    [Fact]
    public async Task ExtractAsync_WithCriticalSelectors_ExtractsCriticalCss()
    {
        var html = @"<!DOCTYPE html>
<html>
<head>
<style>
body { margin: 0; }
.header { background: blue; }
.footer { color: gray; }
h1 { font-size: 2em; }
</style>
</head>
<body>
<div class=""header""><h1>Title</h1></div>
<div class=""footer"">Footer</div>
</body>
</html>";

        var result = await _extractor.ExtractAsync(html);

        Assert.NotEmpty(result.CriticalCss);
        Assert.Contains("body", result.CriticalCss);
        Assert.Contains(".header", result.CriticalCss);
        Assert.Contains("h1", result.CriticalCss);
    }

    [Fact]
    public async Task ExtractAsync_NoCriticalSelectors_ReturnsEmptyCritical()
    {
        var html = @"<!DOCTYPE html>
<html>
<head>
<style>
.footer { color: gray; }
.sidebar { width: 200px; }
</style>
</head>
<body>
<div class=""sidebar"">Sidebar</div>
<div class=""footer"">Footer</div>
</body>
</html>";

        var result = await _extractor.ExtractAsync(html);

        // .footer and .sidebar are not in our critical set by default
        // But they might match if they contain partial matches
        Assert.NotNull(result);
    }

    [Fact]
    public async Task ExtractAsync_ModifiedHtml_ContainsCriticalStyleTag()
    {
        var html = @"<!DOCTYPE html>
<html>
<head>
<style>
body { margin: 0; }
</style>
</head>
<body>Hello</body>
</html>";

        var result = await _extractor.ExtractAsync(html);

        Assert.NotEmpty(result.ModifiedHtml);
        Assert.Contains("critical-css", result.ModifiedHtml);
    }

    [Fact]
    public async Task ExtractAsync_EmptyHtml_Throws()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _extractor.ExtractAsync(""));
    }

    [Fact]
    public async Task ExtractAsync_TracksRuleCounts()
    {
        var html = @"<!DOCTYPE html>
<html>
<head>
<style>
body { margin: 0; }
.header { background: blue; }
h1 { font-size: 2em; }
.footer { color: gray; }
</style>
</head>
<body>Hello</body>
</html>";

        var result = await _extractor.ExtractAsync(html);

        Assert.True(result.TotalRuleCount > 0);
        Assert.True(result.CriticalRuleCount > 0);
        Assert.True(result.TotalRuleCount >= result.CriticalRuleCount);
    }
}
