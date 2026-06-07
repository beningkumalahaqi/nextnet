using NextNet.Templates.Abstractions;
using NextNet.Templates.Models;

namespace NextNet.TemplateEngine.Conditionals;

/// <summary>
/// Filters a list of <see cref="TemplateFile"/> entries based on their conditional
/// expressions, evaluating each condition against a variable context.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="ConditionalFileFilter"/> uses <see cref="ConditionParser"/> and
/// <see cref="ConditionEvaluator"/> to evaluate each file's condition expression.
/// Files without a condition are always included. Files whose condition evaluates
/// to <c>true</c> are included; those evaluating to <c>false</c> are returned as
/// excluded with the condition text.
/// </para>
/// <para>
/// Files whose condition contains a parse error are included by default (fail-open
/// behavior), ensuring that malformed conditions do not silently omit files from
/// the generated output.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var filter = new ConditionalFileFilter();
/// var files = new[] {
///     new TemplateFile("src/Api.cs", "Api.cs", "features.api"),
///     new TemplateFile("src/NoAuth.cs", "NoAuth.cs", "!features.auth")
/// };
/// var context = VariableContext.CreateBuilder()
///     .SetNested("features", new { api = true, auth = false })
///     .Build();
/// var result = filter.Filter(files, context);
/// // result.Included has Api.cs only
/// // result.Excluded has NoAuth.cs with condition "!features.auth"
/// </code>
/// </example>
public sealed class ConditionalFileFilter
{
    private readonly ConditionParser _parser;
    private readonly ConditionEvaluator _evaluator;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConditionalFileFilter"/> class.
    /// </summary>
    /// <param name="parser">Optional parser instance. A new one is created if not provided.</param>
    /// <param name="evaluator">Optional evaluator instance. A new one is created if not provided.</param>
    public ConditionalFileFilter(ConditionParser? parser = null, ConditionEvaluator? evaluator = null)
    {
        _parser = parser ?? new ConditionParser();
        _evaluator = evaluator ?? new ConditionEvaluator();
    }

    /// <summary>
    /// Filters the provided files based on their condition expressions.
    /// </summary>
    /// <param name="files">The list of template files to filter. Must not be null.</param>
    /// <param name="context">The variable context for evaluating conditions. Must not be null.</param>
    /// <returns>A <see cref="FileFilterResult"/> containing included and excluded files.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="files"/> or <paramref name="context"/> is null.</exception>
    public FileFilterResult Filter(IReadOnlyList<TemplateFile> files, IVariableContext context)
    {
        ArgumentNullException.ThrowIfNull(files);
        ArgumentNullException.ThrowIfNull(context);

        var included = new List<TemplateFile>();
        var excluded = new List<ExcludedFile>();

        foreach (var file in files)
        {
            if (string.IsNullOrWhiteSpace(file.Condition))
            {
                included.Add(file);
                continue;
            }

            try
            {
                var expr = _parser.Parse(file.Condition);
                if (_evaluator.Evaluate(expr, context))
                    included.Add(file);
                else
                    excluded.Add(new ExcludedFile(file, file.Condition));
            }
            catch (ParseException)
            {
                // Fail-open: include files with parse errors in their conditions
                included.Add(file);
            }
        }

        return new FileFilterResult(included, excluded);
    }
}

/// <summary>
/// The result of filtering template files by their conditional expressions.
/// </summary>
/// <remarks>
/// <para>
/// Contains two lists: files that passed the condition check (<see cref="Included"/>)
/// and files that were excluded along with their condition text (<see cref="Excluded"/>).
/// </para>
/// </remarks>
/// <param name="Included">The list of files that were included (no condition or condition evaluated to true).</param>
/// <param name="Excluded">The list of files that were excluded because their condition evaluated to false.</param>
public sealed record FileFilterResult(
    IReadOnlyList<TemplateFile> Included,
    IReadOnlyList<ExcludedFile> Excluded
);

/// <summary>
/// Represents a template file that was excluded due to a false condition.
/// </summary>
/// <remarks>
/// <para>
/// Records which file was excluded and the condition expression that evaluated to false.
/// This allows consumers to report or log the exclusion decision.
/// </para>
/// </remarks>
/// <param name="File">The template file that was excluded.</param>
/// <param name="Condition">The condition expression that evaluated to false.</param>
public sealed record ExcludedFile(
    TemplateFile File,
    string Condition
);
