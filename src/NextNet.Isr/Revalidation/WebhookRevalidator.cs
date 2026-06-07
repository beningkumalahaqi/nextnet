using System.Security.Cryptography;
using System.Text;

namespace NextNet.Isr.Revalidation;

/// <summary>
/// Handles webhook-triggered revalidation from external services (CMS, etc.)
/// with HMAC-SHA256 signature verification for request authenticity.
/// </summary>
public class WebhookRevalidator
{
    private readonly IIsrRevalidationManager _revalidationManager;
    private readonly IsrGlobalOptions _globalOptions;

    /// <summary>
    /// Initializes a new instance of <see cref="WebhookRevalidator"/>.
    /// </summary>
    /// <param name="revalidationManager">The revalidation manager.</param>
    /// <param name="globalOptions">Global ISR options containing the webhook secret.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public WebhookRevalidator(
        IIsrRevalidationManager revalidationManager,
        IsrGlobalOptions globalOptions)
    {
        _revalidationManager = revalidationManager ?? throw new ArgumentNullException(nameof(revalidationManager));
        _globalOptions = globalOptions ?? throw new ArgumentNullException(nameof(globalOptions));
    }

    /// <summary>
    /// Verifies that the provided signature matches an HMAC-SHA256 of the body
    /// computed with the configured webhook secret.
    /// </summary>
    /// <param name="body">The raw request body bytes.</param>
    /// <param name="signature">The signature from the webhook request header.</param>
    /// <returns><c>true</c> if the signature is valid; otherwise <c>false</c>.</returns>
    public bool VerifySignature(byte[] body, string? signature)
    {
        if (string.IsNullOrEmpty(_globalOptions.WebhookSecret))
            return true; // No webhook secret configured = skip verification

        if (string.IsNullOrEmpty(signature) || body == null || body.Length == 0)
            return false;

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_globalOptions.WebhookSecret));
        var computedHash = hmac.ComputeHash(body);
        var computedSignature = "sha256=" + Convert.ToHexString(computedHash).ToLowerInvariant();

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computedSignature),
            Encoding.UTF8.GetBytes(signature));
    }

    /// <summary>
    /// Processes a webhook revalidation request. Validates the signature and
    /// triggers revalidation for the specified routes or tags.
    /// </summary>
    /// <param name="body">The raw webhook body.</param>
    /// <param name="signature">The HMAC signature from the webhook header.</param>
    /// <param name="routes">The routes to revalidate.</param>
    /// <param name="tags">The tags to invalidate by.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The revalidation result, or a failure if signature is invalid.</returns>
    public async Task<RevalidationResult> ProcessWebhookAsync(
        byte[] body,
        string? signature,
        IReadOnlyList<string>? routes,
        IReadOnlyList<string>? tags,
        CancellationToken cancellationToken = default)
    {
        if (!VerifySignature(body, signature))
            return RevalidationResult.Fail("Invalid webhook signature.");

        var revalidated = new List<string>();

        if (routes != null && routes.Count > 0)
        {
            foreach (var route in routes)
            {
                var result = await _revalidationManager.RevalidateAsync(route, cancellationToken);
                if (result.Success)
                    revalidated.Add(route);
            }
        }

        if (tags != null && tags.Count > 0)
        {
            var tagResult = await _revalidationManager.InvalidateByTagsAsync(tags, cancellationToken);
            if (tagResult.Success && tagResult.Routes != null)
            {
                revalidated.AddRange(tagResult.Routes);
            }
        }

        return revalidated.Count > 0
            ? RevalidationResult.Ok(revalidated)
            : RevalidationResult.Ok(Array.Empty<string>());
    }
}
