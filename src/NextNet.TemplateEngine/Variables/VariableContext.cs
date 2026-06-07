using System.Collections.Immutable;
using NextNet.Templates.Abstractions;

namespace NextNet.TemplateEngine.Variables;

/// <summary>
/// An immutable context that holds variable values, supporting dot-notation lookup
/// for nested values. This is the full-featured replacement for the preliminary
/// <c>VariableContext</c> in NextNet.Templates.Abstractions.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="VariableContext"/> stores values in a pre-flattened dictionary whose
/// keys may contain dots to represent nested object paths. The flattening is performed
/// at build time by <see cref="VariableContextBuilder.SetNested"/> so that lookups
/// are O(1) without runtime reflection.
/// </para>
/// <para>
/// Instances are created exclusively through <see cref="VariableContextBuilder"/>;
/// there is no public constructor. Once built, the context is immutable.
/// </para>
/// <example>
/// <code>
/// var ctx = VariableContext.CreateBuilder()
///     .Set("name", "MyApp")
///     .SetNested("project", new { version = "1.0", framework = "net8.0" })
///     .Build();
///
/// string name = ctx.Get&lt;string&gt;("name")!;           // "MyApp"
/// string ver = ctx.Get&lt;string&gt;("project.version")!;  // "1.0"
/// </code>
/// </example>
/// </remarks>
public sealed class VariableContext : IVariableContext
{
    private readonly IReadOnlyDictionary<string, object?> _values;

    /// <summary>
    /// Initializes a new instance of the <see cref="VariableContext"/> class.
    /// </summary>
    /// <param name="values">The flattened dictionary of variable names to values.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is null.</exception>
    internal VariableContext(IReadOnlyDictionary<string, object?> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        _values = values.ToImmutableDictionary(StringComparer.Ordinal);
    }

    /// <summary>
    /// Creates a new <see cref="VariableContextBuilder"/> for constructing a context.
    /// </summary>
    /// <returns>A new <see cref="VariableContextBuilder"/> instance.</returns>
    /// <example>
    /// <code>
    /// var builder = VariableContext.CreateBuilder();
    /// builder.Set("key", "value");
    /// var ctx = builder.Build();
    /// </code>
    /// </example>
    public static VariableContextBuilder CreateBuilder()
    {
        return new VariableContextBuilder();
    }

    /// <summary>
    /// Gets the value of a variable by name, supporting dot notation for nested paths.
    /// </summary>
    /// <param name="name">The variable name, which may contain dots (e.g., "project.name").</param>
    /// <returns>The variable value, or <c>null</c> if the variable is not present.</returns>
    /// <example>
    /// <code>
    /// var value = context.Get("project.version");
    /// </code>
    /// </example>
    public object? Get(string name)
    {
        return _values.TryGetValue(name, out var value) ? value : null;
    }

    /// <summary>
    /// Gets the value of a variable by name, cast to the specified type.
    /// </summary>
    /// <typeparam name="T">The target type.</typeparam>
    /// <param name="name">The variable name, which may contain dots.</param>
    /// <returns>The variable value cast to <typeparamref name="T"/>, or <c>default(T)</c>
    /// if the variable is not present or the cast fails.</returns>
    /// <example>
    /// <code>
    /// int port = context.Get&lt;int&gt;("port");
    /// </code>
    /// </example>
    public T? Get<T>(string name)
    {
        if (_values.TryGetValue(name, out var value) && value is T typed)
        {
            return typed;
        }

        return default;
    }

    /// <summary>
    /// Attempts to retrieve the value of a variable by name.
    /// </summary>
    /// <param name="name">The variable name, which may contain dots.</param>
    /// <param name="value">When this method returns, contains the value if found; otherwise, <c>null</c>.</param>
    /// <returns><c>true</c> if the variable exists; otherwise <c>false</c>.</returns>
    /// <example>
    /// <code>
    /// if (context.TryGet("port", out object? value))
    /// {
    ///     // use value
    /// }
    /// </code>
    /// </example>
    public bool TryGet(string name, out object? value)
    {
        return _values.TryGetValue(name, out value);
    }

    /// <summary>
    /// Attempts to retrieve the value of a variable by name, cast to the specified type.
    /// </summary>
    /// <typeparam name="T">The target type.</typeparam>
    /// <param name="name">The variable name, which may contain dots.</param>
    /// <param name="value">When this method returns, contains the typed value if successful.</param>
    /// <returns><c>true</c> if the variable exists and is of type <typeparamref name="T"/>; otherwise <c>false</c>.</returns>
    /// <example>
    /// <code>
    /// if (context.TryGet&lt;string&gt;("name", out string? name))
    /// {
    ///     Console.WriteLine(name);
    /// }
    /// </code>
    /// </example>
    public bool TryGet<T>(string name, out T? value)
    {
        if (_values.TryGetValue(name, out var raw) && raw is T typed)
        {
            value = typed;
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Determines whether the context contains a variable with the specified name.
    /// </summary>
    /// <param name="name">The variable name to check, which may contain dots.</param>
    /// <returns><c>true</c> if the variable exists; otherwise <c>false</c>.</returns>
    /// <example>
    /// <code>
    /// if (context.Contains("project.version"))
    /// {
    ///     // use it
    /// }
    /// </code>
    /// </example>
    public bool Contains(string name)
    {
        return _values.ContainsKey(name);
    }

    /// <summary>
    /// Gets the collection of all variable names (keys) stored in this context.
    /// </summary>
    /// <remarks>
    /// Keys may contain dots representing nested object paths.
    /// </remarks>
    public IEnumerable<string> Keys => _values.Keys;

    /// <summary>
    /// Gets a read-only snapshot of all variable name-value pairs in this context.
    /// </summary>
    /// <remarks>
    /// The returned dictionary is a snapshot and will not reflect any subsequent
    /// modifications to the context (the context is immutable).
    /// </remarks>
    public IReadOnlyDictionary<string, object?> Values =>
        _values.ToImmutableDictionary(StringComparer.Ordinal);
}
