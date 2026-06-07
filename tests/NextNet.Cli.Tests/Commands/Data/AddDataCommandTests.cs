using NextNet.Cli.Commands.Data;
using Xunit;

namespace NextNet.Cli.Tests.Commands.Data;

public class AddDataCommandTests
{
    [Fact]
    public void Create_ReturnsCommand()
    {
        var command = AddDataCommand.Create();
        Assert.NotNull(command);
        Assert.Equal("data", command.Name);
    }

    [Fact]
    public void Create_HasProviderArgument()
    {
        var command = AddDataCommand.Create();
        var arg = command.Arguments.FirstOrDefault(a => a.Name == "provider");
        Assert.NotNull(arg);
    }

    [Fact]
    public void Create_HasProjectOption()
    {
        var command = AddDataCommand.Create();
        var opt = command.Options.FirstOrDefault(o => o.Name == "project");
        Assert.NotNull(opt);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public async Task ExecuteAsync_InvalidProvider_ReturnsInputError(string? provider)
    {
        var exitCode = await AddDataCommand.ExecuteAsync(provider);
        Assert.Equal(2, exitCode);
    }

    [Fact]
    public async Task ExecuteAsync_UnknownProvider_ReturnsInputError()
    {
        var exitCode = await AddDataCommand.ExecuteAsync("nonexistent");
        Assert.Equal(2, exitCode);
    }

    [Theory]
    [InlineData("ef")]
    [InlineData("EF")]
    [InlineData("dapper")]
    [InlineData("Dapper")]
    [InlineData("mongo")]
    [InlineData("Mongo")]
    public async Task ExecuteAsync_KnownProvider_ReturnsExecutionErrorWhenNoProject(string provider)
    {
        // Without a project context, the dotnet package add will fail,
        // but the validation should pass. Expect execution error (4).
        var exitCode = await AddDataCommand.ExecuteAsync(provider);
        Assert.Equal(4, exitCode);
    }

    [Fact]
    public async Task ExecuteAsync_WithProjectPath_ReturnsExecutionErrorWhenProjectNotFound()
    {
        var exitCode = await AddDataCommand.ExecuteAsync("ef", "/nonexistent/path/project.csproj");
        Assert.Equal(4, exitCode);
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidProjectDirectory_ReturnsExecutionError()
    {
        var exitCode = await AddDataCommand.ExecuteAsync("dapper", "/nonexistent/directory");
        Assert.Equal(4, exitCode);
    }
}
