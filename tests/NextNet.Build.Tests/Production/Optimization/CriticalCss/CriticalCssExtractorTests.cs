using NextNet.Build.Production.Optimization.CriticalCss;
using Xunit;

namespace NextNet.Build.Tests.Production.Optimization.CriticalCss;

public class CriticalCssExtractorTests
{
    private readonly CriticalCssExtractor _extractor = new();

    [Fact]
    public async Task ExtractAsync_Should_ExtractCriticalCss_When_CriticalSelectorsPresent()
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
    public async Task ExtractAsync_Should_ReturnResult_When_NoCriticalSelectors()
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

        Assert.NotNull(result);
    }

    [Fact]
    public async Task ExtractAsync_Should_ContainCriticalStyleTag_When_Modified()
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
    public async Task ExtractAsync_Should_ThrowArgumentException_When_EmptyHtml()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _extractor.ExtractAsync(""));
    }

    [Fact]
    public async Task ExtractAsync_Should_TrackRuleCounts_When_Extracted()
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
