namespace NextNet.TemplateEngine.Variables;

/// <summary>
/// Options that control the behavior of <see cref="VariableReplacer"/>.
/// </summary>
/// <remarks>
/// <para>
/// All properties have sensible defaults, so typical usage can omit options entirely.
/// </para>
/// <para>
/// Delimiters default to <c>{{</c> and <c>}}</c> (Mustache-style). Custom delimiters of
/// any length are supported. The escape character defaults to backslash (<c>\</c>).
/// </para>
/// <example>
/// <code>
/// var options = new VariableReplacementOptions
/// {
///     OpenDelimiter = "${",
///     CloseDelimiter = "}",
///     ThrowOnUndefinedVariable = true,
///     CaseSensitive = false
/// };
/// var replacer = new VariableReplacer(options);
/// </code>
/// </example>
/// </remarks>
public sealed record VariableReplacementOptions
{
    /// <summary>
    /// Gets the opening delimiter for variable placeholders.
    /// Defaults to <c>{{</c>.
    /// </summary>
    public string OpenDelimiter { get; init; } = "{{";

    /// <summary>
    /// Gets the closing delimiter for variable placeholders.
    /// Defaults to <c>}}</c>.
    /// </summary>
    public string CloseDelimiter { get; init; } = "}}";

    /// <summary>
    /// Gets the character used to escape a delimiter sequence.
    /// Defaults to <c>\</c> (backslash).
    /// </summary>
    /// <remarks>
    /// When the escape character precedes an open delimiter, the delimiter is treated
    /// as literal text and the escape character is removed from the output.
    /// For example: <c>\{{literal}}</c> produces <c>{{literal}}</c>.
    /// </remarks>
    public char EscapeChar { get; init; } = '\\';

    /// <summary>
    /// Gets whether to throw an <see cref="InvalidOperationException"/> when a variable
    /// referenced in the content is not defined in the context.
    /// When <c>false</c> (the default), undefined variables are replaced with an empty string.
    /// </summary>
    public bool ThrowOnUndefinedVariable { get; init; } = false;

    /// <summary>
    /// Gets whether variable name matching is case-sensitive.
    /// Defaults to <c>true</c>.
    /// </summary>
    public bool CaseSensitive { get; init; } = true;
}
