using System.Text.Json;
using BenchmarkDotNet.Attributes;
using NextNet.Data.Abstractions.Models;

namespace NextNet.Data.Benchmarks.Benchmarks;

/// <summary>
/// Benchmarks for health check performance and overhead.
/// Ensures health check aggregation and response generation meet SLA targets.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(launchCount: 1, warmupCount: 3, iterationCount: 10)]
public class HealthCheckBenchmarks
{
    private HealthCheckResult _healthyResult = null!;
    private HealthCheckResult _unhealthyResult = null!;
    private List<HealthCheckResult> _threeResults = null!;
    private string _serializedResult = null!;

    /// <summary>
    /// Prepares health check result instances for benchmarking.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        _healthyResult = new HealthCheckResult(
            true,
            "Healthy",
            TimeSpan.FromMilliseconds(5),
            "Database is reachable and responding.",
            new Dictionary<string, object> { { "version", "1.0.0" } });

        _unhealthyResult = new HealthCheckResult(
            false,
            "Unhealthy",
            TimeSpan.FromMilliseconds(100),
            "Connection refused.",
            new Dictionary<string, object> { { "error", "Timeout" } });

        _threeResults = new List<HealthCheckResult>
        {
            _healthyResult,
            new HealthCheckResult(true, "Healthy", TimeSpan.FromMilliseconds(2), "OK"),
            _unhealthyResult
        };

        _serializedResult = JsonSerializer.Serialize(_healthyResult);
    }

    /// <summary>
    /// Measures the overhead of aggregated health check with zero registered providers.
    /// SLA: &lt; 1ms.
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("HealthCheck")]
    public bool HealthCheck_NoProviders()
    {
        var results = Array.Empty<HealthCheckResult>();
        return results.All(r => r.IsHealthy);
    }

    /// <summary>
    /// Measures the overhead of aggregating a single healthy provider result.
    /// SLA: &lt; 5ms.
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("HealthCheck")]
    public (bool, int) HealthCheck_1Provider_Healthy()
    {
        var results = new[] { _healthyResult };
        return (results.All(r => r.IsHealthy), results.Length);
    }

    /// <summary>
    /// Measures the overhead of aggregating three provider results with mixed status.
    /// SLA: &lt; 5ms.
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("HealthCheck")]
    public (bool, int) HealthCheck_3Providers()
    {
        return (_threeResults.All(r => r.IsHealthy), _threeResults.Count);
    }

    /// <summary>
    /// Measures JSON serialization performance of a health check result.
    /// SLA: &lt; 1ms.
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("HealthCheck")]
    public string HealthCheck_Serialize()
    {
        return JsonSerializer.Serialize(_healthyResult);
    }
}
