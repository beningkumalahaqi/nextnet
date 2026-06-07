using System.CommandLine;
using Xunit;

namespace NextNet.Cli.Tests;

public class NextNetCliTests
{
    [Fact]
    public void Create_ReturnsRootCommand()
    {
        var root = NextNetCli.Create();
        Assert.NotNull(root);
        Assert.Equal("NextNet CLI — full-stack web framework for .NET", root.Description);
    }

    [Fact]
    public void Create_HasNewCommand()
    {
        var root = NextNetCli.Create();
        Assert.Contains(root.Subcommands, c => c.Name == "new");
    }

    [Fact]
    public void Create_HasBuildCommand()
    {
        var root = NextNetCli.Create();
        Assert.Contains(root.Subcommands, c => c.Name == "build");
    }

    [Fact]
    public void Create_HasDevCommand()
    {
        var root = NextNetCli.Create();
        Assert.Contains(root.Subcommands, c => c.Name == "dev");
    }

    [Fact]
    public void Create_HasInfoCommand()
    {
        var root = NextNetCli.Create();
        Assert.Contains(root.Subcommands, c => c.Name == "info");
    }

    [Fact]
    public void Create_HasDoctorCommand()
    {
        var root = NextNetCli.Create();
        Assert.Contains(root.Subcommands, c => c.Name == "doctor");
    }

    [Fact]
    public void Create_HasDevToolsCommand()
    {
        var root = NextNetCli.Create();
        Assert.Contains(root.Subcommands, c => c.Name == "devtools");
    }

    [Fact]
    public void Create_HasPlainOption()
    {
        var root = NextNetCli.Create();
        // GlobalOptions may not be available in this version, so check options directly
        var allOptions = root.Options;
        Assert.Contains(allOptions, o => o.Name == "plain");
    }

    [Fact]
    public void Create_HasNoColorOption()
    {
        var root = NextNetCli.Create();
        var allOptions = root.Options;
        Assert.Contains(allOptions, o => o.Name == "no-color");
    }

    [Fact]
    public void Create_HasVerboseOption()
    {
        var root = NextNetCli.Create();
        var allOptions = root.Options;
        Assert.Contains(allOptions, o => o.Name == "verbose");
    }

    [Fact]
    public void Create_HasAddCommand()
    {
        var root = NextNetCli.Create();
        Assert.Contains(root.Subcommands, c => c.Name == "add");
    }

    [Fact]
    public void Create_AddCommand_HasDataSubcommand()
    {
        var root = NextNetCli.Create();
        var addCmd = Assert.Single(root.Subcommands, c => c.Name == "add");
        Assert.Contains(addCmd.Subcommands, c => c.Name == "data");
    }

    [Fact]
    public void Create_HasDbCommand()
    {
        var root = NextNetCli.Create();
        Assert.Contains(root.Subcommands, c => c.Name == "db");
    }

    [Fact]
    public void Create_DbCommand_HasInitSubcommand()
    {
        var root = NextNetCli.Create();
        var dbCmd = Assert.Single(root.Subcommands, c => c.Name == "db");
        Assert.Contains(dbCmd.Subcommands, c => c.Name == "init");
    }

    [Fact]
    public void Create_DbInitCommand_HasSqliteAndPostgreSqlSubcommands()
    {
        var root = NextNetCli.Create();
        var dbCmd = Assert.Single(root.Subcommands, c => c.Name == "db");
        var initCmd = Assert.Single(dbCmd.Subcommands, c => c.Name == "init");
        Assert.Contains(initCmd.Subcommands, c => c.Name == "sqlite");
        Assert.Contains(initCmd.Subcommands, c => c.Name == "postgresql");
    }

    [Fact]
    public void Create_HasGenerateCommand()
    {
        var root = NextNetCli.Create();
        Assert.Contains(root.Subcommands, c => c.Name == "generate");
    }

    [Fact]
    public void Create_GenerateCommand_HasModelSubcommand()
    {
        var root = NextNetCli.Create();
        var generateCmd = Assert.Single(root.Subcommands, c => c.Name == "generate");
        Assert.Contains(generateCmd.Subcommands, c => c.Name == "model");
    }

    [Fact]
    public void Create_GenerateCommand_HasRepositorySubcommand()
    {
        var root = NextNetCli.Create();
        var generateCmd = Assert.Single(root.Subcommands, c => c.Name == "generate");
        Assert.Contains(generateCmd.Subcommands, c => c.Name == "repository");
    }

    [Fact]
    public void Create_GenerateCommand_HasCrudSubcommand()
    {
        var root = NextNetCli.Create();
        var generateCmd = Assert.Single(root.Subcommands, c => c.Name == "generate");
        Assert.Contains(generateCmd.Subcommands, c => c.Name == "crud");
    }

    [Fact]
    public void Create_DbCommand_HasMigrateSubcommand()
    {
        var root = NextNetCli.Create();
        var dbCmd = Assert.Single(root.Subcommands, c => c.Name == "db");
        Assert.Contains(dbCmd.Subcommands, c => c.Name == "migrate");
    }

    [Fact]
    public void Create_DbCommand_HasRollbackSubcommand()
    {
        var root = NextNetCli.Create();
        var dbCmd = Assert.Single(root.Subcommands, c => c.Name == "db");
        Assert.Contains(dbCmd.Subcommands, c => c.Name == "rollback");
    }

    [Fact]
    public void Create_DbCommand_HasMigrationSubcommand()
    {
        var root = NextNetCli.Create();
        var dbCmd = Assert.Single(root.Subcommands, c => c.Name == "db");
        Assert.Contains(dbCmd.Subcommands, c => c.Name == "migration");
    }

    [Fact]
    public void Create_DbMigrationCommand_HasAddSubcommand()
    {
        var root = NextNetCli.Create();
        var dbCmd = Assert.Single(root.Subcommands, c => c.Name == "db");
        var migrationCmd = Assert.Single(dbCmd.Subcommands, c => c.Name == "migration");
        Assert.Contains(migrationCmd.Subcommands, c => c.Name == "add");
    }

    [Fact]
    public void Create_DbMigrationCommand_HasStatusSubcommand()
    {
        var root = NextNetCli.Create();
        var dbCmd = Assert.Single(root.Subcommands, c => c.Name == "db");
        var migrationCmd = Assert.Single(dbCmd.Subcommands, c => c.Name == "migration");
        Assert.Contains(migrationCmd.Subcommands, c => c.Name == "status");
    }

    [Fact]
    public void Create_GenerateCommand_HasAdminSubcommand()
    {
        var root = NextNetCli.Create();
        var generateCmd = Assert.Single(root.Subcommands, c => c.Name == "generate");
        Assert.Contains(generateCmd.Subcommands, c => c.Name == "admin");
    }

    [Fact]
    public void Create_DbCommand_HasExploreSubcommand()
    {
        var root = NextNetCli.Create();
        var dbCmd = Assert.Single(root.Subcommands, c => c.Name == "db");
        Assert.Contains(dbCmd.Subcommands, c => c.Name == "explore");
    }
}
