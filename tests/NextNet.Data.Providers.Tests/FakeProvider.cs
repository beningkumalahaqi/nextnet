namespace NextNet.Data.Providers.Tests;

/// <summary>
/// A fake <see cref="IDataProvider"/> implementation for testing purposes.
/// Allows control over initialization and health check behavior.
/// </summary>
public sealed class FakeProvider : IDataProvider
{
    private readonly Func<CancellationToken, Task>? _onInitialize;
    private readonly Func<CancellationToken, Task<DataProviderHealthResult>>? _onHealthCheck;

    /// <summary>
    /// Initializes a new instance of the <see cref="FakeProvider"/> class.
    /// </summary>
    /// <param name="name">The provider name. Defaults to "FakeProvider".</param>
    /// <param name="onInitialize">Optional callback invoked during <see cref="InitializeAsync"/>.</param>
    /// <param name="onHealthCheck">Optional callback invoked during <see cref="IsHealthyAsync"/>.</param>
    public FakeProvider(
        string? name = null,
        Func<CancellationToken, Task>? onInitialize = null,
        Func<CancellationToken, Task<DataProviderHealthResult>>? onHealthCheck = null)
    {
        Name = name ?? "FakeProvider";
        _onInitialize = onInitialize;
        _onHealthCheck = onHealthCheck;
    }

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public string DisplayName => $"{Name} Display";

    /// <inheritdoc />
    public Version Version => new(1, 0, 0);

    /// <inheritdoc />
    public Task InitializeAsync(CancellationToken cancellationToken = default)
        => _onInitialize is not null
            ? _onInitialize(cancellationToken)
            : Task.CompletedTask;

    /// <inheritdoc />
    public Task<DataProviderHealthResult> IsHealthyAsync(CancellationToken cancellationToken = default)
        => _onHealthCheck is not null
            ? _onHealthCheck(cancellationToken)
            : Task.FromResult(DataProviderHealthResult.Healthy());

    /// <summary>
    /// Gets a value indicating how many times <see cref="InitializeAsync"/> was called.
    /// </summary>
    public int InitializeCallCount { get; private set; }

    /// <summary>
    /// Gets a value indicating how many times <see cref="IsHealthyAsync"/> was called.
    /// </summary>
    public int HealthCheckCallCount { get; private set; }
}
