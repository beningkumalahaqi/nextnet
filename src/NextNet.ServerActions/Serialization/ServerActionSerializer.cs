using System.Text.Json;
using NextNet.ServerActions.Results;

namespace NextNet.ServerActions.Serialization;

/// <summary>
/// Provides JSON serialization and deserialization for server action parameters and results.
/// Uses <see cref="System.Text.Json"/> with source-generated context support for AOT compatibility.
/// </summary>
/// <example>
/// <code>
/// var serializer = new ServerActionSerializer();
/// var json = serializer.Serialize(ActionSuccess.With(new { id = 1 }));
/// var parameters = serializer.DeserializeParameters(@"{""name"":""Alice""}");
/// var result = serializer.DeserializeResult&lt;string&gt;(json);
/// </code>
/// </example>
public sealed class ServerActionSerializer
{
    /// <summary>
    /// Serializes an action result to a JSON string.
    /// Uses the runtime type to include derived type properties (e.g. <c>ActionResult&lt;T&gt;.Data</c>).
    /// </summary>
    public string Serialize(ActionResult result)
    {
        return JsonSerializer.Serialize(result, result.GetType(), ServerActionsSerialization.DefaultOptions);
    }

    /// <summary>
    /// Deserializes a JSON string to the specified type.
    /// </summary>
    public T? Deserialize<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json, ServerActionsSerialization.DefaultOptions);
    }

    /// <summary>
    /// Deserializes a JSON string to an action result of the specified type.
    /// </summary>
    public ActionResult<T>? DeserializeResult<T>(string json)
    {
        return JsonSerializer.Deserialize<ActionResult<T>>(json, ServerActionsSerialization.DefaultOptions);
    }

    /// <summary>
    /// Deserializes request parameters from a JSON string.
    /// </summary>
    public Dictionary<string, object?> DeserializeParameters(string json)
    {
        return JsonSerializer.Deserialize<Dictionary<string, object?>>(
            json, ServerActionsSerialization.DefaultOptions) ?? new();
    }
}
