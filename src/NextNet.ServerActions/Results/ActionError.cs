namespace NextNet.ServerActions.Results;

/// <summary>
/// Factory for creating error action results.
/// Provides typed error responses for validation, not-found, unauthorized, and server errors.
/// </summary>
/// <example>
/// Return a validation error:
/// <code>
/// return ActionError.Validation("Email is required");
/// </code>
/// Return a not-found error:
/// <code>
/// return ActionError.NotFound("User not found");
/// </code>
/// Return an unauthorized error:
/// <code>
/// return ActionError.Unauthorized("Access denied");
/// </code>
/// </example>
public static class ActionError
{
    /// <summary>
    /// Creates a validation error result with a single message.
    /// Returns HTTP 400.
    /// </summary>
    public static ActionResult<T> Validation<T>(string message)
    {
        return new ActionResult<T>
        {
            IsSuccess = false,
            IsError = true,
            ErrorType = "validation",
            StatusCode = 400,
            Message = message
        };
    }

    /// <summary>
    /// Creates a validation error result with a dictionary of field-level errors.
    /// Returns HTTP 400.
    /// </summary>
    public static ActionResult<T> Validation<T>(IReadOnlyDictionary<string, string[]> errors)
    {
        return new ActionResult<T>
        {
            IsSuccess = false,
            IsError = true,
            ErrorType = "validation",
            StatusCode = 400,
            Errors = errors
        };
    }

    /// <summary>
    /// Creates a validation error result with a single message (non-generic).
    /// Returns HTTP 400.
    /// </summary>
    public static ActionResult Validation(string message)
    {
        return new SimpleErrorResult
        {
            IsSuccess = false,
            IsError = true,
            ErrorType = "validation",
            StatusCode = 400,
            Message = message
        };
    }

    /// <summary>
    /// Creates a not-found error result.
    /// Returns HTTP 404.
    /// </summary>
    public static ActionResult<T> NotFound<T>(string message = "Not found")
    {
        return new ActionResult<T>
        {
            IsSuccess = false,
            IsError = true,
            ErrorType = "notFound",
            StatusCode = 404,
            Message = message
        };
    }

    /// <summary>
    /// Creates a not-found error result (non-generic).
    /// Returns HTTP 404.
    /// </summary>
    public static ActionResult NotFound(string message = "Not found")
    {
        return new SimpleErrorResult
        {
            IsSuccess = false,
            IsError = true,
            ErrorType = "notFound",
            StatusCode = 404,
            Message = message
        };
    }

    /// <summary>
    /// Creates an unauthorized error result.
    /// Returns HTTP 401.
    /// </summary>
    public static ActionResult<T> Unauthorized<T>(string message = "Unauthorized")
    {
        return new ActionResult<T>
        {
            IsSuccess = false,
            IsError = true,
            ErrorType = "unauthorized",
            StatusCode = 401,
            Message = message
        };
    }

    /// <summary>
    /// Creates an unauthorized error result (non-generic).
    /// Returns HTTP 401.
    /// </summary>
    public static ActionResult Unauthorized(string message = "Unauthorized")
    {
        return new SimpleErrorResult
        {
            IsSuccess = false,
            IsError = true,
            ErrorType = "unauthorized",
            StatusCode = 401,
            Message = message
        };
    }

    /// <summary>
    /// Creates a generic error result.
    /// Returns HTTP 500.
    /// </summary>
    public static ActionResult<T> Error<T>(string message, Exception? ex = null)
    {
        return new ActionResult<T>
        {
            IsSuccess = false,
            IsError = true,
            ErrorType = "error",
            StatusCode = 500,
            Message = message
        };
    }

    /// <summary>
    /// Creates a generic error result (non-generic).
    /// Returns HTTP 500.
    /// </summary>
    public static ActionResult Error(string message, Exception? ex = null)
    {
        return new SimpleErrorResult
        {
            IsSuccess = false,
            IsError = true,
            ErrorType = "error",
            StatusCode = 500,
            Message = message
        };
    }

    /// <summary>
    /// Internal simple error result type.
    /// </summary>
    private sealed class SimpleErrorResult : ActionResult { }
}
