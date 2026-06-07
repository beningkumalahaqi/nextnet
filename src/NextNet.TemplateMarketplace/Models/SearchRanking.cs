namespace NextNet.TemplateMarketplace;

/// <summary>
/// Results of a search query ranked by relevance.
/// V4+ will incorporate ranking algorithms from the marketplace API.
/// </summary>
public sealed record SearchRanking
{
    /// <summary>The search query string.</summary>
    public string Query { get; init; } = "";

    /// <summary>Ranked list of matching templates.</summary>
    public IReadOnlyList<RankingResult> Results { get; init; } = Array.Empty<RankingResult>();

    /// <summary>Timestamp when the ranking was computed.</summary>
    public DateTime ComputedAt { get; init; }
}

/// <summary>
/// A single result entry in a search ranking.
/// </summary>
public sealed record RankingResult
{
    /// <summary>Name of the template.</summary>
    public string TemplateName { get; init; } = "";

    /// <summary>Author or publisher of the template.</summary>
    public string Author { get; init; } = "";

    /// <summary>Relevance score (higher is more relevant).</summary>
    public double Score { get; init; }

    /// <summary>Total number of downloads.</summary>
    public long Downloads { get; init; }

    /// <summary>Average user rating.</summary>
    public double AverageRating { get; init; }

    /// <summary>Date the template was last updated.</summary>
    public DateTime LastUpdated { get; init; }
}
