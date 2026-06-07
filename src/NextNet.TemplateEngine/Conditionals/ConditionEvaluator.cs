using System.Globalization;
using NextNet.TemplateEngine.Conditionals.Ast;
using NextNet.Templates.Abstractions;

namespace NextNet.TemplateEngine.Conditionals;

/// <summary>
/// Evaluates conditional expression ASTs against a variable context to produce a boolean result.
/// </summary>
/// <remarks>
/// <para>
/// The evaluator walks the AST produced by <see cref="ConditionParser"/> and resolves
/// variable values through the <see cref="IVariableContext"/> interface.
/// </para>
/// <para>
/// Key evaluation behaviors:
/// <list type="bullet">
///   <item><c>&amp;&amp;</c> and <c>||</c> use short-circuit evaluation</item>
///   <item>Undefined variables are treated as falsy (return <c>false</c>)</item>
///   <item>Type-aware equality: <c>"1" == 1</c> is <c>false</c> (string vs int)</item>
///   <item>Numeric comparisons coerce strings to numbers when possible</item>
///   <item>The <c>in</c> operator checks membership in arrays, lists, or other collections</item>
/// </list>
/// </para>
/// <example>
/// <code>
/// var parser = new ConditionParser();
/// var evaluator = new ConditionEvaluator();
/// var expr = parser.Parse("features.auth == true");
/// var context = VariableContext.CreateBuilder()
///     .SetNested("features", new { auth = true })
///     .Build();
/// bool result = evaluator.Evaluate(expr, context); // true
/// </code>
/// </example>
/// </remarks>
public sealed class ConditionEvaluator
{
    /// <summary>
    /// Evaluates a parsed expression AST against the given variable context.
    /// </summary>
    /// <param name="expression">The expression AST to evaluate.</param>
    /// <param name="context">The variable context providing variable values.</param>
    /// <returns><c>true</c> if the expression evaluates to true; otherwise <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="expression"/> or <paramref name="context"/> is null.</exception>
    /// <exception cref="EvaluationException">Thrown when evaluation encounters an error (e.g., unknown operator).</exception>
    public bool Evaluate(Expression expression, IVariableContext context)
    {
        ArgumentNullException.ThrowIfNull(expression);
        ArgumentNullException.ThrowIfNull(context);
        return EvaluateInternal(expression, context);
    }

    /// <summary>
    /// Parses and evaluates a condition string against the given variable context.
    /// </summary>
    /// <param name="condition">The condition string to parse and evaluate.</param>
    /// <param name="context">The variable context providing variable values.</param>
    /// <returns><c>true</c> if the condition evaluates to true; otherwise <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="condition"/> or <paramref name="context"/> is null.</exception>
    /// <exception cref="ParseException">Thrown when the condition string contains a syntax error.</exception>
    /// <exception cref="EvaluationException">Thrown when evaluation encounters an error.</exception>
    public bool Evaluate(string condition, IVariableContext context)
    {
        ArgumentNullException.ThrowIfNull(condition);
        ArgumentNullException.ThrowIfNull(context);

        var parser = new ConditionParser();
        var expr = parser.Parse(condition);
        return Evaluate(expr, context);
    }

    /// <summary>
    /// Internal recursive evaluation of an expression node.
    /// </summary>
    private bool EvaluateInternal(Expression expr, IVariableContext context)
    {
        switch (expr)
        {
            case LiteralExpression lit:
                return IsTruthy(lit.Value);

            case VariableExpression varExpr:
                return IsTruthy(context.Get(varExpr.Name));

            case UnaryExpression unary when unary.Operator == "!":
                return !EvaluateInternal(unary.Operand, context);

            case BinaryExpression binary:
                return EvaluateBinary(binary, context);

            case GroupExpression group:
                return EvaluateInternal(group.Inner, context);

            default:
                throw new EvaluationException($"Unknown expression type: {expr.GetType().Name}");
        }
    }

    /// <summary>
    /// Evaluates a binary expression based on its operator.
    /// </summary>
    private bool EvaluateBinary(BinaryExpression binary, IVariableContext context)
    {
        switch (binary.Operator)
        {
            case "&&":
                // Short-circuit: if left is false, skip right
                if (!EvaluateInternal(binary.Left, context))
                    return false;
                return EvaluateInternal(binary.Right, context);

            case "||":
                // Short-circuit: if left is true, skip right
                if (EvaluateInternal(binary.Left, context))
                    return true;
                return EvaluateInternal(binary.Right, context);

            case "==":
                return AreEqual(EvaluateValue(binary.Left, context), EvaluateValue(binary.Right, context));

            case "!=":
                return !AreEqual(EvaluateValue(binary.Left, context), EvaluateValue(binary.Right, context));

            case ">":
                return CompareNumeric(EvaluateValue(binary.Left, context), EvaluateValue(binary.Right, context)) > 0;

            case ">=":
                return CompareNumeric(EvaluateValue(binary.Left, context), EvaluateValue(binary.Right, context)) >= 0;

            case "<":
                return CompareNumeric(EvaluateValue(binary.Left, context), EvaluateValue(binary.Right, context)) < 0;

            case "<=":
                return CompareNumeric(EvaluateValue(binary.Left, context), EvaluateValue(binary.Right, context)) <= 0;

            case "+":
                return IsTruthy(EvaluateConcat(EvaluateValue(binary.Left, context), EvaluateValue(binary.Right, context)));

            case "in":
                return CheckMembership(EvaluateValue(binary.Left, context), EvaluateValue(binary.Right, context));

            default:
                throw new EvaluationException($"Unknown operator: {binary.Operator}");
        }
    }

