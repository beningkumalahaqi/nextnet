namespace NextNet.Data.MultiDb.Tests.Exceptions;

public class ProviderMismatchExceptionTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public void Constructor_Should_SetCorrectCode()
    {
        var ex = new ProviderMismatchException("Analytics", "EntityFramework", "Dapper", "Provider type mismatch");
        Assert.Equal("DS-553", ex.ErrorCode);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Message_Should_IncludeConnectionName()
    {
        var ex = new ProviderMismatchException("Analytics", "EF", "Dapper", "mismatch");
        Assert.Contains("Analytics", ex.Message);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Properties_Should_BeSet()
    {
        var ex = new ProviderMismatchException("Analytics", "EntityFramework", "Dapper", "Type mismatch");
        Assert.Equal("Analytics", ex.ConnectionName);
        Assert.Equal("EntityFramework", ex.ExpectedProvider);
        Assert.Equal("Dapper", ex.ActualProvider);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Should_BeNextNetDataException()
    {
        var ex = new ProviderMismatchException("Test", "A", "B", "Msg");
        Assert.IsAssignableFrom<NextNetDataException>(ex);
    }
}
