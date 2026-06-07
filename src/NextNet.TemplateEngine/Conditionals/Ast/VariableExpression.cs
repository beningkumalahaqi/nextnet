namespace NextNet.TemplateEngine.Conditionals.Ast;

/// <summary>
/// Represents a reference to a named variable in a conditional expression.
/// </summary>
/// <remarks>
/// <para>
/// Variable names support dot notation for nested object access
/// (e.g., <c>project.version</c>). The variable value is resolved at
/// evaluation time through the <see cref="Templates.Abstractions.IVariableContext" />.
/// </para>
/// <para>
/// Undefined variables evaluate as falsy (return <c>false</c>) rather
/// than throwing an exception, making it safe to test for optional features.
/// </para>
/// </remarks>
/// <param name="Name">The variable name, which may contain dots for nested access.</param>
public sealed record VariableExpression(
    string Name
) : Expression;
