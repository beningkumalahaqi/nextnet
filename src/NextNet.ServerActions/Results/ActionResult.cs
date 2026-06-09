using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;

namespace NextNet.ServerActions.Results;

/// <summary>
/// Base class for server action results.
/// Provides a uniform response envelope over the wire.
/// </summary>
/// <example>
/// Return from a server action method:
/// <code>
/// [ServerAction]
/// public static Task&lt;ActionResult&gt; Ping()
/// {
///     return Task.FromResult(ActionSuccess.Empty());
/// }
/// </code>
/// The JSON response will contain <c>{"isSuccess":true,"isError":false}</c>.
/// </example>
public abstract class ActionResult
{
    /// <summary>
    /// Indicates whether the action succeeded.
    /// </summary>
    [JsonPropertyName("isSuccess")]
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Indicates whether the action failed.
    /// </summary>
    [JsonPropertyName("isError")]
    public bool IsError { get; set; }

    /// <summary>
    /// The HTTP status code for the response.
    /// </summary>
    [JsonIgnore]
    public int StatusCode { get; set; } = 200;

    /// <summary>
    /// A human-readable message associated with the result.
    /// </summary>
    [JsonPropertyName("message")]
    public string? Message { get; set; }

    /// <summary>
    /// The error type discriminator (e.g. "validation", "notFound", "unauthorized", "error").
    /// Only set when <see cref="IsError"/> is <c>true</c>.
    /// </summary>
    [JsonPropertyName("errorType")]
    public string? ErrorType { get; set; }

    /// <summary>
    /// A dictionary of field-level validation errors.
    /// Only set when <see cref="ErrorType"/> is "validation".
    /// </summary>
    [JsonPropertyName("errors")]
    public IReadOnlyDictionary<string, string[]>? Errors { get; set; }

    /// <summary>
    /// Writes the action result to the HTTP response.
    /// </summary>
    public virtual async Task WriteAsync(HttpContext context)
    {
        context.Response.StatusCode = StatusCode;
        context.Response.ContentType = "application/json; charset=utf-8";
        await ServerActionsSerialization.SerializeAsync(context.Response.Body, this);
    }
}

/// <summary>
/// Generic server action result that carries typed payload data.
/// </summary>
/// <example>
/// Return typed data from a server action:
/// <code>
/// [ServerAction]
/// public static Task&lt;ActionResult&lt;UserDto&gt;&gt; GetUser(int id)
/// {
///     var user = new UserDto { Id = id, Name = "Alice" };
///     return Task.FromResult(ActionSuccess.With(user));
/// }
/// </code>
/// The JSON response will contain <c>{"isSuccess":true,"data":{"id":1,"name":"Alice"}}</c>.
/// </example>
public sealed class ActionResult<T> : ActionResult
{
    /// <summary>
    /// The success payload data. Only set when <see cref="ActionResult.IsSuccess"/> is <c>true</c>.
    /// </summary>
    [JsonPropertyName("data")]
    public T? Data { get; set; }
}
