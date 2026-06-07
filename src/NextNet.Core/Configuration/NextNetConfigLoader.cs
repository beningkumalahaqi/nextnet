using System.Text.Json;
using NextNet.Exceptions;
using NextNet.IO;

namespace NextNet.Configuration;

/// <summary>
/// Loads <see cref="NextNetConfig"/> from the file system by searching
/// upward from a starting directory for <c>nextnet.config.json</c>.
/// Returns default configuration if no config file is found.
/// </summary>
public class NextNetConfigLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    private readonly ISharpFileSystem _fileSystem;

    /// <summary>
    /// Initializes a new instance of <see cref="NextNetConfigLoader"/>
    /// using the default file system.
    /// </summary>
    public NextNetConfigLoader()
        : this(new DefaultSharpFileSystem())
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="NextNetConfigLoader"/>
    /// with a specified file system abstraction.
    /// </summary>
    /// <param name="fileSystem">The file system abstraction to use.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="fileSystem"/> is <c>null</c>.</exception>
    public NextNetConfigLoader(ISharpFileSystem fileSystem)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }

    /// <summary>
    /// Loads configuration by searching upward from <paramref name="startDirectory"/>
    /// for a <c>nextnet.config.json</c> file.
    /// </summary>
    /// <param name="startDirectory">
    /// The directory to start searching from. If <c>null</c>, the current working directory is used.
    /// </param>
    /// <returns>A <see cref="NextNetConfig"/> instance with values from the file or defaults.</returns>
    /// <exception cref="NextNetConfigurationException">Thrown when the config file exists but cannot be parsed.</exception>
    public NextNetConfig Load(string? startDirectory = null)
    {
        startDirectory ??= Directory.GetCurrentDirectory();

        var configPath = FindConfigFile(startDirectory);
        if (configPath == null)
        {
            return new NextNetConfig();
        }

        try
        {
            var json = _fileSystem.ReadAllText(configPath);
            var config = JsonSerializer.Deserialize<NextNetConfig>(json, JsonOptions);
            return config ?? new NextNetConfig();
        }
        catch (JsonException ex)
        {
            throw new NextNetConfigurationException(
                $"Failed to parse configuration file at '{configPath}'.", ex);
        }
    }

    /// <summary>
    /// Searches upward from <paramref name="startDirectory"/> for a <c>nextnet.config.json</c> file.
    /// </summary>
    /// <returns>The full path to the config file, or <c>null</c> if not found.</returns>
    internal string? FindConfigFile(string startDirectory)
    {
        var current = _fileSystem.GetFullPath(startDirectory);

        while (current != null)
        {
            var candidate = _fileSystem.Combine(current, Conventions.NextNetConventions.ConfigFileName);
            if (_fileSystem.FileExists(candidate))
            {
                return candidate;
            }
            current = _fileSystem.GetDirectoryName(current);
        }

        return null;
    }
}
