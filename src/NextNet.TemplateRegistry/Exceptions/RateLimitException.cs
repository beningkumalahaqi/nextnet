namespace NextNet.TemplateRegistry;

/// <summary>
/// The exception that is thrown when the registry API rate limit has been exceeded.
/// </summary>
public sealed class RateLimitException : Exception
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
        : base($"Rate limited. Retry after {retryAfter.TotalSeconds} seconds.")
    {
        RetryAfter = retryAfter;
    }
}
