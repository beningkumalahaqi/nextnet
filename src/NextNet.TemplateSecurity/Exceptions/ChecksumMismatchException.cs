namespace NextNet.TemplateSecurity;

/// <summary>
/// Exception thrown when a computed checksum does not match the expected value.
/// Error code: <see cref="TemplateSecurityErrorCodes.ChecksumMismatch"/> (DS-820).
/// </summary>
/// <remarks>
/// <example>
/// <code>
/// try
/// {
///     await verifier.VerifyAsync(stream, expectedChecksum);
/// }
/// catch (ChecksumMismatchException ex) when (ex.ErrorCode == TemplateSecurityErrorCodes.ChecksumMismatch)
/// {
///     Console.WriteLine($"Checksum mismatch: expected {ex.Expected}, got {ex.Actual}");
/// }
/// </code>
/// </example>
/// </remarks>
public sealed class ChecksumMismatchException : TemplateSecurityException
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
        : base(TemplateSecurityErrorCodes.ChecksumMismatch, $"Checksum mismatch. Expected: {expected}, Actual: {actual}")
    {
        Expected = expected;
        Actual = actual;
    }
}
