using NextNet.Cli.UI;
using NextNet.DevTools;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace NextNet.Cli.Commands;

/// <summary>
/// Implements the <c>nextnet devtools</c> command — starts the DevTools TUI
/// in standalone mode for inspecting routes, components, and performance.
/// </summary>
public static class DevToolsCommand
{
    /// <summary>
    /// Creates the <c>devtools</c> command.
    /// </summary>
    public static Command Create()
    {
        var headlessOption = new Option<bool>("--headless", "Start in headless API mode instead of TUI");
        var portOption = new Option<int>("--port", () => 3001, "Port for headless API mode");

        var command = new Command("devtools", "Open the DevTools panel (route inspector, profiler, console)")
        {
            headlessOption,
            portOption
        };

        Handler.SetHandler(command, (InvocationContext context) =>
        {
            var headless = context.ParseResult.GetValueForOption(headlessOption);
            var port = context.ParseResult.GetValueForOption(portOption);
            var plain = context.ParseResult.GetValueForOption(NextNetCli.PlainOption);
            var noColor = context.ParseResult.GetValueForOption(NextNetCli.NoColorOption);
            var verbose = context.ParseResult.GetValueForOption(NextNetCli.VerboseOption);

            var exitCode = ExecuteAsync(headless, port, plain, noColor, verbose).GetAwaiter().GetResult();
            context.ExitCode = exitCode;
        });

        return command;
    }

    /// <summary>
    /// Execute the devtools command.
    /// </summary>
    public static async Task<int> ExecuteAsync(bool headless = false, int port = 3001, bool plain = false, bool noColor = false, bool verbose = false)
    {
        var options = new DevToolsOptions
        {
            Mode = headless ? DevToolsMode.Headless : DevToolsMode.Tui,
            Port = port
        };

        var server = new DevToolsServer(options);
        await server.StartAsync();

        if (options.Mode == DevToolsMode.Tui)
        {
            var console = NextNetConsole.Create(plain, noColor);
            console.WriteHeading("NextNet DevTools");
            console.WriteInfo("Starting TUI mode...");
            console.WriteLine();

            server.RunTuiLoop();
        }
        else
        {
            var console = NextNetConsole.Create(plain, noColor);
            console.WriteHeading("NextNet DevTools (Headless)");
            console.WriteInfo($"API running on http://localhost:{port}/__devtools");
            console.WriteInfo("WebSocket on ws://localhost:{port}/__devtools/ws");
            console.WriteLine();
            console.WriteMuted("Press Ctrl+C to stop.");
            console.WriteLine();

            // Keep running until cancelled
            try
            {
                await Task.Delay(Timeout.Infinite);
            }
            catch (TaskCanceledException)
            {
                // Graceful shutdown
            }
        }

        await server.StopAsync();
        return 0;
    }
}
