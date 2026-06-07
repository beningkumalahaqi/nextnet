using System.Diagnostics;

namespace NextNet.Cli.Services;

/// <summary>
/// Wraps the <c>dotnet</c> CLI for programmatic invocation of NuGet package
/// operations such as <c>dotnet add package</c>.
/// </summary>
public static class DotNetCliService
{
    /// <summary>
    /// Adds a NuGet package reference to the specified project.
    /// </summary>
    /// <param name="packageId">The NuGet package ID to add.</param>
    /// <param name="projectPath">Optional path to the project file (.csproj). Defaults to auto-discovery.</param>
    /// <param name="version">Optional package version constraint.</param>
    /// <param name="timeoutMs">Timeout in milliseconds (default 120000).</param>
    /// <returns>True if the package was added successfully.</returns>
    public static async Task<bool> AddPackageAsync(
        string packageId,
        string? projectPath = null,
        string? version = null,
        int timeoutMs = 120_000)
    {
        var args = $"add package {packageId}";

        if (!string.IsNullOrEmpty(version))
            args += $" --version {version}";

        if (!string.IsNullOrEmpty(projectPath))
            args += $" --project \"{projectPath}\"";

        return await RunDotNetCommandAsync(args, timeoutMs);
    }

    /// <summary>
    /// Adds a NuGet package reference with a specific version to the project.
    /// </summary>
    public static async Task<bool> AddPackageWithVersionAsync(
        string packageId,
        string version,
        string? projectPath = null,
        int timeoutMs = 120_000)
    {
        return await AddPackageAsync(packageId, projectPath, version, timeoutMs);
    }

    /// <summary>
    /// Runs a raw <c>dotnet</c> command and returns true if the exit code is 0.
    /// </summary>
    /// <param name="arguments">The command-line arguments to pass to dotnet.</param>
    /// <param name="timeoutMs">Timeout in milliseconds.</param>
    /// <param name="workingDirectory">Optional working directory.</param>
    /// <returns>True if the process exits with code 0.</returns>
    public static async Task<bool> RunDotNetCommandAsync(
        string arguments,
        int timeoutMs = 120_000,
        string? workingDirectory = null)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = workingDirectory ?? Environment.CurrentDirectory
            };

            using var process = new Process { StartInfo = psi };
            process.Start();

            var stdoutTask = process.StandardOutput.ReadToEndAsync();
            var stderrTask = process.StandardError.ReadToEndAsync();

            var exited = process.WaitForExit(timeoutMs);

            if (!exited)
            {
                process.Kill(entireProcessTree: true);
                return false;
            }

            await Task.WhenAll(stdoutTask, stderrTask);
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks whether the <c>dotnet</c> CLI is available on the current system PATH.
    /// </summary>
    public static bool IsDotNetSdkAvailable()
    {
        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = "--version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            process.StandardOutput.ReadToEnd();
            process.WaitForExit(3000);
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
}
