namespace NextNet.TemplateEngine.Tests.Conditionals;

using NextNet.TemplateEngine.Conditionals;
using NextNet.TemplateEngine.Conditionals.Ast;
using Xunit;

public class ConditionParserTests
{
    private readonly ConditionParser _parser = new();

    [Fact]
    public void Parse_Should_ParseVariable_When_SimpleIdentifier()
    {
        var expr = _parser.Parse("enabled");

        var varExpr = Assert.IsType<VariableExpression>(expr);
        Assert.Equal("enabled", varExpr.Name);
    }

    [Fact]
    public void Parse_Should_ParseEquality_When_UsingEqualsOperator()
    {
        var expr = _parser.Parse("x == 5");

        var binary = Assert.IsType<BinaryExpression>(expr);
        Assert.Equal("==", binary.Operator);

        var left = Assert.IsType<VariableExpression>(binary.Left);
        Assert.Equal("x", left.Name);

        var right = Assert.IsType<LiteralExpression>(binary.Right);
        Assert.Equal(5, right.Value);
    }

    [Fact]
    public void Parse_Should_ParseAndExpression_When_UsingDoubleAmpersand()
    {
        var expr = _parser.Parse("a && b");

        var binary = Assert.IsType<BinaryExpression>(expr);
        Assert.Equal("&&", binary.Operator);

        Assert.IsType<VariableExpression>(binary.Left);
        Assert.IsType<VariableExpression>(binary.Right);
    }

    [Fact]
    public void Parse_Should_ParseOrExpression_When_UsingDoublePipe()
    {
        var expr = _parser.Parse("a || b");

        var binary = Assert.IsType<BinaryExpression>(expr);
        Assert.Equal("||", binary.Operator);

        Assert.IsType<VariableExpression>(binary.Left);
        Assert.IsType<VariableExpression>(binary.Right);
    }

    [Fact]
    public void Parse_Should_ParseNotExpression_When_UsingExclamation()
    {
        var expr = _parser.Parse("!enabled");

        var unary = Assert.IsType<UnaryExpression>(expr);
        Assert.Equal("!", unary.Operator);

        var inner = Assert.IsType<VariableExpression>(unary.Operand);
        Assert.Equal("enabled", inner.Name);
    }

    [Fact]
    public void Parse_Should_ParseGroupedExpression_When_UsingParentheses()
    {
        var expr = _parser.Parse("(a || b) && c");

        var outer = Assert.IsType<BinaryExpression>(expr);
        Assert.Equal("&&", outer.Operator);

        var group = Assert.IsType<GroupExpression>(outer.Left);
        var inner = Assert.IsType<BinaryExpression>(group.Inner);
        Assert.Equal("||", inner.Operator);

        var right = Assert.IsType<VariableExpression>(outer.Right);
        Assert.Equal("c", right.Name);
    }

    [Fact]
    public void Parse_Should_ParseStringLiteral_When_UsingSingleQuotes()
    {
        var expr = _parser.Parse("name == 'hello'");

        var binary = Assert.IsType<BinaryExpression>(expr);
        var right = Assert.IsType<LiteralExpression>(binary.Right);
        Assert.Equal("hello", right.Value);
    }

    [Fact]
    public void Parse_Should_ParseStringLiteral_When_UsingDoubleQuotes()
    {
        var expr = _parser.Parse(@"name == ""hello""");

        var binary = Assert.IsType<BinaryExpression>(expr);
        var right = Assert.IsType<LiteralExpression>(binary.Right);
        Assert.Equal("hello", right.Value);
    }

    [Fact]
    public void Parse_Should_ParseBooleanLiteral_When_UsingTrueKeyword()
    {
        var expr = _parser.Parse("enabled == true");

        var binary = Assert.IsType<BinaryExpression>(expr);
        var right = Assert.IsType<LiteralExpression>(binary.Right);
        Assert.Equal(true, right.Value);
    }

    [Fact]
    public void Parse_Should_ParseNumericLiteral_When_UsingInteger()
    {
        var expr = _parser.Parse("count > 42");

        var binary = Assert.IsType<BinaryExpression>(expr);
        var right = Assert.IsType<LiteralExpression>(binary.Right);
        Assert.Equal(42, right.Value);
    }

    [Fact]
    public void Parse_Should_ParseNullLiteral_When_UsingNullKeyword()
    {
        var expr = _parser.Parse("value == null");

        var binary = Assert.IsType<BinaryExpression>(expr);
        var right = Assert.IsType<LiteralExpression>(binary.Right);
        Assert.Null(right.Value);
    }

    [Fact]
    public void Parse_Should_RespectPrecedence_When_AndBindsTighterThanOr()
    {
        // Without parentheses, && should bind tighter than ||
        var expr = _parser.Parse("a || b && c");

        // Expected: a || (b && c)
        var outer = Assert.IsType<BinaryExpression>(expr);
        Assert.Equal("||", outer.Operator);

        Assert.IsType<VariableExpression>(outer.Left);

        var inner = Assert.IsType<BinaryExpression>(outer.Right);
        Assert.Equal("&&", inner.Operator);
    }

