namespace NextNet.ServerActions.Errors;

/// <summary>
/// Defines DS-6xx error codes for the NextNet ServerActions component.
/// Codes are code-only strings used as prefixes in formatted messages.
/// </summary>
/// <example>
/// Throw a clear action-not-found error:
/// <code>
/// throw new InvalidOperationException(
///     $"{ServerActionErrorCodes.ActionNotFound}: Action '{actionName}' not found.");
/// </code>
/// Or return it in an action error response:
/// <code>
/// return ActionError.NotFound(
///     $"{ServerActionErrorCodes.ActionNotFound}: Action '{actionName}' not found.");
/// </code>
/// </example>
public static class ServerActionErrorCodes
{
    /// <summary>
    /// DS-600: Action name is required but was null or empty.
    /// </summary>
    /// <example>
    /// <code>
    /// if (string.IsNullOrWhiteSpace(actionName))
    ///     throw new ArgumentException(
    ///         $"{ServerActionErrorCodes.ActionNameRequired}: Action name is required.", nameof(actionName));
    /// </code>
    /// </example>
    public const string ActionNameRequired = "DS-600";

    /// <summary>
    /// DS-601: The specified action was not found in the registry.
    /// </summary>
    /// <example>
    /// <code>
    /// return ActionError.NotFound(
    ///     $"{ServerActionErrorCodes.ActionNotFound}: Action '{actionName}' not found.");
    /// </code>
    /// </example>
    public const string ActionNotFound = "DS-601";

    /// <summary>
    /// DS-602: Authentication is required for this action.
    /// </summary>
    /// <example>
    /// <code>
    /// return ActionError.Unauthorized(
    ///     $"{ServerActionErrorCodes.AuthenticationRequired}: Authentication required for action '{actionName}'.");
    /// </code>
    /// </example>
    public const string AuthenticationRequired = "DS-602";

    /// <summary>
    /// DS-603: Anti-forgery token validation failed.
    /// </summary>
    public const string AntiForgeryValidationFailed = "DS-603";

    /// <summary>
    /// DS-604: Failed to deserialize the request body.
    /// </summary>
    public const string RequestDeserializationFailed = "DS-604";

    /// <summary>
    /// DS-605: Action execution failed due to an unexpected error.
    /// </summary>
    public const string ActionExecutionFailed = "DS-605";

    /// <summary>
    /// DS-606: Invalid parameter type conversion.
    /// </summary>
    public const string InvalidParameterTypeConversion = "DS-606";

    /// <summary>
    /// DS-607: An action with the same name is already registered.
    /// </summary>
    public const string ActionAlreadyRegistered = "DS-607";

    /// <summary>
    /// DS-608: Assembly scan failed.
    /// </summary>
    public const string AssemblyScanFailed = "DS-608";

    /// <summary>
    /// DS-609: Serialization context is missing a required type.
    /// </summary>
    public const string SerializationContextMissingType = "DS-609";
}
