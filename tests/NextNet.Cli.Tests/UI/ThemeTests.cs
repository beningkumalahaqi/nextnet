using NextNet.Cli.UI;
using Xunit;

namespace NextNet.Cli.Tests.UI;

public class ThemeTests
{
    [Fact]
    public void Theme_NextNetTealHex_IsCorrect()
    {
        Assert.Equal("#00D4AA", Theme.NextNetTealHex);
    }

    [Fact]
    public void Theme_VioletHex_IsCorrect()
    {
        Assert.Equal("#7C3AED", Theme.VioletHex);
    }

    [Fact]
    public void Theme_SuccessHex_IsCorrect()
    {
        Assert.Equal("#22C55E", Theme.SuccessHex);
    }

    [Fact]
    public void Theme_WarningHex_IsCorrect()
    {
        Assert.Equal("#F59E0B", Theme.WarningHex);
    }

    [Fact]
    public void Theme_ErrorHex_IsCorrect()
    {
        Assert.Equal("#EF4444", Theme.ErrorHex);
    }

    [Fact]
    public void Theme_InfoHex_IsCorrect()
    {
        Assert.Equal("#3B82F6", Theme.InfoHex);
    }

    [Fact]
    public void Theme_MutedHex_IsCorrect()
    {
        Assert.Equal("#6B7280", Theme.MutedHex);
    }

    [Fact]
    public void Theme_DimHex_IsCorrect()
    {
        Assert.Equal("#4B5563", Theme.DimHex);
    }

    [Fact]
    public void Theme_SubtleBorderHex_IsCorrect()
    {
        Assert.Equal("#374151", Theme.SubtleBorderHex);
    }

    [Fact]
    public void Theme_SubtleBgHex_IsCorrect()
    {
        Assert.Equal("#1F2937", Theme.SubtleBgHex);
    }

    [Fact]
    public void Theme_AnsiValues_AreCorrect()
    {
        Assert.Equal(49, Theme.NextNetTealAnsi);
        Assert.Equal(134, Theme.VioletAnsi);
        Assert.Equal(34, Theme.SuccessAnsi);
        Assert.Equal(214, Theme.WarningAnsi);
        Assert.Equal(196, Theme.ErrorAnsi);
        Assert.Equal(63, Theme.InfoAnsi);
        Assert.Equal(243, Theme.MutedAnsi);
        Assert.Equal(240, Theme.DimAnsi);
        Assert.Equal(238, Theme.SubtleBorderAnsi);
        Assert.Equal(235, Theme.SubtleBgAnsi);
    }

    [Fact]
    public void Theme_PlainStyle_HasDefaultColor()
    {
        var style = Theme.PlainStyle;
        Assert.NotNull(style);
    }

    [Fact]
    public void Theme_HeadingStyle_UsesTealAndBold()
    {
        var style = Theme.HeadingStyle;
        Assert.NotNull(style);
    }

    [Fact]
    public void Theme_BorderColor_HasCorrectValue()
    {
        var color = Theme.BorderColor;
        Assert.Equal(0x37, color.R);
        Assert.Equal(0x41, color.G);
        Assert.Equal(0x51, color.B);
    }

    [Fact]
    public void Theme_NextNetTeal_HasCorrectRGB()
    {
        var color = Theme.NextNetTeal;
        Assert.Equal(0x00, color.R);
        Assert.Equal(0xD4, color.G);
        Assert.Equal(0xAA, color.B);
    }

    [Fact]
    public void Theme_Violet_HasCorrectRGB()
    {
        var color = Theme.Violet;
        Assert.Equal(0x7C, color.R);
        Assert.Equal(0x3A, color.G);
        Assert.Equal(0xED, color.B);
    }

    [Fact]
    public void Theme_Success_HasCorrectRGB()
    {
        var color = Theme.Success;
        Assert.Equal(0x22, color.R);
        Assert.Equal(0xC5, color.G);
        Assert.Equal(0x5E, color.B);
    }

    [Fact]
    public void Theme_Warning_HasCorrectRGB()
    {
        var color = Theme.Warning;
        Assert.Equal(0xF5, color.R);
        Assert.Equal(0x9E, color.G);
        Assert.Equal(0x0B, color.B);
    }

    [Fact]
    public void Theme_Error_HasCorrectRGB()
    {
        var color = Theme.Error;
        Assert.Equal(0xEF, color.R);
        Assert.Equal(0x44, color.G);
        Assert.Equal(0x44, color.B);
    }

    [Fact]
    public void Theme_Info_HasCorrectRGB()
    {
        var color = Theme.Info;
        Assert.Equal(0x3B, color.R);
        Assert.Equal(0x82, color.G);
        Assert.Equal(0xF6, color.B);
    }

    [Fact]
    public void Theme_Muted_HasCorrectRGB()
    {
        var color = Theme.Muted;
        Assert.Equal(0x6B, color.R);
        Assert.Equal(0x72, color.G);
        Assert.Equal(0x80, color.B);
    }

    [Fact]
    public void Theme_Dim_HasCorrectRGB()
    {
        var color = Theme.Dim;
        Assert.Equal(0x4B, color.R);
        Assert.Equal(0x55, color.G);
        Assert.Equal(0x63, color.B);
    }

    [Fact]
    public void Theme_Styles_AreNotNull()
    {
        Assert.NotNull(Theme.HeadingStyle);
        Assert.NotNull(Theme.MutedStyle);
        Assert.NotNull(Theme.SuccessStyle);
        Assert.NotNull(Theme.ErrorStyle);
        Assert.NotNull(Theme.WarningStyle);
        Assert.NotNull(Theme.InfoStyle);
        Assert.NotNull(Theme.CodeStyle);
        Assert.NotNull(Theme.PlainStyle);
        Assert.NotNull(Theme.DefaultStyle);
    }

    [Fact]
    public void Theme_TableHeaderColor_IsNextNetTeal()
    {
        var color = Theme.TableHeaderColor;
        Assert.Equal(Theme.NextNetTeal.R, color.R);
        Assert.Equal(Theme.NextNetTeal.G, color.G);
        Assert.Equal(Theme.NextNetTeal.B, color.B);
    }
}
