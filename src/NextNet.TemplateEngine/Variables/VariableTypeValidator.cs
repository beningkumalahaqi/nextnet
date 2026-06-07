using System.Globalization;
using NextNet.Templates.Abstractions;
using NextNet.Templates.Models;

namespace NextNet.TemplateEngine.Variables;

/// <summary>
/// Validates variable values against their type definitions in the template manifest.
/// Supports type checking, coercion, and enumeration of validation errors.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="VariableTypeValidator"/> performs type-level validation of variable values
/// provided in a <see cref="VariableContext"/> against the type definitions declared in
/// a template's <see cref="TemplateManifest"/>.
/// </para>
/// <para>
/// Supported types: <c>string</c>, <c>bool</c>, <c>number</c>, and <c>enum</c>.
/// For <c>enum</c> types, the <see cref="TemplateVariable.AllowedValues"/> list defines
/// the acceptable values.
/// </para>
/// <para>
/// Use <see cref="ValidateAll"/> to validate a complete set of variables, and
/// <see cref="TryCoerce"/> to attempt type conversion for individual values.
/// </para>
/// <example>
/// <code>
/// var validator = new VariableTypeValidator();
/// var defs = new[]
/// {
///     new TemplateVariable("port", "number", Required: true),
///     new TemplateVariable("name", "string")
/// };
///
/// var ctx = VariableContext.CreateBuilder()
///     .Set("port", 8080)
///     .Set("name", "MyApp")
///     .Build();
///
/// var result = validator.ValidateAll(defs, ctx);
/// if (!result.IsValid)
/// {
///     foreach (var error in result.Errors!)
///         Console.WriteLine(error);
/// }
/// </code>
/// </example>
/// </remarks>
public sealed class VariableTypeValidator
{
    /// <summary>
    /// Validates a single variable value against its type definition.
    /// </summary>
    /// <param name="definition">The variable type definition from the manifest.</param>
    /// <param name="value">The value to validate.</param>
    /// <returns>A list of validation error messages. An empty list indicates success.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="definition"/> is null.</exception>
    /// <example>
    /// <code>
    /// var def = new TemplateVariable("enabled", "bool");
    /// var errors = validator.Validate(def, "notabool");
    /// </code>
    /// </example>
    public IReadOnlyList<string> Validate(TemplateVariable definition, object? value)
    {
        ArgumentNullException.ThrowIfNull(definition);

        var errors = new List<string>();

        // Required check
        if (definition.Required && value is null)
        {
            errors.Add($"Variable '{definition.Name}' is required but value is null.");
            return errors;
        }

        // Null is allowed for optional variables
        if (value is null)
            return errors;

        switch (definition.Type?.ToLowerInvariant())
        {
            case "string":
                // Any value is valid; string conversion is implicit
                break;

            case "bool":
                if (!TryParseBool(value, out _))
                    errors.Add($"Variable '{definition.Name}' expects a bool, but got '{value}'.");
                break;

            case "number":
                if (!TryParseNumber(value, out _))
                    errors.Add($"Variable '{definition.Name}' expects a number, but got '{value}'.");
                break;

            case "enum":
                if (definition.AllowedValues is null || definition.AllowedValues.Count == 0)
                {
                    errors.Add($"Variable '{definition.Name}' has type 'enum' but no AllowedValues defined.");
                }
                else
                {
                    var stringValue = value.ToString();
                    if (!definition.AllowedValues.Contains(stringValue!, StringComparer.Ordinal))
                        errors.Add($"Variable '{definition.Name}' value '{value}' is not in allowed values: {string.Join(", ", definition.AllowedValues)}.");
                }
                break;

            default:
                errors.Add($"Variable '{definition.Name}' has unknown type '{definition.Type}'.");
                break;
        }

        return errors;
    }

    /// <summary>
    /// Validates all variables in a context against their definitions.
    /// </summary>
    /// <param name="definitions">The list of variable type definitions from the manifest.</param>
    /// <param name="context">The variable context containing the values to validate.</param>
    /// <returns>A <see cref="ValidationResult"/> describing all errors and warnings found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="definitions"/> or <paramref name="context"/> is null.</exception>
    /// <example>
    /// <code>
    /// var result = validator.ValidateAll(definitions, context);
    /// if (!result.IsValid)
    ///     // handle errors
    /// </code>
    /// </example>
    public ValidationResult ValidateAll(IReadOnlyList<TemplateVariable> definitions, VariableContext context)
    {
        ArgumentNullException.ThrowIfNull(definitions);
        ArgumentNullException.ThrowIfNull(context);

        var allErrors = new List<string>();
        var warnings = new List<string>();

        foreach (var def in definitions)
        {
            if (context.TryGet(def.Name, out var value))
            {
                var errors = Validate(def, value);
                allErrors.AddRange(errors);
            }
            else if (def.Required)
            {
                allErrors.Add($"Required variable '{def.Name}' is missing from context.");
            }
        }

        return new ValidationResult(
            IsValid: allErrors.Count == 0,
            Errors: allErrors.Count > 0 ? allErrors : null,
            Warnings: warnings.Count > 0 ? warnings : null
        );
    }

    /// <summary>
    /// Attempts to coerce a value to the expected type defined in the template variable.
    /// </summary>
    /// <param name="definition">The variable type definition from the manifest.</param>
    /// <param name="value">The value to coerce.</param>
    /// <param name="coerced">When this method returns, contains the coerced value if successful.</param>
    /// <returns><c>true</c> if coercion succeeded; otherwise <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="definition"/> is null.</exception>
    /// <example>
    /// <code>
    /// var def = new TemplateVariable("count", "number");
    /// if (validator.TryCoerce(def, "42", out var coerced))
    /// {
    ///     Console.WriteLine(coerced); // 42.0
    /// }
    /// </code>
    /// </example>
    public bool TryCoerce(TemplateVariable definition, object? value, out object? coerced)
    {
        ArgumentNullException.ThrowIfNull(definition);
        coerced = null;

        if (value is null)
            return !definition.Required;

        switch (definition.Type?.ToLowerInvariant())
        {
            case "string":
                coerced = value.ToString();
                return true;

            case "bool":
                if (TryParseBool(value, out var b))
                {
                    coerced = b;
                    return true;
                }
                return false;

            case "number":
                if (TryParseNumber(value, out var n))
                {
                    coerced = n;
                    return true;
                }
                return false;

            case "enum":
                if (definition.AllowedValues?.Contains(value.ToString()!, StringComparer.Ordinal) == true)
                {
                    coerced = value.ToString();
                    return true;
                }
                return false;

            default:
                return false;
        }
    }

    private static bool TryParseBool(object value, out bool result)
    {
        switch (value)
        {
            case bool b:
                result = b;
                return true;
            case string s when bool.TryParse(s, out result):
                return true;
            default:
                result = false;
                return false;
        }
    }

    private static bool TryParseNumber(object value, out double result)
    {
        switch (value)
        {
            case int i:
                result = i;
                return true;
            case long l:
                result = l;
                return true;
            case float f:
                result = f;
                return true;
            case double d:
                result = d;
                return true;
            case decimal dec:
                result = (double)dec;
                return true;
            case string s when double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out result):
                return true;
            default:
                result = 0;
                return false;
        }
    }
}
