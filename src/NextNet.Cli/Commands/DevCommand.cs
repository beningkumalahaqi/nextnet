using NextNet.Cli.Commands.Dev;
using NextNet.Cli.Config;
using NextNet.Cli.Errors;
using NextNet.Cli.UI;
using NextNet.Cli.UI.Messages;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace NextNet.Cli.Commands;

/// <summary>
/// Implements the <c>nextnet dev</c> command — starts the development server
/// with ASP.NET Core hosting, SSR middleware, file watching, and HMR.
/// </summary>
public static class DevCommand
{
    public static Command Create()
    {
        var portOption = new Option<int>("--port", () => 3000, "Port number");
        portOption.AddAlias("-p");
        var httpsOption = new Option<bool>("--https", "Enable HTTPS");
        var hostnameOption = new Option<string>("--hostname", () => "localhost", "Bind hostname");
        var noHmrOption = new Option<bool>("--no-hmr", "Disable hot module replacement");

        var command = new Command("dev", "Start the development server with HMR")
        {
            portOption,
            httpsOption,
            hostnameOption,
            noHmrOption
        };

        Handler.SetHandler(command, (InvocationContext context) =>
        {
            var port = context.ParseResult.GetValueForOption(portOption);
            var https = context.ParseResult.GetValueForOption(httpsOption);
            var hostname = context.ParseResult.GetValueForOption(hostnameOption) ?? "localhost";
            var noHmr = context.ParseResult.GetValueForOption(noHmrOption);
            var plain = context.ParseResult.GetValueForOption(NextNetCli.PlainOption);
            var noColor = context.ParseResult.GetValueForOption(NextNetCli.NoColorOption);
            var verbose = context.ParseResult.GetValueForOption(NextNetCli.VerboseOption);

            // Load config to get default port if not specified
            var config = ConfigLoader.Load();
            if (config?.Dev?.Port > 0 && port == 3000)
                port = config.Dev.Port;

            var exitCode = ExecuteAsync(port, https, hostname, noHmr, verbose, plain, noColor).GetAwaiter().GetResult();
            context.ExitCode = exitCode;
        });

        return command;
    }

    public static async Task<int> ExecuteAsync(
        int port = 3000,
        bool https = false,
        string hostname = "localhost",
        bool noHmr = false,
        bool verbose = false,
        bool plain = false,
        bool noColor = false)
    {
        var console = NextNetConsole.Create(plain, noColor);

        try
        {
            var server = new DevServer(console, port, https, hostname, noHmr, verbose);
            return await server.StartAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage.WriteSimple(console, ex.Message, verbose ? ex : null);
            return 1;
        }
    }
}
