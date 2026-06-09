using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace NextNet.ServerActions.ServerActions;

/// <summary>
/// Invokes server action methods using cached compiled delegates for performance.
/// Handles parameter binding: deserialized request parameters, DI-resolved services,
/// and CancellationToken.
/// </summary>
/// <example>
/// The invoker is used internally by <see cref="ServerActionExecutor"/>:
/// <code>
/// var invoker = new ServerActionInvoker();
/// var result = await invoker.InvokeAsync(descriptor, parameters, serviceProvider);
/// </code>
/// </example>
public sealed class ServerActionInvoker
{
    private static readonly ConcurrentDictionary<MethodInfo, Func<object?[], object?>?> InvocationCache = new();

    /// <summary>
    /// Invokes the specified server action method with the given parameters.
    /// </summary>
    /// <param name="descriptor">The action descriptor.</param>
    /// <param name="requestParameters">Parameters deserialized from the request body (as a dictionary).</param>
    /// <param name="serviceProvider">The request-scoped service provider for DI.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The action result returned by the method.</returns>
    public async Task<object?> InvokeAsync(
        ServerActionDescriptor descriptor,
        IReadOnlyDictionary<string, object?> requestParameters,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        if (descriptor == null)
            throw new ArgumentNullException(nameof(descriptor));
        if (requestParameters == null)
            throw new ArgumentNullException(nameof(requestParameters));
        if (serviceProvider == null)
            throw new ArgumentNullException(nameof(serviceProvider));

        // Build the parameter array
        var parameters = descriptor.Parameters;
        var args = new object?[parameters.Count];

        for (var i = 0; i < parameters.Count; i++)
        {
            var param = parameters[i];

            if (param.IsCancellationToken)
            {
                args[i] = cancellationToken;
            }
            else if (param.IsService)
            {
                args[i] = ResolveService(serviceProvider, param.ParameterType);
            }
            else
            {
                // Deserialized from request body
                if (requestParameters.TryGetValue(param.Name, out var value))
                {
                    args[i] = ConvertValue(value, param.ParameterType);
                }
                else
                {
                    args[i] = param.ParameterType.IsValueType
                        ? Activator.CreateInstance(param.ParameterType)
                        : null;
                }
            }
        }

        // Get or create target instance (for instance methods)
        object? target = null;
        if (!descriptor.IsStatic)
        {
            target = ActivatorUtilities.CreateInstance(serviceProvider, descriptor.DeclaringType);

            // If the target implements IServerAction, inject services
            if (target is IServerAction actionTarget)
            {
                actionTarget.SetServices(serviceProvider);
            }
        }

        // Get or build compiled delegate
        var invoker = InvocationCache.GetOrAdd(descriptor.MethodInfo, BuildInvoker);

        // Build invoke args: [target, param1, param2, ...]
        var invokeArgs = new object?[1 + args.Length];
        invokeArgs[0] = target;
        Array.Copy(args, 0, invokeArgs, 1, args.Length);

        // Invoke
        var result = invoker?.Invoke(invokeArgs);

        // If the method returns a Task, await it
        if (result is Task task)
        {
            await task;
            var taskType = task.GetType();
            if (taskType.IsGenericType && taskType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                var resultProperty = taskType.GetProperty("Result");
                return resultProperty?.GetValue(task);
            }
            return null; // Task (non-generic) completed
        }

        return result;
    }

