using NextNet.Cli.UI;
using Xunit;

namespace NextNet.Cli.Tests.UI;

public class NextNetTableTests
{
    [Fact]
    public void BuildSummary_CreatesTable_WithStepAndDuration()
    {
        var table = NextNetTable.BuildSummary();
        Assert.NotNull(table);
        var spectreTable = table.GetSpectreTable();
        Assert.NotNull(spectreTable);
    }

    [Fact]
    public void BuildSummary_PlainMode_CreatesTable()
    {
        var table = NextNetTable.BuildSummary(OutputMode.Plain);
        Assert.NotNull(table);
    }

    [Fact]
    public void Routes_CreatesTable_WithCorrectColumns()
    {
        var table = NextNetTable.Routes();
        Assert.NotNull(table);
    }

    [Fact]
    public void Plugins_CreatesTable_WithCorrectColumns()
    {
        var table = NextNetTable.Plugins();
        Assert.NotNull(table);
    }

    [Fact]
    public void Info_CreatesTable_WithCorrectColumns()
    {
        var table = NextNetTable.Info();
        Assert.NotNull(table);
    }

    [Fact]
    public void OutputFiles_CreatesTable_WithCorrectColumns()
    {
        var table = NextNetTable.OutputFiles();
        Assert.NotNull(table);
    }

    [Fact]
    public void AddRow_StringValues_DoesNotThrow()
    {
        var table = NextNetTable.BuildSummary();
        table.AddRow("Step 1", "12ms");
        table.AddRow("Step 2", "45ms");
    }

    [Fact]
    public void AddSeparator_DoesNotThrow()
    {
        var table = NextNetTable.BuildSummary();
        table.AddSeparator();
    }

    [Fact]
    public void AddTotalRow_DoesNotThrow()
    {
        var table = NextNetTable.BuildSummary();
        table.AddRow("Test", "100ms");
        table.AddTotalRow("Total", "100ms");
    }

    [Fact]
    public void AddStepRow_DoesNotThrow()
    {
        var table = NextNetTable.BuildSummary();
        table.AddStepRow("Compilation", "1.2s", completed: true);
        table.AddStepRow("Failed Step", "0ms", completed: false);
    }

    [Fact]
    public void Table_PlainMode_AddRow_DoesNotThrow()
    {
        var table = NextNetTable.BuildSummary(OutputMode.Plain);
        table.AddRow("Step 1", "12ms");
    }
}
