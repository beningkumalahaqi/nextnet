namespace NextNet.Errors;

/// <summary>
/// Defines error codes for the NextNet.Core library.
/// Each code follows the DS-NNN format used across the framework.
/// </summary>
public static class CoreErrorCodes
{
    /// <summary>
    /// The HttpContext has not been set on the component or route handler.
    /// </summary>
    public const string HttpContextNotSet = "DS-000";

    /// <summary>
    /// Failed to parse the NextNet configuration file.
    /// </summary>
    public const string ConfigParseFailed = "DS-001";

    /// <summary>
    /// The application directory (AppDir) configuration value is empty or whitespace.
    /// </summary>
    public const string ConfigAppDirEmpty = "DS-002";

    /// <summary>
    /// The development port (DevPort) configuration value is out of the valid range (1–65535).
    /// </summary>
    public const string ConfigDevPortOutOfRange = "DS-003";

    /// <summary>
    /// The watch debounce milliseconds configuration value is invalid (must be greater than zero).
    /// </summary>
    public const string ConfigWatchDebounceInvalid = "DS-004";

    /// <summary>
    /// A required file was not found by the file system abstraction.
    /// </summary>
    public const string FileNotFound = "DS-005";

    /// <summary>
    /// Access to a file or directory was denied by the operating system.
    /// </summary>
    public const string FileSystemAccessDenied = "DS-006";
}
