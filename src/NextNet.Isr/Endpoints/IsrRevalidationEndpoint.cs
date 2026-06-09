using System.Text.Json;
using Microsoft.AspNetCore.Http;
using NextNet.Isr.Revalidation;
using NextNet.Logging;

namespace NextNet.Isr.Endpoints;

/// <summary>
/// Handles the on-demand revalidation endpoint (<c>POST /_isr/revalidate</c>).
/// Validates the secret and triggers revalidation for the specified route or tags.
/// </summary>
public sealed class IsrRevalidationEndpoint
{
    private readonly OnDemandRevalidator _onDemandRevalidator;
    private readonly INextNetLogger? _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Initializes a new instance of <see cref="IsrRevalidationEndpoint"/>.
    /// </summary>
    /// <param name="onDemandRevalidator">The on-demand revalidator.</param>
    /// <param name="logger">Optional logger.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onDemandRevalidator"/> is null.</exception>
    public IsrRevalidationEndpoint(
        OnDemandRevalidator onDemandRevalidator,
        INextNetLogger? logger = null)
    {
        _onDemandRevalidator = onDemandRevalidator ?? throw new ArgumentNullException(nameof(onDemandRevalidator));
        _logger = logger;
    }

    /// <summary>
    /// Handles the revalidation request.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task HandleAsync(HttpContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        // Only accept POST
        if (!HttpMethods.IsPost(context.Request.Method))
        {
            context.Response.StatusCode = StatusCodes.Status405MethodNotAllowed;
            await WriteJsonResponse(context, new { error = $"[{IsrErrorCodes.MethodNotAllowed}] Method not allowed. Use POST." });
            return;
        }

        // Parse the request body
        IsrRevalidationRequest? request;
        try
        {
            request = await JsonSerializer.DeserializeAsync<IsrRevalidationRequest>(
                context.Request.Body, JsonOptions, context.RequestAborted);
        }
        catch (JsonException ex)
        {
            _logger?.Warn("Invalid revalidation request body: {Exception}", ex);
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await WriteJsonResponse(context, new { error = $"[{IsrErrorCodes.InvalidRequestBody}] Invalid request body." });
            return;
        }

        if (request == null)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await WriteJsonResponse(context, new { error = $"[{IsrErrorCodes.RequestBodyRequired}] Request body is required." });
            return;
        }

        // Validate secret
        if (!_onDemandRevalidator.ValidateSecret(request.Secret))
        {
            _logger?.Warn("Revalidation request with invalid secret");
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await WriteJsonResponse(context, new { error = $"[{IsrErrorCodes.InvalidOrMissingSecret}] Invalid or missing secret." });
            return;
        }

        // Must specify path or tags
        if (string.IsNullOrWhiteSpace(request.Path) && (request.Tags == null || request.Tags.Length == 0))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await WriteJsonResponse(context, new { error = $"[{IsrErrorCodes.PathOrTagsRequired}] Either 'path' or 'tags' must be specified." });
            return;
        }

        RevalidationResult result;

        if (!string.IsNullOrWhiteSpace(request.Path))
        {
            // Revalidate a single route
            _logger?.Info("On-demand revalidation requested for path: {Path}", request.Path);
            result = await _onDemandRevalidator.RevalidateRouteAsync(request.Path, context.RequestAborted);
        }
        else
        {
            // Revalidate by tags
            _logger?.Info("On-demand revalidation requested for tags: {Tags}", string.Join(", ", request.Tags!));
            result = await _onDemandRevalidator.RevalidateByTagsAsync(request.Tags!, context.RequestAborted);
        }

        // Write response
        if (result.Success)
        {
            context.Response.StatusCode = StatusCodes.Status200OK;
            await WriteJsonResponse(context, new
            {
                revalidated = result.RevalidatedCount,
                routes = result.Routes ?? (result.Route != null ? new[] { result.Route } : Array.Empty<string>())
            });
        }
        else
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await WriteJsonResponse(context, new
            {
                error = result.ErrorMessage ?? $"[{IsrErrorCodes.RevalidationFailed}] Revalidation failed.",
                revalidated = 0
            });
        }
    }

    private static async Task WriteJsonResponse(HttpContext context, object value)
    {
        context.Response.ContentType = "application/json; charset=utf-8";
        await JsonSerializer.SerializeAsync(context.Response.Body, value, JsonOptions, context.RequestAborted);
    }
}
