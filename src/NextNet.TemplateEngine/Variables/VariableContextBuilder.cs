using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace NextNet.TemplateEngine.Variables;

/// <summary>
/// Builds an immutable <see cref="VariableContext"/> with support for dot-notation
/// flattening of nested objects.
/// </summary>
/// <remarks>
/// <para>
/// Use <see cref="SetNested"/> to flatten an object's public properties into
/// dot-notation keys. For example:
/// <c>SetNested("project", new { name = "MyApp", version = "1.0" })</c>
/// produces keys <c>"project.name"</c> and <c>"project.version"</c>.
/// </para>
/// <para>
/// Name validation:
/// <list type="bullet">
///   <item>Names must not be null, empty, or whitespace.</item>
///   <item>Names must not contain <c>{</c> or <c>}</c> characters.</item>
/// </list>
/// </para>
/// <para>
/// The builder supports fluent chaining.
/// </para>
/// <example>
/// <code>
/// var ctx = VariableContext.CreateBuilder()
///     .Set("name", "MyApp")
///     .Set("port", 8080)
///     .SetNested("project", new { version = "1.0.0" })
///     .Remove("port")
///     .Build();
/// </code>
/// </example>
/// </remarks>
public sealed class VariableContextBuilder
{
    private readonly Dictionary<string, object?> _values = new(StringComparer.Ordinal);

    /// <summary>
    /// Initializes a new instance of the <see cref="VariableContextBuilder"/> class.
    /// </summary>
    internal VariableContextBuilder()
    {
    }

    /// <summary>
    /// Sets a variable value by name.
    /// </summary>
    /// <param name="name">The variable name. Must not be null, empty, or contain braces.</param>
    /// <param name="value">The variable value.</param>
    /// <returns>This builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="name"/> is null, empty, or contains <c>{</c> or <c>}</c>.</exception>
    /// <example>
    /// <code>
    /// builder.Set("projectName", "MyApp");
    /// </code>
    /// </example>
    public VariableContextBuilder Set(string name, object? value)
    {
        ValidateName(name);
        _values[name] = value;
        return this;
    }

    /// <summary>
    /// Sets a strongly-typed variable value by name.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="name">The variable name. Must not be null, empty, or contain braces.</param>
    /// <param name="value">The variable value.</param>
    /// <returns>This builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="name"/> is null, empty, or contains <c>{</c> or <c>}</c>.</exception>
    /// <example>
    /// <code>
    /// builder.Set&lt;int&gt;("port", 8080);
    /// </code>
    /// </example>
    public VariableContextBuilder Set<T>(string name, T value)
    {
        ValidateName(name);
        _values[name] = value;
        return this;
    }

    /// <summary>
    /// Sets nested object properties as dot-notation keys.
    /// </summary>
    /// <param name="name">The prefix for the flattened keys (e.g., "project").</param>
    /// <param name="value">The object whose public properties will be flattened.</param>
    /// <returns>This builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="name"/> is null, empty, or contains braces.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <example>
    /// <code>
    /// builder.SetNested("project", new { name = "MyApp", version = "1.0.0" });
    /// // Creates keys: "project.name" = "MyApp", "project.version" = "1.0.0"
    /// </code>
    /// </example>
    public VariableContextBuilder SetNested(string name, object value)
    {
        ValidateName(name);
        ArgumentNullException.ThrowIfNull(value);

        FlattenObject(name, value, _values);
        return this;
    }

    /// <summary>
    /// Sets multiple variable values from a dictionary.
    /// </summary>
    /// <param name="values">The dictionary of name-value pairs to add.</param>
    /// <returns>This builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="values"/> is null.</exception>
    /// <example>
    /// <code>
    /// builder.SetMany(new Dictionary&lt;string, object?&gt;
    /// {
    ///     ["key1"] = "value1",
    ///     ["key2"] = 42
    /// });
    /// </code>
    /// </example>
    public VariableContextBuilder SetMany(IReadOnlyDictionary<string, object?> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        foreach (var kvp in values)
        {
            ValidateName(kvp.Key);
            _values[kvp.Key] = kvp.Value;
        }

        return this;
    }

    /// <summary>
    /// Removes a variable from the builder.
    /// </summary>
    /// <param name="name">The name of the variable to remove.</param>
    /// <returns>This builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="name"/> is null, empty, or contains braces.</exception>
    /// <example>
    /// <code>
    /// builder.Remove("temporaryKey");
    /// </code>
    /// </example>
    public VariableContextBuilder Remove(string name)
    {
        ValidateName(name);
        _values.Remove(name);
        return this;
    }

    /// <summary>
    /// Builds an immutable <see cref="VariableContext"/> from the accumulated values.
    /// </summary>
    /// <returns>An immutable <see cref="VariableContext"/> instance.</returns>
    /// <example>
    /// <code>
    /// var ctx = builder.Build();
    /// </code>
    /// </example>
    public VariableContext Build()
    {
        return new VariableContext(_values);
    }

    /// <summary>
    /// Validates that a variable name is not null, empty, and does not contain braces.
    /// </summary>
    /// <param name="name">The name to validate.</param>
    /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
    private static void ValidateName([NotNull] string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Variable name must not be null or empty.", nameof(name));
        }

        if (name.Contains('{') || name.Contains('}'))
        {
            throw new ArgumentException("Variable name must not contain '{' or '}' characters.", nameof(name));
        }
    }

    /// <summary>
    /// Recursively flattens an object's public readable properties into dot-notation keys.
    /// </summary>
    /// <param name="prefix">The current key prefix.</param>
    /// <param name="value">The object to flatten.</param>
    /// <param name="target">The target dictionary to populate.</param>
    private static void FlattenObject(string prefix, object value, Dictionary<string, object?> target)
    {
        var type = value.GetType();

        // Value types, strings, and primitives are leaf values
        if (type.IsPrimitive || type.IsValueType || value is string or decimal or DateTime or DateTimeOffset or Guid or Uri)
        {
            target[prefix] = value;
            return;
        }

        // Enumerations are stored as-is (could be arrays, lists, etc.)
        if (value is System.Collections.IEnumerable && value is not string)
        {
            target[prefix] = value;
            return;
        }

        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var prop in properties)
        {
            if (prop.CanRead && prop.GetIndexParameters().Length == 0)
            {
                var propValue = prop.GetValue(value);
                var propName = $"{prefix}.{prop.Name}";

                if (propValue is null)
                {
                    target[propName] = null;
                }
                else
                {
                    var propType = propValue.GetType();
                    if (propType.IsPrimitive || propType.IsValueType || propValue is string or decimal or DateTime or DateTimeOffset or Guid or Uri)
                    {
                        target[propName] = propValue;
                    }
                    else if (propValue is System.Collections.IEnumerable and not string)
                    {
                        target[propName] = propValue;
                    }
                    else
                    {
                        // Recursively flatten nested objects
                        FlattenObject(propName, propValue, target);
                    }
                }
            }
        }
    }
}
