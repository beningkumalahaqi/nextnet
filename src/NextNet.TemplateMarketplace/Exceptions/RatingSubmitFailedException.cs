namespace NextNet.TemplateMarketplace;

/// <summary>
/// Thrown when submitting a rating or review to the marketplace fails.
/// </summary>
public sealed class RatingSubmitFailedException : Exception
{
    /// <summary>Initializes a new instance with the specified error message.</summary>
    public RatingSubmitFailedException(string message) : base(message) { }

    /// <summary>Initializes a new instance with the specified error message and inner exception.</summary>
    public RatingSubmitFailedException(string message, Exception inner) : base(message, inner) { }
}
