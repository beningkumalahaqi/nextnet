namespace NextNet.Plugins.Errors;

/// <summary>
/// Defines DS-8xx error codes used throughout the NextNet.Plugins system.
/// These codes are embedded in exception messages and log entries to
/// provide traceable error classification.
/// </summary>
public static class PluginErrorCodes
{
    /// <summary>
    /// DS-800: Plugin assembly file not found at the specified path.
    /// </summary>
    public const string AssemblyNotFound = "DS-800";

    /// <summary>
    /// DS-801: The plugin type does not implement <see cref="INextNetPlugin"/>.
    /// </summary>
    public const string InvalidPluginType = "DS-801";

    /// <summary>
    /// DS-802: A plugin with the same name is already registered.
    /// </summary>
    public const string AlreadyRegistered = "DS-802";

    /// <summary>
    /// DS-803: A circular dependency was detected in the plugin dependency graph.
    /// </summary>
    public const string CircularDependency = "DS-803";

    /// <summary>
    /// DS-804: Plugin initialization failed during <see cref="INextNetPlugin.OnInitializeAsync"/>.
    /// </summary>
    public const string InitializationFailed = "DS-804";

    /// <summary>
    /// DS-805: The plugin manifest is malformed or missing required fields.
    /// </summary>
    public const string ManifestInvalid = "DS-805";

    /// <summary>
    /// DS-806: The <see cref="PluginAssemblyLoadContext"/> failed to load the assembly.
    /// </summary>
    public const string AlcLoadFailure = "DS-806";

    /// <summary>
    /// DS-807: Plugin configuration entry is invalid (e.g., missing name, bad JSON).
    /// </summary>
    public const string ConfigInvalid = "DS-807";

    /// <summary>
    /// DS-808: The plugin directory was not found.
    /// </summary>
    public const string DirectoryNotFound = "DS-808";

    /// <summary>
    /// DS-809: Failed to create an instance of the plugin type.
    /// </summary>
    public const string InstanceCreationFailed = "DS-809";

    /// <summary>
    /// DS-810: A dependency required by a plugin was not found in the registry.
    /// </summary>
    public const string DependencyNotFound = "DS-810";

    /// <summary>
    /// DS-811: The plugin assembly load context does not support unloading.
    /// </summary>
    public const string UnloadingNotSupported = "DS-811";
}
