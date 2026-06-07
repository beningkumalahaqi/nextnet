namespace NextNet.TemplateEngine.Conditionals.Ast;

/// <summary>
/// Represents a parenthesized sub-expression, used to override operator precedence.
/// </summary>
/// <remarks>
/// <para>
/// In the expression <c>(a || b) &amp;&amp; c</c>, the <c>(a || b)</c> portion is
/// represented as a <see cref="GroupExpression"/> containing an OR binary expression.
/// </para>
/// <para>
/// During evaluation, the inner expression is evaluated recursively and its result
/// is returned directly — the grouping itself has no semantic meaning beyond precedence.
/// </para>
/// </remarks>
/// <param name="Inner">The wrapped expression inside the parentheses.</param>
public sealed record GroupExpression(
    Expression Inner
) : Expression;
