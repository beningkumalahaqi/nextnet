namespace NextNet.Edge.Compatibility.EdgeApiSubset;

/// <summary>
/// Provides detailed information about types that are explicitly allowed
/// on edge runtimes. Organises the whitelisted types by category.
/// </summary>
public static class AllowedTypes
{
    /// <summary>
    /// Gets the set of allowed serialisation-related types.
    /// </summary>
    public static IReadOnlySet<string> Serialization { get; } = new HashSet<string>
    {
        "System.Text.Json.JsonSerializer",
        "System.Text.Json.JsonDocument",
        "System.Text.Json.JsonElement",
        "System.Text.Json.JsonProperty",
        "System.Text.Json.JsonSerializerOptions",
        "System.Text.Json.Utf8JsonWriter",
        "System.Text.Json.Utf8JsonReader",
        "System.Text.Encoding",
        "System.Text.StringBuilder",
        "System.Text.RegularExpressions.Regex",
    };

    /// <summary>
    /// Gets the set of allowed collection-related types.
    /// </summary>
    public static IReadOnlySet<string> Collections { get; } = new HashSet<string>
    {
        "System.Collections.Generic.List`1",
        "System.Collections.Generic.Dictionary`2",
        "System.Collections.Generic.HashSet`1",
        "System.Collections.Generic.Queue`1",
        "System.Collections.Generic.Stack`1",
        "System.Collections.Generic.LinkedList`1",
        "System.Collections.Generic.SortedList`2",
        "System.Collections.Generic.SortedDictionary`2",
        "System.Collections.Generic.SortedSet`1",
        "System.Collections.Concurrent.ConcurrentDictionary`2",
        "System.Collections.Concurrent.ConcurrentQueue`1",
        "System.Collections.Concurrent.ConcurrentBag`1",
        "System.Collections.Concurrent.ConcurrentStack`1",
        "System.Collections.Immutable.ImmutableArray`1",
        "System.Collections.Immutable.ImmutableList`1",
        "System.Collections.Immutable.ImmutableDictionary`2",
    };

    /// <summary>
    /// Gets the set of allowed async/task-related types.
    /// </summary>
    public static IReadOnlySet<string> Async { get; } = new HashSet<string>
    {
        "System.Threading.Tasks.Task",
        "System.Threading.Tasks.Task`1",
        "System.Threading.Tasks.ValueTask",
        "System.Threading.Tasks.ValueTask`1",
        "System.Threading.Tasks.TaskCompletionSource`1",
        "System.Threading.CancellationToken",
        "System.Threading.CancellationTokenSource",
    };

    /// <summary>
    /// Gets the set of allowed in-memory I/O types.
    /// </summary>
    public static IReadOnlySet<string> MemoryIO { get; } = new HashSet<string>
    {
        "System.IO.Stream",
        "System.IO.MemoryStream",
        "System.IO.TextReader",
        "System.IO.TextWriter",
        "System.IO.StringReader",
        "System.IO.StringWriter",
        "System.IO.StreamReader",
        "System.IO.StreamWriter",
        "System.IO.Pipelines.Pipe",
        "System.IO.Pipelines.PipeWriter",
        "System.IO.Pipelines.PipeReader",
    };

    /// <summary>
    /// Gets the set of allowed networking types (outbound only).
    /// </summary>
    public static IReadOnlySet<string> Networking { get; } = new HashSet<string>
    {
        "System.Net.Http.HttpClient",
        "System.Net.Http.HttpRequestMessage",
        "System.Net.Http.HttpResponseMessage",
        "System.Net.Http.Headers.HttpHeaders",
        "System.Net.Http.Json.HttpClientJsonExtensions",
        "System.Net.Http.HttpMethod",
        "System.Net.HttpStatusCode",
    };

    /// <summary>
    /// Gets all allowed types across all categories.
    /// </summary>
    public static IReadOnlySet<string> All { get; } = new HashSet<string>(
        Serialization
            .Concat(Collections)
            .Concat(Async)
            .Concat(MemoryIO)
            .Concat(Networking));
}
