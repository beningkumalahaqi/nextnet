using NextNet.Cli.Commands;
using Xunit;

namespace NextNet.Cli.Tests.Commands;

public class BuildCommandTests
{
    [Fact]
    public async Task ExecuteAsync_ReturnsSuccess()
    {
        var exitCode = await BuildCommand.ExecuteAsync();
        Assert.Equal(0, exitCode);
    }

    [Fact]
    public async Task ExecuteAsync_WithMinify_ReturnsSuccess()
    {
        var exitCode = await BuildCommand.ExecuteAsync(minify: true);
        Assert.Equal(0, exitCode);
    }

    [Fact]
    public async Task ExecuteAsync_WithNoMinify_ReturnsSuccess()
    {
        var exitCode = await BuildCommand.ExecuteAsync(minify: false, noMinify: true);
        Assert.Equal(0, exitCode);
    }

    [Fact]
    public async Task ExecuteAsync_WithSourceMap_ReturnsSuccess()
    {
        var exitCode = await BuildCommand.ExecuteAsync(sourcemap: true);
        Assert.Equal(0, exitCode);
    }

    [Fact]
    public async Task ExecuteAsync_WithNoGzip_ReturnsSuccess()
    {
        var exitCode = await BuildCommand.ExecuteAsync(noGzip: true);
        Assert.Equal(0, exitCode);
    }

    [Fact]
    public void Create_ReturnsCommand()
    {
        var command = BuildCommand.Create();
        Assert.NotNull(command);
        Assert.Equal("build", command.Name);
    }
}
