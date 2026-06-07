namespace NextNet.TemplateEngine.Conditionals.Ast;

/// <summary>
/// Base type for all expression AST nodes in the conditional expression tree.
/// </summary>
/// <remarks>
/// <para>
/// All concrete expression types derive from this abstract record. The AST is
/// produced by <see cref="ConditionParser"/> and consumed by <see cref="ConditionEvaluator"/>.
/// </para>
/// <para>
/// Each node type represents a distinct syntactic construct:
/// <list type="bullet">
///   <item><see cref="BinaryExpression"/> — binary operations (==, !=, &amp;&amp;, ||, in, etc.)</item>
///   <item><see cref="UnaryExpression"/> — unary ! operator</item>
///   <item><see cref="LiteralExpression"/> — literal values (strings, numbers, bools, null)</item>
///   <item><see cref="VariableExpression"/> — variable references with optional dot-notation</item>
///   <item><see cref="GroupExpression"/> — parenthesized sub-expressions</item>
/// </list>
/// </para>
/// </remarks>
public abstract record Expression;
