using NextNet.Components;
using NextNet.UI.Theming;
using NextNet.UI.Theming.Css;
using NextNet.UI.Theming.Presets;
using Xunit;

namespace NextNet.UI.Theming.Tests.Css;

public class ThemeStyleBuilderTests
{
    [Fact]
    public void Build_Should_ThrowArgumentNullException_When_ThemeIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => ThemeStyleBuilder.Build(null!, CssVariableScope.Root));
    }

    [Fact]
    public void Build_Should_ReturnIHtmlContent()
    {
        var theme = LightTheme.Create();
        var result = ThemeStyleBuilder.Build(theme, CssVariableScope.Root);

        Assert.NotNull(result);
        Assert.IsAssignableFrom<IHtmlContent>(result);
    }

    [Fact]
    public void Build_Should_WrapInStyleTag()
    {
        var theme = LightTheme.Create();
        var result = ThemeStyleBuilder.Build(theme, CssVariableScope.Root);
        var html = result.ToHtml();

        Assert.StartsWith("<style>", html.Trim());
        Assert.Contains("</style>", html);
    }

    [Fact]
    public void Build_Should_UseRootSelector_When_ScopeIsRoot()
    {
        var theme = LightTheme.Create();
        var result = ThemeStyleBuilder.Build(theme, CssVariableScope.Root);
        var html = result.ToHtml();

        Assert.Contains(":root", html);
        Assert.DoesNotContain("[data-theme", html);
    }

    [Fact]
    public void Build_Should_UseThemeSelector_When_ScopeIsTheme()
    {
        var theme = DarkTheme.Create();
        var result = ThemeStyleBuilder.Build(theme, CssVariableScope.Theme);
        var html = result.ToHtml();

        Assert.Contains("[data-theme=\"dark\"]", html);
        Assert.DoesNotContain(":root", html);
    }

    [Fact]
    public void Build_Should_UseComponentSelector_When_ScopeIsComponent()
    {
        var theme = LightTheme.Create();
        var result = ThemeStyleBuilder.Build(theme, CssVariableScope.Component);
        var html = result.ToHtml();

        Assert.Contains(".theme-light", html);
    }

    [Fact]
    public void Build_Should_IncludeCssCustomProperties()
    {
        var theme = LightTheme.Create();
        var result = ThemeStyleBuilder.Build(theme, CssVariableScope.Root);
        var html = result.ToHtml();

        Assert.Contains("--color-primary-500", html);
        Assert.Contains("--spacing-4", html);
    }

    [Fact]
    public void Build_Should_IncludeBraces()
    {
        var theme = LightTheme.Create();
        var result = ThemeStyleBuilder.Build(theme, CssVariableScope.Root);
        var html = result.ToHtml();

        Assert.Contains("{", html);
        Assert.Contains("}", html);
    }

    [Fact]
    public void Build_Should_FormatMultiLine()
    {
        var theme = LightTheme.Create();
        var result = ThemeStyleBuilder.Build(theme, CssVariableScope.Root);
        var html = result.ToHtml();

        var lines = html.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        Assert.True(lines.Length > 3, $"Expected multiple lines but got {lines.Length}");
    }

    [Fact]
    public void Build_Should_ThrowArgumentException_When_ScopeIsUndefined()
    {
        var theme = LightTheme.Create();
        var ex = Assert.Throws<ArgumentException>(() =>
            ThemeStyleBuilder.Build(theme, (CssVariableScope)999));
        Assert.Contains("DS-206", ex.Message);
    }

    [Fact]
    public void Build_WithDarkTheme_Should_UseDarkSelector()
    {
        var theme = DarkTheme.Create();
        var result = ThemeStyleBuilder.Build(theme, CssVariableScope.Theme);
        var html = result.ToHtml();

        Assert.Contains("[data-theme=\"dark\"]", html);
    }

    [Fact]
    public void Build_ForRootScope_ShouldNotContainDataThemeAttribute()
    {
        var theme = DarkTheme.Create();
        var result = ThemeStyleBuilder.Build(theme, CssVariableScope.Root);
        var html = result.ToHtml();

        Assert.DoesNotContain("[data-theme", html);
    }

    // --- XSS prevention tests ---

    public static IEnumerable<object[]> MaliciousThemeNames()
    {
        yield return new object[] { "dark\"; }</style><script>alert(1)</script>" };
        yield return new object[] { "light'-->" };
        yield return new object[] { "theme</style><script>evil()</script>" };
        yield return new object[] { "mytheme\"></div><script src=x></script>" };
        yield return new object[] { "foo onload=\"evil()\"" };
        yield return new object[] { "bar;color:expression(alert(1))" };
        yield return new object[] { "../dangerous" };
        yield return new object[] { "with space" };
        yield return new object[] { "with.dot" };
        yield return new object[] { "unicode\u00A0space" };
    }

    [Theory]
    [MemberData(nameof(MaliciousThemeNames))]
    public void Build_Should_RejectMaliciousThemeName(string maliciousName)
    {
        var baseTheme = LightTheme.Create();
        // Create a theme with a malicious name using the record's with-expression
        var maliciousTheme = baseTheme with { Name = maliciousName };

        var ex = Assert.Throws<ArgumentException>(() =>
            ThemeStyleBuilder.Build(maliciousTheme, CssVariableScope.Root));
        Assert.Contains("invalid characters", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}
