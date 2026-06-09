using System.Reflection;
using System.Text;

namespace NextNet.Data.PostgreSQL.Internal;

/// <summary>
/// Helper for reading and rendering the embedded Docker Compose template.
/// </summary>
/// <remarks>
/// <para>
/// The Docker Compose template is embedded as a managed resource in the
/// <c>NextNet.Data.PostgreSQL</c> assembly. The CLI uses this helper to
/// render and write the template to the project root.
/// </para>
/// </remarks>
internal static class DockerComposeTemplate
{
    private const string ResourceName = "NextNet.Data.PostgreSQL.Resources.docker-compose.postgres.yml";
    private const string ProjectNamePlaceholder = "{{ProjectName}}";

    /// <summary>
    /// Loads the raw Docker Compose template from the embedded resource.
    /// </summary>
    /// <returns>The raw YAML template string.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the embedded resource cannot be found.</exception>
    public static string LoadTemplate()
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(ResourceName);

        if (stream is null)
        {
            throw new InvalidOperationException(
                $"[DS-516] Embedded resource '{ResourceName}' not found in assembly '{assembly.FullName}'.");
        }

        using var reader = new StreamReader(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// Renders the Docker Compose template with the specified project name.
    /// Replaces the <c>{{ProjectName}}</c> placeholder with the actual project name.
    /// </summary>
    /// <param name="projectName">The project name to substitute into the template.</param>
    /// <returns>The rendered YAML string.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="projectName"/> is null or empty.</exception>
    public static string Render(string projectName)
    {
        if (string.IsNullOrWhiteSpace(projectName))
            throw new ArgumentException("Project name must not be null or empty.", nameof(projectName));

        var template = LoadTemplate();
        return template.Replace(ProjectNamePlaceholder, projectName);
    }

    /// <summary>
    /// Writes the rendered Docker Compose template to the specified project directory.
    /// Does not overwrite an existing file.
    /// </summary>
    /// <param name="projectDirectory">The project root directory.</param>
    /// <param name="projectName">The project name to substitute.</param>
    /// <returns><c>true</c> if the file was written; <c>false</c> if it already exists.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="projectDirectory"/> or <paramref name="projectName"/> is null or empty.</exception>
    /// <exception cref="DirectoryNotFoundException">Thrown when <paramref name="projectDirectory"/> does not exist.</exception>
    public static bool WriteToFile(string projectDirectory, string projectName)
    {
        if (string.IsNullOrWhiteSpace(projectDirectory))
            throw new ArgumentException("Project directory must not be null or empty.", nameof(projectDirectory));

        if (string.IsNullOrWhiteSpace(projectName))
            throw new ArgumentException("Project name must not be null or empty.", nameof(projectName));

        if (!Directory.Exists(projectDirectory))
            throw new DirectoryNotFoundException($"[DS-516] Project directory '{projectDirectory}' not found.");

        var filePath = Path.Combine(projectDirectory, "docker-compose.postgres.yml");

        if (File.Exists(filePath))
            return false;

        var content = Render(projectName);
        File.WriteAllText(filePath, content, Encoding.UTF8);
        return true;
    }

    /// <summary>
    /// Checks whether the Docker Compose file already exists in the specified project directory.
    /// </summary>
    /// <param name="projectDirectory">The project root directory.</param>
    /// <returns><c>true</c> if the file exists; <c>false</c> otherwise.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="projectDirectory"/> is null or empty.</exception>
    public static bool Exists(string projectDirectory)
    {
        if (string.IsNullOrWhiteSpace(projectDirectory))
            throw new ArgumentException("Project directory must not be null or empty.", nameof(projectDirectory));

        var filePath = Path.Combine(projectDirectory, "docker-compose.postgres.yml");
        return File.Exists(filePath);
    }
}
