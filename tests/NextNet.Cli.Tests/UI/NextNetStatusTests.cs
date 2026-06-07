using NextNet.Cli.UI;
using Xunit;

namespace NextNet.Cli.Tests.UI;

public class NextNetStatusTests
{
    [Fact]
    public void Success_ReturnsMarkup()
    {
        var markup = NextNetStatus.Success("Build successful");
        Assert.NotNull(markup);
    }

    [Fact]
    public void Error_ReturnsMarkup()
    {
        var markup = NextNetStatus.Error("Build failed");
        Assert.NotNull(markup);
    }

    [Fact]
    public void Warning_ReturnsMarkup()
    {
        var markup = NextNetStatus.Warning("Deprecated option");
        Assert.NotNull(markup);
    }

    [Fact]
    public void Info_ReturnsMarkup()
    {
        var markup = NextNetStatus.Info("12 routes discovered");
        Assert.NotNull(markup);
    }

    [Fact]
    public void Step_ReturnsMarkup()
    {
        var markup = NextNetStatus.Step(2, 6, "Source generation");
        Assert.NotNull(markup);
    }

    [Fact]
    public void PlainSuccess_ReturnsPlainText()
    {
        var text = NextNetStatus.PlainSuccess("Build successful");
        Assert.Equal("[OK] Build successful", text);
    }

    [Fact]
    public void PlainError_ReturnsPlainText()
    {
        var text = NextNetStatus.PlainError("Build failed");
        Assert.Equal("[ERR] Build failed", text);
    }

    [Fact]
    public void PlainWarning_ReturnsPlainText()
    {
        var text = NextNetStatus.PlainWarning("Deprecated option");
        Assert.Equal("[WARN] Deprecated option", text);
    }

    [Fact]
    public void PlainInfo_ReturnsPlainText()
    {
        var text = NextNetStatus.PlainInfo("12 routes discovered");
        Assert.Equal("[INFO] 12 routes discovered", text);
    }

    [Fact]
    public void PlainStep_ReturnsPlainText()
    {
        var text = NextNetStatus.PlainStep(2, 6, "Source generation");
        Assert.Equal("[2/6] Source generation", text);
    }
}
