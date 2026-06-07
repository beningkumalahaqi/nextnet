namespace NextNet.Cli.Community;

/// <summary>
/// Represents the outcome of a community template removal operation.
/// </summary>
/// <remarks>
/// <para>
/// Created by <see cref="CommunityTemplateManager.Remove"/> to report whether
/// the template was successfully removed from disk and the registry manifest.
/// </para>
/// </remarks>
public sealed record RemoveResult
{
    /// <summary>
    /// Gets a value indicating whether the removal completed successfully.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Gets a human-readable message describing the result (e.g., error detail on failure).
    /// </summary>
    public string? Message { get; init; }
}
