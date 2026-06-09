namespace NextNet.TemplateSecurity;

/// <summary>
/// Exception thrown when a signature is invalid or cannot be verified.
/// Error code: <see cref="TemplateSecurityErrorCodes.SignatureInvalid"/> (DS-822).
/// </summary>
/// <remarks>
/// <example>
/// <code>
/// try
/// {
///     var isValid = await verifier.VerifySignatureAsync(data, signature, key);
/// }
/// catch (SignatureInvalidException ex) when (ex.ErrorCode == TemplateSecurityErrorCodes.SignatureInvalid)
/// {
///     Console.WriteLine($"Signature verification failed: {ex.Message}");
/// }
/// </code>
/// </example>
/// </remarks>
public sealed class SignatureInvalidException : TemplateSecurityException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SignatureInvalidException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public SignatureInvalidException(string message)
        : base(TemplateSecurityErrorCodes.SignatureInvalid, message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="SignatureInvalidException"/> class with an inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="inner">The inner exception.</param>
    public SignatureInvalidException(string message, Exception inner)
        : base(TemplateSecurityErrorCodes.SignatureInvalid, message, inner) { }
}
