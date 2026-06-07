namespace NextNet.Cli.Community;

/// <summary>
/// Represents the outcome of a community template installation operation.
/// </summary>
/// <remarks>
/// <para>
/// Created by <see cref="CommunityTemplateManager.InstallAsync"/> to report
/// whether the installation succeeded or failed, along with contextual details.
/// </para>
/// <para>
/// Use the static factory methods <see cref="CreateSuccess"/> and <see cref="CreateFailure"/>
/// to construct instances rather than directly instantiating this record.
/// </para>
/// </remarks>
public sealed record InstallResult
{
    /// <summary>
    /// Gets a value indicating whether the installation completed successfully.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Gets a human-readable message describing the result (e.g., error detail on failure).
    /// </summary>
    public string? Message { get; init; }

    /// <summary>
    /// Gets the name of the template that was installed, if applicable.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Gets the version that was installed, if applicable.
    /// </summary>
    public string? Version { get; init; }

    /// <summary>
    /// Gets the filesystem path where the template was installed, if applicable.
    /// </summary>
    public string? InstallPath { get; init; }

    /// <summary>
    /// Creates a failure result with the given error message.
    /// </summary>
    /// <param name="message">A description of what went wrong.</param>
    /// <returns>An <see cref="InstallResult"/> with <see cref="Success"/> set to <c>false</c>.</returns>
    public static InstallResult CreateFailure(string message) => new() { Success = false, Message = message };

    /// <summary>
    /// Creates a success result with the given installation details.
    /// </summary>
    /// <param name="name">The name of the installed template.</param>
    /// <param name="version">The version that was installed.</param>
    /// <param name="path">The filesystem path where the template was installed.</param>
    /// <returns>An <see cref="InstallResult"/> with <see cref="Success"/> set to <c>true</c>.</returns>
    public static InstallResult CreateSuccess(string name, string version, string path)
        => new() { Success = true, Name = name, Version = version, InstallPath = path };
}
