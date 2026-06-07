namespace NextNet.TemplateSecurity;

/// <summary>
/// Exception thrown when a signature is invalid or cannot be verified.
/// </summary>
public sealed class SignatureInvalidException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SignatureInvalidException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public SignatureInvalidException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="SignatureInvalidException"/> class with an inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="inner">The inner exception.</param>
    public SignatureInvalidException(string message, Exception inner) : base(message, inner) { }
}
