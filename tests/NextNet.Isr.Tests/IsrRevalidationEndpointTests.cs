using System.Text;
using Microsoft.AspNetCore.Http;
using Moq;
using NextNet.Isr.Endpoints;
using NextNet.Isr.Revalidation;

namespace NextNet.Isr.Tests;

public class IsrRevalidationEndpointTests
{
    private readonly IsrGlobalOptions _globalOptions;
    private readonly Mock<IIsrRevalidationManager> _mockManager;
    private readonly OnDemandRevalidator _revalidator;
    private readonly IsrRevalidationEndpoint _endpoint;

    public IsrRevalidationEndpointTests()
    {
        _globalOptions = new IsrGlobalOptions { RevalidationSecret = "my-secret" };
        _mockManager = new Mock<IIsrRevalidationManager>(MockBehavior.Strict);
        _revalidator = new OnDemandRevalidator(_mockManager.Object, _globalOptions);
        _endpoint = new IsrRevalidationEndpoint(_revalidator);
    }

    [Fact]
    public async Task HandleAsync_NonPost_Returns405()
    {
        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Response.Body = new MemoryStream();

        await _endpoint.HandleAsync(context);

        Assert.Equal(StatusCodes.Status405MethodNotAllowed, context.Response.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_InvalidJson_Returns400()
    {
        var context = new DefaultHttpContext();
        context.Request.Method = "POST";
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes("invalid json"));
        context.Response.Body = new MemoryStream();

        await _endpoint.HandleAsync(context);

        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_EmptyBody_Returns400()
    {
        var context = new DefaultHttpContext();
        context.Request.Method = "POST";
        context.Request.Body = new MemoryStream(Array.Empty<byte>());
        context.Response.Body = new MemoryStream();

        await _endpoint.HandleAsync(context);

        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_InvalidSecret_Returns401()
    {
        var context = new DefaultHttpContext();
        context.Request.Method = "POST";
        var body = "{\"path\":\"/test\",\"secret\":\"wrong\"}";
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
        context.Response.Body = new MemoryStream();

        await _endpoint.HandleAsync(context);

        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_MissingPathAndTags_Returns400()
    {
        var context = new DefaultHttpContext();
        context.Request.Method = "POST";
        var body = "{\"secret\":\"my-secret\"}";
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
        context.Response.Body = new MemoryStream();

        await _endpoint.HandleAsync(context);

        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_WithValidPath_Returns200()
    {
        _mockManager.Setup(m => m.RevalidateAsync("/test", It.IsAny<CancellationToken>()))
            .ReturnsAsync(RevalidationResult.Ok("/test"));

        var context = new DefaultHttpContext();
        context.Request.Method = "POST";
        var body = "{\"path\":\"/test\",\"secret\":\"my-secret\"}";
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
        context.Response.Body = new MemoryStream();

        await _endpoint.HandleAsync(context);

        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_WithTags_Returns200()
    {
        _mockManager.Setup(m => m.InvalidateByTagsAsync(
                new[] { "blog" }, It.IsAny<CancellationToken>()))
            .ReturnsAsync(RevalidationResult.Ok(new[] { "/blog/post-1" }));

        var context = new DefaultHttpContext();
        context.Request.Method = "POST";
        var body = "{\"tags\":[\"blog\"],\"secret\":\"my-secret\"}";
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
        context.Response.Body = new MemoryStream();

        await _endpoint.HandleAsync(context);

        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
    }
}
