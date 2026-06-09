namespace NextNet.TemplateRegistry;

/// <summary>
/// The abstract base class for all NextNet template-registry-related exceptions.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="TemplateRegistryException"/> provides a common base for the registry
/// exception hierarchy. All registry exceptions carry an <see cref="ErrorCode"/>
/// property that uniquely identifies the error type (e.g., "DS-720" for registry
/// unavailable).
/// </para>
/// <para>
/// Derive from this class when creating new registry exception types to ensure
/// consistent error handling and reporting.
/// </para>
/// </remarks>
public abstract class TemplateRegistryException : Exception
{
    /// <summary>
    /// Gets the unique error code for this exception (e.g., "DS-720").
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TemplateRegistryException"/> class.
    /// </summary>
    /// <param name="errorCode">The unique error code identifying this exception type.</param>
    /// <param name="message">A human-readable description of the error.</param>
    /// <param name="inner">An optional inner exception that caused this error.</param>
    protected TemplateRegistryException(string errorCode, string message, Exception? inner = null)
        : base(message, inner)
    {
        ErrorCode = errorCode;
    }
}
