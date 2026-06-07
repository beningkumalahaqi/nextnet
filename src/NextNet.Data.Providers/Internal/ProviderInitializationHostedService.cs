using Microsoft.Extensions.Hosting;
using NextNet.Data.Exceptions;

namespace NextNet.Data.Internal;

/// <summary>
/// <see cref="IHostedService"/> that initializes all registered data providers
/// at application startup. Runs asynchronously before the first request is processed.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="ProviderInitializationHostedService"/> is registered automatically
/// by <see cref="NextNetDataBuilder.Build"/> during the deferred build phase.
/// It resolves all providers from the <see cref="IDataProviderRegistry"/> and calls
/// <see cref="IDataProvider.InitializeAsync"/> on each one.
/// </para>
/// <para>
/// Initialization behavior is controlled by <see cref="DataAbstractionsOptions.FailOnInitializationError"/>:
/// - When <c>true</c> (default), a failed initialization throws <see cref="ProviderInitializationException"/>,
///   preventing the application from starting.
/// - When <c>false</c>, initialization errors are logged but the application continues.
/// </para>
/// <para>
/// All providers are initialized in parallel using <c>Task.WhenAll</c> to minimize startup time.
/// </para>
/// </remarks>
internal sealed class ProviderInitializationHostedService : IHostedService
{
    private readonly IDataProviderRegistry _registry;
    private readonly ILogger<ProviderInitializationHostedService> _logger;
    private readonly DataAbstractionsOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProviderInitializationHostedService"/> class.
    /// </summary>
    /// <param name="registry">The provider registry containing all registered providers.</param>
    /// <param name="logger">The logger for initialization diagnostics.</param>
    /// <param name="options">The data abstraction options controlling initialization behavior.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="registry"/>, <paramref name="logger"/>, or <paramref name="options"/> is <c>null</c>.
    /// </exception>
    public ProviderInitializationHostedService(
        IDataProviderRegistry registry,
        ILogger<ProviderInitializationHostedService> logger,
        DataAbstractionsOptions options)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Triggered when the application host is ready to start the service.
    /// Initializes all registered providers in parallel.
    /// </summary>
    /// <param name="cancellationToken">Indicates that the start process has been aborted.</param>
    /// <returns>A task that represents the asynchronous initialization operation.</returns>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var providers = _registry.GetAll();

        if (providers.Count == 0)
        {
            _logger.LogInformation("No data providers registered. Skipping initialization.");
            return;
        }

        _logger.LogInformation("Initializing {ProviderCount} data provider(s)...", providers.Count);

        var initTasks = providers.Select(provider => InitializeProviderAsync(provider, cancellationToken));
        var results = await Task.WhenAll(initTasks);

        var failures = results.Where(r => r is not null).ToList();

        if (failures.Count > 0)
        {
            var errorMessage = $"{failures.Count} provider(s) failed initialization.";
            _logger.LogError("Provider initialization failed: {ErrorMessage}", errorMessage);

            if (_options.FailOnInitializationError)
            {
                throw new ProviderInitializationException(
                    string.Join(", ", failures.Select(f => f!.ProviderName)),
                    new AggregateException(failures.Select(f => f!.Exception)!));
            }
        }
    }

    /// <summary>
    /// Triggered when the application host is performing a graceful shutdown.
    /// No special shutdown logic is required for providers.
    /// </summary>
    /// <param name="cancellationToken">Indicates that the shutdown process should no longer be graceful.</param>
    /// <returns>A completed task.</returns>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Data provider shutdown complete.");
        return Task.CompletedTask;
    }

    private async Task<InitializationFailure?> InitializeProviderAsync(IDataProvider provider, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Initializing provider '{ProviderName}'...", provider.Name);
            await provider.InitializeAsync(cancellationToken);
            _logger.LogInformation("Provider '{ProviderName}' initialized successfully.", provider.Name);
            return null;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Provider '{ProviderName}' initialization was cancelled.", provider.Name);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Provider '{ProviderName}' initialization failed: {ErrorMessage}", provider.Name, ex.Message);
            return new InitializationFailure(provider.Name, ex);
        }
    }

    private sealed record InitializationFailure(string ProviderName, Exception Exception);
}
