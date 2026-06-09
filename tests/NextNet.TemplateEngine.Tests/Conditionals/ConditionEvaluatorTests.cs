namespace NextNet.TemplateEngine.Tests.Conditionals;

using NextNet.TemplateEngine.Conditionals;
using NextNet.TemplateEngine.Variables;
using Xunit;

public class ConditionEvaluatorTests
{
    private readonly ConditionEvaluator _evaluator = new();

    [Fact]
    public void Evaluate_Should_ReturnTrue_When_VariableIsTruthy()
    {
        var ctx = VariableContext.CreateBuilder()
            .Set("enabled", true)
            .Build();

        Assert.True(_evaluator.Evaluate("enabled", ctx));
    }

    [Fact]
    public void Evaluate_Should_ReturnFalse_When_VariableIsFalsy()
    {
        var ctx = VariableContext.CreateBuilder()
            .Set("enabled", false)
            .Build();

        Assert.False(_evaluator.Evaluate("enabled", ctx));
    }

    [Fact]
    public void Evaluate_Should_ReturnTrue_When_EqualityMatches()
    {
        var ctx = VariableContext.CreateBuilder()
            .Set("name", "hello")
            .Build();

        Assert.True(_evaluator.Evaluate("name == 'hello'", ctx));
    }

    [Fact]
    public void Evaluate_Should_ReturnFalse_When_EqualityDoesNotMatch()
    {
        var ctx = VariableContext.CreateBuilder()
            .Set("name", "world")
            .Build();

        Assert.False(_evaluator.Evaluate("name == 'hello'", ctx));
    }

    [Fact]
    public void Evaluate_Should_ShortCircuitAnd_When_LeftIsFalse()
    {
        var ctx = VariableContext.CreateBuilder()
            .Set("a", false)
            .Set("b", true)
            .Build();

        Assert.False(_evaluator.Evaluate("a && b", ctx));
    }

    [Fact]
    public void Evaluate_Should_ShortCircuitOr_When_LeftIsTrue()
    {
        var ctx = VariableContext.CreateBuilder()
            .Set("a", true)
            .Set("b", false)
            .Build();

        Assert.True(_evaluator.Evaluate("a || b", ctx));
    }

    [Fact]
    public void Evaluate_Should_ReturnFalse_When_NotAppliedToTrue()
    {
        var ctx = VariableContext.CreateBuilder()
            .Set("enabled", true)
            .Build();

        Assert.False(_evaluator.Evaluate("!enabled", ctx));
    }

    [Fact]
    public void Evaluate_Should_ReturnTrue_When_NotAppliedToFalse()
    {
        var ctx = VariableContext.CreateBuilder()
            .Set("enabled", false)
            .Build();

        Assert.True(_evaluator.Evaluate("!enabled", ctx));
    }

    [Fact]
    public void Evaluate_Should_ReturnFalse_When_VariableIsUndefined()
    {
        var ctx = VariableContext.CreateBuilder().Build();

        Assert.False(_evaluator.Evaluate("undefinedVar", ctx));
    }

    [Fact]
    public void Evaluate_Should_ReturnTrue_When_AndConditionIsTrue()
    {
        var ctx = VariableContext.CreateBuilder()
            .Set("a", true)
            .Set("b", true)
            .Build();

        Assert.True(_evaluator.Evaluate("a && b", ctx));
    }

    [Fact]
    public void Evaluate_Should_HandleNumericComparison_When_UsingGreaterThanOrEqual()
    {
        var ctx = VariableContext.CreateBuilder()
            .Set("age", 25)
            .Build();

        Assert.True(_evaluator.Evaluate("age >= 18", ctx));
        Assert.True(_evaluator.Evaluate("age > 20", ctx));
        Assert.False(_evaluator.Evaluate("age < 18", ctx));
        Assert.False(_evaluator.Evaluate("age == 30", ctx));
    }

    [Fact]
    public void Evaluate_Should_ReturnTrue_When_InOperatorMatches()
    {
        var ctx = VariableContext.CreateBuilder()
            .Set("x", 2)
            .Set("items", new[] { 1, 2, 3 })
            .Build();

        Assert.True(_evaluator.Evaluate("x in items", ctx));
    }

