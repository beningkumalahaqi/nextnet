using System.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace NextNet.Build.Production.Logging;

/// <summary>
/// Middleware that measures and logs request duration.
/// </summary>
public sealed class RequestTimingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ProductionLogger _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="RequestTimingMiddleware"/>.
    /// </summary>
    public RequestTimingMiddleware(RequestDelegate next, ProductionLogger logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Processes the request, measuring and logging timing.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();
        var method = context.Request.Method ?? "UNKNOWN";
        var path = context.Request.Path.Value ?? "/";

        try
        {
            await _next(context);
        }
        finally
        {
            sw.Stop();
            var statusCode = context.Response.StatusCode;

            _logger.LogRequest(
                method,
                path,
                statusCode,
                sw.ElapsedMilliseconds,
                null);
        }
    }
}
