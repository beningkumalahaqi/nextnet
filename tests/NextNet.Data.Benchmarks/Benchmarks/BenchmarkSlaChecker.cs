using BenchmarkDotNet.Reports;

namespace NextNet.Data.Benchmarks.Benchmarks;

/// <summary>
/// SLA validation tool that checks benchmark results against defined performance targets.
/// </summary>
/// <remarks>
/// <para>
/// This checker processes BenchmarkDotNet summaries and flags any results that exceed
/// the defined SLA thresholds. It is used both in CI (to gate releases) and locally
/// (to alert developers to regressions).
/// </para>
/// <para>
/// SLA thresholds are defined per benchmark category:
/// <list type="bullet">
///   <item><description>ProviderResolution: &lt; 2ms</description></item>
///   <item><description>ConnectionCreation: &lt; 10ms</description></item>
///   <item><description>Repository CRUD: &lt; 50ms insert, &lt; 30ms find/delete, &lt; 50ms update/getall</description></item>
///   <item><description>HealthCheck: &lt; 5ms overhead (excluding provider execution)</description></item>
///   <item><description>ConfigBuilding: &lt; 1ms</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed class BenchmarkSlaChecker
{
    private readonly Dictionary<string, double> _slaThresholds = new(StringComparer.OrdinalIgnoreCase)
    {
        // Provider resolution: < 2ms (2000000ns)
        ["ResolveDefaultProvider_Time"] = 2_000_000,
        ["ResolveSingleProvider_Default"] = 2_000_000,
        ["ResolveSingleProvider_Named"] = 2_000_000,

        // Connection creation: < 10ms
        ["CreateConnection_Time"] = 10_000_000,
        ["CreateConnection_Default"] = 10_000_000,
        ["CreateConnection_Named"] = 10_000_000,

        // Config building: < 1ms
        ["BuildDataConfig_Time"] = 1_000_000,

        // Repository: < 50ms insert, < 30ms find/delete, < 50ms update/getall
        ["Repository_Insert"] = 50_000_000,
        ["Repository_FindById"] = 30_000_000,
        ["Repository_GetAll"] = 50_000_000,
        ["Repository_Update"] = 50_000_000,
        ["Repository_Delete"] = 30_000_000,

        // Health check: < 5ms overhead
        ["HealthCheck_NoProviders"] = 1_000_000,
        ["HealthCheck_1Provider_Healthy"] = 5_000_000,
        ["HealthCheck_3Providers"] = 5_000_000,
        ["HealthCheck_Serialize"] = 1_000_000,
    };

    /// <summary>
    /// Checks the given benchmark summaries against defined SLA thresholds.
    /// </summary>
    /// <param name="summaries">The benchmark summaries to validate.</param>
    /// <returns>A list of SLA violation messages. Empty if all benchmarks pass.</returns>
    public List<string> Check(params Summary[] summaries)
    {
        var violations = new List<string>();

        foreach (var summary in summaries)
        {
            foreach (var report in summary.Reports)
            {
                var benchmarkName = report.BenchmarkCase.Descriptor.WorkloadMethodDisplayInfo ?? report.BenchmarkCase.DisplayInfo;

                if (report.ResultStatistics is { } stats && _slaThresholds.TryGetValue(benchmarkName, out var thresholdNs))
                {
                    var meanNs = stats.Mean;
                    if (meanNs > thresholdNs)
                    {
                        violations.Add(
                            $"SLA VIOLATION: {benchmarkName} = {meanNs / 1_000_000:F2}ms " +
                            $"(threshold: {thresholdNs / 1_000_000:F2}ms)");
                    }
                }
            }
        }

        return violations;
    }
}
