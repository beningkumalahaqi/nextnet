namespace NextNet.DesignSystem.Parsing;

/// <summary>
/// Specifies the serialization format for design token definition files.
/// </summary>
/// <remarks>
/// This enumeration is used by <see cref="TokenParser"/> to determine how to deserialize
/// token data from a string. Currently supports <see cref="Json"/> format.
/// </remarks>
/// <example>
/// <code>
/// var tokens = TokenParser.Parse(jsonContent, TokenFileFormat.Json);
/// </code>
/// </example>
public enum TokenFileFormat
{
    /// <summary>
    /// JSON format. Tokens are represented as nested JSON objects.
    /// This is the default and recommended format.
    /// </summary>
    Json,

    /// <summary>
    /// YAML format (not yet implemented).
    /// </summary>
    Yaml
}
