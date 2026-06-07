using NextNet.Data.MongoDB.Tests.Fixtures;

namespace NextNet.Data.MongoDB.Tests.Internal;

/// <summary>
/// Tests for <see cref="FilterParser"/>.
/// </summary>
public sealed class FilterParserTests
{
    [Fact]
    public void Parse_ShouldReturnFilterDefinition_ForValidJson()
    {
        var filter = FilterParser.Parse<TestEntity>("{ \"age\": { \"$gte\": 21 } }");
        Assert.NotNull(filter);
    }

    [Fact]
    public void Parse_ShouldReturnFilterDefinition_ForSimpleField()
    {
        var filter = FilterParser.Parse<TestEntity>("{ \"name\": \"Alice\" }");
        Assert.NotNull(filter);
    }

    [Fact]
    public void Parse_ShouldReturnFilterDefinition_ForEmptyObject()
    {
        var filter = FilterParser.Parse<TestEntity>("{ }");
        Assert.NotNull(filter);
    }

    [Fact]
    public void Parse_ShouldThrow_ForInvalidJson()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            FilterParser.Parse<TestEntity>("{ invalid json }"));
        Assert.Contains("BSON", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Parse_ShouldThrow_ForNullInput()
    {
        Assert.Throws<ArgumentNullException>(() => FilterParser.Parse<TestEntity>(null!));
    }

    [Fact]
    public void Parse_ShouldThrow_ForEmptyInput()
    {
        Assert.Throws<ArgumentException>(() => FilterParser.Parse<TestEntity>(string.Empty));
    }

    [Fact]
    public void Parse_ShouldRejectDollarWhereOperator()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            FilterParser.Parse<TestEntity>("{ \"$where\": \"this.password.length > 10\" }"));
        Assert.Contains("$where", ex.Message);
    }

    [Fact]
    public void Parse_ShouldRejectDollarFunctionOperator()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            FilterParser.Parse<TestEntity>("{ \"$function\": { \"body\": \"...\" } }"));
        Assert.Contains("$function", ex.Message);
    }

    [Fact]
    public void Parse_ShouldRejectDollarWhere_InNestedDocument()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            FilterParser.Parse<TestEntity>("{ \"$or\": [ { \"$where\": \"evil()\" }, { \"age\": 5 } ] }"));
        Assert.Contains("$where", ex.Message);
    }

    [Fact]
    public void Empty_ShouldReturnEmptyFilter()
    {
        var filter = FilterParser.Empty<TestEntity>();
        Assert.NotNull(filter);
    }

    [Fact]
    public void Parse_ShouldHandleComplexFilter()
    {
        var filter = FilterParser.Parse<TestEntity>(@"{
            ""age"": { ""$gte"": 18, ""$lte"": 65 },
            ""name"": { ""$regex"": ""^A"" }
        }");
        Assert.NotNull(filter);
    }
}
