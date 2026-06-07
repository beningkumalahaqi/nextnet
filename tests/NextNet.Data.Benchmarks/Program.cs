using BenchmarkDotNet.Running;
using NextNet.Data.Benchmarks.Benchmarks;

namespace NextNet.Data.Benchmarks;

/// <summary>
/// Entry point for the NextNet Data Layer performance benchmarks.
/// </summary>
public static class Program
{
    /// <summary>
    /// Runs all registered benchmarks and optionally validates SLAs.
    /// </summary>
    /// <param name="args">Command-line arguments passed to BenchmarkDotNet.</param>
    public static void Main(string[] args)
    {
        var summaries = new[]
        {
            BenchmarkRunner.Run<ProviderResolutionBenchmarks>(),
            BenchmarkRunner.Run<RepositoryBenchmarks>(),
            BenchmarkRunner.Run<HealthCheckBenchmarks>(),
        };

        foreach (var summary in summaries)
        {
            Console.WriteLine($"Benchmark completed: {summary.Title}");
            Console.WriteLine($"  Total time: {summary.TotalTime}");
            Console.WriteLine($"  Reports: {summary.Reports.Count()}");

            foreach (var report in summary.Reports)
            {
                if (report.ResultStatistics is { } stats)
                {
                    Console.WriteLine($"  - {report.BenchmarkCase.DisplayInfo}: Mean={stats.Mean:N2}ns");
                }
            }
        }

        // Run SLA validation
        var slaChecker = new BenchmarkSlaChecker();
        var violations = slaChecker.Check(summaries);

        if (violations.Count > 0)
        {
            Console.Error.WriteLine("SLA VIOLATIONS DETECTED:");
            foreach (var violation in violations)
            {
                Console.Error.WriteLine($"  {violation}");
            }
            Environment.ExitCode = 1;
        }
        else
        {
            Console.WriteLine("All benchmarks passed SLA requirements.");
        }
    }
}
