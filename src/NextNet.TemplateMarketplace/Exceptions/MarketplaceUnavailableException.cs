namespace NextNet.TemplateMarketplace;

/// <summary>
/// Thrown when the marketplace API is unreachable or returns a server error.
/// </summary>
public sealed class MarketplaceUnavailableException : Exception
{
    /// <summary>Initializes a new instance with the specified error message.</summary>
    public MarketplaceUnavailableException(string message) : base(message) { }

    /// <summary>Initializes a new instance with the specified error message and inner exception.</summary>
    public MarketplaceUnavailableException(string message, Exception inner) : base(message, inner) { }
}
