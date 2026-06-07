using NextNet.DevTools.Headless;
using Xunit;

namespace NextNet.DevTools.Tests.Headless;

public class DevToolsApiControllerTests
{
    [Fact]
    public void SerializeResponse_ReturnsValidJson()
    {
        var json = DevToolsApiController.SerializeResponse(new
        {
            message = "hello",
            value = 42
        });

        Assert.Contains("\"message\"", json);
        Assert.Contains("\"hello\"", json);
        Assert.Contains("\"value\"", json);
        Assert.Contains("42", json);
    }

    [Fact]
    public void SerializeResponse_UsesCamelCase()
    {
        var json = DevToolsApiController.SerializeResponse(new
        {
            SomeProperty = "test",
            AnotherValue = 123
        });

        Assert.Contains("\"someProperty\"", json);
        Assert.Contains("\"anotherValue\"", json);
    }

    [Fact]
    public void CreateErrorResponse_ContainsErrorMessage()
    {
        var json = DevToolsApiController.CreateErrorResponse("Not found", 404);

        Assert.Contains("\"message\"", json);
        Assert.Contains("\"Not found\"", json);
        Assert.Contains("\"statusCode\"", json);
        Assert.Contains("404", json);
    }

    [Fact]
    public void CreateErrorResponse_DefaultStatusCode()
    {
        var json = DevToolsApiController.CreateErrorResponse("Bad request");

        Assert.Contains("400", json);
    }

    [Fact]
    public void CreateSuccessResponse_ContainsSuccess()
    {
        var json = DevToolsApiController.CreateSuccessResponse(new
        {
            id = 1,
            name = "test"
        });

        Assert.Contains("\"success\"", json);
        Assert.Contains("\"data\"", json);
        Assert.Contains("\"name\"", json);
        Assert.Contains("\"test\"", json);
    }
}
