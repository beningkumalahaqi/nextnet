namespace NextNet.Exceptions;

/// <summary>
/// Exception thrown when the NextNet configuration is invalid,
/// cannot be loaded, or contains unsupported values.
/// </summary>
public sealed class NextNetConfigurationException : Exception
{
    /// <summary>
    /// Initializes a new instance of <see cref="NextNetConfigurationException"/>.
    /// </summary>
    public NextNetConfigurationException()
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="NextNetConfigurationException"/>
    /// with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public NextNetConfigurationException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="NextNetConfigurationException"/>
    /// with a specified error message and inner exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="inner">The exception that is the cause of the current exception.</param>
    public NextNetConfigurationException(string message, Exception inner) : base(message, inner)
    {
    }
}
