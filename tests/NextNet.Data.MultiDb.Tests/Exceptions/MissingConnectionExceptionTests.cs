namespace NextNet.Data.MultiDb.Tests.Exceptions;

public class MissingConnectionExceptionTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public void Constructor_Should_SetCorrectCode()
    {
        var ex = new MissingConnectionException("Analytics");
        Assert.Equal("DS-550", ex.ErrorCode);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Message_Should_IncludeConnectionName()
    {
        var ex = new MissingConnectionException("Analytics");
        Assert.Contains("Analytics", ex.Message);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ConnectionName_Should_BeSet()
    {
        var ex = new MissingConnectionException("Primary");
        Assert.Equal("Primary", ex.ConnectionName);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Should_BeNextNetDataException()
    {
        var ex = new MissingConnectionException("Test");
        Assert.IsAssignableFrom<NextNetDataException>(ex);
    }
}
