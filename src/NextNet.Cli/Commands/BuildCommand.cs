using NextNet.Cli.Build;
using NextNet.Cli.Config;
using NextNet.Cli.UI;
using NextNet.Cli.UI.Messages;
using NextNet.Cli.Errors;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace NextNet.Cli.Commands;

/// <summary>
/// Implements the <c>nextnet build</c> command — runs the SSG build pipeline
/// with progress display and summary output.
/// </summary>
public static class BuildCommand
{
    public static Command Create()
    {
        var outputOption = new Option<string>("--output", () => "dist", "Output directory");
        outputOption.AddAlias("-o");
        var minifyOption = new Option<bool>("--minify", () => true, "Minify output");
        var noMinifyOption = new Option<bool>("--no-minify", "Disable HTML minification");
        var noGzipOption = new Option<bool>("--no-gzip", "Disable gzip compression");
        var sourcemapOption = new Option<bool>("--sourcemap", "Generate source maps");
        var command = new Command("build", "Build the NextNet project for production")
        {
            outputOption,
            minifyOption,
            noMinifyOption,
            noGzipOption,
            sourcemapOption
        };

        Handler.SetHandler(command, (InvocationContext context) =>
        {
            var output = context.ParseResult.GetValueForOption(outputOption) ?? "dist";
            var minify = context.ParseResult.GetValueForOption(minifyOption);
            var noMinify = context.ParseResult.GetValueForOption(noMinifyOption);
            var noGzip = context.ParseResult.GetValueForOption(noGzipOption);
            var sourcemap = context.ParseResult.GetValueForOption(sourcemapOption);
            var plain = context.ParseResult.GetValueForOption(NextNetCli.PlainOption);
            var noColor = context.ParseResult.GetValueForOption(NextNetCli.NoColorOption);
            var verbose = context.ParseResult.GetValueForOption(NextNetCli.VerboseOption);

            var exitCode = ExecuteAsync(output, minify, noMinify, noGzip, sourcemap, verbose, plain, noColor)
                .GetAwaiter().GetResult();
            context.ExitCode = exitCode;
        });

        return command;
    }

    public static async Task<int> ExecuteAsync(
        string output = "dist",
        bool minify = true,
        bool noMinify = false,
        bool noGzip = false,
        bool sourcemap = false,
        bool verbose = false,
        bool plain = false,
        bool noColor = false)
    {
        var console = NextNetConsole.Create(plain, noColor);

        try
        {
            // Load config to determine effective output directory
            var config = ConfigLoader.Load();
            var effectiveMinify = minify && !noMinify;
            var outputDir = config?.Build?.Output ?? output;

            // If ssg section has output override, use that
            if (config?.Ssg?.Output is not null)
                outputDir = config.Ssg.Output;

            var runner = new BuildRunner(console, outputDir, effectiveMinify, noGzip, verbose);
            return await runner.RunAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage.WriteSimple(console, ex.Message, verbose ? ex : null);
            return 4;
        }
    }
}
