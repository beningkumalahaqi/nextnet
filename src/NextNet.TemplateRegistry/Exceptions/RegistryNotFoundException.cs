namespace NextNet.TemplateRegistry;

/// <summary>
/// The exception that is thrown when a requested resource is not found in the template registry.
/// </summary>
public sealed class RegistryNotFoundException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RegistryNotFoundException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public RegistryNotFoundException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="RegistryNotFoundException"/> class with an inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="inner">The inner exception.</param>
    public RegistryNotFoundException(string message, Exception inner) : base(message, inner) { }
}
