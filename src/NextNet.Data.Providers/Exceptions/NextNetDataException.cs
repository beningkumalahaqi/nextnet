namespace NextNet.Data.Exceptions;

/// <summary>
/// Base exception type for all NextNet Data framework exceptions.
/// Provides an error code for programmatic identification of error types.
/// </summary>
/// <remarks>
/// <para>
/// All exceptions in the NextNet Data layer inherit from <see cref="NextNetDataException"/>.
/// Each derived type carries a unique error code (e.g., <c>SKDATA_PROVIDER_001</c>) that
/// enables automated error handling, logging, and diagnostic tooling.
/// </para>
/// <example>
/// <code>
/// try
/// {
///     // ... data operation ...
/// }
/// catch (NextNetDataException ex)
/// {
///     Console.WriteLine($"Error [{ex.ErrorCode}]: {ex.Message}");
/// }
/// </code>
/// </example>
/// </remarks>
public class NextNetDataException : Exception
{
    /// <summary>
    /// Gets the unique error code identifying the exception type
    /// (e.g., "SKDATA_PROVIDER_001").
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="NextNetDataException"/> class.
    /// </summary>
    /// <param name="errorCode">The unique error code for this exception.</param>
    /// <param name="message">A human-readable description of the error.</param>
    public NextNetDataException(string errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NextNetDataException"/> class
    /// with an inner exception.
    /// </summary>
    /// <param name="errorCode">The unique error code for this exception.</param>
    /// <param name="message">A human-readable description of the error.</param>
    /// <param name="innerException">The exception that caused this exception.</param>
    public NextNetDataException(string errorCode, string message, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}
