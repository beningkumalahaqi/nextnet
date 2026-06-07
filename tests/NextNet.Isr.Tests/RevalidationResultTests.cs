using NextNet.Isr.Revalidation;

namespace NextNet.Isr.Tests;

public class RevalidationResultTests
{
    [Fact]
    public void Ok_SingleRoute_SetsSuccessAndCount()
    {
        var result = RevalidationResult.Ok("/blog/post");

        Assert.True(result.Success);
        Assert.Equal("/blog/post", result.Route);
        Assert.Equal(1, result.RevalidatedCount);
    }

    [Fact]
    public void Ok_MultipleRoutes_SetsSuccessAndCount()
    {
        var routes = new[] { "/blog/post-1", "/blog/post-2" };
        var result = RevalidationResult.Ok(routes);

        Assert.True(result.Success);
        Assert.Equal(2, result.RevalidatedCount);
        Assert.Equal(routes, result.Routes);
    }

    [Fact]
    public void Fail_SetsErrorMessage()
    {
        var result = RevalidationResult.Fail("Something went wrong");

        Assert.False(result.Success);
        Assert.Equal("Something went wrong", result.ErrorMessage);
        Assert.Equal(0, result.RevalidatedCount);
        Assert.Null(result.Route);
    }
}
