using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace NextNet.Build.Production.Health;

/// <summary>
/// Handles the GET /_health endpoint, returning a JSON health report.
/// </summary>
public class HealthCheckEndpoint
{
    private readonly NextNetHealthCheck _healthCheck;

    /// <summary>
    /// Initializes a new instance of <see cref="HealthCheckEndpoint"/>.
    /// </summary>
    public HealthCheckEndpoint(NextNetHealthCheck healthCheck)
    {
        _healthCheck = healthCheck ?? throw new ArgumentNullException(nameof(healthCheck));
    }

    /// <summary>
    /// Handles the health check HTTP request.
    /// </summary>
    public async Task HandleAsync(HttpContext context)
    {
        var report = await _healthCheck.CheckAsync();

        context.Response.ContentType = "application/json";
        context.Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate";

        var statusCode = report.Status switch
        {
            "Healthy" => 200,
            "Degraded" => 200, // Still return 200 but with degraded status
            "Unhealthy" => 503,
            _ => 500,
        };

        context.Response.StatusCode = statusCode;

        var json = JsonSerializer.Serialize(report, new JsonSerializerOptions
        {
            WriteIndented = true,
        });

        await context.Response.WriteAsync(json);
    }
}
