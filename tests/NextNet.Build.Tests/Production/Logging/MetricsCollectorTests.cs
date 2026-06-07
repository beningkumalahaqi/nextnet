using NextNet.Build.Production.Logging;
using Xunit;

namespace NextNet.Build.Tests.Production.Logging;

public class MetricsCollectorTests
{
    private readonly MetricsCollector _collector = new();

    [Fact]
    public void RecordAndSnapshot_ReturnsCorrectCounts()
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
    public void RecordRequest_ErrorStatus_IncrementsErrorCount()
    {
        _collector.RecordRequest("GET", "/fail", 500, 50);

        var snapshot = _collector.GetSnapshot();
        Assert.Equal(1, snapshot.TotalErrors);
        Assert.Equal(100.0, snapshot.ErrorRate); // 1/1 = 100%
    }

    [Fact]
    public void RecordRequest_MultipleCallsToSameEndpoint_Aggregates()
    {
        _collector.RecordRequest("GET", "/api", 200, 10);
        _collector.RecordRequest("GET", "/api", 200, 20);
        _collector.RecordRequest("GET", "/api", 200, 30);

        var snapshot = _collector.GetSnapshot();
        var endpoint = Assert.Single(snapshot.Endpoints);

        Assert.Equal(3, endpoint.Count);
        Assert.Equal(20, endpoint.AvgDurationMs); // (10+20+30)/3
        Assert.Equal(30, endpoint.MaxDurationMs);
        Assert.Equal(10, endpoint.MinDurationMs);
    }

    [Fact]
    public void Reset_ClearsAllMetrics()
    {
        _collector.RecordRequest("GET", "/", 200, 10);
        _collector.Reset();

        var snapshot = _collector.GetSnapshot();
        Assert.Equal(0, snapshot.TotalRequests);
        Assert.Equal(0, snapshot.TotalErrors);
        Assert.Empty(snapshot.Endpoints);
    }

    [Fact]
    public void Snapshot_ContainsTimestamp()
    {
        var snapshot = _collector.GetSnapshot();
        Assert.True(snapshot.CollectedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void EmptyCollector_ReturnsZeroSnapshot()
    {
        var snapshot = _collector.GetSnapshot();
        Assert.Equal(0, snapshot.TotalRequests);
        Assert.Equal(0, snapshot.TotalErrors);
        Assert.Equal(0, snapshot.ErrorRate);
        Assert.Empty(snapshot.Endpoints);
    }
}
