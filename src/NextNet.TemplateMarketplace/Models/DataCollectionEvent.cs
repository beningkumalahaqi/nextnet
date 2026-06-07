namespace NextNet.TemplateMarketplace;

/// <summary>
/// Types of data collection events that can be recorded.
/// </summary>
public enum DataCollectionEventType
{
    /// <summary>A template was installed.</summary>
    TemplateInstall,

    /// <summary>A template was used to generate a project.</summary>
    TemplateGeneration,

    /// <summary>A template was updated.</summary>
    TemplateUpdate,

    /// <summary>An error occurred during template installation or generation.</summary>
    Error
}

/// <summary>
/// An anonymous event recorded for marketplace analytics.
/// Only collected when the user has opted in via <see cref="MarketplaceOptions.EnableDataCollection"/>.
/// </summary>
public sealed record DataCollectionEvent
{
    /// <summary>Type of event.</summary>
    public DataCollectionEventType Type { get; init; }

    /// <summary>Name of the template involved.</summary>
    public string TemplateName { get; init; } = "";

    /// <summary>Version of the template involved.</summary>
    public string TemplateVersion { get; init; } = "";

    /// <summary>UTC timestamp of when the event occurred.</summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    /// <summary>Stable anonymous identifier for the user.</summary>
    public string? AnonymousUserId { get; init; }

    /// <summary>Whether the operation succeeded.</summary>
    public bool? Success { get; init; }

    /// <summary>Duration of the operation, if applicable.</summary>
    public TimeSpan? Duration { get; init; }

    /// <summary>Error message if the operation failed.</summary>
    public string? ErrorMessage { get; init; }
}
