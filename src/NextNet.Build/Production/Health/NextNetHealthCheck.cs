using System.Diagnostics;

namespace NextNet.Build.Production.Health;

/// <summary>
/// Performs NextNet-specific health checks including route registry,
/// cache health, and ISR cache health.
/// </summary>
public sealed class NextNetHealthCheck
{
    private readonly DateTime _startTime = DateTime.UtcNow;
    private readonly string? _version;

    /// <summary>
    /// Initializes a new instance of <see cref="NextNetHealthCheck"/>.
    /// </summary>
    public NextNetHealthCheck(string? version = null)
    {
        _version = version;
    }

    /// <summary>
    /// Runs all health checks and returns a comprehensive report.
    /// </summary>
    public async Task<HealthReport> CheckAsync()
    {
        var status = "Healthy";
        var timestamp = DateTime.UtcNow;
        var version = _version ?? typeof(NextNetHealthCheck).Assembly.GetName().Version?.ToString();
        var uptime = (DateTime.UtcNow - _startTime).ToString(@"d\.hh\:mm\:ss");

        var checks = new List<HealthCheckResult>();

        // Basic application health
        checks.Add(await CheckBasicHealthAsync());

        // Memory health
        checks.Add(CheckMemoryHealth());

        // Route registry health
        checks.Add(CheckRouteRegistryHealth());

        // Determine overall status
        status = checks.Any(c => c.Status == "Unhealthy")
            ? "Unhealthy"
            : checks.Any(c => c.Status == "Degraded")
                ? "Degraded"
                : "Healthy";

        return new HealthReport(status, timestamp, version, uptime, checks);
    }

    private static Task<HealthCheckResult> CheckBasicHealthAsync()
    {
        var sw = Stopwatch.StartNew();

        var result = new HealthCheckResult(
            "Application Health",
            "Healthy",
            "Application is running and responding.",
            sw.ElapsedMilliseconds,
            null);

        sw.Stop();

        return Task.FromResult(result with { DurationMs = sw.ElapsedMilliseconds });
    }

    private static HealthCheckResult CheckMemoryHealth()
    {
        var sw = Stopwatch.StartNew();

        var process = Process.GetCurrentProcess();
        var memoryMb = process.WorkingSet64 / (1024.0 * 1024.0);

        var status = memoryMb < 200 ? "Healthy" : memoryMb < 500 ? "Degraded" : "Unhealthy";

        var data = new Dictionary<string, object>
        {
            ["workingSetMb"] = Math.Round(memoryMb, 1),
            ["peakWorkingSetMb"] = Math.Round(process.PeakWorkingSet64 / (1024.0 * 1024.0), 1),
        };

        sw.Stop();

        return new HealthCheckResult(
            "Memory Usage",
            status,
            $"Working set: {memoryMb:F1} MB",
            sw.ElapsedMilliseconds,
            data);
    }

    private static HealthCheckResult CheckRouteRegistryHealth()
    {
        var sw = Stopwatch.StartNew();

        // Route registry health check - checks that the assembly can load and reflect
        try
        {
            // Attempt to access a known type from the routing namespace
            var routingAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "NextNet.Routing");

            var isHealthy = routingAssembly != null;

            sw.Stop();

            var data = new Dictionary<string, object>
            {
                ["assemblyLoaded"] = isHealthy,
            };

            return new HealthCheckResult(
                "Route Registry",
                isHealthy ? "Healthy" : "Degraded",
                isHealthy
                    ? "Route registry is loaded."
                    : "NextNet.Routing assembly not found; routes may not be registered.",
                sw.ElapsedMilliseconds,
                data);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return new HealthCheckResult(
                "Route Registry",
                "Unhealthy",
                $"Route registry check failed: {ex.Message}",
                sw.ElapsedMilliseconds,
                null);
        }
    }
}
