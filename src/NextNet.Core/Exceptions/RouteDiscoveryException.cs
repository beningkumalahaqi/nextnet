namespace NextNet.Exceptions;

/// <summary>
/// Exception thrown when the route discovery process encounters an error,
/// such as duplicate routes, invalid route patterns, or file system access issues.
/// </summary>
public class RouteDiscoveryException : Exception
{
    /// <summary>
    /// Initializes a new instance of <see cref="RouteDiscoveryException"/>.
    /// </summary>
    public RouteDiscoveryException()
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="RouteDiscoveryException"/>
    /// with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public RouteDiscoveryException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="RouteDiscoveryException"/>
    /// with a specified error message and inner exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="inner">The exception that is the cause of the current exception.</param>
    public RouteDiscoveryException(string message, Exception inner) : base(message, inner)
    {
    }
}
