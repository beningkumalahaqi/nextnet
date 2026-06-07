namespace NextNet.Middleware;

/// <summary>
/// Defines well-known order constants for middleware execution priority.
/// Middleware with lower order values execute earlier in the pipeline.
/// </summary>
public static class MiddlewareOrder
{
    /// <summary>
    /// Built-in Logging middleware order. Runs first to capture all requests.
    /// </summary>
    public const int Logging = 0;

    /// <summary>
    /// Built-in StaticFiles middleware order. Runs after logging, before compression.
    /// </summary>
    public const int StaticFiles = 100;

    /// <summary>
    /// Built-in Compression middleware order. Runs after static files.
    /// </summary>
    public const int Compression = 200;

    /// <summary>
    /// Built-in ErrorHandling middleware order. Runs last as a safety net.
    /// </summary>
    public const int ErrorHandling = 1000;

    /// <summary>
    /// Runs first before any other middleware.
    /// </summary>
    public const int First = int.MinValue;

    /// <summary>
    /// Runs early in the pipeline (e.g., security, CORS).
    /// </summary>
    public const int Early = -500;

    /// <summary>
    /// Default/normal priority.
    /// </summary>
    public const int Normal = 0;

    /// <summary>
    /// Runs late in the pipeline (e.g., response transformation).
    /// </summary>
    public const int Late = 500;

    /// <summary>
    /// Runs last after all other middleware.
    /// </summary>
    public const int Last = int.MaxValue;
}
