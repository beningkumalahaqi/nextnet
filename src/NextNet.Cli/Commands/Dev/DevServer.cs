using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NextNet.Cli.Config;
using NextNet.Cli.Dev;
using NextNet.Cli.UI;
using NextNet.Conventions;
using NextNet.Rendering;
using NextNet.Rendering.Extensions;
using NextNet.Routing;

namespace NextNet.Cli.Commands.Dev;

/// <summary>
/// Manages the development server lifecycle, including ASP.NET Core hosting,
/// SSR middleware, file watching, HMR, and graceful shutdown.
/// </summary>
public sealed class DevServer : IDisposable
{
    private readonly NextNetConsole _console;
    private readonly int _port;
    private readonly bool _https;
    private readonly string _hostname;
    private readonly bool _noHmr;
    private readonly bool _verbose;
    private readonly CancellationTokenSource _cts = new();
    private FileWatcher? _fileWatcher;
    private Task? _serverTask;
    private WebApplication? _app;

    /// <summary>
    /// Creates a new dev server instance.
    /// </summary>
    public DevServer(
        NextNetConsole console,
        int port = 3000,
        bool https = false,
        string hostname = "localhost",
        bool noHmr = false,
        bool verbose = false)
    {
        _console = console ?? throw new ArgumentNullException(nameof(console));
        _port = port;
        _https = https;
        _hostname = hostname;
        _noHmr = noHmr;
        _verbose = verbose;
    }

    /// <summary>
    /// Start the development server.
    /// </summary>
    public async Task<int> StartAsync()
    {
        try
        {
            _console.WriteHeading("Starting NextNet dev server...");
            _console.WriteLine();

            // Check port availability
            if (!IsPortAvailable(_port))
            {
                _console.WriteError($"Port {_port} is already in use.");
                _console.WriteInfo("Try: nextnet dev --port 3001");
                return 2;
            }

            // Check if running in a NextNet project
            var config = ConfigLoader.Load();
            var appDir = config?.Routing?.Dir ?? NextNetConventions.AppDirectory;
            var appDirAbsolute = Path.GetFullPath(appDir);

            if (!Directory.Exists(appDirAbsolute))
            {
                _console.WriteWarning($"App directory '{appDir}/' not found. Create it or run from a NextNet project root.");
                _console.WriteLine();
            }

            // Start file watcher (unless --no-hmr)
            if (!_noHmr)
            {
                StartFileWatcher(appDirAbsolute);
            }

            // Build and start the ASP.NET Core web application
            _app = BuildWebApplication(appDirAbsolute);

            // Start the server in the background
            _serverTask = _app.StartAsync();

            // Display startup banner
            ShowStartupBanner();

            // Wait for shutdown signal
            _console.WriteMuted("Press Ctrl+C to stop the dev server.");
            _console.WriteLine();

            try
            {
                await Task.Delay(Timeout.Infinite, _cts.Token);
            }
            catch (TaskCanceledException)
            {
                // Graceful shutdown
            }

            await StopAsync();
            return 0;
        }
        catch (Exception ex)
        {
            _console.WriteError($"Dev server error: {ex.Message}");
            if (_verbose && ex.StackTrace is not null)
                _console.WriteMuted(ex.StackTrace);
            return 1;
        }
    }

    /// <summary>
    /// Stop the development server gracefully.
    /// </summary>
    public async Task StopAsync()
    {
        _cts.Cancel();

        if (_fileWatcher is not null)
        {
            _fileWatcher.Stop();
            _fileWatcher.Dispose();
            _fileWatcher = null;
        }

        if (_app is not null)
        {
            try
            {
                await _app.StopAsync();
            }
            catch
            {
                // Ignore shutdown errors
            }

            await _app.DisposeAsync();
            _app = null;
        }

        _console.WriteInfo("Dev server stopped.");
    }

    /// <summary>
    /// Builds the ASP.NET Core web application with NextNet SSR middleware.
    /// </summary>
    private WebApplication BuildWebApplication(string appDirAbsolute)
    {
        var builder = WebApplication.CreateBuilder();

        // Configure URLs
        var scheme = _https ? "https" : "http";
        builder.WebHost.UseUrls($"{scheme}://{_hostname}:{_port}");

        // Register NextNet rendering services
        builder.Services.AddNextNetRendering(options =>
        {
            options.Streaming = true;
            options.EnableCompression = true;
        });

        // Register route scanner and manifest
        builder.Services.AddSingleton(sp =>
        {
            var scanner = new RouteScanner(appDirAbsolute);
            return scanner.Scan();
        });

        // Register SSR renderer with scanned manifest
        builder.Services.AddScoped(sp =>
        {
            var manifest = sp.GetRequiredService<RouteManifest>();
            var svcProvider = sp.GetRequiredService<IServiceProvider>();
            return new SsrRenderer(svcProvider, manifest);
        });

        // Build the app
        var app = builder.Build();

        // Add NextNet SSR middleware
        app.UseNextNet();

        // Add a fallback for static files (if any in public/)
        var publicDir = Path.Combine(
            Path.GetDirectoryName(appDirAbsolute) ?? Environment.CurrentDirectory,
            NextNetConventions.PublicDirectory);
        if (Directory.Exists(publicDir))
        {
            app.UseStaticFiles();
        }

        // Add a default route handler
        app.MapFallback(async (HttpContext context) =>
        {
            context.Response.ContentType = "text/plain";
            await context.Response.WriteAsync("NextNet Dev Server — no route matched");
        });

        return app;
    }

    private void ShowStartupBanner()
    {
        var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.1.0";

        // Build info panel content
        var content = new System.Text.StringBuilder();
        var scheme = _https ? "https" : "http";

        content.AppendLine($"  NextNet v{version}");
        content.AppendLine($"  {(char)0x2502}");
        content.AppendLine($"  {(char)0x251C} \u2705 Ready");
        content.AppendLine($"  {(char)0x251C} \uD83D\uDCE1 {scheme}://{_hostname}:{_port}");
        content.AppendLine($"  {(char)0x2502}");
        content.AppendLine($"  {(char)0x251C} SSR: enabled");
        content.AppendLine($"  {(char)0x2514} HMR: {(_noHmr ? "disabled" : "watching for changes...")}");

        if (!_console.IsPlain)
        {
            var panel = new NextNetPanel(" Dev Server ", _console.Mode);
            panel.SetContent(content.ToString());
            _console.Write(panel.GetSpectrePanel());
        }
        else
        {
            _console.WriteLine(content.ToString());
        }
        _console.WriteLine();
    }

    private void StartFileWatcher(string appDir)
    {
        try
        {
            _fileWatcher = new FileWatcher(appDir, _console);
            _fileWatcher.OnFullReload += () =>
            {
                _console.WriteInfo("Full reload triggered by file change...");
                // Signal HMR reload — in a real implementation this would
                // send a WebSocket message to connected clients
            };
            _fileWatcher.Start();
        }
        catch (Exception ex)
        {
            _console.WriteWarning($"File watcher failed to start: {ex.Message}");
        }
    }

    private static bool IsPortAvailable(int port)
    {
        try
        {
            var listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, port);
            listener.Start();
            listener.Stop();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _fileWatcher?.Dispose();
        _app?.DisposeAsync().AsTask().GetAwaiter().GetResult();
    }
}
