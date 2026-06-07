namespace NextNet.TemplateEngine.Conditionals.Ast;

/// <summary>
/// Represents a unary operation applied to a single operand expression.
/// </summary>
/// <remarks>
/// <para>
/// Currently only the logical NOT operator (<c>!</c>) is supported.
/// The operator is placed before the operand in the expression string,
/// e.g., <c>!enabled</c> or <c>!(x == y)</c>.
/// </para>
/// </remarks>
/// <param name="Operator">The operator string (currently only <c>"!"</c>).</param>
/// <param name="Operand">The operand expression the operator is applied to.</param>
public sealed record UnaryExpression(
    string Operator,
    Expression Operand
) : Expression;
