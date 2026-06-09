using System.Text.Json;

namespace NextNet.DevTools.Headless;

/// <summary>
/// Provides structured API responses for the DevTools headless endpoints.
/// This is a helper used by the middleware to construct response DTOs with consistent JSON formatting.
/// </summary>
/// <example>
/// <code>
/// // Create a success response
/// var json = DevToolsApiController.CreateSuccessResponse(new { count = 5 });
///
/// // Create an error response
/// var errorJson = DevToolsApiController.CreateErrorResponse("Route not found", 404);
/// </code>
/// </example>
public static class DevToolsApiController
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes a response object to JSON using DevTools API conventions (camelCase, no indentation).
    /// </summary>
    /// <param name="data">The object to serialize.</param>
    /// <returns>A JSON string representation.</returns>
    public static string SerializeResponse(object data)
    {
        return JsonSerializer.Serialize(data, JsonOptions);
    }

    /// <summary>
    /// Creates a standard error response with the given message and status code.
    /// </summary>
    /// <param name="message">The error description.</param>
    /// <param name="statusCode">HTTP status code (default 400).</param>
    /// <returns>A JSON string with error, message, and statusCode fields.</returns>
    public static string CreateErrorResponse(string message, int statusCode = 400)
    {
        return SerializeResponse(new
        {
            error = true,
            message,
            statusCode
        });
    }

    /// <summary>
    /// Creates a standard success response wrapping the data payload.
    /// </summary>
    /// <param name="data">The data payload to include.</param>
    /// <returns>A JSON string with success=true and the data field.</returns>
    public static string CreateSuccessResponse(object data)
    {
        return SerializeResponse(new
        {
            success = true,
            data
        });
    }
}
