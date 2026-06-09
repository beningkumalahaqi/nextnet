using System.IO;
using Microsoft.Extensions.DependencyInjection;
using NextNet.UI.Abstractions.Theme;
using NextNet.UI.Theming.Extensions;
using Xunit;

namespace NextNet.UI.Theming.Tests.Extensions;

public class ThemeServiceExtensionsTests
{
    [Fact]
    public void AddNextNetTheming_Should_RegisterThemeManager()
    {
        var services = new ServiceCollection();
        services.AddNextNetTheming();

        var provider = services.BuildServiceProvider();
        var manager = provider.GetRequiredService<IThemeProvider>();

        Assert.NotNull(manager);
    }

    [Fact]
    public void AddNextNetTheming_Should_ReturnSameServiceCollection()
    {
        var services = new ServiceCollection();
        var result = services.AddNextNetTheming();

        Assert.Same(services, result);
    }

    [Fact]
    public void AddNextNetTheming_Should_RegisterSingleton()
    {
        var services = new ServiceCollection();
        services.AddNextNetTheming();

        var provider = services.BuildServiceProvider();
        var manager1 = provider.GetRequiredService<IThemeProvider>();
        var manager2 = provider.GetRequiredService<IThemeProvider>();

        Assert.Same(manager1, manager2);
    }

    [Fact]
    public void AddNextNetTheming_Should_RegisterLightAndDarkThemes()
    {
        var services = new ServiceCollection();
        services.AddNextNetTheming();

        var provider = services.BuildServiceProvider();
        var manager = provider.GetRequiredService<IThemeProvider>();

        Assert.Contains("light", manager.AvailableThemes);
        Assert.Contains("dark", manager.AvailableThemes);
    }

    [Fact]
    public void AddNextNetTheming_Should_SetDefaultThemeToLight()
    {
        var services = new ServiceCollection();
        services.AddNextNetTheming();

        var provider = services.BuildServiceProvider();
        var manager = provider.GetRequiredService<IThemeProvider>();

        Assert.Equal("light", manager.ActiveTheme);
    }

    [Fact]
    public void AddNextNetTheming_Should_RespectCustomDefaultTheme()
    {
        var services = new ServiceCollection();
        services.AddNextNetTheming(options =>
        {
            options.DefaultThemeName = "dark";
        });

        var provider = services.BuildServiceProvider();
        var manager = provider.GetRequiredService<IThemeProvider>();

        Assert.Equal("dark", manager.ActiveTheme);
    }

