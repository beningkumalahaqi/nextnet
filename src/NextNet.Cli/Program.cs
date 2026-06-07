using System.CommandLine;
using NextNet.Cli;

/// <summary>
/// NextNet CLI entry point.
/// </summary>
public static class Program
{
    /// <summary>
    /// Main entry point — creates the CLI app and invokes it with the provided arguments.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    /// <returns>Exit code (0 = success, non-zero = error).</returns>
    public static async Task<int> Main(string[] args)
    {
        var app = NextNetCli.Create();
        return await app.InvokeAsync(args, console: null);
    }
}
