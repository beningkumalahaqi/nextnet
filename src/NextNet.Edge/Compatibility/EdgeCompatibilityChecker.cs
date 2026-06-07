using NextNet.Configuration;
using NextNet.Routing;

namespace NextNet.Edge.Compatibility;

/// <summary>
/// Analyzes NextNet projects for edge runtime compatibility.
/// Checks route manifests, middleware registrations, and configuration
/// against the allowed edge API surface.
/// </summary>
public class EdgeCompatibilityChecker
{
    private readonly EdgeApiWhitelist _whitelist;
    private readonly EdgeOptions _options;

    /// <summary>
    /// Initializes a new instance of <see cref="EdgeCompatibilityChecker"/>.
    /// </summary>
    /// <param name="whitelist">The API whitelist for edge compatibility.</param>
    /// <param name="options">Optional edge configuration options.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="whitelist"/> is null.</exception>
    public EdgeCompatibilityChecker(EdgeApiWhitelist whitelist, EdgeOptions? options = null)
    {
        _whitelist = whitelist ?? throw new ArgumentNullException(nameof(whitelist));
        _options = options ?? new EdgeOptions();
    }

    /// <summary>
    /// Runs a full compatibility check against the route manifest and configuration.
    /// </summary>
    /// <param name="manifest">The route manifest to check.</param>
    /// <param name="config">The NextNet configuration to check.</param>
    /// <returns>A report containing all violations found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="manifest"/> or <paramref name="config"/> is null.</exception>
    public EdgeCompatibilityReport Check(RouteManifest manifest, NextNetConfig config)
    {
        if (manifest == null) throw new ArgumentNullException(nameof(manifest));
        if (config == null) throw new ArgumentNullException(nameof(config));

        var report = new EdgeCompatibilityReport();

        CheckStaticGeneration(manifest, config, report);
        CheckRoutes(manifest, report);
        CheckMiddleware(manifest, report);
        CheckServerActions(manifest, report);
        CheckStreaming(config, report);

        return report;
    }

    /// <summary>
    /// Checks a single simplified code snippet (type/member references) for edge compatibility.
    /// Useful for the Roslyn analyzer integration.
    /// </summary>
    /// <param name="typeNames">The fully-qualified type names referenced in the code.</param>
    /// <param name="sourcePath">Optional source file path for reporting.</param>
    /// <returns>A report containing any violations found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="typeNames"/> is null.</exception>
    public EdgeCompatibilityReport CheckTypes(IEnumerable<string> typeNames, string? sourcePath = null)
    {
        if (typeNames == null) throw new ArgumentNullException(nameof(typeNames));

        var report = new EdgeCompatibilityReport();

        foreach (var typeName in typeNames)
        {
            if (!_whitelist.IsTypeAllowed(typeName))
            {
                var severity = _options.Strict
                    ? EdgeViolationSeverity.Error
                    : EdgeViolationSeverity.Warning;

                var suggestion = _whitelist.GetAlternative(typeName);

                report.AddViolation(new EdgeViolation(
                    severity,
                    $"Type '{typeName}' is not allowed on edge runtime.",
                    filePath: sourcePath,
                    typeName: typeName,
                    suggestion: suggestion));
            }
        }

        return report;
    }

    /// <summary>
    /// Checks whether SSG dependencies are compatible with edge.
    /// </summary>
    private void CheckStaticGeneration(RouteManifest manifest, NextNetConfig config, EdgeCompatibilityReport report)
    {
        // SSG output files are not available at the edge file system
        if (config.Ssg)
        {
            report.AddViolation(new EdgeViolation(
                EdgeViolationSeverity.Warning,
                "Static site generation (SSG) output files are not available at the edge. " +
                "Serve static files via CDN or provider object storage (R2, S3).",
                suggestion: "Disable SSG or serve pre-generated files from edge storage."));
        }
    }

    /// <summary>
    /// Checks all routes for edge-related issues.
    /// </summary>
    private void CheckRoutes(RouteManifest manifest, EdgeCompatibilityReport report)
    {
        foreach (var page in manifest.Pages)
        {
            // Check for file system-dependent route attributes
            if (page.FilePath != null && page.FilePath.Contains("App_Data", StringComparison.OrdinalIgnoreCase))
            {
                report.AddViolation(new EdgeViolation(
                    EdgeViolationSeverity.Warning,
                    $"Route '{page.RoutePattern}' references App_Data which may not be available on edge.",
                    filePath: page.FilePath));
            }
        }
    }

