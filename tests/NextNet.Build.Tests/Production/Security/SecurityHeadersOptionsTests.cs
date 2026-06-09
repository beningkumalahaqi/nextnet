using NextNet.Build.Production.Security;
using Xunit;

namespace NextNet.Build.Tests.Production.Security;

public class SecurityHeadersOptionsTests
{
    [Fact]
    public void DefaultOptions_Should_HaveSecureDefaults_When_NewInstance()
    {
        var options = new SecurityHeadersOptions();
        Assert.Equal("DENY", options.XFrameOptions);
        Assert.Equal("nosniff", options.XContentTypeOptions);
        Assert.Equal("1; mode=block", options.XssProtection);
        Assert.Equal("strict-origin-when-cross-origin", options.ReferrerPolicy);
        Assert.True(options.EnableSecurityHeaders);
        Assert.True(options.EnableHsts);
        Assert.Equal(365, options.HstsMaxAgeDays);
        Assert.True(options.HstsIncludeSubDomains);
    }
}
