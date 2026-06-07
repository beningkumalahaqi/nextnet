namespace NextNet.Isr.Revalidation;

/// <summary>
/// Represents the outcome of a revalidation operation.
/// </summary>
public class RevalidationResult
{
    /// <summary>
    /// Gets a value indicating whether the revalidation succeeded.
    /// </summary>
    public bool Success { get; }

    /// <summary>
    /// Gets the route that was revalidated, if applicable.
    /// </summary>
    public string? Route { get; }

    /// <summary>
    /// Gets the list of routes that were revalidated, if this was a bulk operation.
    /// </summary>
    public IReadOnlyList<string>? Routes { get; }

    /// <summary>
    /// Gets the error message if the revalidation failed.
    /// </summary>
    public string? ErrorMessage { get; }

    /// <summary>
    /// Gets the number of routes that were revalidated.
    /// </summary>
    public int RevalidatedCount { get; }

    private RevalidationResult(bool success, string? route, IReadOnlyList<string>? routes, string? errorMessage, int count)
    {
        Success = success;
        Route = route;
        Routes = routes;
        ErrorMessage = errorMessage;
        RevalidatedCount = count;
    }

    /// <summary>
    /// Creates a successful result for a single route revalidation.
    /// </summary>
    public static RevalidationResult Ok(string route) =>
        new(true, route, null, null, 1);

    /// <summary>
    /// Creates a successful result for a bulk revalidation.
    /// </summary>
    public static RevalidationResult Ok(IReadOnlyList<string> routes) =>
        new(true, null, routes, null, routes.Count);

    /// <summary>
    /// Creates a failed result.
    /// </summary>
    public static RevalidationResult Fail(string errorMessage) =>
        new(false, null, null, errorMessage, 0);
}
