using NextNet.Build.Production.Security;
using Xunit;

namespace NextNet.Build.Tests.Production.Security;

public class ContentSecurityPolicyBuilderTests
{
    [Fact]
    public void CreateDefault_Should_GenerateSecurePolicy_When_Called()
    {
        var csp = ContentSecurityPolicyBuilder.CreateDefault().Build();
        Assert.Contains("default-src 'self'", csp);
        Assert.Contains("script-src 'self'", csp);
        Assert.Contains("style-src 'self'", csp);
        Assert.Contains("img-src 'self'", csp);
        Assert.Contains("object-src 'none'", csp);
        Assert.Contains("frame-ancestors 'none'", csp);
    }

    [Fact]
    public void Build_Should_IncludeCustomDirective_When_Added()
    {
        var csp = ContentSecurityPolicyBuilder.CreateDefault()
            .WithDirective("upgrade-insecure-requests")
            .Build();

        Assert.Contains("upgrade-insecure-requests", csp);
    }

    [Fact]
    public void ReportOnly_Should_ReturnCorrectHeaderName_When_Called()
    {
        var builder = ContentSecurityPolicyBuilder.CreateDefault().AsReportOnly();
        Assert.Equal("Content-Security-Policy-Report-Only", builder.GetHeaderName());
    }

    [Fact]
    public void Default_Should_ReturnCorrectHeaderName_When_Called()
    {
        var builder = ContentSecurityPolicyBuilder.CreateDefault();
        Assert.Equal("Content-Security-Policy", builder.GetHeaderName());
    }

    [Fact]
    public void Build_Should_AppendAdditionalSources_When_Added()
    {
        var csp = ContentSecurityPolicyBuilder.CreateDefault()
            .WithScriptSrc("https://cdn.example.com")
            .Build();

        Assert.Contains("script-src 'self' https://cdn.example.com", csp);
    }

    [Fact]
    public void Build_Should_IncludeReportDirectives_When_Configured()
    {
        var csp = ContentSecurityPolicyBuilder.CreateDefault()
            .WithReportUri("/csp-violations")
            .WithReportTo("csp-endpoint")
            .Build();

        Assert.Contains("report-uri", csp);
        Assert.Contains("report-to", csp);
    }
}
