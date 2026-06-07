namespace NextNet.TemplateSecurity;

/// <summary>
/// Exception thrown when a publisher has been revoked.
/// </summary>
public sealed class PublisherRevokedException : Exception
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
        : base($"Publisher '{publisherId}' has been revoked.")
    {
        PublisherId = publisherId;
    }
}
