using System.Text;

namespace NextNet.Logging;

/// <summary>
/// Default implementation of <see cref="INextNetLogger"/> that writes
/// formatted log messages to the console with timestamps and colors.
/// </summary>
public sealed class NextNetLogger : INextNetLogger
{
    private readonly string _categoryName;
    private readonly object _lock = new();
    private readonly Stack<string> _scopeStack = new();

    /// <summary>
    /// Initializes a new instance of <see cref="NextNetLogger"/>.
    /// </summary>
    /// <param name="categoryName">The category name for this logger instance (e.g. component name).</param>
    public NextNetLogger(string? categoryName = null)
    {
        _categoryName = categoryName ?? string.Empty;
    }

    /// <inheritdoc />
    public void Info(string message, params object?[] args)
    {
        WriteLog("info", ConsoleColor.Green, message, args);
    }

    /// <inheritdoc />
    public void Warn(string message, params object?[] args)
    {
        WriteLog("warn", ConsoleColor.Yellow, message, args);
    }

    /// <inheritdoc />
    public void Error(string message, params object?[] args)
    {
        WriteLog("error", ConsoleColor.Red, message, args);
    }

    /// <inheritdoc />
    public void Debug(string message, params object?[] args)
    {
        WriteLog("debug", ConsoleColor.DarkGray, message, args);
    }

    /// <inheritdoc />
    public IDisposable BeginScope(string scopeName)
    {
        lock (_lock)
        {
            _scopeStack.Push(scopeName);
        }

        return new ScopePopper(this, scopeName);
    }

    private void WriteLog(string level, ConsoleColor color, string message, object?[] args)
    {
        lock (_lock)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            var scopePrefix = BuildScopePrefix();
            var formattedMessage = FormatMessage(message, args);

            // Attempt to use colored output; fall back to plain if color not supported
            try
            {
                Console.ResetColor();
                Console.Write('[');
                Console.Write(timestamp);
                Console.Write("] ");

                if (!string.IsNullOrEmpty(_categoryName))
                {
                    Console.Write('[');
                    Console.Write(_categoryName);
                    Console.Write("] ");
                }

                if (!string.IsNullOrEmpty(scopePrefix))
                {
                    Console.Write(scopePrefix);
                    Console.Write(' ');
                }

                Console.ForegroundColor = color;
                Console.Write(level.PadRight(5));
                Console.ResetColor();
                Console.Write(": ");
                Console.WriteLine(formattedMessage);
            }
            catch
            {
                // Fallback if console colors are not supported (e.g. redirected output)
                Console.WriteLine($"[{timestamp}] [{_categoryName}] {scopePrefix}{level.PadRight(5)}: {formattedMessage}");
            }
        }
    }

    private string BuildScopePrefix()
    {
        if (_scopeStack.Count == 0)
            return string.Empty;

        var sb = new StringBuilder();
        foreach (var scope in _scopeStack.Reverse())
        {
            sb.Append("=> ");
            sb.Append(scope);
            sb.Append(' ');
        }

        return sb.ToString().TrimEnd();
    }

    private static string FormatMessage(string message, object?[] args)
    {
        if (args == null || args.Length == 0)
            return message;

        try
        {
            return string.Format(message, args);
        }
        catch (FormatException)
        {
            return message;
        }
    }

    /// <summary>
    /// Removes the specified scope from the stack. Called when a scope is disposed.
    /// </summary>
    /// <param name="scopeName">The scope name to remove.</param>
    internal void PopScope(string scopeName)
    {
        lock (_lock)
        {
            if (_scopeStack.Count > 0 && _scopeStack.Peek() == scopeName)
            {
                _scopeStack.Pop();
            }
        }
    }

    private sealed class ScopePopper : IDisposable
    {
        private readonly NextNetLogger _logger;
        private readonly string _scopeName;
        private bool _disposed;

        public ScopePopper(NextNetLogger logger, string scopeName)
        {
            _logger = logger;
            _scopeName = scopeName;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _logger.PopScope(_scopeName);
            }
        }
    }
}
