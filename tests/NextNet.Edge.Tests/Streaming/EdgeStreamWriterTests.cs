using NextNet.Edge.Streaming;
using Xunit;

namespace NextNet.Edge.Tests.Streaming;

public class EdgeStreamWriterTests
{
    [Fact]
    public void SupportsStreaming_Should_ReturnTrue_When_Called()
    {
        var writer = new EdgeStreamWriter(new MemoryStream(), new EdgeOptions());
        Assert.True(writer.SupportsStreaming);
    }

    [Fact]
    public async Task WriteAsync_Should_WriteDataToStream_When_Called()
    {
        // Arrange
        var stream = new MemoryStream();
        var writer = new EdgeStreamWriter(stream, new EdgeOptions());
        var data = new byte[] { 1, 2, 3 };

        // Act
        await writer.WriteAsync(data);

        // Assert
        Assert.Equal(3, stream.Length);
    }

    [Fact]
    public async Task WriteAsync_Should_WriteEncodedString_When_Called()
    {
        // Arrange
        var stream = new MemoryStream();
        var writer = new EdgeStreamWriter(stream, new EdgeOptions());

        // Act
        await writer.WriteAsync("Hello");

        // Assert
        Assert.Equal(5, stream.Length);
    }

    [Fact]
    public async Task WriteAsync_Should_WriteNothing_When_StringIsEmpty()
    {
        // Arrange
        var stream = new MemoryStream();
        var writer = new EdgeStreamWriter(stream, new EdgeOptions());

        // Act
        await writer.WriteAsync("");

        // Assert
        Assert.Equal(0, stream.Length);
    }

    [Fact]
    public async Task FlushAsync_Should_NotThrow_When_Called()
    {
        var writer = new EdgeStreamWriter(new MemoryStream(), new EdgeOptions());
        await writer.FlushAsync();
    }

    [Fact]
    public async Task CompleteAsync_Should_PreventFurtherWrites_When_Called()
    {
        var writer = new EdgeStreamWriter(new MemoryStream(), new EdgeOptions());
        await writer.CompleteAsync();

        // Writing after complete should throw
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            writer.WriteAsync("test"));
    }

    [Fact]
    public void Constructor_Should_Throw_When_StreamIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new EdgeStreamWriter((Stream)null!, new EdgeOptions()));
    }

    [Fact]
    public void Constructor_Should_Throw_When_OptionsIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new EdgeStreamWriter(new MemoryStream(), null!));
    }

    [Fact]
    public void Constructor_Should_WrapHttpResponseBody_When_FromHttpResponse()
    {
        // Arrange
        var httpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();

        // Act
        var writer = new EdgeStreamWriter(httpContext.Response, new EdgeOptions());

        // Assert
        Assert.True(writer.SupportsStreaming);
    }
}
