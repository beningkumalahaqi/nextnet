using NextNet.Logging;
using NextNet.Routing.Errors;
using NextNet.Routing.Models;

namespace NextNet.Routing;

/// <summary>
/// Watches the application directory for file changes and raises events
/// with debounced notifications for route file updates.
/// </summary>
public sealed class RouteFileWatcher : IDisposable
{
    private readonly string _appDir;
    private readonly INextNetLogger? _logger;
    private FileSystemWatcher? _watcher;
    private readonly int _debounceMilliseconds;
    private Timer? _debounceTimer;
    private readonly object _lock = new();
    private readonly HashSet<(string FilePath, FileChangeType ChangeType)> _pendingEvents = new();
    private bool _disposed;

    /// <summary>
    /// Occurs when a file change is detected and debounced.
    /// </summary>
    public event Action<FileChangeEvent>? OnChanged;

    /// <summary>
    /// Initializes a new instance of <see cref="RouteFileWatcher"/>.
    /// </summary>
    /// <param name="appDir">The absolute path to the application directory to watch.</param>
    /// <param name="logger">Optional logger instance.</param>
    /// <param name="debounceMilliseconds">The debounce interval in milliseconds. Default is 50ms.</param>
    public RouteFileWatcher(
        string appDir,
        INextNetLogger? logger = null,
        int debounceMilliseconds = 50)
    {
        _appDir = appDir ?? throw new ArgumentNullException(nameof(appDir));
        _logger = logger;
        _debounceMilliseconds = debounceMilliseconds;
    }

    /// <summary>
    /// Starts watching the application directory for file changes.
    /// Only .cs files are monitored. Filter out non-route files in the event handler.
    /// </summary>
    public void Start()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(RouteFileWatcher));

        if (_watcher != null)
            return; // Already started

        if (!Directory.Exists(_appDir))
        {
            _logger?.Warn("[{Code}] Cannot start file watcher: directory '{AppDir}' does not exist.",
                RoutingErrorCodes.FileWatcherDirectoryNotFound, _appDir);
            return;
        }

        _watcher = new FileSystemWatcher(_appDir)
        {
            Filter = "*.cs",
            IncludeSubdirectories = true,
            EnableRaisingEvents = false,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime,
        };

        _watcher.Created += OnFileSystemEvent;
        _watcher.Changed += OnFileSystemEvent;
        _watcher.Deleted += OnFileSystemEvent;
        _watcher.Renamed += OnRenamedEvent;
        _watcher.Error += OnWatcherError;

        _watcher.EnableRaisingEvents = true;
        _logger?.Info("File watcher started on '{AppDir}' with {Debounce}ms debounce", _appDir, _debounceMilliseconds);
    }

    /// <summary>
    /// Stops watching the application directory.
    /// </summary>
    public void Stop()
    {
        if (_watcher != null)
        {
            _watcher.EnableRaisingEvents = false;
            _watcher.Created -= OnFileSystemEvent;
            _watcher.Changed -= OnFileSystemEvent;
            _watcher.Deleted -= OnFileSystemEvent;
            _watcher.Renamed -= OnRenamedEvent;
            _watcher.Error -= OnWatcherError;
            _watcher.Dispose();
            _watcher = null;
        }

        lock (_lock)
        {
            _debounceTimer?.Dispose();
            _debounceTimer = null;
            _pendingEvents.Clear();
        }

        _logger?.Info("File watcher stopped.");
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
            return;

        Stop();
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Handles file system events by queuing them for debouncing.
    /// </summary>
    private void OnFileSystemEvent(object sender, FileSystemEventArgs e)
    {
        var changeType = e.ChangeType switch
        {
            WatcherChangeTypes.Created => FileChangeType.Created,
            WatcherChangeTypes.Changed => FileChangeType.Modified,
            WatcherChangeTypes.Deleted => FileChangeType.Deleted,
            _ => FileChangeType.Modified,
        };

        // Filter out non-CS or non-route files early
        var fileName = Path.GetFileName(e.FullPath);
        if (ShouldSkipFile(fileName))
            return;

        QueueEvent(e.FullPath.Replace('\\', '/'), changeType);
    }

    /// <summary>
    /// Handles rename events by treating the old name as deleted and the new name as created.
    /// </summary>
    private void OnRenamedEvent(object sender, RenamedEventArgs e)
    {
        var oldFileName = Path.GetFileName(e.OldFullPath);
        var newFileName = Path.GetFileName(e.FullPath);

        // If old file was a route file, queue deletion
        if (!ShouldSkipFile(oldFileName))
        {
            QueueEvent(e.OldFullPath.Replace('\\', '/'), FileChangeType.Deleted);
        }

        // If new file is a route file, queue creation
        if (!ShouldSkipFile(newFileName))
        {
            QueueEvent(e.FullPath.Replace('\\', '/'), FileChangeType.Created);
        }
    }

    /// <summary>
    /// Handles watcher errors (e.g., buffer overflow) by logging.
    /// </summary>
    private void OnWatcherError(object sender, ErrorEventArgs e)
    {
        _logger?.Error("File watcher error: {Message}", e.GetException()?.Message);
    }

    /// <summary>
    /// Queues a file change event for debounced delivery.
    /// </summary>
    internal void QueueEvent(string filePath, FileChangeType changeType)
    {
        lock (_lock)
        {
            // For modified events, if we already have a pending create/modify, keep it
            // For deleted, always override previous pending events for the same file
            _pendingEvents.Remove((filePath, FileChangeType.Created));
            _pendingEvents.Remove((filePath, FileChangeType.Modified));
            _pendingEvents.Remove((filePath, FileChangeType.Deleted));
            _pendingEvents.Add((filePath, changeType));

            _debounceTimer?.Dispose();
            _debounceTimer = new Timer(
                FirePendingEvents,
                null,
                _debounceMilliseconds,
                Timeout.Infinite);
        }
    }

    /// <summary>
    /// Fires all pending events after the debounce period.
    /// </summary>
    private void FirePendingEvents(object? state)
    {
        List<(string FilePath, FileChangeType ChangeType)> eventsToFire;

        lock (_lock)
        {
            eventsToFire = new List<(string, FileChangeType)>(_pendingEvents);
            _pendingEvents.Clear();
            _debounceTimer?.Dispose();
            _debounceTimer = null;
        }

        foreach (var (filePath, changeType) in eventsToFire)
        {
            try
            {
                OnChanged?.Invoke(new FileChangeEvent(filePath, changeType));
            }
            catch (Exception ex)
            {
                _logger?.Error("Error in file change handler for '{FilePath}': {Message}",
                    filePath, ex.Message);
            }
        }
    }

    /// <summary>
    /// Determines whether a file should be skipped (non-route files like .g.cs, . razor.g.cs, etc.).
    /// </summary>
    internal static bool ShouldSkipFile(string fileName)
    {
        // Skip generated files and non-route files
        if (fileName.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase))
            return true;
        if (fileName.EndsWith(".razor.g.cs", StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    }
}
