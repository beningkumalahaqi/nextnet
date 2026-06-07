namespace NextNet.Data.Abstractions.Models;

/// <summary>
/// Represents a paginated result set returned by <see cref="Abstractions.IRepository{T}.GetAllAsync"/>.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
/// <remarks>
/// <para>
/// Provides pagination metadata alongside the result items. The <see cref="HasNextPage"/> and
/// <see cref="HasPreviousPage"/> properties are computed from the total count and current page.
/// </para>
/// <example>
/// <code>
/// var result = new PagedResult&lt;User&gt;(
///     new List&lt;User&gt; { user1, user2 },
///     totalCount: 42,
///     page: 2,
///     pageSize: 10);
/// // HasNextPage = true (page 2 of 5)
/// // HasPreviousPage = true (page 2 > 1)
/// </code>
/// </example>
/// </remarks>
/// <param name="Items">The items on the current page.</param>
/// <param name="TotalCount">The total number of items across all pages.</param>
/// <param name="Page">The current page number (1-based).</param>
/// <param name="PageSize">The number of items per page.</param>
/// <param name="HasNextPage">Whether there is a subsequent page of results.</param>
/// <param name="HasPreviousPage">Whether there is a preceding page of results.</param>
public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    long TotalCount,
    int Page,
    int PageSize,
    bool HasNextPage,
    bool HasPreviousPage
)
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PagedResult{T}"/> record.
    /// </summary>
    /// <param name="items">The items on the current page.</param>
    /// <param name="totalCount">The total number of items across all pages.</param>
    /// <param name="page">The current page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    public PagedResult(IReadOnlyList<T> items, long totalCount, int page, int pageSize)
        : this(items, totalCount, page, pageSize,
              (page * pageSize) < totalCount,
              page > 1)
    {
    }
}
