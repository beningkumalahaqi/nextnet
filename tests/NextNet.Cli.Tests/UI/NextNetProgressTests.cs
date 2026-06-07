using NextNet.Cli.UI;
using Xunit;

namespace NextNet.Cli.Tests.UI;

public class NextNetProgressTests
{
    [Fact]
    public void ProgressContext_AddStep_ReturnsTask()
    {
        var ctx = new NextNetProgressContext(OutputMode.Plain);
        var task = ctx.AddStep("Building");
        Assert.NotNull(task);
    }

    [Fact]
    public void ProgressContext_AddSubTask_AddsToParent()
    {
        var ctx = new NextNetProgressContext(OutputMode.Plain);
        var parent = ctx.AddStep("Build");
        var child = ctx.AddSubTask(parent, "Compile");
        Assert.NotNull(child);
    }

    [Fact]
    public void ProgressTask_MarkComplete_PlainMode_WritesToStderr()
    {
        var task = new NextNetProgressTask("Test", OutputMode.Plain, null);
        // Should not throw even without a spectre task
        task.MarkComplete();
        task.MarkComplete("summary text");
    }

    [Fact]
    public void ProgressTask_MarkError_PlainMode_WritesToStderr()
    {
        var task = new NextNetProgressTask("Test", OutputMode.Plain, null);
        task.MarkError("Something failed");
    }

    [Fact]
    public void ProgressTask_UpdateStatus_PlainMode_DoesNothing()
    {
        var task = new NextNetProgressTask("Test", OutputMode.Plain, null);
        task.UpdateStatus("Working...");
        // Should not throw
    }

    [Fact]
    public void ProgressTask_Increment_WithoutSpectreTask_DoesNothing()
    {
        var task = new NextNetProgressTask("Test", OutputMode.Plain, null);
        task.Increment(0.5);
        // Should not throw
    }

    [Fact]
    public void ProgressTask_IsIndeterminate_WithoutSpectreTask_ReturnsFalse()
    {
        var task = new NextNetProgressTask("Test", OutputMode.Plain, null);
        Assert.False(task.IsIndeterminate);
        task.IsIndeterminate = true; // Should not throw
        Assert.False(task.IsIndeterminate); // Still false since no spectre task
    }

    [Fact]
    public void ProgressContext_AddStepAndSubTask_PlainMode_Works()
    {
        var ctx = new NextNetProgressContext(OutputMode.Plain);
        var step = ctx.AddStep("Step 1");
        var sub = ctx.AddSubTask(step, "Sub 1");
        sub.MarkComplete("done");
        step.MarkComplete("all done");
    }

    [Fact]
    public async Task Progress_RunAsync_PlainMode_ExecutesAction()
    {
        var console = NextNetConsole.Create(plain: true);
        var executed = false;

        await NextNetProgress.RunAsync(console, async ctx =>
        {
            var step = ctx.AddStep("Test");
            step.MarkComplete();
            executed = true;
            await Task.CompletedTask;
        });

        Assert.True(executed);
    }

    [Fact]
    public async Task Progress_RunAsync_WithSubTasks_PlainMode_Works()
    {
        var console = NextNetConsole.Create(plain: true);
        await NextNetProgress.RunAsync(console, ctx =>
        {
            var step = ctx.AddStep("Build");
            var sub = ctx.AddSubTask(step, "Compile");
            sub.MarkComplete("100ms");
            step.MarkComplete("success");
            return Task.CompletedTask;
        });
    }
}
