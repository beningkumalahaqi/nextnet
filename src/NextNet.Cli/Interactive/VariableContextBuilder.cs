using NextNet.TemplateEngine.Variables;

namespace NextNet.Cli.Interactive;

/// <summary>
/// A fluent builder that wraps <see cref="VariableContextBuilder"/> to provide
/// a simplified API for constructing an interactive project's variable context.
/// </summary>
/// <remarks>
/// <para>
/// This builder is specifically designed for the interactive project generator flow.
/// It exposes typed <c>SetString</c> and <c>SetBool</c> methods for clarity,
/// while delegating all storage to the underlying <see cref="VariableContextBuilder"/>.
/// </para>
/// <para>
/// Use <see cref="Build"/> to produce an immutable <see cref="VariableContext"/>
/// that can be passed to the template engine for code generation.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var builder = new InteractiveVariableContextBuilder();
/// builder.SetString("projectName", "MyApp");
/// builder.SetBool("includeAuth", true);
/// var ctx = builder.Build();
/// </code>
/// </example>
public sealed class InteractiveVariableContextBuilder
{
    private readonly VariableContextBuilder _builder = VariableContext.CreateBuilder();

    /// <summary>
    /// Sets a variable value by name.
    /// </summary>
    /// <param name="name">The variable name.</param>
    /// <param name="value">The variable value.</param>
    /// <returns>This builder instance for fluent chaining.</returns>
    public InteractiveVariableContextBuilder Set(string name, object? value)
    {
        _builder.Set(name, value);
        return this;
    }

    /// <summary>
    /// Sets a boolean variable value by name.
    /// </summary>
    /// <param name="name">The variable name.</param>
    /// <param name="value">The boolean value.</param>
    /// <returns>This builder instance for fluent chaining.</returns>
    public InteractiveVariableContextBuilder SetBool(string name, bool value)
    {
        _builder.Set(name, value);
        return this;
    }

    /// <summary>
    /// Sets a string variable value by name.
    /// </summary>
    /// <param name="name">The variable name.</param>
    /// <param name="value">The string value.</param>
    /// <returns>This builder instance for fluent chaining.</returns>
    public InteractiveVariableContextBuilder SetString(string name, string value)
    {
        _builder.Set(name, value);
        return this;
    }

    /// <summary>
    /// Builds an immutable <see cref="VariableContext"/> from the accumulated values.
    /// </summary>
    /// <returns>An immutable <see cref="VariableContext"/> instance.</returns>
    public VariableContext Build() => _builder.Build();
}
