namespace NextNet.TemplateSecurity;

/// <summary>
/// Exception thrown when no key is found for a given publisher.
/// Error code: <see cref="TemplateSecurityErrorCodes.KeyNotFound"/> (DS-821).
/// </summary>
/// <remarks>
/// <example>
/// <code>
/// try
/// {
///     var key = await keyManager.GetKeyAsync(publisherId);
/// }
/// catch (PublisherKeyNotFoundException ex) when (ex.ErrorCode == TemplateSecurityErrorCodes.KeyNotFound)
/// {
///     Console.WriteLine($"No key found for publisher '{ex.PublisherId}'");
/// }
/// </code>
/// </example>
/// </remarks>
public sealed class PublisherKeyNotFoundException : TemplateSecurityException
{
    /// <summary>
    /// The publisher identifier for which no key was found.
    /// </summary>
    public string PublisherId { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PublisherKeyNotFoundException"/> class.
    /// </summary>
    /// <param name="publisherId">The publisher identifier.</param>
    public PublisherKeyNotFoundException(string publisherId)
        : base(TemplateSecurityErrorCodes.KeyNotFound, $"No key found for publisher '{publisherId}'.")
    {
        PublisherId = publisherId;
    }
}
