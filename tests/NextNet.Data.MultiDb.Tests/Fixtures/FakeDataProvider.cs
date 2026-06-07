namespace NextNet.Data.MultiDb.Tests.Fixtures;

/// <summary>
/// A fake <see cref="IDataProvider"/> for testing multi-database scenarios.
/// Implements both the Abstractions and Providers <c>IDataProvider</c> interfaces.
/// Returns configurable health check results and tracks initialization.
/// </summary>
internal sealed class FakeDataProvider : NextNet.Data.Abstractions.Abstractions.IDataProvider,
    NextNet.Data.IDataProvider
{
    private readonly string _name;
    private bool _initialized;
    private bool _healthy;

    public FakeDataProvider(string name, bool initiallyHealthy = true)
    {
        _name = name;
        _healthy = initiallyHealthy;
    }

    // --- Abstractions IDataProvider ---
    string NextNet.Data.Abstractions.Abstractions.IDataProvider.Name => _name;

    Task NextNet.Data.Abstractions.Abstractions.IDataProvider.InitializeAsync(DataConfig config, CancellationToken cancellationToken)
    {
        _initialized = true;
        return Task.CompletedTask;
    }

    Task<HealthCheckResult> NextNet.Data.Abstractions.Abstractions.IDataProvider.IsHealthyAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(new HealthCheckResult(
            _healthy,
            _healthy ? "Healthy" : "Unhealthy",
            TimeSpan.Zero));
    }

    // --- Providers IDataProvider ---
    public string Name => _name;

    public string DisplayName => _name;

    public Version Version => new(1, 0, 0);

    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        _initialized = true;
        return Task.CompletedTask;
    }

    public Task<DataProviderHealthResult> IsHealthyAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_healthy
            ? DataProviderHealthResult.Healthy("Healthy")
            : DataProviderHealthResult.Unhealthy("Unhealthy"));
    }

    // --- Common ---
    public bool IsInitialized => _initialized;

    public void SetHealth(bool healthy)
    {
        _healthy = healthy;
    }
}
