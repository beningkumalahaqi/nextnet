using NextNet.Cli.Config;
using Xunit;

namespace NextNet.Cli.Tests.Config;

public class ConfigValidatorTests
{
    [Fact]
    public void Validate_NullConfig_ReturnsError()
    {
        var issues = ConfigValidator.Validate(null);
        Assert.NotEmpty(issues);
        Assert.Contains(issues, i => i.Severity == ConfigIssueSeverity.Error);
    }

    [Fact]
    public void Validate_EmptyName_ReturnsError()
    {
        var config = new NextNetProjectConfig { Name = "" };
        var issues = ConfigValidator.Validate(config);
        Assert.Contains(issues, i => i.Severity == ConfigIssueSeverity.Error && i.Message.Contains("name"));
    }

    [Fact]
    public void Validate_InvalidName_ReturnsError()
    {
        var config = new NextNetProjectConfig { Name = "Invalid Name With Spaces" };
        var issues = ConfigValidator.Validate(config);
        Assert.Contains(issues, i => i.Severity == ConfigIssueSeverity.Error);
    }

    [Fact]
    public void Validate_ValidConfig_ReturnsNoIssues()
    {
        var config = new NextNetProjectConfig
        {
            Name = "my-app",
            Version = "1.0.0",
            Routing = new RoutingConfig { Dir = "app" },
            Build = new BuildConfig { Output = "dist", Target = "net10.0" },
            Dev = new DevConfig { Port = 3000 }
        };

        var issues = ConfigValidator.Validate(config);
        Assert.Empty(issues);
    }

    [Fact]
    public void Validate_InvalidPort_ReturnsError()
    {
        var config = new NextNetProjectConfig
        {
            Name = "my-app",
            Dev = new DevConfig { Port = 0 }
        };

        var issues = ConfigValidator.Validate(config);
        Assert.Contains(issues, i => i.Message.Contains("port", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_PortTooHigh_ReturnsError()
    {
        var config = new NextNetProjectConfig
        {
            Name = "my-app",
            Dev = new DevConfig { Port = 70000 }
        };

        var issues = ConfigValidator.Validate(config);
        Assert.Contains(issues, i => i.Message.Contains("port", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_InvalidSemver_Warning()
    {
        var config = new NextNetProjectConfig
        {
            Name = "my-app",
            Version = "abc"
        };

        var issues = ConfigValidator.Validate(config);
        Assert.Contains(issues, i => i.Severity == ConfigIssueSeverity.Warning && i.Message.Contains("semver"));
    }

    [Fact]
    public void Validate_EmptyRoutingDir_ReturnsError()
    {
        var config = new NextNetProjectConfig
        {
            Name = "my-app",
            Routing = new RoutingConfig { Dir = "" }
        };

        var issues = ConfigValidator.Validate(config);
        Assert.Contains(issues, i => i.Severity == ConfigIssueSeverity.Error && i.Message.Contains("Routing"));
    }

    [Fact]
    public void Validate_EmptyOutputDir_ReturnsError()
    {
        var config = new NextNetProjectConfig
        {
            Name = "my-app",
            Build = new BuildConfig { Output = "" }
        };

        var issues = ConfigValidator.Validate(config);
        Assert.Contains(issues, i => i.Message.Contains("output"));
    }

    [Fact]
    public void ValidateOrThrow_ValidConfig_DoesNotThrow()
    {
        var config = new NextNetProjectConfig { Name = "my-app" };
        ConfigValidator.ValidateOrThrow(config); // Should not throw
    }

    [Fact]
    public void ValidateOrThrow_InvalidConfig_Throws()
    {
        var config = new NextNetProjectConfig { Name = "" };
        Assert.Throws<NextNetConfigException>(() => ConfigValidator.ValidateOrThrow(config));
    }

    [Fact]
    public void ProjectNameRegex_ValidNames()
    {
        Assert.True(ProjectNameRegex.IsValid("my-app"));
        Assert.True(ProjectNameRegex.IsValid("app"));
        Assert.True(ProjectNameRegex.IsValid("my-cool-project-2"));
        Assert.True(ProjectNameRegex.IsValid("a"));
    }

    [Fact]
    public void ProjectNameRegex_InvalidNames()
    {
        Assert.False(ProjectNameRegex.IsValid("My-App"));
        Assert.False(ProjectNameRegex.IsValid("_app"));
        Assert.False(ProjectNameRegex.IsValid("-app"));
        Assert.False(ProjectNameRegex.IsValid("app name"));
        Assert.False(ProjectNameRegex.IsValid(""));
    }
}
