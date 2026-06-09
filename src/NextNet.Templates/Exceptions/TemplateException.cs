namespace NextNet.Templates.Exceptions;

/// <summary>
/// The abstract base class for all NextNet template-related exceptions.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="TemplateException"/> provides a common base for the template exception
/// hierarchy. All template exceptions carry an <see cref="ErrorCode"/> property that
/// uniquely identifies the error type (e.g., "DS-760" for template not found).
/// </para>
/// <para>
/// Derive from this class when creating new template exception types to ensure
/// consistent error handling and reporting.
/// </para>
/// </remarks>
public abstract class TemplateException : Exception
{
    /// <summary>
    /// Gets the unique error code for this exception (e.g., "DS-760").
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TemplateException"/> class.
    /// </summary>
    /// <param name="errorCode">The unique error code identifying this exception type.</param>
    /// <param name="message">A human-readable description of the error.</param>
    /// <param name="inner">An optional inner exception that caused this error.</param>
    protected TemplateException(string errorCode, string message, Exception? inner = null)
        : base(message, inner)
    {
        ErrorCode = errorCode;
    }
}
