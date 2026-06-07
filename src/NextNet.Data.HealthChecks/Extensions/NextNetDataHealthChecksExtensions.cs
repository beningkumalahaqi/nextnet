using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using NextNet.Data.HealthChecks;
using NextNet.Data.HealthChecks.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for registering NextNet data health checks
/// on the ASP.NET Core DI container and endpoint routing.
/// </summary>
/// <remarks>
/// <para>
/// These extensions provide a single-line setup for database health monitoring:
/// <list type="bullet">
///   <item><see cref="NextNetDataHealthChecksExtensions.AddNextNetHealthChecks"/> registers
///     the aggregator and providers in DI.</item>
///   <item><see cref="NextNetDataHealthChecksExtensions.MapNextNetHealthChecks"/> maps the
///     health check HTTP endpoint.</item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Program.cs
/// builder.Services.AddNextNetHealthChecks(options =>
/// {
///     options.ShowDetails = true;
///     options.CacheTtl = TimeSpan.FromSeconds(10);
/// });
///
/// var app = builder.Build();
/// app.MapNextNetHealthChecks();
/// app.Run();
/// </code>
/// </example>
public static class NextNetDataHealthChecksExtensions
{
    /// <summary>
    /// Registers the NextNet data health check aggregator and all
    /// <see cref="IHealthCheckProvider"/> implementations with the ASP.NET Core health check system.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configureOptions">An optional delegate to configure <see cref="NextNetDataHealthCheckOptions"/>.</param>
    /// <returns>The same service collection for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is <c>null</c>.</exception>
    /// <remarks>
    /// <para>
    /// This method performs the following registrations:
    /// <list type="bullet">
    ///   <item>Binds <see cref="NextNetDataHealthCheckOptions"/> as a singleton</item>
    ///   <item>Registers <see cref="HealthCheckResultCache"/> as a singleton</item>
    ///   <item>Registers <see cref="NextNetDataHealthCheck"/> as <see cref="IHealthCheck"/></item>
    ///   <item>Discovers all <see cref="IHealthCheckProvider"/> implementations from DI</item>
    /// </list>
    /// </para>
    /// </remarks>
    public static IServiceCollection AddNextNetHealthChecks(
        this IServiceCollection services,
        Action<NextNetDataHealthCheckOptions>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Configure options
        var options = new NextNetDataHealthCheckOptions();
        configureOptions?.Invoke(options);
        services.AddSingleton<IOptions<NextNetDataHealthCheckOptions>>(new OptionsWrapper<NextNetDataHealthCheckOptions>(options));

        // Register cache
        services.AddSingleton<HealthCheckResultCache>();

        // Register the aggregator health check
        services.AddSingleton<IHealthCheck, NextNetDataHealthCheck>();

        return services;
    }

    /// <summary>
    /// Maps the NextNet data health check endpoint on the configured path.
    /// </summary>
    /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to add the endpoint to.</param>
    /// <returns>The endpoint route builder for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="endpoints"/> is <c>null</c>.</exception>
    /// <remarks>
    /// <para>
    /// This method reads the endpoint path from <see cref="NextNetDataHealthCheckOptions.EndpointPath"/>
    /// (defaults to <c>"/health"</c>) and maps the health check endpoint using ASP.NET Core's
    /// <c>MapHealthChecks</c>.
    /// </para>
    /// <para>
    /// The following endpoints are mapped:
    /// <list type="bullet">
    ///   <item><c>GET {path}</c> — Full health check executing all registered providers</item>
    ///   <item><c>GET {path}/live</c> — Lightweight liveness probe (returns 200 immediately)</item>
    ///   <item><c>GET {path}/ready</c> — Readiness probe (same full check as the main endpoint)</item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// app.MapNextNetHealthChecks();
    /// // Maps GET /health, GET /health/live, GET /health/ready
    /// </code>
    /// </example>
    public static IEndpointRouteBuilder MapNextNetHealthChecks(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var options = endpoints.ServiceProvider.GetRequiredService<IOptions<NextNetDataHealthCheckOptions>>().Value;
        var path = options.EndpointPath;

        // Main health check endpoint
        endpoints.MapHealthChecks(path, new HealthCheckOptions
        {
            ResponseWriter = WriteHealthCheckResponseAsync,
            AllowCachingResponses = false,
        });

        // Liveness probe — lightweight, returns 200 immediately
        endpoints.MapGet($"{path}/live", async context =>
        {
            context.Response.StatusCode = 200;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(
                """{"status":"Healthy","description":"Service is alive"}""");
        });

        // Readiness probe — same as main health check
        endpoints.MapHealthChecks($"{path}/ready", new HealthCheckOptions
        {
            ResponseWriter = WriteHealthCheckResponseAsync,
            AllowCachingResponses = false,
        });

        return endpoints;
    }

    /// <summary>
    /// Custom JSON response writer for health check results.
    /// </summary>
    private static Task WriteHealthCheckResponseAsync(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json; charset=utf-8";

        var response = new
        {
            status = report.Status.ToString(),
            totalDurationMs = report.TotalDuration.TotalMilliseconds,
            timestamp = DateTime.UtcNow.ToString("O"),
            entries = report.Entries.ToDictionary(
                e => e.Key,
                e => new
                {
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description,
                    durationMs = e.Value.Duration.TotalMilliseconds,
                    data = e.Value.Data,
                })
        };

        return System.Text.Json.JsonSerializer.SerializeAsync(
            context.Response.Body,
            response,
            new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
            });
    }
}
