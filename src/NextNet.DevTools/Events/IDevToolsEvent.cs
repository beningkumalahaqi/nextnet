namespace NextNet.DevTools;

/// <summary>
/// Marker interface for all DevTools events published on the event bus.
/// </summary>
/// <example>
/// <code>
/// public sealed record MyCustomEvent : IDevToolsEvent
/// {
///     public string Data { get; init; }
/// }
/// </code>
/// </example>
public interface IDevToolsEvent { }

/// <summary>
/// Event fired when a new route is discovered by the route scanner.
/// </summary>
/// <example>
/// <code>
/// eventBus.Publish(new RouteDiscoveredEvent
/// {
///     Path = "/blog/[slug]",
///     Type = "dynamic",
///     File = "app/blog/[slug]/page.cs"
/// });
/// </code>
/// </example>
public sealed record RouteDiscoveredEvent : IDevToolsEvent
{
    /// <summary>The route path pattern (e.g., "/blog/[slug]").</summary>
    public string Path { get; init; } = string.Empty;

    /// <summary>Route type: static, dynamic, api, or layout.</summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>Source file path relative to the app directory.</summary>
    public string File { get; init; } = string.Empty;
}

/// <summary>
/// Event fired when a component finishes rendering.
/// </summary>
/// <example>
/// <code>
/// eventBus.Publish(new ComponentRenderedEvent
/// {
///     Component = "Header",
///     DurationMs = 42,
///     Route = "/"
/// });
/// </code>
/// </example>
public sealed record ComponentRenderedEvent : IDevToolsEvent
{
    /// <summary>The name of the component that rendered.</summary>
    public string Component { get; init; } = string.Empty;

    /// <summary>Render duration in milliseconds.</summary>
    public long DurationMs { get; init; }

    /// <summary>The route path being rendered.</summary>
    public string Route { get; init; } = string.Empty;
}

/// <summary>
/// Event fired when a build completes, including per-step metrics.
/// </summary>
/// <example>
/// <code>
/// eventBus.Publish(new BuildCompletedEvent
/// {
///     TotalDurationMs = 1500,
///     Success = true,
///     Steps = new[]
///     {
///         new BuildStepMetric { Name = "Compile", DurationMs = 800 },
///         new BuildStepMetric { Name = "Bundle", DurationMs = 700 }
///     }
/// });
/// </code>
/// </example>
public sealed record BuildCompletedEvent : IDevToolsEvent
{
    /// <summary>Total build duration in milliseconds.</summary>
    public long TotalDurationMs { get; init; }

    /// <summary>Whether the build completed successfully.</summary>
    public bool Success { get; init; }

    /// <summary>Individual build step metrics.</summary>
    public IReadOnlyList<BuildStepMetric> Steps { get; init; } = Array.Empty<BuildStepMetric>();
}

/// <summary>
/// A single build step metric (name + duration).
/// </summary>
/// <example>
/// <code>
/// var step = new BuildStepMetric { Name = "Compile", DurationMs = 500 };
/// </code>
/// </example>
public sealed record BuildStepMetric
{
    /// <summary>The name of the build step.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Duration of the build step in milliseconds.</summary>
    public long DurationMs { get; init; }
}

/// <summary>
/// Event fired when an HMR (Hot Module Replacement) update is applied.
/// </summary>
/// <example>
/// <code>
/// eventBus.Publish(new HmrUpdatedEvent
/// {
///     Files = new[] { "app/page.cs", "app/layout.cs" },
///     DurationMs = 120,
///     Success = true
/// });
/// </code>
/// </example>
public sealed record HmrUpdatedEvent : IDevToolsEvent
{
    /// <summary>The list of files that were updated.</summary>
    public IReadOnlyList<string> Files { get; init; } = Array.Empty<string>();

    /// <summary>Duration of the HMR update in milliseconds.</summary>
    public long DurationMs { get; init; }

    /// <summary>Whether the HMR update was applied successfully.</summary>
    public bool Success { get; init; }
}

/// <summary>
/// Event fired when an error occurs in the dev server.
/// </summary>
/// <example>
/// <code>
/// eventBus.Publish(new ErrorOccurredEvent
/// {
///     Message = "Null reference in route handler",
///     File = "app/page.cs",
///     Line = 42
/// });
/// </code>
/// </example>
public sealed record ErrorOccurredEvent : IDevToolsEvent
{
    /// <summary>Error message describing what went wrong.</summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>Optional file path where the error occurred.</summary>
    public string? File { get; init; }

    /// <summary>Optional line number where the error occurred.</summary>
    public int? Line { get; init; }
}
