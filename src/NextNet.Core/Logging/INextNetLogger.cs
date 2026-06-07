namespace NextNet.Logging;

/// <summary>
/// Abstraction for logging within NextNet components.
/// Provides structured logging with severity levels and scope support.
/// </summary>
public interface INextNetLogger
{
    /// <summary>
    /// Logs an informational message.
    /// </summary>
    /// <param name="message">The message template.</param>
    /// <param name="args">Optional positional arguments for the message template.</param>
    void Info(string message, params object?[] args);

    /// <summary>
    /// Logs a warning message.
    /// </summary>
    /// <param name="message">The message template.</param>
    /// <param name="args">Optional positional arguments for the message template.</param>
    void Warn(string message, params object?[] args);

    /// <summary>
    /// Logs an error message.
    /// </summary>
    /// <param name="message">The message template.</param>
    /// <param name="args">Optional positional arguments for the message template.</param>
    void Error(string message, params object?[] args);

    /// <summary>
    /// Logs a debug message.
    /// Debug messages may be filtered out in non-development environments.
    /// </summary>
    /// <param name="message">The message template.</param>
    /// <param name="args">Optional positional arguments for the message template.</param>
    void Debug(string message, params object?[] args);

    /// <summary>
    /// Begins a logical operation scope.
    /// </summary>
    /// <param name="scopeName">The name of the scope.</param>
    /// <returns>An <see cref="IDisposable"/> that ends the scope when disposed.</returns>
    IDisposable BeginScope(string scopeName);
}
