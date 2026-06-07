namespace NextNet.TemplateEngine.Variables;

/// <summary>
/// Performs single-pass variable replacement on text content using a StringBuilder
/// for performance (no regex backtracking).
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="VariableReplacer"/> processes text content in a single forward pass,
/// replacing variable placeholders (delimited by configurable markers) with values from
/// a <see cref="VariableContext"/>. It supports escaped delimiters, custom delimiters,
/// case-sensitive or case-insensitive matching, and configurable behavior for undefined
/// variables.
/// </para>
/// <para>
/// Default delimiters are Mustache-style: <c>{{variable}}</c>. The escape character
/// defaults to backslash (<c>\</c>), so <c>\{{literal}}</c> produces <c>{{literal}}</c>.
/// </para>
/// <example>
/// <code>
/// var ctx = VariableContext.CreateBuilder()
///     .Set("name", "World")
///     .Build();
///
/// var replacer = new VariableReplacer();
/// var result = await replacer.ReplaceAsync("Hello {{name}}!", ctx);
/// // result: "Hello World!"
/// </code>
/// </example>
/// </remarks>
public sealed class VariableReplacer
{
    private readonly VariableReplacementOptions _options;
    private readonly StringComparer _comparer;

    /// <summary>
    /// Initializes a new instance of the <see cref="VariableReplacer"/> class.
    /// </summary>
    /// <param name="options">Optional replacement options. If not specified, defaults are used.</param>
    /// <example>
    /// <code>
    /// var replacer = new VariableReplacer(new VariableReplacementOptions
    /// {
    ///     ThrowOnUndefinedVariable = true,
    ///     CaseSensitive = false
    /// });
    /// </code>
    /// </example>
    public VariableReplacer(VariableReplacementOptions? options = null)
    {
        _options = options ?? new VariableReplacementOptions();
        _comparer = _options.CaseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;
    }

    /// <summary>
    /// Replaces all variable placeholders in the content with values from the context.
    /// </summary>
    /// <param name="content">The text content containing variable placeholders.</param>
    /// <param name="context">The variable context providing values for replacement.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The content with all variable placeholders replaced.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="content"/> or <paramref name="context"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when <c>ThrowOnUndefinedVariable</c> is set and a variable is not defined.</exception>
    /// <example>
    /// <code>
    /// var result = await replacer.ReplaceAsync("Hello {{name}}", ctx);
    /// </code>
    /// </example>
    public Task<string> ReplaceAsync(string content, VariableContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(context);

        var result = new StringBuilder(content.Length);
        var i = 0;
        var openDelim = _options.OpenDelimiter;
        var closeDelim = _options.CloseDelimiter;
        var escapeChar = _options.EscapeChar;

        while (i < content.Length)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Check for escape sequence: escapeChar followed by open delimiter
            if (i + openDelim.Length <= content.Length &&
                content[i] == escapeChar &&
                i + 1 < content.Length &&
                string.CompareOrdinal(content, i + 1, openDelim, 0, openDelim.Length) == 0)
            {
                // Escape: output the delimiter as literal
                result.Append(openDelim);
                i += 1 + openDelim.Length;

                // Find and copy until close delimiter
                var closeIdx = content.IndexOf(closeDelim, i, StringComparison.Ordinal);
                if (closeIdx >= 0)
                {
                    result.Append(content, i, closeIdx - i);
                    result.Append(closeDelim);
                    i = closeIdx + closeDelim.Length;
                }
                else
                {
                    // No close delimiter found; append rest as-is
                    result.Append(content, i, content.Length - i);
                    i = content.Length;
                }
                continue;
            }

            // Check for variable start
            if (i + openDelim.Length <= content.Length &&
                string.CompareOrdinal(content, i, openDelim, 0, openDelim.Length) == 0)
            {
                var closeIdx = content.IndexOf(closeDelim, i + openDelim.Length, StringComparison.Ordinal);
                if (closeIdx >= 0)
                {
                    var keyStart = i + openDelim.Length;
                    var keyLength = closeIdx - keyStart;
                    var key = content.Substring(keyStart, keyLength);

                    // Look up value
                    if (TryGetValueCaseInsensitive(context, key, out var value))
                    {
                        result.Append(value);
                    }
                    else if (_options.ThrowOnUndefinedVariable)
                    {
                        throw new InvalidOperationException($"Variable '{key}' is not defined.");
                    }
                    // else: replace with empty string (default behavior)

                    i = closeIdx + closeDelim.Length;
                    continue;
                }
            }

            // Append current character
            result.Append(content[i]);
            i++;
        }

        return Task.FromResult(result.ToString());
    }

    private bool TryGetValueCaseInsensitive(VariableContext context, string key, out string? value)
    {
        if (_options.CaseSensitive)
        {
            value = context.Get(key)?.ToString();
            return context.Contains(key);
        }
        else
        {
            foreach (var k in context.Keys)
            {
                if (string.Equals(k, key, StringComparison.OrdinalIgnoreCase))
                {
                    value = context.Get(k)?.ToString();
                    return true;
                }
            }
            value = null;
            return false;
        }
    }
}
