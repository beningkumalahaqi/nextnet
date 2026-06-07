using Microsoft.AspNetCore.Http;

namespace {{NAMESPACE}}.App.Api.Health;

/// <summary>
/// Health check API endpoint — GET /api/health
/// </summary>
public class HealthRoute
{
    public static async Task HandleAsync(HttpContext context)
    {
        await context.Response.WriteAsJsonAsync(new
        {
            status = "healthy",
            project = "{{PROJECT_NAME}}",
            timestamp = DateTime.UtcNow
        });
    }
}
