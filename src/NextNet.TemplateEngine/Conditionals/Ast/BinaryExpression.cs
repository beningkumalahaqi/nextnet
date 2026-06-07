namespace NextNet.TemplateEngine.Conditionals.Ast;

/// <summary>
/// Represents a binary operation: <c>left operator right</c>.
/// </summary>
/// <remarks>
/// <para>
/// Supported operators include:
/// <list type="bullet">
///   <item><c>==</c> — equality</item>
///   <item><c>!=</c> — inequality</item>
///   <item><c>&amp;&amp;</c> — logical AND (short-circuit)</item>
///   <item><c>||</c> — logical OR (short-circuit)</item>
///   <item><c>&gt;</c>, <c>&gt;=</c>, <c>&lt;</c>, <c>&lt;=</c> — numeric comparisons</item>
///   <item><c>in</c> — membership test</item>
/// </list>
/// </para>
/// <para>
/// Operator precedence (highest to lowest):
/// <list type="number">
///   <item>Unary !</item>
///   <item>Comparison (&gt;, &gt;=, &lt;, &lt;=)</item>
///   <item>Equality (==, !=)</item>
///   <item>Logical AND (&amp;&amp;)</item>
///   <item>Logical OR (||)</item>
/// </list>
/// </para>
/// </remarks>
/// <param name="Left">The left-hand operand expression.</param>
/// <param name="Operator">The operator string (e.g., "==", "&amp;&amp;", "in").</param>
/// <param name="Right">The right-hand operand expression.</param>
public sealed record BinaryExpression(
    Expression Left,
    string Operator,
    Expression Right
) : Expression;
