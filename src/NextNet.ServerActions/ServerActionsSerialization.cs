using System.Text.Json;

namespace NextNet.ServerActions;

/// <summary>
/// Internal helper for JSON serialization settings and utilities
/// shared across the ServerActions project.
/// </summary>
internal static class ServerActionsSerialization
{
    /// <summary>
    /// Default JSON serializer options for server action serialization.
    /// </summary>
    public static readonly JsonSerializerOptions DefaultOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        Converters =
        {
            new System.Text.Json.Serialization.JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        }
    };

    /// <summary>
    /// Serializes a value to the specified stream as JSON.
    /// </summary>
    public static async Task SerializeAsync(Stream stream, object value)
    {
        await JsonSerializer.SerializeAsync(stream, value, value.GetType(), DefaultOptions);
    }
}
