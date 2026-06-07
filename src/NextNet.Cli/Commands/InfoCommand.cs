using NextNet.Cli.Config;
using NextNet.Cli.UI;
using NextNet.Conventions;
using NextNet.Routing;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace NextNet.Cli.Commands;

/// <summary>
/// Implements the <c>nextnet info</c> command — displays the NextNet environment,
/// .NET SDK version, project configuration, layout chain, API routes, and SSG config.
/// </summary>
public static class InfoCommand
{
    /// <summary>
    /// Register the <c>info</c> command with System.CommandLine.
    /// </summary>
    public static Command Create()
    {
        var command = new Command("info", "Show NextNet environment and project information");

        Handler.SetHandler(command, (InvocationContext context) =>
        {
            var plain = context.ParseResult.GetValueForOption(NextNetCli.PlainOption);
            var noColor = context.ParseResult.GetValueForOption(NextNetCli.NoColorOption);
            var verbose = context.ParseResult.GetValueForOption(NextNetCli.VerboseOption);
            var exitCode = Execute(plain, noColor, verbose);
            context.ExitCode = exitCode;
        });

        return command;
    }

    /// <summary>
    /// Execute the <c>nextnet info</c> command logic.
    /// </summary>
    public static int Execute(bool plain = false, bool noColor = false, bool verbose = false)
    {
        var console = UI.NextNetConsole.Create(plain, noColor);

        try
        {
            var cliVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.1.0";
            var dotnetVersion = DetectDotNetSdkVersion();
            var osDescription = System.Runtime.InteropServices.RuntimeInformation.OSDescription;
            var osArch = System.Runtime.InteropServices.RuntimeInformation.OSArchitecture;

            var tree = console.CreateTree("NextNet Environment");

            // ── SDK info ──────────────────────────────────────────────
            var sdkNode = tree.AddNode($"NextNet:      v{cliVersion}");
            sdkNode.AddChild($".NET SDK:      {dotnetVersion}");
            sdkNode.AddChild($"Runtime:       {Environment.Version}");
            sdkNode.AddChild($"OS:            {osDescription} ({osArch})");

            // ── Project config ────────────────────────────────────────
            var config = ConfigLoader.Load();
            if (config is not null)
            {
                var projectNode = tree.AddNode($"Project: {config.Name ?? "unknown"}");
                projectNode.AddChild($"Config:        nextnet.config.json \u2713");
                projectNode.AddChild($"SSR:           {(config.Framework?.Ssr == true ? "enabled" : "disabled")}");
                projectNode.AddChild($"SSG:           {(config.Framework?.Ssg == true ? "enabled" : "disabled")}");
                projectNode.AddChild($"Output:        {config.Build?.Output ?? "dist"}");
                projectNode.AddChild($"App dir:       {config.Routing?.Dir ?? "app"}");

                // ── SSG config ────────────────────────────────────────
                if (config.Ssg is not null)
                {
                    var ssgNode = projectNode.AddChild("SSG Configuration:");
                    ssgNode.AddChild($"Output:        {config.Ssg.Output ?? "(default)"}");
                    ssgNode.AddChild($"Minify:        {config.Ssg.Minify?.ToString() ?? "(default true)"}");
                    ssgNode.AddChild($"Gzip:          {config.Ssg.Gzip?.ToString() ?? "(default true)"}");
                    ssgNode.AddChild($"Manifest:      {config.Ssg.GenerateManifest?.ToString() ?? "(default true)"}");
                    ssgNode.AddChild($"Clean output:  {config.Ssg.CleanOutput?.ToString() ?? "(default true)"}");
                    if (config.Ssg.ExcludePaths is { Length: > 0 })
                        ssgNode.AddChild($"Exclude:       {string.Join(", ", config.Ssg.ExcludePaths)}");
                }

                // ── Layout chain info ─────────────────────────────────
                ShowLayoutChain(console, tree, config);

                // ── API routes ────────────────────────────────────────
                ShowApiRoutes(console, tree, config);
            }
            else
            {
                tree.AddNode("Project:      (no nextnet.config.json found)");
            }

            console.Write(tree.Build());
            return 0;
        }
        catch (Exception ex)
        {
            console.WriteError($"Failed to get info: {ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// Shows the layout chain for the current project by scanning route files.
    /// </summary>
    private static void ShowLayoutChain(UI.NextNetConsole console, NextNetTree tree, NextNetProjectConfig config)
    {
        try
        {
            var appDir = config.Routing?.Dir ?? NextNetConventions.AppDirectory;
            var appDirAbsolute = Path.GetFullPath(appDir);

            if (!Directory.Exists(appDirAbsolute))
            {
                tree.AddNode("Layout chain:  (app directory not found)");
                return;
            }

            var scanner = new RouteScanner(appDirAbsolute);
            var manifest = scanner.Scan();

            if (manifest.Layouts.Count == 0)
            {
                tree.AddNode("Layout chain:  (no layouts found)");
                return;
            }

            if (manifest.Pages.Count == 0)
            {
                tree.AddNode("Layout chain:  (no pages found)");
                return;
            }

            var layoutNode = tree.AddNode("Layout Chain:");
            foreach (var page in manifest.Pages)
            {
                var pageNode = layoutNode.AddChild($"Page: {page.RoutePattern}");
                if (page.LayoutChain.Count > 0)
                {
                    foreach (var layoutPath in page.LayoutChain)
                    {
                        pageNode.AddChild($"  Layout: {layoutPath}");
                    }
                }
                else
                {
                    pageNode.AddChild("  (no layouts)");
                }
            }
        }
        catch (Exception ex)
        {
            tree.AddNode($"Layout chain:  (error: {ex.Message})");
        }
    }

    /// <summary>
    /// Shows discovered API routes from the route manifest.
    /// </summary>
    private static void ShowApiRoutes(UI.NextNetConsole console, NextNetTree tree, NextNetProjectConfig config)
    {
        try
        {
            var appDir = config.Routing?.Dir ?? NextNetConventions.AppDirectory;
            var appDirAbsolute = Path.GetFullPath(appDir);

            if (!Directory.Exists(appDirAbsolute))
            {
                tree.AddNode("API Routes:    (app directory not found)");
                return;
            }

            var apiScanner = new ApiRouteScanner(appDirAbsolute);
            var apiRoutes = apiScanner.Scan();

            if (apiRoutes.Count == 0)
            {
                tree.AddNode("API Routes:    (none discovered)");
                return;
            }

            var apiNode = tree.AddNode("API Routes:");
            foreach (var route in apiRoutes)
            {
                var methods = route.HttpMethods.Count > 0
                    ? string.Join(", ", route.HttpMethods)
                    : "GET, POST, PUT, PATCH, DELETE";
                apiNode.AddChild($"{route.RoutePattern} [{methods}]");
            }
        }
        catch (Exception ex)
        {
            tree.AddNode($"API Routes:    (error: {ex.Message})");
        }
    }

    private static string DetectDotNetSdkVersion()
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
            if (process is null) return "not detected";
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit(5000);
            return process.ExitCode == 0 ? output.Trim() : "not detected";
        }
        catch
        {
            return "not detected";
        }
    }
}
