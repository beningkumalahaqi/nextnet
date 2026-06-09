using NextNet.Build.Production;
using Xunit;

namespace NextNet.Build.Tests.Production;

public class ProductionMiddlewareOptionsTests
{
    [Fact]
    public void DefaultOptions_Should_AllBeEnabled_When_NewInstance()
    {
        var options = new ProductionMiddlewareOptions();
        Assert.True(options.EnableSecurityHeaders);
        Assert.True(options.EnableCompression);
        Assert.True(options.EnableCaching);
        Assert.True(options.EnableRequestTiming);
        Assert.True(options.EnableHealthEndpoint);
    }
}
