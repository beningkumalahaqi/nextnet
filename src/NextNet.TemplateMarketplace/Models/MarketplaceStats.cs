namespace NextNet.TemplateMarketplace;

/// <summary>
/// Download statistics for a template on the marketplace.
/// </summary>
public sealed record MarketplaceStats
{
    /// <summary>Name of the template.</summary>
    public string TemplateName { get; init; } = "";

    /// <summary>Total number of downloads since publication.</summary>
    public long TotalDownloads { get; init; }

    /// <summary>Number of downloads in the past week.</summary>
    public long WeeklyDownloads { get; init; }

    /// <summary>Number of downloads in the past month.</summary>
    public long MonthlyDownloads { get; init; }

    /// <summary>Daily download history for trending analysis.</summary>
    public IReadOnlyList<DailyStat> DailyHistory { get; init; } = Array.Empty<DailyStat>();
}

/// <summary>
/// A single day's download count for a template.
/// </summary>
public sealed record DailyStat
{
    /// <summary>The calendar date.</summary>
    public DateTime Date { get; init; }

    /// <summary>Number of downloads on this date.</summary>
    public long Downloads { get; init; }
}
