using System.Text.Json;

namespace NextNet.DevTools.Headless;

/// <summary>
/// Provides structured API responses for the DevTools headless endpoints.
/// This is a helper used by the middleware to construct response DTOs.
/// </summary>
public static class DevToolsApiController
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes a response object to JSON using DevTools API conventions.
    /// </summary>
    public static string SerializeResponse(object data)
    {
        return JsonSerializer.Serialize(data, JsonOptions);
    }

    /// <summary>
    /// Creates a standard error response.
    /// </summary>
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
    /// Creates a standard success response.
    /// </summary>
    public static string CreateSuccessResponse(object data)
    {
        return SerializeResponse(new
        {
            success = true,
            data
        });
    }
}
