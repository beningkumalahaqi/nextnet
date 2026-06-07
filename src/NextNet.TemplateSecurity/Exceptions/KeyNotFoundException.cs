namespace NextNet.TemplateSecurity;

/// <summary>
/// Exception thrown when no key is found for a given publisher.
/// </summary>
public sealed class KeyNotFoundException : Exception
{
    /// <summary>
    /// The publisher identifier for which no key was found.
    /// </summary>
    public string PublisherId { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="KeyNotFoundException"/> class.
    /// </summary>
    /// <param name="publisherId">The publisher identifier.</param>
    public KeyNotFoundException(string publisherId)
        : base($"No key found for publisher '{publisherId}'.")
    {
        PublisherId = publisherId;
    }
}
