using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using NextNet.Data;
using NextNet.Data.Abstractions.Abstractions;
using NextNet.Data.Abstractions.Configuration;

namespace NextNet.Data.Benchmarks.Benchmarks;

/// <summary>
/// Benchmarks for data provider resolution performance.
/// Ensures provider lookup meets SLA targets (&lt; 2ms per lookup).
/// </summary>
[MemoryDiagnoser]
[SimpleJob(launchCount: 1, warmupCount: 3, iterationCount: 10)]
public class ProviderResolutionBenchmarks
{
    private IDataProviderRegistry _registry = null!;
    private IDataProvider _provider = null!;

    /// <summary>
    /// Sets up the benchmark by registering a mock data provider.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        _provider = new MockDataProvider();

        var services = new ServiceCollection();
        services.AddSingleton<IDataProvider>(_provider);
        services.AddSingleton<IDataProviderRegistry>(sp =>
        {
            var registry = new SimpleProviderRegistry();
            registry.Register("MockProvider", sp.GetRequiredService<IDataProvider>());
            return registry;
        });

        var provider = services.BuildServiceProvider();
        _registry = provider.GetRequiredService<IDataProviderRegistry>();
    }

    /// <summary>
    /// Measures the time to resolve a default provider.
    /// SLA: &lt; 2ms.
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("ProviderResolution")]
    public IDataProvider? ResolveDefaultProvider_Time()
    {
        return _registry.GetDefault();
    }

    /// <summary>
    /// Measures the time to resolve a named provider.
    /// SLA: &lt; 2ms.
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("ProviderResolution")]
    public IDataProvider? ResolveNamedProvider_Time()
    {
        return _registry.GetByName("MockProvider");
    }

    /// <summary>
    /// Measures the performance of building a DataConfig.
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("ConfigBuilding")]
    public DataConfig BuildDataConfig_Time()
    {
        return new DataConfig(
            DefaultConnection: "Default",
            Connections: new Dictionary<string, ConnectionConfig>
            {
                ["Default"] = new("Server=localhost;Database=Test;", "Mock"),
                ["Analytics"] = new("Server=analytics;Database=Reports;", "Mock")
            });
    }
}

/// <summary>
/// Simple in-memory provider registry for benchmarking.
/// </summary>
internal sealed class SimpleProviderRegistry : IDataProviderRegistry
{
    private readonly List<IDataProvider> _providers = new();

    public void Register(string name, IDataProvider provider)
    {
        _providers.Add(provider);
    }

    public IReadOnlyList<IDataProvider> GetAll() => _providers.AsReadOnly();

    public IDataProvider? GetByName(string name)
        => _providers.FirstOrDefault(p => p.Name == name);

    public IDataProvider? GetDefault()
        => _providers.FirstOrDefault();
}
