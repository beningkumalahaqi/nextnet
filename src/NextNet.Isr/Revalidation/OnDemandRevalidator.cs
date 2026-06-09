using System.Security.Cryptography;

namespace NextNet.Isr.Revalidation;

/// <summary>
/// Handles on-demand revalidation triggered by API calls or server actions.
/// Validates the provided secret before allowing revalidation to proceed.
/// </summary>
public sealed class OnDemandRevalidator
{
    private readonly IIsrRevalidationManager _revalidationManager;
    private readonly IsrGlobalOptions _globalOptions;

    /// <summary>
    /// Initializes a new instance of <see cref="OnDemandRevalidator"/>.
    /// </summary>
    /// <param name="revalidationManager">The revalidation manager.</param>
    /// <param name="globalOptions">Global ISR options containing the revalidation secret.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public OnDemandRevalidator(
        IIsrRevalidationManager revalidationManager,
        IsrGlobalOptions globalOptions)
    {
        _revalidationManager = revalidationManager ?? throw new ArgumentNullException(nameof(revalidationManager));
        _globalOptions = globalOptions ?? throw new ArgumentNullException(nameof(globalOptions));
    }

    /// <summary>
    /// Validates the revalidation secret. Uses constant-time comparison to prevent timing attacks.
    /// </summary>
    /// <param name="providedSecret">The secret provided in the request.</param>
    /// <returns><c>true</c> if the secret is valid; otherwise <c>false</c>.</returns>
    public bool ValidateSecret(string? providedSecret)
    {
        if (string.IsNullOrEmpty(_globalOptions.RevalidationSecret))
            return true; // No secret configured = no auth required

        if (string.IsNullOrEmpty(providedSecret))
            return false;

        return CryptographicOperations.FixedTimeEquals(
            System.Text.Encoding.UTF8.GetBytes(_globalOptions.RevalidationSecret),
            System.Text.Encoding.UTF8.GetBytes(providedSecret));
    }

    /// <summary>
    /// Revalidates a single route on demand.
    /// </summary>
    /// <param name="route">The route to revalidate.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The revalidation result.</returns>
    public Task<RevalidationResult> RevalidateRouteAsync(string route, CancellationToken cancellationToken = default)
    {
        return _revalidationManager.RevalidateAsync(route, cancellationToken);
    }

    /// <summary>
    /// Revalidates all routes associated with the specified tags.
    /// </summary>
    /// <param name="tags">The tags to invalidate by.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The revalidation result.</returns>
    public Task<RevalidationResult> RevalidateByTagsAsync(IReadOnlyList<string> tags, CancellationToken cancellationToken = default)
    {
        return _revalidationManager.InvalidateByTagsAsync(tags, cancellationToken);
    }
}
