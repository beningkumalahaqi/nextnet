namespace NextNet.TemplateSecurity;

/// <summary>
/// Exception thrown when a computed checksum does not match the expected value.
/// </summary>
public sealed class ChecksumMismatchException : Exception
{
    /// <summary>
    /// The expected checksum value.
    /// </summary>
    public string Expected { get; }

    /// <summary>
    /// The actual computed checksum value.
    /// </summary>
    public string Actual { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChecksumMismatchException"/> class.
    /// </summary>
    /// <param name="expected">The expected checksum.</param>
    /// <param name="actual">The actual checksum.</param>
    public ChecksumMismatchException(string expected, string actual)
        : base($"Checksum mismatch. Expected: {expected}, Actual: {actual}")
    {
        Expected = expected;
        Actual = actual;
    }
}
