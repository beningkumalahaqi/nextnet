namespace NextNet.Data.Abstractions.Models;

/// <summary>
/// Options for filtering, sorting, and paginating repository queries.
/// Passed to <see cref="Abstractions.IRepository{T}.GetAllAsync"/>.
/// </summary>
/// <remarks>
/// <para>
/// Provides a unified way to specify query parameters across all data providers.
/// The <see cref="Filter"/> and <see cref="Includes"/> properties use provider-specific
/// syntax and semantics.
/// </para>
/// <example>
/// <code>
/// var options = new RepositoryQueryOptions(
///     filter: "Age > 18",
///     sortBy: "LastName",
///     sortDescending: false,
///     page: 1,
///     pageSize: 20,
///     includes: new[] { "Orders", "Profile" });
/// </code>
/// </example>
/// </remarks>
/// <param name="Filter">Optional filter expression (provider-specific syntax).</param>
/// <param name="SortBy">Optional property name to sort by.</param>
/// <param name="SortDescending">Whether to sort in descending order. Defaults to <c>false</c>.</param>
/// <param name="Page">The page number (1-based). Defaults to 1.</param>
/// <param name="PageSize">The number of items per page. Defaults to 20. Maximum 1000.</param>
/// <param name="Includes">Optional related entities to include (provider-specific).</param>
public sealed record RepositoryQueryOptions(
    string? Filter = null,
    string? SortBy = null,
    bool SortDescending = false,
    int Page = 1,
    int PageSize = 20,
    IReadOnlyList<string>? Includes = null
)
{
    /// <summary>
    /// Gets the effective page size, clamped to a maximum of 1000.
    /// </summary>
    public int EffectivePageSize => PageSize > 1000 ? 1000 : PageSize < 1 ? 20 : PageSize;

    /// <summary>
    /// Gets the effective page number, ensuring it is at least 1.
    /// </summary>
    public int EffectivePage => Page < 1 ? 1 : Page;
}
