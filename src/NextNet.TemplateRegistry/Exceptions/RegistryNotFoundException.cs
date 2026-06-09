namespace NextNet.TemplateRegistry;

/// <summary>
/// The exception that is thrown when a requested resource is not found in the template registry.
/// </summary>
/// <remarks>
/// <para>
/// Error code: DS-721. This exception is thrown when the registry API responds with a
/// 404 Not Found for the requested resource. It extends <see cref="TemplateRegistryException"/>
/// to carry the error code for structured error handling.
/// </para>
/// </remarks>
public sealed class RegistryNotFoundException : TemplateRegistryException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RegistryNotFoundException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public RegistryNotFoundException(string message)
        : base(TemplateRegistryErrorCodes.RegistryNotFound, message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RegistryNotFoundException"/> class with an inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="inner">The inner exception.</param>
    public RegistryNotFoundException(string message, Exception inner)
        : base(TemplateRegistryErrorCodes.RegistryNotFound, message, inner)
    {
    }
}
