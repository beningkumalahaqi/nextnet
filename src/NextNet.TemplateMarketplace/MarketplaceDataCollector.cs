namespace NextNet.TemplateMarketplace;

using System.Collections.Concurrent;

/// <summary>
/// Opt-in anonymous data collector for marketplace usage analytics.
/// Only records data when <see cref="MarketplaceOptions.EnableDataCollection"/> is true.
/// Events are batched and flushed to a local JSONL buffer file.
/// V4+ will add HTTP delivery to the marketplace API.
/// </summary>
public sealed class MarketplaceDataCollector : IAsyncDisposable
{
    private readonly MarketplaceOptions _options;
    private readonly ConcurrentQueue<DataCollectionEvent> _queue = new();
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _flushTask;

    /// <summary>Initializes a new instance of the <see cref="MarketplaceDataCollector"/>.</summary>
    public MarketplaceDataCollector(MarketplaceOptions options)
    {
        _options = options;
        if (options.EnableDataCollection)
        {
            _flushTask = Task.Run(() => FlushLoopAsync(_cts.Token));
        }
        else
        {
            _flushTask = Task.CompletedTask;
        }
    }

    /// <summary>Records a template install event.</summary>
    public void RecordInstall(string templateName, string version)
    {
        if (!_options.EnableDataCollection) return;
        _queue.Enqueue(new DataCollectionEvent
        {
            Type = DataCollectionEventType.TemplateInstall,
            TemplateName = templateName,
            TemplateVersion = version,
            AnonymousUserId = GetAnonymousUserId()
        });
    }

    /// <summary>Records a template generation event.</summary>
    public void RecordGeneration(string templateName, string version, bool success, TimeSpan duration)
    {
        if (!_options.EnableDataCollection) return;
        _queue.Enqueue(new DataCollectionEvent
        {
            Type = DataCollectionEventType.TemplateGeneration,
            TemplateName = templateName,
            TemplateVersion = version,
            Success = success,
            Duration = duration,
            AnonymousUserId = GetAnonymousUserId()
        });
    }

    /// <summary>Records an error event.</summary>
    public void RecordError(string templateName, string version, string errorMessage)
    {
        if (!_options.EnableDataCollection) return;
        _queue.Enqueue(new DataCollectionEvent
        {
            Type = DataCollectionEventType.Error,
            TemplateName = templateName,
            TemplateVersion = version,
            ErrorMessage = errorMessage,
            AnonymousUserId = GetAnonymousUserId()
        });
    }

    /// <summary>Current number of events in the queue awaiting flush.</summary>
    public int QueueSize => _queue.Count;

    /// <summary>Gets or creates a stable anonymous user ID stored in ~/.nextnet/anonymous-id.</summary>
    private static string GetAnonymousUserId()
    {
        var idPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".nextnet", "anonymous-id");
        if (File.Exists(idPath))
        {
            return File.ReadAllText(idPath).Trim();
        }

        var newId = Guid.NewGuid().ToString("N");
        var dir = Path.GetDirectoryName(idPath)!;
        Directory.CreateDirectory(dir);
        File.WriteAllText(idPath, newId);
        return newId;
    }

    /// <summary>Background loop that flushes the event queue at the configured interval.</summary>
    private async Task FlushLoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_options.DataCollectionFlushInterval, ct);
                await FlushAsync(ct);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch
            {
                // Best-effort collection — failures are silently ignored
            }
        }
    }

    /// <summary>Flushes pending events to the local buffer file.</summary>
    public async Task FlushAsync(CancellationToken ct = default)
    {
        var batch = new List<DataCollectionEvent>();
        while (_queue.TryDequeue(out var evt))
        {
            batch.Add(evt);
            if (batch.Count >= 100) break;
        }

        if (batch.Count == 0) return;

        // Write to local buffer — V4+ will also POST to the marketplace API
        var logPath = Path.Combine(_options.CacheDirectory, "data-collection.jsonl");
        Directory.CreateDirectory(_options.CacheDirectory);
        var lines = batch.Select(e => JsonSerializer.Serialize(e)).ToArray();
        await File.AppendAllLinesAsync(logPath, lines, ct);
    }

    /// <summary>Disposes the collector, flushing any remaining events.</summary>
    public async ValueTask DisposeAsync()
    {
        _cts.Cancel();
        try
        {
            await _flushTask;
        }
        catch
        {
            // Ignore cancellation exceptions
        }

        await FlushAsync();
        _cts.Dispose();
    }
}
