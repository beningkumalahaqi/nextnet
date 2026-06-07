using NextNet.Cli.Config;
using NextNet.Cli.UI;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace NextNet.Cli.Commands;

/// <summary>
/// Implements the <c>nextnet doctor</c> command — diagnoses SDK installation,
/// configuration validity, and project structure.
/// </summary>
public static class DoctorCommand
{
    public static Command Create()
    {
        var fixOption = new Option<bool>("--fix", "Attempt to auto-fix issues");

        var command = new Command("doctor", "Diagnose NextNet environment and project issues")
        {
            fixOption
        };

        Handler.SetHandler(command, (InvocationContext context) =>
        {
            var fix = context.ParseResult.GetValueForOption(fixOption);
            var plain = context.ParseResult.GetValueForOption(NextNetCli.PlainOption);
            var noColor = context.ParseResult.GetValueForOption(NextNetCli.NoColorOption);
            var verbose = context.ParseResult.GetValueForOption(NextNetCli.VerboseOption);
            var exitCode = Execute(fix, plain, noColor, verbose);
            context.ExitCode = exitCode;
        });

        return command;
    }

    public static int Execute(bool fix = false, bool plain = false, bool noColor = false, bool verbose = false)
    {
        var console = NextNetConsole.Create(plain, noColor);
        var issues = new List<(bool IsError, string Message)>();
        var warnings = new List<string>();

        console.WriteHeading("NextNet Doctor");
        console.WriteLine();

        // Check 1: .NET SDK
        console.WriteLine("Checking .NET SDK...");
        var dotnetVersion = DetectDotNetSdkVersion();
        if (dotnetVersion is not null)
        {
            console.WriteSuccess($".NET SDK installed (v{dotnetVersion})");
        }
        else
        {
            issues.Add((true, ".NET SDK not found"));
            console.WriteError(".NET SDK not found");
            warnings.Add("Install .NET SDK from https://dot.net/download");
        }

        // Check 2: NextNet CLI version
        console.WriteLine("Checking NextNet CLI...");
        var cliVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.1.0";
        console.WriteSuccess($"NextNet CLI v{cliVersion}");

        // Check 3: Configuration file
        console.WriteLine("Checking configuration...");
        var config = ConfigLoader.Load();
        if (config is not null)
        {
            var configIssues = ConfigValidator.Validate(config);
            if (configIssues.Count == 0)
            {
                console.WriteSuccess("nextnet.config.json valid");
            }
            else
            {
                foreach (var ci in configIssues)
                {
                    if (ci.Severity == ConfigIssueSeverity.Error)
                    {
                        issues.Add((true, ci.Message));
                        console.WriteError(ci.Message);
                    }
                    else
                    {
                        warnings.Add(ci.Message);
                        console.WriteWarning(ci.Message);
                    }
                }
            }
        }
        else
        {
            issues.Add((false, "nextnet.config.json not found (not in a NextNet project)"));
            console.WriteMuted("nextnet.config.json not found");
        }

        // Check 4: Project structure
        console.WriteLine("Checking project structure...");
        if (config is not null && config.Routing is not null)
        {
            var appDir = Path.Combine(Environment.CurrentDirectory, config.Routing.Dir);
            if (Directory.Exists(appDir))
            {
                console.WriteSuccess($"Project structure valid ({config.Routing.Dir}/ exists)");
            }
            else
            {
                warnings.Add($"App directory '{config.Routing.Dir}/' not found");
                console.WriteWarning($"App directory '{config.Routing.Dir}/' not found");
            }
        }

        console.WriteLine();

        // Summary
        if (issues.Count == 0 && warnings.Count == 0)
        {
            console.WriteSuccess("All checks passed!");
        }
        else
        {
            if (issues.Count > 0)
                console.WriteError($"Issues found: {issues.Count} error(s)");
            if (warnings.Count > 0)
                console.WriteWarning($"Warnings: {warnings.Count}");

            console.WriteLine();
            console.WriteLine("Fix suggestions:");
            foreach (var w in warnings)
                console.WriteMuted($"  \u2022 {w}");

            if (fix && issues.Count > 0)
            {
                console.WriteLine();
                console.WriteInfo("Auto-fix mode not yet implemented. Please fix manually.");
            }
        }

        return issues.Count > 0 ? 1 : 0;
    }

    private static string? DetectDotNetSdkVersion()
    {
        try
        {
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "--version",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var process = System.Diagnostics.Process.Start(psi);
            if (process is null) return null;
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit(5000);
            return process.ExitCode == 0 ? output.Trim() : null;
        }
        catch
        {
            return null;
        }
    }
}
