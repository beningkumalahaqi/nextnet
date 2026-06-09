using System.Text;

namespace NextNet.TemplateEngine.Variables;

/// <summary>
/// Processes inline block conditionals (<c>{{#if}}...{{/if}}</c> and <c>{{#unless}}...{{/if}}</c>)
/// within template content, evaluating conditions against a <see cref="VariableContext"/>
/// and retaining only the matching branch.
/// </summary>
/// <remarks>
/// <para>
/// This processor handles conditionals before variable substitution so that blocks
/// conditioned on transitive feature dependencies or user-selected features can
/// include or exclude content. It runs as a pre-processor step: block tags are
/// stripped and only the content from the satisfied branch is kept.
/// </para>
/// <para>
/// Supported syntax:
/// <list type="bullet">
///   <item><c>{{#if featureName}}...{{/if}}</c> — include content if <c>featureName</c> is truthy.</item>
///   <item><c>{{#if featureName}}...{{else}}...{{/if}}</c> — include first block if truthy, second if falsy.</item>
///   <item><c>{{#unless featureName}}...{{/if}}</c> — include content if <c>featureName</c> is falsy.</item>
///   <item><c>{{#unless featureName}}...{{else}}...{{/if}}</c> — negated variant with alternate branch.</item>
/// </list>
/// </para>
/// <para>
/// Nested blocks are supported. Conditions are evaluated by checking the variable name
/// in the context: a variable is truthy if it exists and its value is not <c>false</c>,
/// <c>null</c>, <c>0</c>, or an empty string.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var ctx = VariableContext.CreateBuilder()
///     .Set("useAuth", true)
///     .Build();
/// var processor = new BlockProcessor();
///
/// var result = processor.Process(
///     "{{#if useAuth}}Auth enabled{{/if}}",
///     ctx);
/// // result: "Auth enabled"
/// </code>
/// </example>
public sealed class BlockProcessor
{
    // Delimiter constants matching default VariableReplacer delimiters
    private const string OpenDelim = "{{";
    private const string CloseDelim = "}}";

    /// <summary>
    /// Processes all block conditionals in the content, evaluating conditions against
    /// the provided variable context and returning content with only the matching branches.
    /// </summary>
    /// <param name="content">The template content containing potential block conditionals.</param>
    /// <param name="context">The variable context used to evaluate conditions.</param>
    /// <returns>The content with block conditionals resolved and non-matching branches removed.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="content"/> or <paramref name="context"/> is <c>null</c>.</exception>
    public string Process(string content, VariableContext context)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(context);

