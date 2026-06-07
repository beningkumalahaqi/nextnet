using NextNet.Build.Production.Caching;
using Xunit;

namespace NextNet.Build.Tests.Production.Caching;

public class CacheHeaderOptionsTests
{
    [Fact]
    public void DefaultOptions_SetReasonableDefaults()
    {
        var options = new CacheHeaderOptions();
        Assert.True(options.EnableCaching);
        Assert.True(options.EnableETag);
        Assert.True(options.EnableLastModified);
        Assert.True(options.SetImmutable);
        Assert.Equal(365, options.ImmutableMaxAge.TotalDays);
        Assert.Equal(5, options.DefaultMaxAge.TotalMinutes);
        Assert.Contains(".css", options.ImmutableExtensions);
        Assert.Contains(".js", options.ImmutableExtensions);
        Assert.Contains(".png", options.ImmutableExtensions);
    }
}
