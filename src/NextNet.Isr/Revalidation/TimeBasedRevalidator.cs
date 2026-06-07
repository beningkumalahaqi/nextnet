namespace NextNet.Isr.Revalidation;

/// <summary>
/// Handles time-based revalidation: checks whether a cached page's age
/// has exceeded its configured revalidation interval.
/// </summary>
public class TimeBasedRevalidator
{
    private readonly IsrGlobalOptions _globalOptions;

    /// <summary>
    /// Initializes a new instance of <see cref="TimeBasedRevalidator"/>.
    /// </summary>
    /// <param name="globalOptions">The global ISR options providing the default revalidation interval.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="globalOptions"/> is null.</exception>
    public TimeBasedRevalidator(IsrGlobalOptions globalOptions)
    {
        _globalOptions = globalOptions ?? throw new ArgumentNullException(nameof(globalOptions));
    }

    /// <summary>
    /// Determines whether the given <paramref name="generatedAt"/> timestamp
    /// is stale relative to the current time and the specified revalidation interval.
    /// </summary>
    /// <param name="generatedAt">The UTC timestamp when the page was generated.</param>
    /// <param name="revalidateIntervalSeconds">
    /// The revalidation interval in seconds. If <c>null</c>, the global default is used.
    /// </param>
    /// <param name="now">The current UTC time. If <c>null</c>, <see cref="DateTime.UtcNow"/> is used.</param>
    /// <returns><c>true</c> if the page is stale and should be revalidated.</returns>
    public bool IsStale(DateTime generatedAt, int? revalidateIntervalSeconds, DateTime? now = null)
    {
        var interval = revalidateIntervalSeconds ?? _globalOptions.DefaultRevalidateSeconds;
        if (interval <= 0)
            return false; // No revalidation interval means never stale

        var currentTime = now ?? DateTime.UtcNow;
        return generatedAt.AddSeconds(interval) <= currentTime;
    }

    /// <summary>
    /// Gets the remaining time in seconds before the page becomes stale.
    /// Returns 0 if already stale, or the global default interval if not applicable.
    /// </summary>
    public double GetTtlSeconds(DateTime generatedAt, int? revalidateIntervalSeconds, DateTime? now = null)
    {
        var interval = revalidateIntervalSeconds ?? _globalOptions.DefaultRevalidateSeconds;
        if (interval <= 0)
            return double.MaxValue; // Never stale

        var currentTime = now ?? DateTime.UtcNow;
        var expiry = generatedAt.AddSeconds(interval);
        var remaining = (expiry - currentTime).TotalSeconds;

        return Math.Max(0, remaining);
    }
}
