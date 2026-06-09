namespace NextNet.TemplateSecurity;

/// <summary>
/// Base exception type for all NextNet Template Security exceptions.
/// Provides an error code for programmatic identification of error types.
/// </summary>
/// <remarks>
/// <para>
/// All exceptions in the NextNet Template Security layer should inherit from
/// <see cref="TemplateSecurityException"/>. Each derived type carries a unique
/// error code from <see cref="TemplateSecurityErrorCodes"/> (e.g., <c>DS-820</c>)
/// that enables automated error handling, logging, and diagnostic tooling.
/// </para>
/// <example>
/// <code>
/// try
/// {
///     // ... security operation ...
/// }
/// catch (TemplateSecurityException ex)
/// {
///     Console.WriteLine($"Error [{ex.ErrorCode}]: {ex.Message}");
/// }
/// </code>
/// </example>
/// </remarks>
public abstract class TemplateSecurityException : Exception
{
    /// <summary>
    /// Gets the unique error code identifying the exception type
    /// (e.g., "DS-820").
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TemplateSecurityException"/> class.
    /// </summary>
    /// <param name="errorCode">The unique error code for this exception.</param>
    /// <param name="message">A human-readable description of the error.</param>
    /// <param name="inner">An optional inner exception that caused this error.</param>
    protected TemplateSecurityException(string errorCode, string message, Exception? inner = null)
        : base(message, inner)
    {
        ErrorCode = errorCode;
    }
}