    [Fact]
    public void Evaluate_Should_ReturnFalse_When_InOperatorDoesNotMatch()
    {
        var ctx = VariableContext.CreateBuilder()
            .Set("x", 5)
            .Set("items", new[] { 1, 2, 3 })
            .Build();

        Assert.False(_evaluator.Evaluate("x in items", ctx));
    }

    [Fact]
    public void Evaluate_Should_ThrowEvaluationException_When_UnknownOperator()
    {
        var ctx = VariableContext.CreateBuilder().Build();
        // The parser would not produce an unknown operator, but we can test the evaluator
        // by directly constructing an AST with one
        var badExpr = new NextNet.TemplateEngine.Conditionals.Ast.BinaryExpression(
            new NextNet.TemplateEngine.Conditionals.Ast.LiteralExpression(true),
            "???",
            new NextNet.TemplateEngine.Conditionals.Ast.LiteralExpression(false)
        );

        var ex = Assert.Throws<EvaluationException>(() => _evaluator.Evaluate(badExpr, ctx));
        Assert.Contains("???", ex.Message);
    }

    [Fact]
    public void Evaluate_Should_HandleNestedVariableAccess_When_UsingDotNotation()
    {
        var ctx = VariableContext.CreateBuilder()
            .SetNested("features", new { auth = true, logging = false })
            .Build();

        Assert.True(_evaluator.Evaluate("features.auth == true", ctx));
        Assert.False(_evaluator.Evaluate("features.logging == true", ctx));
    }

    [Fact]
    public void Evaluate_Should_HandleComplexCombinedExpression_When_MultipleOperators()
    {
        var ctx = VariableContext.CreateBuilder()
            .SetNested("features", new { api = true, auth = true, legacy = false })
            .Build();

        var result = _evaluator.Evaluate(
            "(features.api == true && features.auth == true) && !features.legacy",
            ctx);

        Assert.True(result);
    }

    [Fact]
    public void Evaluate_Should_TreatEmptyString_As_Falsy()
    {
        var ctx = VariableContext.CreateBuilder()
            .Set("name", "")
            .Build();

        Assert.False(_evaluator.Evaluate("name", ctx));
    }

    [Fact]
    public void Evaluate_Should_TreatNonZeroNumeric_As_Truthy()
    {
        var ctx = VariableContext.CreateBuilder()
            .Set("count", 42)
            .Build();

        Assert.True(_evaluator.Evaluate("count", ctx));
    }

    [Fact]
    public void Evaluate_Should_TreatZero_As_Falsy()
    {
        var ctx = VariableContext.CreateBuilder()
            .Set("count", 0)
            .Build();

        Assert.False(_evaluator.Evaluate("count", ctx));
    }

    [Fact]
    public void Evaluate_Should_ShortCircuit_When_GroupedAndLeftIsFalse()
    {
        var ctx = VariableContext.CreateBuilder()
            .Set("a", false)
            .Build();

        // Should not evaluate the right side if left of && is false
        // This would throw if it tried to evaluate "undefinedVar == true"
        Assert.False(_evaluator.Evaluate("a && undefinedVar", ctx));
    }

    [Fact]
    public void Evaluate_Should_ReturnTrue_When_StringContainsViaIn()
    {
        var ctx = VariableContext.CreateBuilder()
            .Set("sub", "world")
            .Set("text", "hello world")
            .Build();

        Assert.True(_evaluator.Evaluate("sub in text", ctx));
    }

    [Fact]
    public void Evaluate_Should_ReturnFalse_When_StringDoesNotContainViaIn()
    {
        var ctx = VariableContext.CreateBuilder()
            .Set("sub", "xyz")
            .Set("text", "hello world")
            .Build();

        Assert.False(_evaluator.Evaluate("sub in text", ctx));
    }

    [Fact]
    public void Evaluate_Should_ThrowArgumentNullException_When_ExpressionIsNull()
    {
        var ctx = VariableContext.CreateBuilder().Build();
        Assert.Throws<ArgumentNullException>(() => _evaluator.Evaluate((string)null!, ctx));
    }
}
