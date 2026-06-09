using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NextNet.Middleware.Attributes;

namespace NextNet.Middleware.BuiltIn;

/// <summary>
/// Options for the <see cref="ErrorHandlingMiddleware"/>.
/// </summary>
/// <example>
/// <code>
/// // Enable detailed error responses in development
/// services.Configure&lt;ErrorHandlingOptions&gt;(options =>
/// {
///     options.IncludeExceptionDetails = true;
/// });
///
/// // Use a custom error handler
/// services.Configure&lt;ErrorHandlingOptions&gt;(options =>
/// {
///     options.CustomErrorHandler = async (ctx, ex) =>
///     {
///         ctx.Response.StatusCode = 500;
///         await ctx.Response.WriteAsync("Custom error");
///     };
/// });
/// </code>
/// </example>
public sealed record ErrorHandlingOptions
{
    /// <summary>
    /// Gets or sets whether to include exception details in error responses.
    /// Defaults to false. Should only be set to true in development.
    /// </summary>
    public bool IncludeExceptionDetails { get; set; }

    /// <summary>
    /// Gets or sets whether to use a custom error handler instead of the default JSON response.
    /// </summary>
    public Func<HttpContext, Exception, Task>? CustomErrorHandler { get; set; }
}

/// <summary>
/// Middleware that catches unhandled exceptions from downstream middleware
/// and returns a structured JSON error response.
/// </summary>
/// <example>
/// <code>
/// // In pipeline configuration (runs last by default):
/// pipeline.Use&lt;ErrorHandlingMiddleware&gt;();
///
/// // The middleware wraps all downstream middleware and catches
/// // any unhandled exceptions, returning a JSON error response.
/// </code>
/// </example>
[MiddlewareOrderAttribute(MiddlewareOrder.ErrorHandling)]
public sealed class ErrorHandlingMiddleware : IMiddleware
{
    private readonly ErrorHandlingOptions _options;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment? _environment;

    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorHandlingMiddleware"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="options">Optional error handling options.</param>
    /// <param name="environment">Optional web host environment.</param>
    public ErrorHandlingMiddleware(
        ILogger<ErrorHandlingMiddleware> logger,
        IOptions<ErrorHandlingOptions>? options = null,
        IWebHostEnvironment? environment = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? new ErrorHandlingOptions();
        _environment = environment;
    }

    /// <inheritdoc />
    public async Task InvokeAsync(MiddlewareContext context, RequestDelegate next)
    {
        try
        {
            await next(context.HttpContext);
        }
        catch (OperationCanceledException) when (context.HttpContext.RequestAborted.IsCancellationRequested)
        {
            // Client disconnected — no-op
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context.HttpContext, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext httpContext, Exception exception)
    {
        _logger.LogError(exception, "Unhandled exception processing request {Method} {Path}",
            httpContext.Request.Method, httpContext.Request.Path);

        if (_options.CustomErrorHandler != null)
        {
            await _options.CustomErrorHandler(httpContext, exception);
            return;
        }

        httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        httpContext.Response.ContentType = "application/json; charset=utf-8";

        var showDetails = _options.IncludeExceptionDetails
            || _environment?.EnvironmentName == "Development";

        var errorCode = Errors.MiddlewareErrorCodes.TerminalDelegateError;
        var errorResponse = new
        {
            error = "Internal server error",
            code = errorCode,
            detail = showDetails ? exception.ToString() : null,
            type = showDetails ? exception.GetType().FullName : null,
        };

        await httpContext.Response.WriteAsJsonAsync(errorResponse);
    }
}
