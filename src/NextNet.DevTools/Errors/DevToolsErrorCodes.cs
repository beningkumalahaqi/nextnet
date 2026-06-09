namespace NextNet.DevTools.Errors;

/// <summary>
/// Defines error codes for the NextNet DevTools system (DS-900..DS-919 range).
/// </summary>
/// <example>
/// <code>
/// // Inject error code into an error response
/// return Results.Problem($"DS-900: Unknown DevTools endpoint '{path}'");
/// </code>
/// </example>
public static class DevToolsErrorCodes
{
    /// <summary>DS-900: An unknown DevTools endpoint was requested.</summary>
    public const string UnknownEndpoint = "DS-900";

    /// <summary>DS-901: A requested route was not found in the route table.</summary>
    public const string RouteNotFound = "DS-901";

    /// <summary>DS-902: A WebSocket connection failed.</summary>
    public const string WebSocketConnectionFailed = "DS-902";

    /// <summary>DS-903: The DevTools data store has exceeded its capacity and entries are being trimmed.</summary>
    public const string DataStoreCapacityExceeded = "DS-903";

    /// <summary>DS-904: A DevTools panel failed to render.</summary>
    public const string PanelRenderError = "DS-904";

    /// <summary>DS-905: An invalid DevTools operating mode was specified.</summary>
    public const string InvalidDevToolsMode = "DS-905";

    /// <summary>DS-906: The DevTools server is already running and cannot be started again.</summary>
    public const string DevToolsServerAlreadyRunning = "DS-906";

    /// <summary>DS-907: The DevTools server is not running and cannot perform the requested operation.</summary>
    public const string DevToolsServerNotRunning = "DS-907";

    /// <summary>DS-908: The DevTools event bus failed to publish an event.</summary>
    public const string EventBusPublishFailed = "DS-908";

    /// <summary>DS-909: The DevTools configuration is invalid.</summary>
    public const string ConfigurationInvalid = "DS-909";
}
