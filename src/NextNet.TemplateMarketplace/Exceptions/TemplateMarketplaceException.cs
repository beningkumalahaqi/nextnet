namespace NextNet.TemplateMarketplace;

/// <summary>
/// Base exception type for all NextNet Template Marketplace exceptions.
/// Provides an error code for programmatic identification of error types.
/// </summary>
/// <remarks>
/// <para>
/// All exceptions in the NextNet Template Marketplace layer should inherit from
/// <see cref="TemplateMarketplaceException"/>. Each derived type carries a unique
/// error code from <see cref="TemplateMarketplaceErrorCodes"/> (e.g., <c>DS-920</c>)
/// that enables automated error handling, logging, and diagnostic tooling.
/// </para>
/// <example>
/// <code>
/// try
/// {
///     // ... marketplace operation ...
/// }
/// catch (TemplateMarketplaceException ex)
/// {
///     Console.WriteLine($"Error [{ex.ErrorCode}]: {ex.Message}");
/// }
/// </code>
/// </example>
/// </remarks>
public abstract class TemplateMarketplaceException : Exception
{
    /// <summary>
    /// Gets the unique error code identifying the exception type
    /// (e.g., "DS-920").
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TemplateMarketplaceException"/> class.
    /// </summary>
    /// <param name="errorCode">The unique error code for this exception.</param>
    /// <param name="message">A human-readable description of the error.</param>
    /// <param name="inner">An optional inner exception that caused this error.</param>
    protected TemplateMarketplaceException(string errorCode, string message, Exception? inner = null)
        : base(message, inner)
    {
        ErrorCode = errorCode;
    }
}
