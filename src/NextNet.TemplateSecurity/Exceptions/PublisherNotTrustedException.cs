namespace NextNet.TemplateSecurity;

/// <summary>
/// Exception thrown when a publisher is not in the trusted list.
/// </summary>
public sealed class PublisherNotTrustedException : Exception
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
        : base($"Publisher '{publisherId}' is not in the trusted list.")
    {
        PublisherId = publisherId;
    }
}
