using Spectre.Console;

namespace NextNet.Cli.UI;

/// <summary>
/// Multi-step progress indicator with sub-task support.
/// Wraps Spectre.Console's <see cref="Progress"/> with NextNet styling.
/// </summary>
public sealed class NextNetProgress
{
    private readonly OutputMode _mode;

    internal NextNetProgress(OutputMode mode)
    {
        _mode = mode;
    }

    /// <summary>
    /// Start a multi-step progress display. The callback receives a
    /// <see cref="NextNetProgressContext"/> for adding and tracking tasks.
    /// </summary>
    /// <param name="console">The console to render to.</param>
    /// <param name="action">The async action that adds and manages steps.</param>
    /// <param name="autoClear">Whether to auto-clear the progress display when complete.</param>
    public static async Task RunAsync(
        NextNetConsole console,
        Func<NextNetProgressContext, Task> action,
        bool autoClear = false)
    {
        if (console.IsPlain)
        {
            var ctx = new NextNetProgressContext(console.Mode);
            await action(ctx);
            return;
        }

        var progress = new Progress(console.SpectreConsole)
        { AutoClear = autoClear, AutoRefresh = true };
        progress.Columns(new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn(), new ElapsedTimeColumn());

        await progress.StartAsync(async ctx =>
        {
            var wrappedCtx = new NextNetProgressContext(console.Mode, ctx);
            await action(wrappedCtx);
        });
    }
}

/// <summary>
/// Context for adding and managing progress steps within a <see cref="NextNetProgress"/> display.
/// </summary>
public sealed class NextNetProgressContext
{
    private readonly OutputMode _mode;
    private readonly ProgressContext? _spectreContext;
    private readonly List<NextNetProgressTask> _tasks = new();

    internal NextNetProgressContext(OutputMode mode, ProgressContext? spectreContext = null)
    {
        _mode = mode;
        _spectreContext = spectreContext;
    }

    /// <summary>Add a top-level build step.</summary>
    public NextNetProgressTask AddStep(string name)
    {
        ProgressTask? spectreTask = null;
        if (_spectreContext is not null)
        {
            spectreTask = _spectreContext.AddTask(name, autoStart: false);
        }

        var task = new NextNetProgressTask(name, _mode, spectreTask);
        _tasks.Add(task);
        return task;
    }

    /// <summary>Add a sub-task under a parent step.</summary>
    public NextNetProgressTask AddSubTask(NextNetProgressTask parent, string name)
    {
        ProgressTask? spectreTask = null;
        if (_spectreContext is not null)
        {
            spectreTask = _spectreContext.AddTask(name, autoStart: false);
        }

        var task = new NextNetProgressTask(name, _mode, spectreTask);
        parent.AddSubTask(task);
        return task;
    }
}

/// <summary>
/// Represents a single step within a multi-step progress display.
/// Provides methods for updating status, marking completion, and reporting errors.
/// </summary>
public sealed class NextNetProgressTask
{
    private readonly string _name;
    private readonly OutputMode _mode;
    private readonly List<NextNetProgressTask> _subTasks = new();
    private readonly ProgressTask? _spectreTask;

    internal NextNetProgressTask(string name, OutputMode mode, ProgressTask? spectreTask)
    {
        _name = name;
        _mode = mode;
        _spectreTask = spectreTask;
    }

    internal void AddSubTask(NextNetProgressTask task) => _subTasks.Add(task);

    /// <summary>Update the status message for this step.</summary>
    public void UpdateStatus(string message)
    {
        if (_mode == OutputMode.Color && _spectreTask is not null)
        {
            _spectreTask.Description = $"[{Theme.NextNetTealHex}]{_name}[/]: {message}";
        }
    }

    /// <summary>Increment progress by a value between 0.0 and 1.0.</summary>
    public void Increment(double value)
    {
        if (_spectreTask is not null)
            _spectreTask.Increment(value);
    }

    /// <summary>Mark this step as complete with an optional summary.</summary>
    public void MarkComplete(string? summary = null)
    {
        var text = summary is not null ? $"{_name} ({summary})" : _name;
        if (_mode == OutputMode.Plain)
        {
            System.Console.Error.WriteLine($"[OK] {text}");
        }
        else if (_spectreTask is not null)
        {
            _spectreTask.Description = $"[bold {Theme.SuccessHex}]\u2713[/] [{Theme.SuccessHex}]{text}[/]";
            _spectreTask.Value = 100;
        }
    }

    /// <summary>Mark this step as failed with an error message.</summary>
    public void MarkError(string error)
    {
        if (_mode == OutputMode.Plain)
        {
            System.Console.Error.WriteLine($"[ERR] {_name}: {error}");
        }
        else if (_spectreTask is not null)
        {
            _spectreTask.Description = $"[bold {Theme.ErrorHex}]\u2717[/] [{Theme.ErrorHex}]{_name}: {error}[/]";
            _spectreTask.Value = 0;
        }
    }

    /// <summary>Whether this task is indeterminate (spinner mode).</summary>
    public bool IsIndeterminate
    {
        get => _spectreTask?.IsIndeterminate ?? false;
        set
        {
            if (_spectreTask is not null)
                _spectreTask.IsIndeterminate = value;
        }
    }
}
