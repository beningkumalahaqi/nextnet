namespace NextNet.TemplateMarketplace;

/// <summary>
/// Represents a template publisher's public profile on the marketplace.
/// </summary>
public sealed record PublisherProfile
{
    /// <summary>Unique identifier for the publisher.</summary>
    public string PublisherId { get; init; } = "";

    /// <summary>Display name shown to users.</summary>
    public string DisplayName { get; init; } = "";

    /// <summary>Short biography or description of the publisher.</summary>
    public string? Description { get; init; }

    /// <summary>Link to the publisher's website.</summary>
    public string? WebsiteUrl { get; init; }

    /// <summary>URL of the publisher's avatar image.</summary>
    public string? AvatarUrl { get; init; }

    /// <summary>Date the publisher joined the marketplace.</summary>
    public DateTime MemberSince { get; init; }

    /// <summary>Total number of templates published.</summary>
    public int TotalTemplates { get; init; }

    /// <summary>Total number of downloads across all templates.</summary>
    public int TotalDownloads { get; init; }

    /// <summary>Whether the publisher's identity has been verified.</summary>
    public bool IsVerified { get; init; }
}
