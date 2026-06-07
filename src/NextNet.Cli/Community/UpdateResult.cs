namespace NextNet.Cli.Community;

/// <summary>
/// Represents the outcome of a bulk template update operation.
/// </summary>
/// <remarks>
/// <para>
/// Created by <see cref="CommunityTemplateManager.UpdateAsync"/> to report which
/// templates were successfully updated and which failed.
/// </para>
/// <para>
/// The <see cref="Success"/> property returns <c>true</c> only when no updates
/// failed (i.e., the <see cref="Failed"/> list is empty).
/// </para>
/// </remarks>
public sealed record UpdateResult
{
    /// <summary>
    /// Gets the list of template names that were successfully updated.
    /// </summary>
    public IReadOnlyList<string> Updated { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets the list of template names whose update failed.
    /// </summary>
    public IReadOnlyList<string> Failed { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets a value indicating whether all updates succeeded (the <see cref="Failed"/> list is empty).
    /// </summary>
    public bool Success => Failed.Count == 0;
}
