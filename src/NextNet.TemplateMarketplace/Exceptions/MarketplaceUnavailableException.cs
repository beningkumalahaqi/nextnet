namespace NextNet.TemplateMarketplace;

/// <summary>
/// Thrown when the marketplace API is unreachable or returns a server error.
/// Error code: <see cref="TemplateMarketplaceErrorCodes.MarketplaceUnavailable"/> (DS-920).
/// </summary>
/// <remarks>
/// <para>
/// This exception is thrown by <see cref="MarketplaceApiClient"/> when the marketplace
/// API is unreachable or returns a server error. The <see cref="TemplateMarketplaceException.ErrorCode"/>
/// property is set to <c>DS-920</c> for programmatic identification.
/// </para>
/// <example>
/// <code>
/// try
/// {
///     var ratings = await client.GetRatingsAsync("my-template");
/// }
/// catch (MarketplaceUnavailableException ex) when (ex.ErrorCode == TemplateMarketplaceErrorCodes.MarketplaceUnavailable)
/// {
///     Console.WriteLine($"Marketplace is down: {ex.Message}");
/// }
/// </code>
/// </example>
/// </remarks>
public sealed class MarketplaceUnavailableException : TemplateMarketplaceException
{
    /// <summary>Initializes a new instance with the specified error message.</summary>
    /// <param name="message">A human-readable description of the error.</param>
    public MarketplaceUnavailableException(string message)
        : base(TemplateMarketplaceErrorCodes.MarketplaceUnavailable, message) { }

    /// <summary>Initializes a new instance with the specified error message and inner exception.</summary>
    /// <param name="message">A human-readable description of the error.</param>
    /// <param name="inner">The exception that caused this exception.</param>
    public MarketplaceUnavailableException(string message, Exception inner)
        : base(TemplateMarketplaceErrorCodes.MarketplaceUnavailable, message, inner) { }
}
