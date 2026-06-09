using NextNet.Build.Production.Logging;
using Xunit;

namespace NextNet.Build.Tests.Production.Logging;

public class MetricsCollectorTests
{
    private readonly MetricsCollector _collector = new();

    [Fact]
    public void RecordAndSnapshot_Should_ReturnCorrectCounts_When_MultipleRequests()
    {
        _collector.RecordRequest("GET", "/", 200, 10);
        _collector.RecordRequest("GET", "/about", 200, 20);
        _collector.RecordRequest("POST", "/api/data", 500, 100);

        var snapshot = _collector.GetSnapshot();

        Assert.Equal(3, snapshot.TotalRequests);
        Assert.Equal(1, snapshot.TotalErrors);
        Assert.Equal(3, snapshot.Endpoints.Count);
    }

    [Fact]
    public void RecordRequest_Should_IncrementErrorCount_When_ErrorStatus()
    {
        _collector.RecordRequest("GET", "/fail", 500, 50);

        var snapshot = _collector.GetSnapshot();
        Assert.Equal(1, snapshot.TotalErrors);
        Assert.Equal(100.0, snapshot.ErrorRate);
    }

    [Fact]
    public void RecordRequest_Should_Aggregate_When_MultipleCallsToSameEndpoint()
    {
        _collector.RecordRequest("GET", "/api", 200, 10);
        _collector.RecordRequest("GET", "/api", 200, 20);
        _collector.RecordRequest("GET", "/api", 200, 30);

        var snapshot = _collector.GetSnapshot();
        var endpoint = Assert.Single(snapshot.Endpoints);

        Assert.Equal(3, endpoint.Count);
        Assert.Equal(20, endpoint.AvgDurationMs);
        Assert.Equal(30, endpoint.MaxDurationMs);
        Assert.Equal(10, endpoint.MinDurationMs);
    }

    [Fact]
    public void Reset_Should_ClearAllMetrics_When_Called()
    {
        _collector.RecordRequest("GET", "/", 200, 10);
        _collector.Reset();

        var snapshot = _collector.GetSnapshot();
        Assert.Equal(0, snapshot.TotalRequests);
        Assert.Equal(0, snapshot.TotalErrors);
        Assert.Empty(snapshot.Endpoints);
    }

    [Fact]
    public void Snapshot_Should_ContainTimestamp_When_Retrieved()
    {
        var snapshot = _collector.GetSnapshot();
        Assert.True(snapshot.CollectedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void EmptyCollector_Should_ReturnZeroSnapshot_When_Queried()
    {
        var snapshot = _collector.GetSnapshot();
        Assert.Equal(0, snapshot.TotalRequests);
        Assert.Equal(0, snapshot.TotalErrors);
        Assert.Equal(0, snapshot.ErrorRate);
        Assert.Empty(snapshot.Endpoints);
    }
}
