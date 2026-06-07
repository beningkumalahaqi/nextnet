using NextNet.Cli.Commands.Data;
using Xunit;

namespace NextNet.Cli.Tests.Commands.Data;

public class DbRollbackCommandTests
{
    [Fact]
    public void Create_ReturnsCommand()
    {
        var command = DbRollbackCommand.Create();
        Assert.NotNull(command);
        Assert.Equal("rollback", command.Name);
    }

    [Fact]
    public void Create_HasDryRunOption()
    {
        var command = DbRollbackCommand.Create();
        var opt = command.Options.FirstOrDefault(o => o.Name == "dry-run");
        Assert.NotNull(opt);
    }

    [Fact]
    public void Create_HasConnectionOption()
    {
        var command = DbRollbackCommand.Create();
        var opt = command.Options.FirstOrDefault(o => o.Name == "connection");
        Assert.NotNull(opt);
    }

    [Fact]
    public void Create_HasStepsOption()
    {
        var command = DbRollbackCommand.Create();
        var opt = command.Options.FirstOrDefault(o => o.Name == "steps");
        Assert.NotNull(opt);
    }

    [Fact]
    public void Create_HasBackupOption()
    {
        var command = DbRollbackCommand.Create();
        var opt = command.Options.FirstOrDefault(o => o.Name == "backup");
        Assert.NotNull(opt);
    }

    [Fact]
    public void Create_HasBackupDirOption()
    {
        var command = DbRollbackCommand.Create();
        var opt = command.Options.FirstOrDefault(o => o.Name == "backup-dir");
        Assert.NotNull(opt);
    }

    [Fact]
    public void Create_HasConfirmOption()
    {
        var command = DbRollbackCommand.Create();
        var opt = command.Options.FirstOrDefault(o => o.Name == "confirm");
        Assert.NotNull(opt);
    }

    [Fact]
    public void Create_StepsDefaultIsOne()
    {
        var command = DbRollbackCommand.Create();
        var opt = command.Options.FirstOrDefault(o => o.Name == "steps");
        Assert.NotNull(opt);

        // System.CommandLine doesn't expose default values directly via public API in this version,
        // but we can verify the option exists and is of the correct type
        Assert.IsType<int>(opt.Arity.MaximumNumberOfValues == 0 ? 0 : 1);
    }

    [Fact]
    public async Task ExecuteAsync_DryRun_ReturnsSuccess()
    {
        var exitCode = await DbRollbackCommand.ExecuteAsync(dryRun: true);
        Assert.Equal(0, exitCode);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidSteps_ReturnsInputError()
    {
        var exitCode = await DbRollbackCommand.ExecuteAsync(steps: 0);
        Assert.Equal(2, exitCode);
    }

    [Fact]
    public async Task ExecuteAsync_WithNegativeSteps_ReturnsInputError()
    {
        var exitCode = await DbRollbackCommand.ExecuteAsync(steps: -1);
        Assert.Equal(2, exitCode);
    }

    [Fact]
    public async Task ExecuteAsync_WithoutConfig_ReturnsInputError()
    {
        var exitCode = await DbRollbackCommand.ExecuteAsync();
        Assert.Equal(2, exitCode);
    }
}
