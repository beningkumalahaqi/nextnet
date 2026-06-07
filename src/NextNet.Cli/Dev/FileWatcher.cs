using NextNet.Cli.UI;
using NextNet.Routing;
using NextNet.Routing.Models;

namespace NextNet.Cli.Dev;

/// <summary>
/// Wraps <see cref="RouteFileWatcher"/> for HMR in the dev server.
/// Provides file change events with debouncing and HMR signalling.
/// </summary>
public sealed class FileWatcher : IDisposable
{
    private readonly RouteFileWatcher _routeWatcher;
    private readonly NextNetConsole _console;
    private readonly string _appDir;
    private bool _disposed;

    /// <summary>
    /// Occurs when a route file has changed and HMR should be triggered.
    /// </summary>
    public event Action<FileChangeEvent>? OnFileChanged;

    /// <summary>
    /// Occurs when a full reload is needed (e.g., layout or config change).
    /// </summary>
    public event Action? OnFullReload;

    /// <summary>
    /// Initializes a new instance of <see cref="FileWatcher"/>.
    /// </summary>
    /// <param name="appDir">The absolute path to the application directory.</param>
    /// <param name="console">The console for output messages.</param>
    public FileWatcher(string appDir, NextNetConsole console)
    {
        _appDir = appDir ?? throw new ArgumentNullException(nameof(appDir));
        _console = console ?? throw new ArgumentNullException(nameof(console));

        _routeWatcher = new RouteFileWatcher(appDir);
    }

    /// <summary>
    /// Starts watching for file changes in the application directory.
    /// </summary>
    public void Start()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(FileWatcher));

        if (!Directory.Exists(_appDir))
        {
            _console.WriteWarning($"App directory '{_appDir}' does not exist. File watching disabled.");
            return;
        }

        _routeWatcher.OnChanged += OnRouteFileChanged;
        _routeWatcher.Start();

        _console.WriteInfo($"Watching {_appDir} for file changes...");
    }

    /// <summary>
    /// Stops watching for file changes.
    /// </summary>
    public void Stop()
    {
        _routeWatcher.OnChanged -= OnRouteFileChanged;
        _routeWatcher.Stop();
    }

    /// <summary>
    /// Handles route file change events from the underlying watcher.
    /// Determines whether a full reload or HMR patch is needed.
    /// </summary>
    private void OnRouteFileChanged(FileChangeEvent changeEvent)
    {
        var filePath = changeEvent.FilePath;
        var changeType = changeEvent.ChangeType;

        var fileName = Path.GetFileName(filePath);

        if (_console.IsPlain)
            _console.WriteLine($"[file] {changeType}: {fileName}");
        else
            _console.WriteInfo($"File {changeType.ToString().ToLowerInvariant()}: {fileName}");

        // Full reload needed for layout files and config changes
        if (IsFullReloadRequired(filePath))
        {
            _console.WriteInfo("Layout/config changed — triggering full reload...");
            OnFullReload?.Invoke();
        }
        else
        {
            // Standard HMR for page/route files
            OnFileChanged?.Invoke(changeEvent);
        }
    }

    /// <summary>
    /// Determines if a file change requires a full page reload (not just HMR).
    /// Layout changes and config changes require full reload.
    /// </summary>
    private static bool IsFullReloadRequired(string filePath)
    {
        var fileName = Path.GetFileName(filePath);

        // Layout changes affect the entire page structure
        if (string.Equals(fileName, "layout.cs", StringComparison.OrdinalIgnoreCase))
            return true;

        // Config changes may affect routing
        if (string.Equals(fileName, "nextnet.config.json", StringComparison.OrdinalIgnoreCase))
            return true;

        // Error page changes affect error rendering
        if (string.Equals(fileName, "error.cs", StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
            return;

        Stop();
        _routeWatcher.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
