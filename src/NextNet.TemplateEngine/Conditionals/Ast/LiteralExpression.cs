namespace NextNet.TemplateEngine.Conditionals.Ast;

/// <summary>
/// Represents a literal value in a conditional expression.
/// </summary>
/// <remarks>
/// <para>
/// Literal values include:
/// <list type="bullet">
///   <item>String literals — <c>"hello"</c> or <c>'hello'</c></item>
///   <item>Numeric literals — <c>42</c>, <c>3.14</c></item>
///   <item>Boolean literals — <c>true</c>, <c>false</c></item>
///   <item>Null literal — <c>null</c></item>
/// </list>
/// </para>
/// <para>
/// The <see cref="Value"/> property stores the parsed .NET value:
/// strings as <see cref="string"/>, numbers as <see cref="int"/> or <see cref="double"/>,
/// booleans as <see cref="bool"/>, and null as <c>null</c>.
/// </para>
/// </remarks>
/// <param name="Value">The parsed literal value (<see cref="string"/>, <see cref="int"/>,
/// <see cref="double"/>, <see cref="bool"/>, or <c>null</c>).</param>
public sealed record LiteralExpression(
    object? Value
) : Expression;
