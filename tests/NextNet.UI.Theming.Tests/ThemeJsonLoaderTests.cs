using System.IO;
using System.Text.Json;
using NextNet.DesignSystem.Tokens;
using Xunit;

namespace NextNet.UI.Theming.Tests;

public class ThemeJsonLoaderTests
{
    [Fact]
    public void Load_Should_ReturnDefaults_When_FileDoesNotExist()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        try
        {
            var loader = new ThemeJsonLoader(tempDir);
            var tokens = loader.Load();
            Assert.NotNull(tokens);
            Assert.NotEmpty(tokens.Colors);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void Load_Should_ReturnDefaults_When_FileIsEmpty()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        try
        {
            File.WriteAllText(Path.Combine(tempDir, "nextnet.theme.json"), "");
            var loader = new ThemeJsonLoader(tempDir);
            var tokens = loader.Load();
            Assert.NotNull(tokens);
            Assert.NotEmpty(tokens.Colors);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void Load_Should_ReturnDefaults_When_InvalidJson()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        try
        {
            File.WriteAllText(Path.Combine(tempDir, "nextnet.theme.json"), "{ invalid json }");
            var loader = new ThemeJsonLoader(tempDir);
            var tokens = loader.Load();
            Assert.NotNull(tokens);
            Assert.NotEmpty(tokens.Colors);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void Load_Should_OverridePrimaryColor()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        try
        {
            var json = JsonSerializer.Serialize(new
            {
                theme = new
                {
                    primary = "#FF6200"
                }
            });
            File.WriteAllText(Path.Combine(tempDir, "nextnet.theme.json"), json);

            var loader = new ThemeJsonLoader(tempDir);
            var tokens = loader.Load();

            Assert.Equal("#FF6200", tokens.Colors["primary-500"].Value);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void Load_Should_OverrideSecondaryColor()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        try
        {
            var json = JsonSerializer.Serialize(new
            {
                theme = new
                {
                    secondary = "#8B5CF6"
                }
            });
            File.WriteAllText(Path.Combine(tempDir, "nextnet.theme.json"), json);

            var loader = new ThemeJsonLoader(tempDir);
            var tokens = loader.Load();

            Assert.Equal("#8B5CF6", tokens.Colors["secondary-500"].Value);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void Load_Should_OverrideRadius()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        try
        {
            var json = JsonSerializer.Serialize(new
            {
                theme = new
                {
                    radius = "1rem"
                }
            });
            File.WriteAllText(Path.Combine(tempDir, "nextnet.theme.json"), json);

            var loader = new ThemeJsonLoader(tempDir);
            var tokens = loader.Load();

            foreach (var (key, border) in tokens.Borders)
            {
                Assert.Equal("1rem", border.Radius);
            }
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void Load_Should_OverrideFont()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        try
        {
            var json = JsonSerializer.Serialize(new
            {
                theme = new
                {
                    font = "Roboto"
                }
            });
            File.WriteAllText(Path.Combine(tempDir, "nextnet.theme.json"), json);

            var loader = new ThemeJsonLoader(tempDir);
            var tokens = loader.Load();

            foreach (var (key, typography) in tokens.Typography)
            {
                Assert.StartsWith("Roboto", typography.FontFamily, System.StringComparison.Ordinal);
            }
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void Load_Should_ApplyAllOverrides_Simultaneously()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        try
        {
            var json = JsonSerializer.Serialize(new
            {
                theme = new
                {
                    primary = "#FF6200",
                    secondary = "#8B5CF6",
                    radius = "0.75rem",
                    font = "Roboto Mono"
                }
            });
            File.WriteAllText(Path.Combine(tempDir, "nextnet.theme.json"), json);

            var loader = new ThemeJsonLoader(tempDir);
            var tokens = loader.Load();

            Assert.Equal("#FF6200", tokens.Colors["primary-500"].Value);
            Assert.Equal("#8B5CF6", tokens.Colors["secondary-500"].Value);
            Assert.StartsWith("Roboto Mono", tokens.Typography["body-base"].FontFamily, System.StringComparison.Ordinal);
            Assert.Equal("0.75rem", tokens.Borders["default"].Radius);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void Load_Should_NotAffectNonOverriddenTokens()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        try
        {
            var json = JsonSerializer.Serialize(new
            {
                theme = new
                {
                    primary = "#FF6200"
                }
            });
            File.WriteAllText(Path.Combine(tempDir, "nextnet.theme.json"), json);

            var loader = new ThemeJsonLoader(tempDir);
            var tokens = loader.Load();

            // Primary should be overridden
            Assert.Equal("#FF6200", tokens.Colors["primary-500"].Value);
            // Danger should remain default
            Assert.Equal("#EF4444", tokens.Colors["danger-500"].Value);
            // Spacing should still be populated
            Assert.NotEmpty(tokens.Spacing);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void Constructor_Should_ThrowArgumentNullException_When_BasePathNull()
    {
        Assert.Throws<ArgumentNullException>(() => new ThemeJsonLoader(null!));
    }

    [Fact]
    public void Load_Should_SkipComments()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        try
        {
            var json = @"{
                /* comment */
                ""theme"": {
                    ""primary"": ""#FF6200""
                }
            }";
            File.WriteAllText(Path.Combine(tempDir, "nextnet.theme.json"), json);

            var loader = new ThemeJsonLoader(tempDir);
            var tokens = loader.Load();

            Assert.Equal("#FF6200", tokens.Colors["primary-500"].Value);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }
}
