namespace NextNet.Cli.UI;

/// <summary>
/// Defines the output mode for the NextNet CLI.
/// Controls whether ANSI colors, Unicode symbols, and structured output are used.
/// </summary>
public enum OutputMode
{
    /// <summary>
    /// Full color output with ANSI escape codes and Unicode symbols.
    /// </summary>
    Color = 0,

    /// <summary>
    /// Plain text output — no ANSI, no Unicode, machine-readable.
    /// </summary>
    Plain = 1,

    /// <summary>
    /// JSON structured output for CI/CD integration (reserved for future use).
    /// </summary>
    Json = 2
}
