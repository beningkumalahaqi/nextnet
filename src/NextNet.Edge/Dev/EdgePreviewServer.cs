using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using NextNet.Edge.Adapters;
using NextNet.Edge.Compatibility;
using NextNet.Edge.Middleware;

namespace NextNet.Edge.Dev;

/// <summary>
/// Preview server that simulates edge runtime constraints locally.
/// Run with <c>--edge</c> flag to enable edge simulation mode.
/// The preview server applies the same API restrictions and size budgets
/// that would apply on a real edge deployment.
/// </summary>
public sealed class EdgePreviewServer
{
    private readonly EdgeOptions _options;
    private readonly EdgeCompatibilityChecker _compatibilityChecker;
    private readonly AdapterRegistry _adapterRegistry;

    /// <summary>
    /// Initializes a new instance of <see cref="EdgePreviewServer"/>.
    /// </summary>
    /// <param name="options">Edge configuration options.</param>
    /// <param name="compatibilityChecker">The edge compatibility checker.</param>
    /// <param name="adapterRegistry">The adapter registry.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public EdgePreviewServer(
        EdgeOptions options,
        EdgeCompatibilityChecker compatibilityChecker,
        AdapterRegistry adapterRegistry)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _compatibilityChecker = compatibilityChecker ?? throw new ArgumentNullException(nameof(compatibilityChecker));
        _adapterRegistry = adapterRegistry ?? throw new ArgumentNullException(nameof(adapterRegistry));
    }

    /// <summary>
    /// Starts the edge preview server.
    /// </summary>
    /// <param name="port">The port to listen on. Defaults to 3000.</param>
    /// <param name="contentRoot">The content root directory for static files.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that completes when the server stops.</returns>
    public async Task StartAsync(
        int port = 3000,
        string? contentRoot = null,
        CancellationToken cancellationToken = default)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ContentRootPath = contentRoot ?? Directory.GetCurrentDirectory(),
        });

        // Configure to use the specified port
        builder.WebHost.UseUrls($"http://localhost:{port}");

        // Register edge services
        builder.Services.AddSingleton(_options);
        builder.Services.AddSingleton(_compatibilityChecker);
        builder.Services.AddSingleton(_adapterRegistry);

        // Register the edge preview middleware
        builder.Services.AddScoped<EdgeDevMiddleware>();

        var app = builder.Build();

        // Add edge middleware (simulates edge constraints)
        app.UseMiddleware<global::NextNet.Edge.Middleware.EdgeMiddleware>();

        // Add edge dev middleware for preview-specific features
        app.UseMiddleware<EdgeDevMiddleware>();

        // Default static files
        app.UseStaticFiles();

        // Default catch-all response
        app.Run(async context =>
        {
            context.Response.StatusCode = 200;
            context.Response.ContentType = "text/html";
            await context.Response.WriteAsync(
                $"<html><body><h1>NextNet Edge Preview Server</h1>" +
                $"<p>Provider: {_options.Provider}</p>" +
                $"<p>Port: {port}</p>" +
                $"<p>Edge simulation: {(context.Response.Headers.ContainsKey("x-edge-simulated") ? "enabled" : "disabled")}</p>" +
                $"</body></html>");
        });

        // Run the app — cancellation is handled by the caller
        await app.RunAsync();
    }

    /// <summary>
    /// Creates and starts the preview server using a default configuration.
    /// </summary>
    /// <param name="port">The port to listen on.</param>
    /// <param name="options">Optional edge configuration.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that completes when the server stops.</returns>
    public static async Task RunAsync(
        int port = 3000,
        EdgeOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= new EdgeOptions { Enabled = true };

        var whitelist = new EdgeApiWhitelist();
        var checker = new EdgeCompatibilityChecker(whitelist, options);
        var services = new ServiceCollection();
        services.AddSingleton(options);
        var sp = services.BuildServiceProvider();
        var registry = new AdapterRegistry(sp);

        var server = new EdgePreviewServer(options, checker, registry);
        await server.StartAsync(port, cancellationToken: cancellationToken);
    }
}
