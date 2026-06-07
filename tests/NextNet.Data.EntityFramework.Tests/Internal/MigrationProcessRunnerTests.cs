using NextNet.Data.EntityFramework.Internal;

namespace NextNet.Data.EntityFramework.Tests.Internal;

/// <summary>
/// Tests for <see cref="MigrationProcessRunner"/>.
/// </summary>
public sealed class MigrationProcessRunnerTests
{
    [Fact]
    public async Task RunProcessAsync_Should_ExecuteSuccessfully_For_ValidCommand()
    {
        // Arrange
        var logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<MigrationProcessRunner>();
        var runner = new MigrationProcessRunner(logger);

        // Act - Run a simple dotnet command
        var (success, output, error) = await runner.RunProcessAsync(
            "dotnet",
            "--version");

        // Assert
        Assert.True(success);
        Assert.NotNull(output);
        Assert.Contains(".", output); // Should contain a version number
    }

    [Fact]
    public async Task RunProcessAsync_Should_Throw_When_FileNameIsEmpty()
    {
        // Arrange
        var logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<MigrationProcessRunner>();
        var runner = new MigrationProcessRunner(logger);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            runner.RunProcessAsync("", "--version"));
    }

    [Fact]
    public async Task RunProcessAsync_Should_Handle_Cancellation()
    {
        // Arrange
        var logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<MigrationProcessRunner>();
        var runner = new MigrationProcessRunner(logger);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            runner.RunProcessAsync("dotnet", "--version", cts.Token));
    }
}
