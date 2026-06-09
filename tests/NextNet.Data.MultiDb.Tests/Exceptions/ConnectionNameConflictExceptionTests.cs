namespace NextNet.Data.MultiDb.Tests.Exceptions;

public class ConnectionNameConflictExceptionTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public void Constructor_Should_SetCorrectCode()
    {
        var ex = new ConnectionNameConflictException("Analytics");
        Assert.Equal("DS-551", ex.ErrorCode);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Message_Should_IncludeConnectionName()
    {
        var ex = new ConnectionNameConflictException("Analytics");
        Assert.Contains("Analytics", ex.Message);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ConnectionName_Should_BeSet()
    {
        var ex = new ConnectionNameConflictException("Duplicate");
        Assert.Equal("Duplicate", ex.ConnectionName);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Should_BeNextNetDataException()
    {
        var ex = new ConnectionNameConflictException("Test");
        Assert.IsAssignableFrom<NextNetDataException>(ex);
    }
}