    /// <summary>
    /// Checks middleware registrations for edge compatibility.
    /// </summary>
    private void CheckMiddleware(RouteManifest manifest, EdgeCompatibilityReport report)
    {
        // Middleware that works in standard ASP.NET Core may not work on edge
        // because it depends on HttpContext features not available in edge runtimes.
        // The route manifest does not track middleware types directly; this check
        // serves as a general reminder when routes are present.
        if (manifest.Routes.Count > 0)
        {
            report.AddViolation(new EdgeViolation(
                EdgeViolationSeverity.Info,
                "Routes are registered. Ensure middleware and handler implementations " +
                "do not use blocked APIs (file system, sockets, reflection emit, etc.)."));
        }
    }

    /// <summary>
    /// Checks server actions for edge compatibility.
    /// </summary>
    private void CheckServerActions(RouteManifest manifest, EdgeCompatibilityReport report)
    {
        // Server actions may use reflection, file I/O, or other blocked APIs
        if (manifest.ApiRoutes?.Count > 0)
        {
            var actionCount = manifest.ApiRoutes.Count;
            report.AddViolation(new EdgeViolation(
                EdgeViolationSeverity.Info,
                $"{actionCount} API route(s) registered. Ensure API handlers do not use blocked edge APIs.",
                suggestion: "Consider migrating heavy server actions to dedicated backend services."));
        }
    }

    /// <summary>
    /// Checks streaming configuration for edge compatibility.
    /// </summary>
    private void CheckStreaming(NextNetConfig config, EdgeCompatibilityReport report)
    {
        // Streaming SSR requires chunked transfer encoding which some edge providers support
        if (config.Streaming)
        {
            report.AddViolation(new EdgeViolation(
                EdgeViolationSeverity.Info,
                "Streaming SSR is enabled. Verify that your edge provider supports " +
                "streaming responses (Cloudflare Workers: yes, Lambda@Edge: limited).",
                suggestion: "Set \"edge.provider\" to \"cloudflare\" for best streaming support."));
        }
    }

    /// <summary>
    /// Checks a type name against the edge API whitelist.
    /// </summary>
    /// <param name="fullTypeName">The fully-qualified type name.</param>
    /// <param name="sourcePath">Optional source file path for reporting.</param>
    /// <returns>A violation if the type is blocked; otherwise null.</returns>
    public EdgeViolation? CheckType(string fullTypeName, string? sourcePath = null)
    {
        if (fullTypeName == null) throw new ArgumentNullException(nameof(fullTypeName));

        if (!_whitelist.IsTypeAllowed(fullTypeName))
        {
            var severity = _options.Strict
                ? EdgeViolationSeverity.Error
                : EdgeViolationSeverity.Warning;

            var suggestion = _whitelist.GetAlternative(fullTypeName);

            return new EdgeViolation(
                severity,
                $"Type '{fullTypeName}' is not allowed on edge runtime.",
                filePath: sourcePath,
                typeName: fullTypeName,
                suggestion: suggestion);
        }

        return null;
    }

    /// <summary>
    /// Checks whether the given type is explicitly allowed on edge.
    /// </summary>
    /// <param name="fullTypeName">The fully-qualified type name.</param>
    /// <returns><c>true</c> if allowed; otherwise <c>false</c>.</returns>
    public bool IsAllowedType(string fullTypeName)
    {
        return _whitelist.IsTypeAllowed(fullTypeName);
    }

    /// <summary>
    /// Gets a suggested alternative API for a blocked type.
    /// </summary>
    /// <param name="blockedType">The blocked type name.</param>
    /// <returns>The alternative suggestion, or null.</returns>
    public string? GetAlternative(string blockedType)
    {
        return _whitelist.GetAlternative(blockedType);
    }

    /// <summary>
    /// Gets the current edge options.
    /// </summary>
    internal EdgeOptions Options => _options;
}
