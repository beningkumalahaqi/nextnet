namespace NextNet.TemplateRegistry;

/// <summary>
/// The exception that is thrown when the template registry is unreachable or returns an error.
/// </summary>
/// <remarks>
/// <para>
/// Error code: DS-720. This exception is thrown when the registry API cannot be reached
/// or returns an unexpected status code. It extends <see cref="TemplateRegistryException"/>
/// to carry the error code for structured error handling.
/// </para>
/// </remarks>
public sealed class RegistryUnavailableException : TemplateRegistryException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RegistryUnavailableException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public RegistryUnavailableException(string message)
        : base(TemplateRegistryErrorCodes.RegistryUnavailable, message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RegistryUnavailableException"/> class with an inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="inner">The inner exception, or <c>null</c> if none.</param>
    public RegistryUnavailableException(string message, Exception? inner)
        : base(TemplateRegistryErrorCodes.RegistryUnavailable, message, inner)
    {
    }
}
