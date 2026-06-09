namespace NextNet.Data.MultiDb.Tests.Exceptions;

public class ConnectionUnavailableExceptionTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public void Constructor_Should_SetCorrectCode()
    {
        var ex = new ConnectionUnavailableException("Analytics", "Dapper", "Connection disabled");
        Assert.Equal("DS-552", ex.ErrorCode);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Message_Should_IncludeConnectionName()
    {
        var ex = new ConnectionUnavailableException("Analytics", "Dapper", "Connection disabled");
        Assert.Contains("Analytics", ex.Message);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Message_Should_IncludeProviderName()
    {
        var ex = new ConnectionUnavailableException("Analytics", "Dapper", "Connection disabled");
        Assert.Contains("Dapper", ex.Message);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Message_Should_IncludeReason()
    {
        var ex = new ConnectionUnavailableException("Analytics", "Dapper", "Connection disabled");
        Assert.Contains("Connection disabled", ex.Message);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Properties_Should_BeSet()
    {
        var ex = new ConnectionUnavailableException("Analytics", "Dapper", "Disabled");
        Assert.Equal("Analytics", ex.ConnectionName);
        Assert.Equal("Dapper", ex.ProviderName);
        Assert.Equal("Disabled", ex.Reason);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Should_BeNextNetDataException()
    {
        var ex = new ConnectionUnavailableException("Test", "P", "R");
        Assert.IsAssignableFrom<NextNetDataException>(ex);
    }
}
