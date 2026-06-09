namespace NextNet.Exceptions;

/// <summary>
/// Exception thrown when an error occurs during HTML rendering,
/// such as circular component references, rendering timeout, or content generation failures.
/// </summary>
public sealed class RenderException : Exception
{
    /// <summary>
    /// Initializes a new instance of <see cref="RenderException"/>.
    /// </summary>
    public RenderException()
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="RenderException"/>
    /// with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public RenderException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="RenderException"/>
    /// with a specified error message and inner exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="inner">The exception that is the cause of the current exception.</param>
    public RenderException(string message, Exception inner) : base(message, inner)
    {
    }
}
