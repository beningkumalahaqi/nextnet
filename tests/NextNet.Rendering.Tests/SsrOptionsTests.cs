using NextNet.Configuration;
using Xunit;

namespace NextNet.Rendering.Tests;

public class SsrOptionsTests
{
    [Fact]
    public void Defaults_Should_BeSetCorrectly_WhenNewInstanceCreated()
    {
        var options = new SsrOptions();

        Assert.True(options.Streaming);
        Assert.Equal(8192, options.BufferSize);
        Assert.True(options.EnableCompression);
        Assert.False(options.EnableCaching);
        Assert.Equal(TimeSpan.FromSeconds(10), options.RenderTimeout);
    }

    [Fact]
    public void Apply_Should_NotModifyBufferSize_WhenConfigProvided()
    {
        var options = new SsrOptions();
        var config = new RenderingConfig
        {
            MaxRecursionDepth = 256,
            PrettyPrint = true,
            Minify = false
        };

        options.Apply(config);

        // BufferSize is independently configurable; Apply does not modify it.
        Assert.Equal(8192, options.BufferSize);
    }

    [Fact]
    public void Apply_Should_ThrowArgumentNullException_WhenConfigIsNull()
    {
        var options = new SsrOptions();
        Assert.Throws<ArgumentNullException>(() => options.Apply(null!));
    }

    [Fact]
    public void BufferSize_Should_BeCustomizable_WhenSetDirectly()
    {
        var options = new SsrOptions { BufferSize = 16384 };
        Assert.Equal(16384, options.BufferSize);
    }

    [Fact]
    public void Streaming_Should_BeDisableable_WhenSetToFalse()
    {
        var options = new SsrOptions { Streaming = false };
        Assert.False(options.Streaming);
    }
}
