using NextNet.Data;

namespace NextNet.Data.Benchmarks.Benchmarks;

/// <summary>
/// Lightweight mock data provider for benchmarking purposes.
/// Implements <see cref="IDataProvider"/> from the Providers package with minimal overhead.
/// </summary>
internal sealed class MockDataProvider : IDataProvider
{
    /// <inheritdoc />
    public string Name => "MockProvider";

    /// <inheritdoc />
    public string DisplayName => "Mock Benchmark Provider";

    /// <inheritdoc />
    public Version Version => new(1, 0, 0);

    /// <inheritdoc />
    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<DataProviderHealthResult> IsHealthyAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(DataProviderHealthResult.Healthy("Mock provider is healthy"));
    }
}

/// <summary>
/// Lightweight mock data connection for benchmarking purposes.
/// </summary>
internal sealed class MockDataConnection
{
    /// <summary>
    /// Gets the logical name of this connection.
    /// </summary>
    public string Name => "MockConnection";

    /// <summary>
    /// Gets the resolved connection string for this connection.
    /// </summary>
    public string ConnectionString => "Server=localhost;Database=Benchmark;";

    /// <summary>
    /// Gets the name of the data provider that owns this connection.
    /// </summary>
    public string ProviderName => "MockProvider";
}
