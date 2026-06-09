namespace NextNet.TemplateSdk;

/// <summary>
/// Represents errors that occur during Template SDK operations.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="TemplateSdkException"/> is the base exception type for all Template SDK
/// errors. It carries an <see cref="ErrorCode"/> property that uniquely identifies
/// the error type (e.g., "DS-740" for source directory not found).
/// </para>
/// <para>
/// Throw or derive from this class to ensure consistent error handling and reporting
/// across all Template SDK operations.
/// </para>
/// </remarks>
public class TemplateSdkException : Exception
{
    /// <summary>
    /// Gets the unique error code for this exception (e.g., "DS-740").
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TemplateSdkException"/> class.
    /// </summary>
    /// <param name="errorCode">The unique error code identifying this exception type.</param>
    /// <param name="message">A human-readable description of the error.</param>
    /// <param name="inner">An optional inner exception that caused this error.</param>
    public TemplateSdkException(string errorCode, string message, Exception? inner = null)
        : base(message, inner)
    {
        ErrorCode = errorCode;
    }
}
