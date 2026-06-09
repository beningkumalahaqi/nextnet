using System.Diagnostics;

namespace NextNet.Data.EntityFramework.Internal;

/// <summary>
/// Wraps <see cref="Process"/> invocation for running <c>dotnet ef</c> commands.
/// </summary>
/// <remarks>
/// <para>
/// Provides a consistent way to shell out to the .NET CLI for EF Core migration
/// operations. Captures standard output and error streams for diagnostic purposes.
/// </para>
/// </remarks>
internal sealed class MigrationProcessRunner
{
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MigrationProcessRunner"/> class.
    /// </summary>
    /// <param name="logger">The logger for process diagnostics.</param>
    public MigrationProcessRunner(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Runs a process with the specified file name and arguments, capturing output.
    /// </summary>
    /// <param name="fileName">The executable file name (e.g., "dotnet").</param>
    /// <param name="arguments">The command-line arguments.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A tuple containing success flag, standard output, and standard error.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="fileName"/> is null or empty.</exception>
    public async Task<(bool Success, string? Output, string? Error)> RunProcessAsync(
        string fileName,
        string arguments,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentNullException(nameof(fileName), $"[{EntityFrameworkErrorCodes.ConfigurationInvalid}] Process file name must not be null or empty.");

        _logger.LogDebug("Running process: {FileName} {Arguments}", fileName, arguments);

        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };

        var outputBuilder = new System.Text.StringBuilder();
        var errorBuilder = new System.Text.StringBuilder();

        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data is not null)
                outputBuilder.AppendLine(e.Data);
        };

        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data is not null)
                errorBuilder.AppendLine(e.Data);
        };

        try
        {
            if (!process.Start())
            {
                return (false, null, "Failed to start process.");
            }

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync(cancellationToken);

            var output = outputBuilder.ToString();
            var error = errorBuilder.ToString();

            var success = process.ExitCode == 0;

            if (success)
            {
                _logger.LogDebug("Process completed successfully with exit code 0.");
            }
            else
            {
                _logger.LogWarning("Process failed with exit code {ExitCode}. Error: {Error}",
                    process.ExitCode, error);
            }

            return (success, output, error);
        }
        catch (OperationCanceledException)
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to run process '{FileName}'.", fileName);
            return (false, null, ex.Message);
        }
    }
}