    [Fact]
    public void AddNextNetTheming_Should_ThrowArgumentNullException_When_ServicesNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            ((IServiceCollection)null!).AddNextNetTheming());
    }

    [Fact]
    public void AddNextNetTheming_Should_RegisterThemeOptions()
    {
        var services = new ServiceCollection();
        services.AddNextNetTheming(options =>
        {
            options.DefaultThemeName = "dark";
            options.AvailableThemes = new[] { "light", "dark", "high-contrast" };
        });

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<ThemeOptions>();

        Assert.Equal("dark", options.DefaultThemeName);
        Assert.Equal(3, options.AvailableThemes.Count);
    }

    [Fact]
    public void AddNextNetTheming_Should_WorkWithoutConfigureAction()
    {
        var services = new ServiceCollection();
        services.AddNextNetTheming();

        var provider = services.BuildServiceProvider();
        var manager = provider.GetRequiredService<IThemeProvider>();

        Assert.NotNull(manager);
        Assert.Equal("light", manager.ActiveTheme);
    }

    [Fact]
    public void AddNextNetTheming_Should_ReturnThemesWithTokens()
    {
        var services = new ServiceCollection();
        services.AddNextNetTheming();

        var provider = services.BuildServiceProvider();
        var manager = provider.GetRequiredService<IThemeProvider>();

        var lightTokens = manager.GetTheme("light");
        var darkTokens = manager.GetTheme("dark");

        Assert.NotEmpty(lightTokens.Colors);
        Assert.NotEmpty(darkTokens.Colors);

        // Light and dark should have different primary-500 values
        Assert.NotEqual(
            lightTokens.Colors["primary-500"].Value,
            darkTokens.Colors["primary-500"].Value);
    }

    [Fact]
    public void ThemeOptions_Should_HaveDefaultValues()
    {
        var options = new ThemeOptions();

        Assert.Equal("light", options.DefaultThemeName);
        Assert.Equal(DarkMode.Light, options.DarkMode);
        Assert.Null(options.ThemeJsonBasePath);
        Assert.Equal(2, options.AvailableThemes.Count);
        Assert.Contains("light", options.AvailableThemes);
        Assert.Contains("dark", options.AvailableThemes);
    }

    [Fact]
    public void AddNextNetTheming_Should_RegisterDefaultSystemPreferenceResolver()
    {
        var services = new ServiceCollection();
        services.AddNextNetTheming();

        var provider = services.BuildServiceProvider();
        var resolver = provider.GetRequiredService<ISystemPreferenceResolver>();

        Assert.NotNull(resolver);
        Assert.IsType<DefaultSystemPreferenceResolver>(resolver);
    }

    [Fact]
    public void AddNextNetTheming_Should_SupportDarkModeOption()
    {
        var services = new ServiceCollection();
        services.AddNextNetTheming(options =>
        {
            options.DarkMode = DarkMode.Dark;
        });

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<ThemeOptions>();

        Assert.Equal(DarkMode.Dark, options.DarkMode);
    }

    [Fact]
    public void AddNextNetTheming_WithDarkModeLight_Should_SetActiveThemeToLight()
    {
        // Light is the default, so this should activate the light theme
        var services = new ServiceCollection();
        services.AddNextNetTheming(options =>
        {
            options.DarkMode = DarkMode.Light;
        });

        var provider = services.BuildServiceProvider();
        var manager = provider.GetRequiredService<IThemeProvider>();

        Assert.Equal("light", manager.ActiveTheme);
    }

    [Fact]
    public void AddNextNetTheming_WithDarkModeDark_Should_SetActiveThemeToDark()
    {
        var services = new ServiceCollection();
        services.AddNextNetTheming(options =>
        {
            options.DarkMode = DarkMode.Dark;
        });

        var provider = services.BuildServiceProvider();
        var manager = provider.GetRequiredService<IThemeProvider>();

        Assert.Equal("dark", manager.ActiveTheme);
    }

    [Fact]
    public void AddNextNetTheming_WithDarkModeSystem_Should_ResolveUsingDefaultResolver()
    {
        // Default resolver returns light, so active theme should be light
        var services = new ServiceCollection();
        services.AddNextNetTheming(options =>
        {
            options.DarkMode = DarkMode.System;
        });

        var provider = services.BuildServiceProvider();
        var manager = provider.GetRequiredService<IThemeProvider>();

        // Default resolver is light
        Assert.Equal("light", manager.ActiveTheme);
    }

    [Fact]
    public void AddNextNetTheming_WithThemeJsonFile_Should_ApplyOverrides()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        try
        {
            var json = System.Text.Json.JsonSerializer.Serialize(new
            {
                theme = new
                {
                    primary = "#FF6200",
                    radius = "1rem",
                    font = "Roboto"
                }
            });
            File.WriteAllText(Path.Combine(tempDir, "nextnet.theme.json"), json);

            var services = new ServiceCollection();
            services.AddNextNetTheming(options =>
            {
                options.ThemeJsonBasePath = tempDir;
            });

            var provider = services.BuildServiceProvider();
            var manager = provider.GetRequiredService<IThemeProvider>();

            var tokens = manager.GetTheme("light");
            Assert.Equal("#FF6200", tokens.Colors["primary-500"].Value);
            Assert.Equal("1rem", tokens.Borders["default"].Radius);
            Assert.StartsWith("Roboto", tokens.Typography["body-base"].FontFamily, System.StringComparison.Ordinal);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void AddNextNetTheming_WithThemeJsonFile_Should_ApplyToBothThemes()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        try
        {
            var json = System.Text.Json.JsonSerializer.Serialize(new
            {
                theme = new
                {
                    primary = "#FF6200",
                    radius = "0.75rem",
                    font = "Roboto"
                }
            });
            File.WriteAllText(Path.Combine(tempDir, "nextnet.theme.json"), json);

            var services = new ServiceCollection();
            services.AddNextNetTheming(options =>
            {
                options.ThemeJsonBasePath = tempDir;
            });

            var provider = services.BuildServiceProvider();
            var manager = provider.GetRequiredService<IThemeProvider>();

            // Light theme should have the overridden primary
            Assert.Equal("#FF6200", manager.GetTheme("light").Colors["primary-500"].Value);

            // Dark theme derives from the same base — but primary uses dark-mode scale values
            var darkPrimary500 = manager.GetTheme("dark").Colors["primary-500"];
            Assert.NotNull(darkPrimary500);
            Assert.NotNull(darkPrimary500.Value);

            // Non-color overrides (radius + font) should apply to both themes
            Assert.Equal("0.75rem", manager.GetTheme("light").Borders["default"].Radius);
            Assert.Equal("0.75rem", manager.GetTheme("dark").Borders["default"].Radius);
            Assert.StartsWith("Roboto", manager.GetTheme("dark").Typography["body-base"].FontFamily, System.StringComparison.Ordinal);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }
}
