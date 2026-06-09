namespace NextNet.TemplateRegistry;

/// <summary>
/// The exception that is thrown when the registry API rate limit has been exceeded.
/// </summary>
/// <remarks>
/// <para>
/// Error code: DS-722. This exception is thrown when the registry responds with a
/// 429 Too Many Requests status code. It extends <see cref="TemplateRegistryException"/>
/// to carry the error code for structured error handling.
/// </para>
/// </remarks>
public sealed class RateLimitException : TemplateRegistryException
{
    /// <summary>
    /// Gets the duration to wait before retrying the request.
    /// </summary>
    public TimeSpan RetryAfter { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimitException"/> class.
    /// </summary>
    /// <param name="retryAfter">The suggested retry delay.</param>
    public RateLimitException(TimeSpan retryAfter)
        : base(TemplateRegistryErrorCodes.RateLimitExceeded, $"Rate limited. Retry after {retryAfter.TotalSeconds} seconds.")
    {
        RetryAfter = retryAfter;
    }
}
