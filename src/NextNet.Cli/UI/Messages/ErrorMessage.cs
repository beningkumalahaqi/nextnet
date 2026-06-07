using NextNet.Cli.Errors;
using Spectre.Console;

namespace NextNet.Cli.UI.Messages;

/// <summary>
/// Formats and writes NextNet error messages with the standard error template:
/// <c>✗ Error [NN-XXX]: {message}</c> with context, usage, and examples.
/// All text content is escaped for Spectre.Console markup safety.
/// </summary>
public static class ErrorMessage
{
    /// <summary>
    /// Write a full formatted error with code, message, context, and suggestions.
    /// </summary>
    /// <param name="console">The console to write to.</param>
    /// <param name="entry">The error entry (code + message + hints).</param>
    /// <param name="customContext">Optional custom context to override the entry's default.</param>
    public static void Write(NextNetConsole console, ErrorEntry entry, string? customContext = null)
    {
        var prefix = console.IsPlain ? "[ERR]" : "✗";
        var code = entry.Code;
        var message = entry.Message.EscapeMarkup();
        var context = (customContext ?? entry.Context)?.EscapeMarkup();
        var usage = entry.Usage?.EscapeMarkup();
        var examples = entry.Examples;

        // Error header
        if (console.IsPlain)
        {
            console.WriteLine($"Error [{code}]: {entry.Message}");
        }
        else
        {
            console.Write(new Markup($"[bold {Theme.ErrorHex}]{prefix} Error [[{code}]][/]: [{Theme.ErrorHex}]{message}[/]"));
            console.WriteLine();
        }

        // Context detail
        if (context is not null)
        {
            if (console.IsPlain)
                console.WriteLine($"  {customContext ?? entry.Context}");
            else
                console.Write(new Markup($"  [{Theme.MutedHex}]{context}[/]"));
            console.WriteLine();
        }

        // Usage
        if (usage is not null)
        {
            console.WriteLine();
            if (console.IsPlain)
                console.WriteLine($"  Usage: {entry.Usage}");
            else
                console.Write(new Markup($"  [bold]Usage:[/] [{Theme.VioletHex}]{usage}[/]"));
            console.WriteLine();
        }

        // Examples
        if (examples is not null && examples.Length > 0)
        {
            if (console.IsPlain)
            {
                console.WriteLine("  Examples:");
                foreach (var ex in examples)
                    console.WriteLine($"    {ex}");
            }
            else
            {
                console.Write(new Markup($"  [bold]Examples:[/]"));
                console.WriteLine();
                foreach (var ex in examples)
                {
                    var escapedEx = ex.EscapeMarkup();
                    console.Write(new Markup($"    [{Theme.VioletHex}]{escapedEx}[/]"));
                    console.WriteLine();
                }
            }
        }

        // Docs link
        console.WriteLine();
        console.WriteLine($"See: https://nextnet.dev/docs/errors/{entry.Code}");
    }

    /// <summary>
    /// Write a simple/unexpected error without the full error template.
    /// </summary>
    /// <param name="console">The console to write to.</param>
    /// <param name="message">The error message.</param>
    /// <param name="exception">Optional exception details.</param>
    public static void WriteSimple(NextNetConsole console, string message, Exception? exception = null)
    {
        var prefix = console.IsPlain ? "[ERR]" : "✗";
        if (console.IsPlain)
        {
            console.WriteLine($"Error: {message}");
        }
        else
        {
            console.Write(new Markup($"[bold {Theme.ErrorHex}]{prefix} Error[/]: {message.EscapeMarkup()}"));
            console.WriteLine();
        }

        if (exception is not null)
        {
            if (console.IsPlain)
                console.WriteLine($"  {exception.Message}");
            else
                console.Write(new Markup($"  [{Theme.MutedHex}]{exception.Message.EscapeMarkup()}[/]"));
            console.WriteLine();
        }
    }
}
