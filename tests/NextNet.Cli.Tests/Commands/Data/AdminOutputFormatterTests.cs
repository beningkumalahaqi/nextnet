using NextNet.Cli.Commands.Data;
using NextNet.Data.Abstractions.Models;
using Xunit;

namespace NextNet.Cli.Tests.Commands.Data;

/// <summary>
/// Tests for the <see cref="AdminOutputFormatter"/> class.
/// </summary>
public class AdminOutputFormatterTests
{
    [Fact]
    public void FormatTableList_WithTables_ReturnsFormattedString()
    {
        var tables = new List<SchemaTableInfo>
        {
            new("Users", "dbo", "Table", 7, 142, false),
            new("Products", "dbo", "Table", 11, 2431, false),
        };

        var result = AdminOutputFormatter.FormatTableList(tables, "Default", "test.db");

        Assert.Contains("Connection: Default", result);
        Assert.Contains("Database:   test.db", result);
        Assert.Contains("Tables (2):", result);
        Assert.Contains("Users", result);
        Assert.Contains("Products", result);
    }

    [Fact]
    public void FormatTableList_WithViews_IncludesViews()
    {
        var tables = new List<SchemaTableInfo>
        {
            new("Users", null, "Table", 7, null, false),
            new("UserSummary", null, "View", 5, null, false),
        };

        var result = AdminOutputFormatter.FormatTableList(tables, "Default", null);

        Assert.Contains("Views (1):", result);
        Assert.Contains("UserSummary", result);
    }

    [Fact]
    public void FormatTableList_WithSystemTables_MarksThemSeparately()
    {
        var tables = new List<SchemaTableInfo>
        {
            new("Users", null, "Table", 7, null, false),
            new("__EFMigrationsHistory", null, "Table", 2, null, true),
        };

        var result = AdminOutputFormatter.FormatTableList(tables, "Default", null);

        Assert.Contains("System Tables (1):", result);
        Assert.Contains("__EFMigrationsHistory", result);
    }

    [Fact]
    public void FormatTableDetail_ReturnsFormattedDetail()
    {
        var columns = new List<SchemaColumnInfo>
        {
            new("Id", "INTEGER", false, true, null, null, true, null),
            new("Name", "TEXT", false, false, null, 200, false, null),
        };

        var detail = new SchemaTableDetail("Users", "dbo", "Table", columns);

        var result = AdminOutputFormatter.FormatTableDetail(detail);

        Assert.Contains("Table: Users", result);
        Assert.Contains("Schema:    dbo", result);
        Assert.Contains("Columns (2):", result);
        Assert.Contains("Id", result);
        Assert.Contains("Name", result);
    }

    [Fact]
    public void FormatTableDetail_WithForeignKeys_IncludesThem()
    {
        var columns = new List<SchemaColumnInfo>
        {
            new("Id", "INTEGER", false, true),
            new("CreatedBy", "INTEGER", false, false),
        };

        var fks = new List<SchemaForeignKey>
        {
            new("FK_Users_CreatedBy", "CreatedBy", "Admins", null, "Id", "CASCADE"),
        };

        var detail = new SchemaTableDetail("Users", null, "Table", columns, ForeignKeys: fks);

        var result = AdminOutputFormatter.FormatTableDetail(detail);

        Assert.Contains("Foreign Keys (1):", result);
        Assert.Contains("FK_Users_CreatedBy", result);
    }

    [Fact]
    public void FormatTableDetail_WithIndexes_IncludesThem()
    {
        var columns = new List<SchemaColumnInfo>
        {
            new("Id", "INTEGER", false, true),
            new("Email", "TEXT", false),
        };

        var indexes = new List<SchemaIndex>
        {
            new("PK_Users", new[] { "Id" }, true, true),
            new("IX_Users_Email", new[] { "Email" }, true, false),
        };

        var detail = new SchemaTableDetail("Users", null, "Table", columns, Indexes: indexes);

        var result = AdminOutputFormatter.FormatTableDetail(detail);

        Assert.Contains("Indexes (2):", result);
        Assert.Contains("PK_Users", result);
        Assert.Contains("IX_Users_Email", result);
    }

    [Fact]
    public void FormatJson_ReturnsValidJson()
    {
        var data = new { name = "test", value = 42 };

        var result = AdminOutputFormatter.FormatJson(data);

        Assert.Contains("\"name\"", result);
        Assert.Contains("\"test\"", result);
        Assert.Contains("42", result);
    }

    [Fact]
    public void BuildTreeView_ReturnsTreeWithNodes()
    {
        var tables = new List<SchemaTableInfo>
        {
            new("Users", null, "Table", 7, 142, false),
            new("Products", null, "Table", 11, null, false),
            new("UserSummary", null, "View", 5, null, false),
        };

        var tree = AdminOutputFormatter.BuildTreeView(tables, "Default");

        Assert.NotNull(tree);
    }
}
