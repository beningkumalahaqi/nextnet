namespace NextNet.TemplateMarketplace;

/// <summary>
/// A single review submitted for a template on the marketplace.
/// </summary>
public sealed record TemplateReview
{
    /// <summary>Unique identifier for the review.</summary>
    public string Id { get; init; } = "";

    /// <summary>Name of the template being reviewed.</summary>
    public string TemplateName { get; init; } = "";

    /// <summary>Version of the template at the time of review.</summary>
    public string TemplateVersion { get; init; } = "";

    /// <summary>Display name of the reviewer.</summary>
    public string ReviewerName { get; init; } = "";

    /// <summary>Star rating (1-5).</summary>
    public int Stars { get; init; }

    /// <summary>Optional textual review comment.</summary>
    public string? Comment { get; init; }

    /// <summary>Date and time the review was submitted.</summary>
    public DateTime SubmittedAt { get; init; }
}
