using NextNet.Edge.Streaming;
using Xunit;

namespace NextNet.Edge.Tests.Streaming;

public class EdgeStreamWriterTests
{
    [Fact]
    public void SupportsStreaming_ReturnsTrue()
    {
        var writer = new EdgeStreamWriter(new MemoryStream(), new EdgeOptions());
        Assert.True(writer.SupportsStreaming);
    }

    [Fact]
    public async Task WriteAsync_Data_WritesToStream()
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
    public async Task WriteAsync_String_WritesEncoded()
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
    public async Task WriteAsync_EmptyString_WritesNothing()
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
    public async Task FlushAsync_DoesNotThrow()
    {
        var writer = new EdgeStreamWriter(new MemoryStream(), new EdgeOptions());
        await writer.FlushAsync();
    }

    [Fact]
    public async Task CompleteAsync_MarksComplete()
    {
        var writer = new EdgeStreamWriter(new MemoryStream(), new EdgeOptions());
        await writer.CompleteAsync();

        // Writing after complete should throw
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            writer.WriteAsync("test"));
    }

    [Fact]
    public void Constructor_NullStream_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new EdgeStreamWriter((Stream)null!, new EdgeOptions()));
    }

    [Fact]
    public void Constructor_NullOptions_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new EdgeStreamWriter(new MemoryStream(), null!));
    }

    [Fact]
    public void Constructor_FromHttpResponse_WrapsBody()
    {
        // Arrange
        var httpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();

        // Act
        var writer = new EdgeStreamWriter(httpContext.Response, new EdgeOptions());

        // Assert
        Assert.True(writer.SupportsStreaming);
    }
}
