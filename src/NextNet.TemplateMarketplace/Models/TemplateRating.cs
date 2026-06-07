namespace NextNet.TemplateMarketplace;

/// <summary>
/// Aggregate rating information for a template on the marketplace.
/// </summary>
public sealed record TemplateRating
{
    /// <summary>Name of the template being rated.</summary>
    public string TemplateName { get; init; } = "";

    /// <summary>Star rating value (1-5).</summary>
    public int Stars { get; init; }

    /// <summary>Total number of ratings submitted.</summary>
    public int TotalRatings { get; init; }

    /// <summary>Average rating across all submissions.</summary>
    public double AverageRating { get; init; }
}
