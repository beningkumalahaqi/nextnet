using NextNet.Build.Production.Health;
using Xunit;

namespace NextNet.Build.Tests.Production.Health;

public class NextNetHealthCheckTests
{
    [Fact]
    public async Task CheckAsync_ReturnsExpectedStatus()
    {
        var healthCheck = new NextNetHealthCheck("1.0.0-test");
        var report = await healthCheck.CheckAsync();

        Assert.NotNull(report);
        Assert.NotEmpty(report.Checks);
        Assert.Equal("1.0.0-test", report.Version);
    }

    [Fact]
    public async Task CheckAsync_IncludesBasicHealthCheck()
    {
        var healthCheck = new NextNetHealthCheck();
        var report = await healthCheck.CheckAsync();

        Assert.Contains(report.Checks, c => c.Name == "Application Health");
    }

    [Fact]
    public async Task CheckAsync_IncludesMemoryCheck()
    {
        var healthCheck = new NextNetHealthCheck();
        var report = await healthCheck.CheckAsync();

        Assert.Contains(report.Checks, c => c.Name == "Memory Usage");
    }

    [Fact]
    public async Task CheckAsync_IncludesRouteRegistryCheck()
    {
        var healthCheck = new NextNetHealthCheck();
        var report = await healthCheck.CheckAsync();

        Assert.Contains(report.Checks, c => c.Name == "Route Registry");
    }

    [Fact]
    public async Task CheckAsync_MemoryCheck_HasData()
    {
        var healthCheck = new NextNetHealthCheck();
        var report = await healthCheck.CheckAsync();

        var memoryCheck = Assert.Single(report.Checks, c => c.Name == "Memory Usage");
        Assert.NotNull(memoryCheck.Data);
        Assert.True(memoryCheck.Data.ContainsKey("workingSetMb"));
        Assert.True(memoryCheck.Data.ContainsKey("peakWorkingSetMb"));
    }

    [Fact]
    public async Task CheckAsync_AllChecksHaveDuration()
    {
        var healthCheck = new NextNetHealthCheck();
        var report = await healthCheck.CheckAsync();

        foreach (var check in report.Checks)
        {
            Assert.True(check.DurationMs >= 0, $"{check.Name} should have non-negative duration");
        }
    }

    [Fact]
    public async Task CheckAsync_UptimeIsFormatted()
    {
        var healthCheck = new NextNetHealthCheck();
        var report = await healthCheck.CheckAsync();

        Assert.NotNull(report.Uptime);
        Assert.NotEmpty(report.Uptime);
    }
}