    /// <summary>
    /// Evaluates an expression to a .NET object value (for non-boolean contexts like equality/comparison).
    /// </summary>
    private object? EvaluateValue(Expression expr, IVariableContext context)
    {
        switch (expr)
        {
            case LiteralExpression lit:
                return lit.Value;

            case VariableExpression varExpr:
                return context.Get(varExpr.Name);

            case UnaryExpression unary when unary.Operator == "!":
                // ! applied to a value returns a boolean
                return !EvaluateInternal(unary.Operand, context);

            case BinaryExpression binary:
                if (binary.Operator is "+")
                {
                    return EvaluateConcat(EvaluateValue(binary.Left, context), EvaluateValue(binary.Right, context));
                }
                // For other binary operators used in value context, evaluate as bool
                return EvaluateBinary(binary, context);

            case GroupExpression group:
                return EvaluateValue(group.Inner, context);

            default:
                throw new EvaluationException($"Unknown expression type: {expr.GetType().Name}");
        }
    }

    /// <summary>
    /// Determines if a value is truthy (used for bare expressions and variable references).
    /// </summary>
    private static bool IsTruthy(object? value)
    {
        return value switch
        {
            null => false,
            bool b => b,
            string s => !string.IsNullOrEmpty(s),
            int i => i != 0,
            long l => l != 0,
            double d => d != 0.0,
            float f => f != 0.0f,
            decimal m => m != 0m,
            // Non-null objects are truthy
            _ => true
        };
    }

    /// <summary>
    /// Type-aware equality comparison.
    /// </summary>
    private static bool AreEqual(object? left, object? right)
    {
        // Both null
        if (left is null && right is null)
            return true;

        // One is null
        if (left is null || right is null)
            return false;

        // Same type — use default equality
        if (left.GetType() == right.GetType())
            return left.Equals(right);

        // Numeric type widening
        var (l, r) = CoerceNumeric(left, right);
        if (l.HasValue && r.HasValue)
            return l.Value == r.Value;

        // String comparison
        if (left is string ls && right is string rs)
            return string.Equals(ls, rs, StringComparison.Ordinal);

        return left.Equals(right);
    }

    /// <summary>
    /// Numeric comparison with coercion.
    /// </summary>
    private static int CompareNumeric(object? left, object? right)
    {
        var (l, r) = CoerceNumeric(left, right);

        if (l.HasValue && r.HasValue)
            return l.Value.CompareTo(r.Value);

        // Fall back to string comparison
        var ls = left?.ToString() ?? string.Empty;
        var rs = right?.ToString() ?? string.Empty;
        return string.Compare(ls, rs, StringComparison.Ordinal);
    }

    /// <summary>
    /// Attempts to coerce both values to double for numeric operations.
    /// </summary>
    private static (double? left, double? right) CoerceNumeric(object? left, object? right)
    {
        double? l = ToDouble(left);
        double? r = ToDouble(right);
        return (l, r);
    }

    /// <summary>
    /// Converts an object to a nullable double if possible.
    /// </summary>
    private static double? ToDouble(object? value)
    {
        if (value is null) return null;

        // Fast path for common numeric types
        if (value is int i) return i;
        if (value is long l) return l;
        if (value is double d) return d;
        if (value is float f) return f;
        if (value is decimal m) return (double)m;
        if (value is short s) return s;
        if (value is byte b) return b;
        if (value is uint ui) return ui;
        if (value is ulong ul) return ul;
        if (value is ushort us) return us;
        if (value is sbyte sb) return sb;

        // String -> numeric coercion
        if (value is string str && !string.IsNullOrWhiteSpace(str))
        {
            if (double.TryParse(str, NumberStyles.Float | NumberStyles.AllowThousands,
                CultureInfo.InvariantCulture, out var result))
                return result;
        }

        return null;
    }

    /// <summary>
    /// Checks membership of an item in a collection for the <c>in</c> operator.
    /// </summary>
    private static bool CheckMembership(object? item, object? collection)
    {
        if (collection is null)
            return false;

        // String containment
        if (collection is string str && item is string substr)
            return str.Contains(substr, StringComparison.Ordinal);

        // Array and list membership
        if (collection is System.Collections.IEnumerable enumerable)
        {
            foreach (var element in enumerable)
            {
                if (AreEqual(item, element))
                    return true;
            }
            return false;
        }

        return false;
    }

    /// <summary>
    /// Concatenates two values for the <c>+</c> operator (string concatenation).
    /// </summary>
    private static string EvaluateConcat(object? left, object? right)
    {
        return (left?.ToString() ?? string.Empty) + (right?.ToString() ?? string.Empty);
    }
}
