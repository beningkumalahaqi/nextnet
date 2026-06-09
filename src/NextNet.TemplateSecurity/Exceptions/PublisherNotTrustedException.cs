namespace NextNet.TemplateSecurity;

/// <summary>
/// Exception thrown when a publisher is not in the trusted list.
/// Error code: <see cref="TemplateSecurityErrorCodes.PublisherNotTrusted"/> (DS-824).
/// </summary>
/// <remarks>
/// <example>
/// <code>
/// try
/// {
///     var publisher = await registry.GetPublisherAsync(publisherId);
/// }
/// catch (PublisherNotTrustedException ex) when (ex.ErrorCode == TemplateSecurityErrorCodes.PublisherNotTrusted)
/// {
///     Console.WriteLine($"Publisher '{ex.PublisherId}' is not in the trusted list.");
/// }
/// </code>
/// </example>
/// </remarks>
public sealed class PublisherNotTrustedException : TemplateSecurityException
{
    /// <summary>
    /// The publisher identifier that was rejected.
    /// </summary>
    public string PublisherId { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PublisherNotTrustedException"/> class.
    /// </summary>
    /// <param name="publisherId">The untrusted publisher identifier.</param>
    public PublisherNotTrustedException(string publisherId)
        : base(TemplateSecurityErrorCodes.PublisherNotTrusted, $"Publisher '{publisherId}' is not in the trusted list.")
    {
        PublisherId = publisherId;
    }
}
