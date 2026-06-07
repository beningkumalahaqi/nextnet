namespace NextNet.Templates.Abstractions;

/// <summary>
/// Defines the read-only contract for a variable context used during template generation.
/// Implementations provide variable value lookup, existence checks, and key enumeration.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="IVariableContext"/> interface represents the minimal surface area
/// needed by the template engine to resolve variable values during rendering. It is
/// implemented by the full-featured <c>VariableContext</c> in the TemplateEngine layer.
/// </para>
/// <para>
/// Keys may contain dots to represent nested object paths (e.g., <c>"project.name"</c>),
/// depending on the implementation.
/// </para>
/// </remarks>
public interface IVariableContext
{
    /// <summary>
    /// Gets the value of a variable by name, supporting dot notation for nested paths.
    /// </summary>
    /// <param name="name">The variable name, which may contain dots (e.g., "project.name").</param>
    /// <returns>The variable value, or <c>null</c> if the variable is not present.</returns>
    object? Get(string name);

    /// <summary>
    /// Determines whether the context contains a variable with the specified name.
    /// </summary>
    /// <param name="name">The variable name to check, which may contain dots.</param>
    /// <returns><c>true</c> if the variable exists; otherwise <c>false</c>.</returns>
    bool Contains(string name);

    /// <summary>
    /// Gets the collection of all variable names (keys) stored in this context.
    /// </summary>
    /// <remarks>
    /// Keys may contain dots representing nested object paths.
    /// </remarks>
    IEnumerable<string> Keys { get; }
}
