namespace NextNet.ServerActions.Results;

/// <summary>
/// Factory for creating successful action results.
/// Provides factory methods for wrapping return values in a standardized success envelope.
/// </summary>
/// <example>
/// Return a success with data:
/// <code>
/// return ActionSuccess.With(new { id = 1, name = "Alice" });
/// </code>
/// Return an empty success:
/// <code>
/// return ActionSuccess.Empty();
/// </code>
/// Return a success with a custom status code:
/// <code>
/// return ActionSuccess.WithStatus(201, "Created");
/// </code>
/// </example>
public static class ActionSuccess
{
    /// <summary>
    /// Creates a successful result with the specified data payload.
    /// </summary>
    public static ActionResult<T> With<T>(T data)
    {
        return new ActionResult<T>
        {
            IsSuccess = true,
            IsError = false,
            Data = data,
            StatusCode = 200
        };
    }

    /// <summary>
    /// Creates a successful result with the specified data payload and message.
    /// </summary>
    public static ActionResult<T> With<T>(T data, string? message)
    {
        return new ActionResult<T>
        {
            IsSuccess = true,
            IsError = false,
            Data = data,
            Message = message,
            StatusCode = 200
        };
    }

    /// <summary>
    /// Creates an empty successful result (no payload).
    /// </summary>
    public static ActionResult Empty()
    {
        return new EmptyActionResult
        {
            IsSuccess = true,
            IsError = false,
            StatusCode = 200
        };
    }

    /// <summary>
    /// Creates a successful result with the specified status code and message.
    /// </summary>
    public static ActionResult WithStatus(int statusCode, string? message = null)
    {
        return new EmptyActionResult
        {
            IsSuccess = true,
            IsError = false,
            StatusCode = statusCode,
            Message = message
        };
    }

    /// <summary>
    /// An empty result type used internally by <see cref="Empty"/>.
    /// </summary>
    private sealed class EmptyActionResult : ActionResult { }
}
