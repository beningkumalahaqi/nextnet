namespace NextNet.TemplateRegistry;

/// <summary>
/// The exception that is thrown when the template registry is unreachable or returns an error.
/// </summary>
public sealed class RegistryUnavailableException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RegistryUnavailableException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public RegistryUnavailableException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="RegistryUnavailableException"/> class with an inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="inner">The inner exception, or <c>null</c> if none.</param>
    public RegistryUnavailableException(string message, Exception? inner) : base(message, inner) { }
}
