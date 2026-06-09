using NextNet.Build.Production.Health;
using Xunit;

namespace NextNet.Build.Tests.Production.Health;

public class HealthCheckEndpointTests
{
    [Fact]
    public void Constructor_Should_ThrowArgumentNullException_When_NullHealthCheck()
    {
        Assert.Throws<ArgumentNullException>(() => new HealthCheckEndpoint(null!));
    }

    [Fact]
    public void Constructor_Should_AcceptValidHealthCheck_When_Provided()
    {
        var healthCheck = new NextNetHealthCheck("1.0.0");
        var endpoint = new HealthCheckEndpoint(healthCheck);
        Assert.NotNull(endpoint);
    }
}
