namespace NextNet.TemplateSecurity;

/// <summary>
/// Exception thrown when a publisher has been revoked.
/// Error code: <see cref="TemplateSecurityErrorCodes.PublisherRevoked"/> (DS-823).
/// </summary>
/// <remarks>
/// <example>
/// <code>
/// try
/// {
///     var publisher = await registry.GetPublisherAsync(publisherId);
/// }
/// catch (PublisherRevokedException ex) when (ex.ErrorCode == TemplateSecurityErrorCodes.PublisherRevoked)
/// {
///     Console.WriteLine($"Publisher '{ex.PublisherId}' has been revoked.");
/// }
/// </code>
/// </example>
/// </remarks>
public sealed class PublisherRevokedException : TemplateSecurityException
{
    /// <summary>
    /// The publisher identifier that was revoked.
    /// </summary>
    public string PublisherId { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PublisherRevokedException"/> class.
    /// </summary>
    /// <param name="publisherId">The revoked publisher identifier.</param>
    public PublisherRevokedException(string publisherId)
        : base(TemplateSecurityErrorCodes.PublisherRevoked, $"Publisher '{publisherId}' has been revoked.")
    {
        PublisherId = publisherId;
    }
}
