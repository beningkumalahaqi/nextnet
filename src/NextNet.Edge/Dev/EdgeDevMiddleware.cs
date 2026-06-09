using Microsoft.AspNetCore.Http;
using NextNet.Edge.Compatibility;

namespace NextNet.Edge.Dev;

/// <summary>
/// Development middleware for the edge preview server.
/// Provides diagnostic information about edge compatibility for the current request,
/// including which APIs would be blocked and why.
/// </summary>
public sealed class EdgeDevMiddleware
{
    private readonly RequestDelegate _next;
    private readonly EdgeOptions _options;
    private readonly EdgeCompatibilityChecker _compatibilityChecker;

    /// <summary>
    /// Initializes a new instance of <see cref="EdgeDevMiddleware"/>.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="options">Edge configuration options.</param>
    /// <param name="compatibilityChecker">The edge compatibility checker.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public EdgeDevMiddleware(
        RequestDelegate next,
        EdgeOptions options,
        EdgeCompatibilityChecker compatibilityChecker)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _compatibilityChecker = compatibilityChecker ?? throw new ArgumentNullException(nameof(compatibilityChecker));
    }

    /// <summary>
    /// Invokes the edge dev middleware.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        // Add edge diagnostic headers for preview mode
        context.Response.Headers["x-edge-preview"] = "true";
        context.Response.Headers["x-edge-provider"] = _options.Provider;
        context.Response.Headers["x-edge-strict-mode"] = _options.Strict ? "true" : "false";
        context.Response.Headers["x-edge-max-bundle-size"] = _options.MaxBundleSize.ToString();

        // Handle the compatibility report endpoint
        if (context.Request.Path.StartsWithSegments("/__edge/compatibility"))
        {
            await HandleCompatibilityEndpoint(context);
            return;
        }

        await _next(context);
    }

    /// <summary>
    /// Handles the compatibility diagnostic endpoint at <c>/__edge/compatibility</c>.
    /// Returns a JSON report of edge compatibility status.
    /// </summary>
    private async Task HandleCompatibilityEndpoint(HttpContext context)
    {
        context.Response.ContentType = "application/json";

        var report = new
        {
            provider = _options.Provider,
            strictMode = _options.Strict,
            compatibilityChecking = _options.CheckCompatibility,
            sizeBudget = new
            {
                maxBundleSize = _options.MaxBundleSize,
                maxBundleSizeFormatted = FormatSize(_options.MaxBundleSize),
                maxStaticAssets = _options.MaxStaticAssets,
                maxDeploymentSize = _options.MaxDeploymentSize,
                maxDeploymentSizeFormatted = FormatSize(_options.MaxDeploymentSize),
            },
            allowedNamespaces = _compatibilityChecker.IsAllowedType("*") ? "All" : "Restricted",
            blockedApiCount = CountBlockedApis(),
        };

        var json = System.Text.Json.JsonSerializer.Serialize(report, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true,
        });

        await context.Response.WriteAsync(json);
    }

    private int CountBlockedApis()
    {
        var blockedTypes = new[]
        {
            "System.IO.File",
            "System.IO.Directory",
            "System.Data.DataTable",
            "System.Diagnostics.Process",
            "System.Net.Sockets.Socket",
            "System.Threading.Thread",
            "System.Reflection.Emit.DynamicMethod",
        };

        return blockedTypes.Count(t => !_compatibilityChecker.IsAllowedType(t));
    }

    private static string FormatSize(long bytes)
    {
        return bytes switch
        {
            < 1024 => $"{bytes} B",
            < 1024 * 1024 => $"{bytes / 1024.0:F1} KB",
            _ => $"{bytes / (1024.0 * 1024.0):F2} MB",
        };
    }
}
