namespace NextNet.TemplateMarketplace;

/// <summary>
/// Thrown when submitting a rating or review to the marketplace fails.
/// Error code: <see cref="TemplateMarketplaceErrorCodes.RatingSubmitFailed"/> (DS-921).
/// </summary>
/// <remarks>
/// <para>
/// This exception is thrown when submitting a rating or review to the marketplace fails.
/// The <see cref="TemplateMarketplaceException.ErrorCode"/> property is set to <c>DS-921</c>
/// for programmatic identification.
/// </para>
/// <example>
/// <code>
/// try
/// {
///     await client.SubmitRatingAsync("my-template", 5, "Great template!");
/// }
/// catch (RatingSubmitFailedException ex) when (ex.ErrorCode == TemplateMarketplaceErrorCodes.RatingSubmitFailed)
/// {
///     Console.WriteLine($"Failed to submit rating: {ex.Message}");
/// }
/// </code>
/// </example>
/// </remarks>
public sealed class RatingSubmitFailedException : TemplateMarketplaceException
{
    /// <summary>Initializes a new instance with the specified error message.</summary>
    /// <param name="message">A human-readable description of the error.</param>
    public RatingSubmitFailedException(string message)
        : base(TemplateMarketplaceErrorCodes.RatingSubmitFailed, message) { }

    /// <summary>Initializes a new instance with the specified error message and inner exception.</summary>
    /// <param name="message">A human-readable description of the error.</param>
    /// <param name="inner">The exception that caused this exception.</param>
    public RatingSubmitFailedException(string message, Exception inner)
        : base(TemplateMarketplaceErrorCodes.RatingSubmitFailed, message, inner) { }
}