        return ProcessInternal(content, context);
    }

    /// <summary>
    /// Internal recursive processing that handles nested blocks.
    /// </summary>
    private static string ProcessInternal(string content, VariableContext context)
    {
        var result = new StringBuilder(content.Length);
        var i = 0;

        while (i < content.Length)
        {
            // Look for opening block tag: {{#if ...}} or {{#unless ...}}
            var ifTagStart = FindTag(content, i, "#if");
            var unlessTagStart = FindTag(content, i, "#unless");

            int tagStart;
            bool isUnless;

            if (ifTagStart >= 0 && (unlessTagStart < 0 || ifTagStart <= unlessTagStart))
            {
                tagStart = ifTagStart;
                isUnless = false;
            }
            else if (unlessTagStart >= 0)
            {
                tagStart = unlessTagStart;
                isUnless = true;
            }
            else
            {
                // No more block tags — append remainder and exit
                result.Append(content, i, content.Length - i);
                break;
            }

            // Append content before the tag
            result.Append(content, i, tagStart - i);

            // Find the end of the opening tag: {{#if condition}}
            var tagClose = content.IndexOf(CloseDelim, tagStart + OpenDelim.Length, StringComparison.Ordinal);
            if (tagClose < 0)
            {
                // Malformed — treat as literal
                result.Append(content, tagStart, content.Length - tagStart);
                break;
            }

            // Extract the condition expression (the variable name after "#if " or "#unless ")
            var conditionStart = tagStart + OpenDelim.Length + (isUnless ? "#unless ".Length : "#if ".Length);
            var conditionExpr = content[conditionStart..tagClose].Trim();

            if (string.IsNullOrWhiteSpace(conditionExpr))
            {
                // Empty condition — treat as literal
                result.Append(content, tagStart, content.Length - tagStart);
                break;
            }

            // Find the matching {{/if}} closing tag, handling nesting
            var blockEnd = FindMatchingEndTag(content, tagClose + CloseDelim.Length);
            if (blockEnd < 0)
            {
                // No matching {{/if}} — treat remaining as literal
                result.Append(content, tagStart, content.Length - tagStart);
                break;
            }

            // Extract the full block content (between opening tag and {{/if}})
            var blockContentStart = tagClose + CloseDelim.Length;
            var blockLength = blockEnd - blockContentStart;
            var blockContent = content.Substring(blockContentStart, blockLength);

            // Find optional {{else}} at the current nesting depth
            var elsePos = FindElseAtDepth(blockContent);

            string trueBranch;
            string falseBranch;

            if (elsePos >= 0)
            {
                trueBranch = blockContent[..elsePos];
                falseBranch = blockContent[(elsePos + OpenDelim.Length + "else".Length + CloseDelim.Length)..];
            }
            else
            {
                trueBranch = blockContent;
                falseBranch = string.Empty;
            }

            // Evaluate the condition
            var conditionMet = EvaluateCondition(conditionExpr, context, isUnless);

            // Recursively process the selected branch (for nested blocks)
            var selectedBranch = conditionMet
                ? ProcessInternal(trueBranch, context)
                : ProcessInternal(falseBranch, context);

            result.Append(selectedBranch);

            // Move past {{/if}}
            i = blockEnd + OpenDelim.Length + "/if".Length + CloseDelim.Length;
        }

        return result.ToString();
    }

    /// <summary>
    /// Finds the next occurrence of a tag pattern (<c>{{#tagName</c>) starting from the given position.
    /// </summary>
    /// <param name="content">The content to search in.</param>
    /// <param name="startIndex">The index to start searching from.</param>
    /// <param name="tagName">The tag name to find (e.g., "#if", "#unless").</param>
    /// <returns>The index of the opening delimiter, or -1 if not found.</returns>
    private static int FindTag(string content, int startIndex, string tagName)
    {
        while (startIndex < content.Length)
        {
            var openIdx = content.IndexOf(OpenDelim, startIndex, StringComparison.Ordinal);
            if (openIdx < 0) return -1;

            // Check that it's followed by the tag name
            var afterOpen = openIdx + OpenDelim.Length;
            if (afterOpen + tagName.Length <= content.Length &&
                string.CompareOrdinal(content, afterOpen, tagName, 0, tagName.Length) == 0)
            {
                // Verify it's followed by a space or end delimiter (to avoid matching partial names)
                var afterTag = afterOpen + tagName.Length;
                if (afterTag < content.Length && (char.IsWhiteSpace(content[afterTag]) ||
                    string.CompareOrdinal(content, afterTag, CloseDelim, 0, CloseDelim.Length) == 0))
                {
                    return openIdx;
                }
            }

            startIndex = openIdx + OpenDelim.Length;
        }

        return -1;
    }

    /// <summary>
    /// Finds the matching <c>{{/if}}</c> tag starting from the given position, handling nested
    /// <c>{{#if}} / {{#unless}}</c> blocks.
    /// </summary>
    /// <param name="content">The content to search in.</param>
    /// <param name="startIndex">The index to start searching from (after the opening tag's close delimiter).</param>
    /// <returns>The index of the opening delimiter of the matching <c>{{/if}}</c>, or -1 if not found.</returns>
    private static int FindMatchingEndTag(string content, int startIndex)
    {
        var depth = 1;
        var i = startIndex;

        while (i < content.Length)
        {
            // Check for nested opening tags
            var nextIf = FindTag(content, i, "#if");
            var nextUnless = FindTag(content, i, "#unless");

            // Check for closing tag {{/if}}
            var closeIdx = FindTag(content, i, "/if");

            if (closeIdx < 0)
            {
                return -1; // No more closing tags
            }

            // Determine which comes first
            var nextOpen = -1;
            if (nextIf >= 0 && nextUnless >= 0)
                nextOpen = Math.Min(nextIf, nextUnless);
            else if (nextIf >= 0)
                nextOpen = nextIf;
            else if (nextUnless >= 0)
                nextOpen = nextUnless;

            if (nextOpen >= 0 && nextOpen < closeIdx)
            {
                // Nested opening tag comes first — increase depth
                depth++;
                i = nextOpen + OpenDelim.Length;
            }
            else
            {
                // Closing tag comes first (or no nested open)
                depth--;
                if (depth == 0)
                {
                    return closeIdx;
                }
                i = closeIdx + OpenDelim.Length;
            }
        }

        return -1;
    }

    /// <summary>
    /// Finds the first <c>{{else}}</c> tag at the current nesting depth within the given block content.
    /// </summary>
    /// <param name="blockContent">The block content to search (between the opening tag and <c>{{/if}}</c>).</param>
    /// <returns>The index of the <c>{{else}}</c> tag at depth 0, or -1 if not found.</returns>
    private static int FindElseAtDepth(string blockContent)
    {
        var depth = 0;
        var i = 0;

        while (i < blockContent.Length)
        {
            // Check for nested opening tags
            var nextIf = FindTag(blockContent, i, "#if");
            var nextUnless = FindTag(blockContent, i, "#unless");

            // Check for else tag
            var elseIdx = FindTag(blockContent, i, "else");

            // Check for closing tag {{/if}}
            var closeIdx = FindTag(blockContent, i, "/if");

            var nextOpen = -1;
            if (nextIf >= 0 && nextUnless >= 0)
                nextOpen = Math.Min(nextIf, nextUnless);
            else if (nextIf >= 0)
                nextOpen = nextIf;
            else if (nextUnless >= 0)
                nextOpen = nextUnless;

            // Determine the next significant tag
            var candidates = new List<(int index, string type)>();
            if (nextOpen >= 0) candidates.Add((nextOpen, "open"));
            if (elseIdx >= 0) candidates.Add((elseIdx, "else"));
            if (closeIdx >= 0) candidates.Add((closeIdx, "close"));

            if (candidates.Count == 0) break;

            var next = candidates.OrderBy(c => c.index).First();

            if (next.type == "open")
            {
                if (depth == 0)
                {
                    // Skip past the opening tag to its matching /if
                    var openTagClose = blockContent.IndexOf(CloseDelim, next.index + OpenDelim.Length, StringComparison.Ordinal);
                    if (openTagClose < 0) break;
                    var endIf = FindMatchingEndTag(blockContent, openTagClose + CloseDelim.Length);
                    if (endIf < 0) break;
                    i = endIf + OpenDelim.Length + "/if".Length + CloseDelim.Length;
                }
                else
                {
                    depth++;
                    i = next.index + OpenDelim.Length;
                }
            }
            else if (next.type == "close")
            {
                if (depth == 0)
                {
                    break; // Shouldn't happen; our block ends at the outer /if
                }
                depth--;
                i = next.index + OpenDelim.Length;
            }
            else if (next.type == "else")
            {
                if (depth == 0)
                {
                    return elseIdx; // Found {{else}} at depth 0
                }
                i = elseIdx + OpenDelim.Length;
            }
        }

        return -1;
    }

    /// <summary>
    /// Evaluates a condition expression against the variable context.
    /// </summary>
    /// <param name="condition">The variable name to evaluate (e.g., "useAuth").</param>
    /// <param name="context">The variable context.</param>
    /// <param name="isUnless">If <c>true</c>, the condition is negated (unless semantics).</param>
    /// <returns><c>true</c> if the condition is satisfied (or falsy for unless).</returns>
    private static bool EvaluateCondition(string condition, VariableContext context, bool isUnless)
    {
        var rawValue = context.Get(condition);
        var isTruthy = IsTruthy(rawValue);
        return isUnless ? !isTruthy : isTruthy;
    }

    /// <summary>
    /// Determines whether a value is "truthy" for template condition evaluation.
    /// </summary>
    /// <remarks>
    /// A value is truthy if it exists and is not <c>false</c>, <c>null</c>, <c>0</c>,
    /// or an empty string/whitespace.
    /// </remarks>
    /// <param name="value">The value to evaluate.</param>
    /// <returns><c>true</c> if the value is truthy; otherwise <c>false</c>.</returns>
    private static bool IsTruthy(object? value)
    {
        if (value is null) return false;
        if (value is bool b) return b;
        if (value is int i) return i != 0;
        if (value is long l) return l != 0;
        if (value is string s) return !string.IsNullOrWhiteSpace(s);
        if (value is short sh) return sh != 0;
        if (value is byte bt) return bt != 0;
        if (value is double d) return d != 0;
        if (value is float f) return f != 0;
        if (value is decimal dec) return dec != 0;
        return true; // Any other non-null object is truthy
    }
}
