using NextNet.Cli.Errors;
using NextNet.Cli.UI;
using NextNet.Cli.UI.Messages;
using Xunit;

namespace NextNet.Cli.Tests.Errors;

public class ErrorSystemTests
{
    [Fact]
    public void ErrorCodes_ProjectNameRequired_HasCorrectCode()
    {
        var error = ErrorCodes.ProjectNameRequired;
        Assert.Equal("NN-001", error.Code);
        Assert.Equal("Input", error.Category);
        Assert.Contains("name", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ErrorCodes_InvalidProjectName_HasCorrectCode()
    {
        var error = ErrorCodes.InvalidProjectName;
        Assert.Equal("NN-002", error.Code);
        Assert.Contains("kebab", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ErrorCodes_DirectoryExists_HasCorrectCode()
    {
        var error = ErrorCodes.DirectoryExists;
        Assert.Equal("NN-003", error.Code);
    }

    [Fact]
    public void ErrorCodes_ConfigFileNotFound_HasCorrectCode()
    {
        var error = ErrorCodes.ConfigFileNotFound;
        Assert.Equal("NN-004", error.Code);
    }

    [Fact]
    public void ErrorCodes_InvalidConfigFile_HasCorrectCode()
    {
        var error = ErrorCodes.InvalidConfigFile;
        Assert.Equal("NN-005", error.Code);
    }

    [Fact]
    public void ErrorCodes_CompilationFailed_HasCorrectCode()
    {
        var error = ErrorCodes.CompilationFailed;
        Assert.Equal("NN-007", error.Code);
    }

    [Fact]
    public void ErrorCodes_DevServerFailed_HasCorrectCode()
    {
        var error = ErrorCodes.DevServerFailed;
        Assert.Equal("NN-010", error.Code);
    }

    [Fact]
    public void ErrorCodes_PortInUse_HasCorrectCode()
    {
        var error = ErrorCodes.PortInUse;
        Assert.Equal("NN-011", error.Code);
    }

    [Fact]
    public void ErrorCodes_DotNetSdkNotFound_HasCorrectCode()
    {
        var error = ErrorCodes.DotNetSdkNotFound;
        Assert.Equal("NN-030", error.Code);
    }

    [Fact]
    public void ErrorCodes_AllCodesAreUnique()
    {
        var codes = new[]
        {
            ErrorCodes.ProjectNameRequired,
            ErrorCodes.InvalidProjectName,
            ErrorCodes.DirectoryExists,
            ErrorCodes.ConfigFileNotFound,
            ErrorCodes.InvalidConfigFile,
            ErrorCodes.UnknownConfigKey,
            ErrorCodes.CompilationFailed,
            ErrorCodes.RouteDiscoveryFailed,
            ErrorCodes.SourceGenerationFailed,
            ErrorCodes.DevServerFailed,
            ErrorCodes.PortInUse,
            ErrorCodes.FileWatcherError,
            ErrorCodes.DeployTargetNotFound,
            ErrorCodes.AuthFailed,
            ErrorCodes.UploadFailed,
            ErrorCodes.PluginNotFound,
            ErrorCodes.PluginLoadFailed,
            ErrorCodes.PluginVersionMismatch,
            ErrorCodes.DotNetSdkNotFound,
            ErrorCodes.UnsupportedDotNetVersion,
            ErrorCodes.PermissionDenied,
            ErrorCodes.DiskSpaceInsufficient,
            ErrorCodes.MigrationAddFailed,
            ErrorCodes.MigrationApplyFailed,
            ErrorCodes.MigrationRollbackFailed,
            ErrorCodes.MigrationStatusFailed,
            ErrorCodes.MigrationNotFound,
            ErrorCodes.ConfirmationRequired,
            ErrorCodes.DryRunOnly
        };

        Assert.Equal(codes.Length, codes.Select(c => c.Code).Distinct().Count());
    }

    [Fact]
    public void ErrorCodes_AllHaveMessages()
    {
        var codes = new[]
        {
            ErrorCodes.ProjectNameRequired,
            ErrorCodes.InvalidProjectName,
            ErrorCodes.DirectoryExists,
            ErrorCodes.ConfigFileNotFound,
            ErrorCodes.InvalidConfigFile,
            ErrorCodes.UnknownConfigKey,
            ErrorCodes.CompilationFailed,
            ErrorCodes.RouteDiscoveryFailed,
            ErrorCodes.SourceGenerationFailed,
            ErrorCodes.DevServerFailed,
            ErrorCodes.PortInUse,
            ErrorCodes.FileWatcherError,
            ErrorCodes.DeployTargetNotFound,
            ErrorCodes.AuthFailed,
            ErrorCodes.UploadFailed,
            ErrorCodes.PluginNotFound,
            ErrorCodes.PluginLoadFailed,
            ErrorCodes.PluginVersionMismatch,
            ErrorCodes.DotNetSdkNotFound,
            ErrorCodes.UnsupportedDotNetVersion,
            ErrorCodes.PermissionDenied,
            ErrorCodes.DiskSpaceInsufficient,
            ErrorCodes.MigrationAddFailed,
            ErrorCodes.MigrationApplyFailed,
            ErrorCodes.MigrationRollbackFailed,
            ErrorCodes.MigrationStatusFailed,
            ErrorCodes.MigrationNotFound,
            ErrorCodes.ConfirmationRequired,
            ErrorCodes.DryRunOnly
        };

        foreach (var code in codes)
        {
            Assert.False(string.IsNullOrWhiteSpace(code.Message));
            Assert.False(string.IsNullOrWhiteSpace(code.Code));
        }
    }

    [Fact]
    public void ErrorMessage_WriteSimple_PlainMode_DoesNotThrow()
    {
        var console = NextNetConsole.Create(plain: true);
        ErrorMessage.WriteSimple(console, "Test error");
    }

    [Fact]
    public void ErrorMessage_WriteSimple_ColorMode_DoesNotThrow()
    {
        var console = NextNetConsole.Create(plain: false);
        ErrorMessage.WriteSimple(console, "Test error");
    }

    [Fact]
    public void ErrorMessage_WriteSimple_WithException_DoesNotThrow()
    {
        var console = NextNetConsole.Create(plain: true);
        ErrorMessage.WriteSimple(console, "Test error", new InvalidOperationException("Detail"));
    }

    [Fact]
    public void ErrorMessage_Write_PlainMode_DoesNotThrow()
    {
        var console = NextNetConsole.Create(plain: true);
        ErrorMessage.Write(console, ErrorCodes.ProjectNameRequired);
    }

    [Fact]
    public void ErrorMessage_Write_ColorMode_DoesNotThrow()
    {
        var console = NextNetConsole.Create(plain: false);
        ErrorMessage.Write(console, ErrorCodes.CompilationFailed, "Custom context");
    }

    [Fact]
    public void ErrorCodes_MigrationAddFailed_HasCorrectCode()
    {
        var error = ErrorCodes.MigrationAddFailed;
        Assert.Equal("NN-050", error.Code);
        Assert.Contains("migration", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ErrorCodes_MigrationApplyFailed_HasCorrectCode()
    {
        var error = ErrorCodes.MigrationApplyFailed;
        Assert.Equal("NN-051", error.Code);
        Assert.Contains("apply", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ErrorCodes_MigrationRollbackFailed_HasCorrectCode()
    {
        var error = ErrorCodes.MigrationRollbackFailed;
        Assert.Equal("NN-052", error.Code);
        Assert.Contains("rollback", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ErrorCodes_MigrationStatusFailed_HasCorrectCode()
    {
        var error = ErrorCodes.MigrationStatusFailed;
        Assert.Equal("NN-053", error.Code);
        Assert.Contains("status", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ErrorCodes_MigrationNotFound_HasCorrectCode()
    {
        var error = ErrorCodes.MigrationNotFound;
        Assert.Equal("NN-054", error.Code);
        Assert.Contains("not found", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ErrorCodes_ConfirmationRequired_HasCorrectCode()
    {
        var error = ErrorCodes.ConfirmationRequired;
        Assert.Equal("NN-055", error.Code);
        Assert.Contains("confirmation", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ErrorCodes_DryRunOnly_HasCorrectCode()
    {
        var error = ErrorCodes.DryRunOnly;
        Assert.Equal("NN-056", error.Code);
        Assert.Contains("dry-run", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ErrorEntry_Properties_AreCorrect()
    {
        var entry = new ErrorEntry("NN-999", "Test", "Test message",
            Context: "Some context",
            Usage: "nextnet test",
            Examples: new[] { "nextnet test --verbose" });

        Assert.Equal("NN-999", entry.Code);
        Assert.Equal("Test", entry.Category);
        Assert.Equal("Test message", entry.Message);
        Assert.Equal("Some context", entry.Context);
        Assert.Equal("nextnet test", entry.Usage);
        Assert.NotNull(entry.Examples);
        Assert.Single(entry.Examples!);
    }
}