    [Fact]
    public void Parse_Should_ThrowParseException_When_UnexpectedToken()
    {
        var ex = Assert.Throws<ParseException>(() => _parser.Parse("a == @bad"));
        Assert.True(ex.Position >= 0);
    }

    [Fact]
    public void Parse_Should_ThrowArgumentException_When_EmptyExpression()
    {
        var ex = Assert.Throws<ArgumentException>(() => _parser.Parse(""));
        Assert.Contains("cannot be null or empty", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Parse_Should_ParseComplexExpression_When_CombiningAndOrNotWithGrouping()
    {
        // (features.auth == true || features.oauth == true) && !features.legacy
        var expr = _parser.Parse("(features.auth == true || features.oauth == true) && !features.legacy");

        var outerAnd = Assert.IsType<BinaryExpression>(expr);
        Assert.Equal("&&", outerAnd.Operator);

        var group = Assert.IsType<GroupExpression>(outerAnd.Left);
        var innerOr = Assert.IsType<BinaryExpression>(group.Inner);
        Assert.Equal("||", innerOr.Operator);

        // Left of OR: features.auth == true
        var leftEq = Assert.IsType<BinaryExpression>(innerOr.Left);
        Assert.Equal("==", leftEq.Operator);
        var leftVar = Assert.IsType<VariableExpression>(leftEq.Left);
        Assert.Equal("features.auth", leftVar.Name);

        // Right of OR: features.oauth == true
        var rightEq = Assert.IsType<BinaryExpression>(innerOr.Right);
        Assert.Equal("==", rightEq.Operator);
        var rightVar = Assert.IsType<VariableExpression>(rightEq.Left);
        Assert.Equal("features.oauth", rightVar.Name);

        // Right of AND: !features.legacy
        var notExpr = Assert.IsType<UnaryExpression>(outerAnd.Right);
        Assert.Equal("!", notExpr.Operator);
        var legacyVar = Assert.IsType<VariableExpression>(notExpr.Operand);
        Assert.Equal("features.legacy", legacyVar.Name);
    }

    [Fact]
    public void Parse_Should_ParseDotNotationVariable_When_DottedIdentifier()
    {
        var expr = _parser.Parse("features.auth.enabled");

        var varExpr = Assert.IsType<VariableExpression>(expr);
        Assert.Equal("features.auth.enabled", varExpr.Name);
    }

    [Fact]
    public void Parse_Should_ParseInOperator_When_UsingInKeyword()
    {
        var expr = _parser.Parse("x in items");

        var binary = Assert.IsType<BinaryExpression>(expr);
        Assert.Equal("in", binary.Operator);

        var left = Assert.IsType<VariableExpression>(binary.Left);
        Assert.Equal("x", left.Name);

        var right = Assert.IsType<VariableExpression>(binary.Right);
        Assert.Equal("items", right.Name);
    }

    [Fact]
    public void Parse_Should_ParseDoubleNumericLiteral_When_UsingDecimal()
    {
        var expr = _parser.Parse("pi == 3.14");

        var binary = Assert.IsType<BinaryExpression>(expr);
        var right = Assert.IsType<LiteralExpression>(binary.Right);
        Assert.Equal(3.14, right.Value);
    }

    [Fact]
    public void Parse_Should_ParseInequality_When_UsingNotEqualOperator()
    {
        var expr = _parser.Parse("x != y");

        var binary = Assert.IsType<BinaryExpression>(expr);
        Assert.Equal("!=", binary.Operator);
    }

    [Fact]
    public void Parse_Should_ParseComparison_When_UsingGreaterThanOrEqual()
    {
        var expr = _parser.Parse("x >= 10");

        var binary = Assert.IsType<BinaryExpression>(expr);
        Assert.Equal(">=", binary.Operator);
    }

    [Fact]
    public void Parse_Should_ParseComparison_When_UsingLessThanOrEqual()
    {
        var expr = _parser.Parse("x <= 100");

        var binary = Assert.IsType<BinaryExpression>(expr);
        Assert.Equal("<=", binary.Operator);
    }

    [Fact]
    public void Parse_Should_ParseStringConcat_When_UsingPlusOperator()
    {
        var expr = _parser.Parse("firstName + ' ' + lastName");

        var outer = Assert.IsType<BinaryExpression>(expr);
        Assert.Equal("+", outer.Operator);

        var inner = Assert.IsType<BinaryExpression>(outer.Left);
        Assert.Equal("+", inner.Operator);

        Assert.IsType<VariableExpression>(inner.Left);
        Assert.IsType<LiteralExpression>(inner.Right);

        Assert.IsType<VariableExpression>(outer.Right);
    }
}
