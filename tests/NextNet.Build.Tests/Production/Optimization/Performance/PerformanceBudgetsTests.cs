using NextNet.Build.Production.Optimization.Performance;
using NextNet.IO;
using Xunit;

namespace NextNet.Build.Tests.Production.Optimization.Performance;

public class PerformanceBudgetsTests
{
    [Fact]
    public void DefaultBudgets_Should_UseWarnAction_When_NewInstance()
    {
        var budgets = new PerformanceBudgets();
        Assert.Equal(BudgetViolationAction.Warn, budgets.Action);
    }

    [Fact]
    public void BudgetValues_Should_DefaultToNull_When_NewInstance()
    {
        var budgets = new PerformanceBudgets();
        Assert.Null(budgets.TotalSize);
        Assert.Null(budgets.JavaScriptSize);
        Assert.Null(budgets.CssSize);
        Assert.Null(budgets.ImageSize);
    }
}

public class PerformanceBudgetEvaluatorTests
{
    [Fact]
    public async Task EvaluateAsync_Should_Pass_When_NoBudgetsSet()
    {
        var fs = new DefaultSharpFileSystem();
        var evaluator = new PerformanceBudgetEvaluator(fs);

        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        try
        {
            await File.WriteAllTextAsync(Path.Combine(tempDir, "test.js"), "var x = 1;");
            await File.WriteAllTextAsync(Path.Combine(tempDir, "test.css"), "body { color: red; }");

            var budgets = new PerformanceBudgets();
            var report = await evaluator.EvaluateAsync(tempDir, budgets);

            Assert.True(report.Passed);
            Assert.Empty(report.Violations);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task EvaluateAsync_Should_ReturnViolation_When_ExceedsTotalSize()
    {
        var fs = new DefaultSharpFileSystem();
        var evaluator = new PerformanceBudgetEvaluator(fs);

        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        try
        {
            await File.WriteAllTextAsync(Path.Combine(tempDir, "big.js"), new string('x', 1000));

            var budgets = new PerformanceBudgets
            {
                TotalSize = 1,
                Action = BudgetViolationAction.Warn,
            };

            var report = await evaluator.EvaluateAsync(tempDir, budgets);

            Assert.False(report.Passed);
            Assert.NotEmpty(report.Violations);
            Assert.Contains(report.Violations, v => v.Metric == "TotalSize");
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task EvaluateAsync_Should_ReturnNoViolations_When_UnderBudget()
    {
        var fs = new DefaultSharpFileSystem();
        var evaluator = new PerformanceBudgetEvaluator(fs);

        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        try
        {
            await File.WriteAllTextAsync(Path.Combine(tempDir, "small.js"), "x=1;");

            var budgets = new PerformanceBudgets
            {
                JavaScriptSize = 1024 * 100,
                Action = BudgetViolationAction.Fail,
            };

            var report = await evaluator.EvaluateAsync(tempDir, budgets);

            Assert.True(report.Passed);
            Assert.Empty(report.Violations);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task EvaluateAsync_Should_NotPass_When_FailActionExceeded()
    {
        var fs = new DefaultSharpFileSystem();
        var evaluator = new PerformanceBudgetEvaluator(fs);

        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        try
        {
            await File.WriteAllTextAsync(Path.Combine(tempDir, "test.js"), new string('y', 5000));

            var budgets = new PerformanceBudgets
            {
                JavaScriptSize = 100,
                Action = BudgetViolationAction.Fail,
            };

            var report = await evaluator.EvaluateAsync(tempDir, budgets);

            Assert.False(report.Passed);
            Assert.NotEmpty(report.Violations);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}
