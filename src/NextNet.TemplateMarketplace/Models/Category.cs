namespace NextNet.TemplateMarketplace;

/// <summary>
/// A category used to organize templates on the marketplace.
/// </summary>
public sealed record Category
{
    /// <summary>Unique identifier for the category.</summary>
    public string Id { get; init; } = "";

    /// <summary>Display name of the category.</summary>
    public string Name { get; init; } = "";

    /// <summary>Optional description of the category.</summary>
    public string? Description { get; init; }

    /// <summary>URL of an icon representing the category.</summary>
    public string? IconUrl { get; init; }

    /// <summary>Number of templates in this category.</summary>
    public int TemplateCount { get; init; }
}
