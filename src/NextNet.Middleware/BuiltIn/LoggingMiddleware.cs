using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NextNet.Middleware.Attributes;

namespace NextNet.Middleware.BuiltIn;

/// <summary>
/// Middleware that logs HTTP request and response information including
/// method, path, status code, and duration.
/// </summary>
[MiddlewareOrderAttribute(MiddlewareOrder.Logging)]
public class LoggingMiddleware : IMiddleware
{
    private readonly ILogger<LoggingMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoggingMiddleware"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public LoggingMiddleware(ILogger<LoggingMiddleware> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task InvokeAsync(MiddlewareContext context, RequestDelegate next)
    {
        var httpContext = context.HttpContext;
        var stopwatch = Stopwatch.StartNew();

        var method = httpContext.Request.Method;
        var path = httpContext.Request.Path;

        _logger.LogInformation("HTTP {Method} {Path} — started", method, path);

        try
        {
            await next(httpContext);

            stopwatch.Stop();
            var statusCode = httpContext.Response.StatusCode;
            var elapsed = stopwatch.ElapsedMilliseconds;

            _logger.LogInformation(
                "HTTP {Method} {Path} — {StatusCode} ({Elapsed}ms)",
                method, path, statusCode, elapsed);
        }
        catch (Exception)
        {
            stopwatch.Stop();
            _logger.LogWarning(
                "HTTP {Method} {Path} — exception after {Elapsed}ms",
                method, path, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}