    private static Func<object?[], object?>? BuildInvoker(MethodInfo method)
    {
        var parameters = method.GetParameters();
        var parameterTypes = parameters.Select(p => p.ParameterType).ToArray();

        // For void-returning methods
        if (method.ReturnType == typeof(void))
        {
            var instanceParam = Expression.Parameter(typeof(object?[]), "args");
            var targetParam = Expression.ArrayIndex(instanceParam, Expression.Constant(0));

            var callParams = new Expression[parameterTypes.Length];
            for (var i = 0; i < parameterTypes.Length; i++)
            {
                var arg = Expression.ArrayIndex(instanceParam, Expression.Constant(i + 1));
                callParams[i] = Expression.Convert(arg, parameterTypes[i]);
            }

            var call = method.IsStatic
                ? Expression.Call(method, callParams)
                : Expression.Call(Expression.Convert(targetParam, method.DeclaringType!), method, callParams);

            var block = Expression.Block(call, Expression.Constant(null, typeof(object)));
            return Expression.Lambda<Func<object?[], object?>>(block, instanceParam).Compile();
        }

        // For methods returning a value or Task
        var instanceParam2 = Expression.Parameter(typeof(object?[]), "args");
        var targetParam2 = Expression.ArrayIndex(instanceParam2, Expression.Constant(0));

        var callParams2 = new Expression[parameterTypes.Length];
        for (var i = 0; i < parameterTypes.Length; i++)
        {
            var arg = Expression.ArrayIndex(instanceParam2, Expression.Constant(i + 1));
            callParams2[i] = Expression.Convert(arg, parameterTypes[i]);
        }

        var call2 = method.IsStatic
            ? (Expression)Expression.Call(method, callParams2)
            : Expression.Call(Expression.Convert(targetParam2, method.DeclaringType!), method, callParams2);

        if (method.ReturnType != typeof(void))
        {
            var convert = Expression.Convert(call2, typeof(object));
            return Expression.Lambda<Func<object?[], object?>>(convert, instanceParam2).Compile();
        }

        return null;
    }

    private static object? ResolveService(IServiceProvider serviceProvider, Type serviceType)
    {
        if (serviceType == typeof(HttpContext))
        {
            return serviceProvider.GetService(typeof(IHttpContextAccessor)) is IHttpContextAccessor accessor
                ? accessor.HttpContext
                : null;
        }

        if (serviceType == typeof(IServiceProvider))
            return serviceProvider;

        return serviceProvider.GetService(serviceType);
    }

    private static object? ConvertValue(object? value, Type targetType)
    {
        if (value == null)
            return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;

        if (targetType.IsInstanceOfType(value))
            return value;

        // Handle JsonElement first — deserialize from its raw text
        if (value is System.Text.Json.JsonElement jsonElement)
        {
            if (targetType == typeof(string))
                return jsonElement.GetString() ?? jsonElement.ToString();

            if (targetType == typeof(int))
                return jsonElement.GetInt32();

            if (targetType == typeof(long))
                return jsonElement.GetInt64();

            if (targetType == typeof(double))
                return jsonElement.GetDouble();

            if (targetType == typeof(decimal))
                return jsonElement.GetDecimal();

            if (targetType == typeof(bool))
                return jsonElement.GetBoolean();

            if (targetType == typeof(Guid))
                return jsonElement.GetGuid();

            if (targetType == typeof(DateTime))
                return jsonElement.GetDateTime();

            if (targetType == typeof(DateTimeOffset))
                return jsonElement.GetDateTimeOffset();

            // Complex type — deserialize from raw JSON
            return System.Text.Json.JsonSerializer.Deserialize(
                jsonElement.GetRawText(), targetType,
                ServerActionsSerialization.DefaultOptions);
        }

        // Handle simple type conversions from non-JsonElement values
        if (targetType == typeof(string))
            return value.ToString();

        if (targetType == typeof(int))
            return Convert.ToInt32(value);

        if (targetType == typeof(long))
            return Convert.ToInt64(value);

        if (targetType == typeof(double))
            return Convert.ToDouble(value);

        if (targetType == typeof(decimal))
            return Convert.ToDecimal(value);

        if (targetType == typeof(bool))
            return Convert.ToBoolean(value);

        if (targetType == typeof(Guid))
            return Guid.Parse(value.ToString()!);

        if (targetType == typeof(DateTime))
            return Convert.ToDateTime(value);

        if (targetType == typeof(DateTimeOffset))
            return DateTimeOffset.Parse(value.ToString()!);

        return value;
    }
}
