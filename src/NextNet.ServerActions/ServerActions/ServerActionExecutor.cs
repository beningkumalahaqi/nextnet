using Microsoft.AspNetCore.Http;
using NextNet.ServerActions.Results;
using NextNet.ServerActions.Serialization;

namespace NextNet.ServerActions.ServerActions;

/// <summary>
/// Coordinates the full server action execution pipeline:
/// 1. Lookup action from registry
/// 2. Deserialize request parameters
/// 3. Invoke the action method via <see cref="ServerActionInvoker"/>
/// 4. Serialize the result to the HTTP response
/// </summary>
public sealed class ServerActionExecutor
{
    private readonly ServerActionRegistry _registry;
    private readonly ServerActionInvoker _invoker;
    private readonly ServerActionSerializer _serializer;

    /// <summary>
    /// Initializes a new instance of <see cref="ServerActionExecutor"/>.
    /// </summary>
    public ServerActionExecutor(
        ServerActionRegistry registry,
        ServerActionInvoker invoker,
        ServerActionSerializer serializer)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _invoker = invoker ?? throw new ArgumentNullException(nameof(invoker));
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
    }

    /// <summary>
    /// Executes a server action from the current HTTP context.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="actionName">The action name to execute.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task ExecuteAsync(
        HttpContext context,
        string actionName,
        CancellationToken cancellationToken = default)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));
        if (string.IsNullOrWhiteSpace(actionName))
            throw new ArgumentException("Action name is required.", nameof(actionName));

        // 1. Lookup action
        if (!_registry.TryGetAction(actionName, out var descriptor) || descriptor == null)
        {
            context.Response.StatusCode = 404;
            context.Response.ContentType = "application/json; charset=utf-8";
            var errorJson = _serializer.Serialize(
                ActionError.NotFound($"Action '{actionName}' not found."));
            await context.Response.WriteAsync(errorJson, cancellationToken);
            return;
        }

        // 2. Check authentication (only if RequireAuth is true)
        if (descriptor.RequireAuth &&
            context.User.Identity?.IsAuthenticated != true)
        {
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json; charset=utf-8";
            var errorJson = _serializer.Serialize(ActionError.Unauthorized());
            await context.Response.WriteAsync(errorJson, cancellationToken);
            return;
        }

        try
        {
            // 3. Deserialize request parameters
            var requestParameters = await DeserializeRequestAsync(context, descriptor);

            // 4. Invoke action method
            var result = await _invoker.InvokeAsync(
                descriptor,
                requestParameters,
                context.RequestServices,
                cancellationToken);

            // 5. Handle the result
            if (result is ActionResult actionResult)
            {
                await actionResult.WriteAsync(context);
            }
            else if (result is IResult aspNetResult)
            {
                await aspNetResult.ExecuteAsync(context);
            }
            else if (result != null)
            {
                // Wrap in a success result
                context.Response.StatusCode = 200;
                context.Response.ContentType = "application/json; charset=utf-8";
                var json = _serializer.Serialize(ActionSuccess.With(result));
                await context.Response.WriteAsync(json, cancellationToken);
            }
            else
            {
                // Null result
                context.Response.StatusCode = 200;
                context.Response.ContentType = "application/json; charset=utf-8";
                var json = _serializer.Serialize(ActionSuccess.Empty());
                await context.Response.WriteAsync(json, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json; charset=utf-8";
            var errorJson = _serializer.Serialize(
                ActionError.Error("An error occurred while executing the action", ex));
            await context.Response.WriteAsync(errorJson, cancellationToken);
        }
    }

    private async Task<IReadOnlyDictionary<string, object?>> DeserializeRequestAsync(
        HttpContext context,
        ServerActionDescriptor descriptor)
    {
        if (context.Request.ContentLength == null || context.Request.ContentLength == 0)
            return new Dictionary<string, object?>();

        var contentType = context.Request.ContentType ?? string.Empty;

        if (contentType.StartsWith("application/json", StringComparison.OrdinalIgnoreCase))
        {
            // Read the body as a single JSON object and match properties to parameters
            using var reader = new StreamReader(context.Request.Body);
            var body = await reader.ReadToEndAsync();
            if (string.IsNullOrWhiteSpace(body))
                return new Dictionary<string, object?>();

            // Deserialize the body to a dictionary first (case-insensitive keys)
            var parameters = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object?>>(
                body, ServerActionsSerialization.DefaultOptions);

            if (parameters != null)
                return new Dictionary<string, object?>(parameters, StringComparer.OrdinalIgnoreCase);

            // If the body is not a JSON object (e.g. a simple string, number), try extracting
            // as a single parameter
            var deserializableParams = descriptor.Parameters
                .Where(p => !p.IsService && !p.IsCancellationToken)
                .ToList();

            if (deserializableParams.Count == 1)
            {
                var param = deserializableParams[0];
                try
                {
                    var deserialized = System.Text.Json.JsonSerializer.Deserialize(
                        body, param.ParameterType, ServerActionsSerialization.DefaultOptions);
                    return new Dictionary<string, object?> { { param.Name, deserialized } };
                }
                catch
                {
                    // Fall through to return empty dictionary
                }
            }

            return new Dictionary<string, object?>();
        }

        if (contentType.StartsWith("multipart/form-data", StringComparison.OrdinalIgnoreCase) ||
            contentType.StartsWith("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase))
        {
            // Form data
            var form = await context.Request.ReadFormAsync();
            var parameters = new Dictionary<string, object?>();
            foreach (var key in form.Keys)
            {
                if (key != null)
                {
                    parameters[key] = form[key].FirstOrDefault();
                }
            }
            // Handle file uploads
            foreach (var file in form.Files)
            {
                parameters[file.Name] = file;
            }
            return parameters;
        }

        return new Dictionary<string, object?>();
    }
}
