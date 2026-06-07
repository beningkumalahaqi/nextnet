using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NextNet.Isr.Revalidation;
using NextNet.Logging;

namespace NextNet.Isr.Background;

/// <summary>
/// <see cref="IHostedService"/> that processes the revalidation queue in the background.
/// Reads from the <see cref="RevalidationQueue"/>, performs revalidation, and
/// handles the concurrency limit per route.
/// </summary>
public class BackgroundRevalidationService : IHostedService, IDisposable
{
    private readonly RevalidationQueue _queue;
    private readonly IIsrRevalidationManager _revalidationManager;
    private readonly INextNetLogger? _logger;
    private readonly SemaphoreSlim _globalThrottle;
    private CancellationTokenSource? _stoppingCts;
    private Task? _processingTask;

    /// <summary>
    /// Initializes a new instance of <see cref="BackgroundRevalidationService"/>.
    /// </summary>
    /// <param name="queue">The revalidation queue.</param>
    /// <param name="revalidationManager">The revalidation manager.</param>
    /// <param name="globalOptions">Global ISR options for throttle configuration.</param>
    /// <param name="logger">Optional logger.</param>
    /// <exception cref="ArgumentNullException">Thrown when required dependencies are null.</exception>
    public BackgroundRevalidationService(
        RevalidationQueue queue,
        IIsrRevalidationManager revalidationManager,
        IsrGlobalOptions globalOptions,
        INextNetLogger? logger = null)
    {
        _queue = queue ?? throw new ArgumentNullException(nameof(queue));
        _revalidationManager = revalidationManager ?? throw new ArgumentNullException(nameof(revalidationManager));

        if (globalOptions == null) throw new ArgumentNullException(nameof(globalOptions));
        _globalThrottle = new SemaphoreSlim(globalOptions.MaxConcurrentRegenerations);
        _logger = logger;
    }

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _stoppingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _processingTask = ProcessQueueAsync(_stoppingCts.Token);
        _logger?.Info("Background revalidation service started");
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger?.Info("Background revalidation service stopping...");

        if (_stoppingCts != null)
        {
            _stoppingCts.Cancel();
        }

        if (_processingTask != null)
        {
            try
            {
                await _processingTask.WaitAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // Expected on shutdown
            }
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_stoppingCts != null)
        {
            try { _stoppingCts.Cancel(); } catch (ObjectDisposedException) { /* already disposed */ }
            _stoppingCts.Dispose();
        }
        _globalThrottle?.Dispose();
    }

    /// <summary>
    /// Background processing loop that reads from the queue and performs revalidation.
    /// </summary>
    private async Task ProcessQueueAsync(CancellationToken cancellationToken)
    {
        try
        {
            await foreach (var request in _queue.ReadAllAsync(cancellationToken))
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                // Use the global throttle to limit concurrent regenerations
                await _globalThrottle.WaitAsync(cancellationToken);

                // Fire-and-forget the revalidation (tracked by the semaphore)
                _ = RevalidateAsync(request, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Shutdown requested
        }
        catch (Exception ex)
        {
            _logger?.Error("Unexpected error in revalidation processing loop: {Exception}", ex);
        }
    }

    private async Task RevalidateAsync(RevalidationRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (!string.IsNullOrEmpty(request.Route))
            {
                await _revalidationManager.RevalidateAsync(request.Route, cancellationToken);
                _queue.CompleteRevalidation(request.Route);
            }
            else if (request.Tags is { Count: > 0 })
            {
                await _revalidationManager.InvalidateByTagsAsync(request.Tags, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Shutdown requested during revalidation
        }
        catch (Exception ex)
        {
            _logger?.Error("Background revalidation failed for {Request}: {Exception}", request, ex);
        }
        finally
        {
            _globalThrottle.Release();
        }
    }
}
