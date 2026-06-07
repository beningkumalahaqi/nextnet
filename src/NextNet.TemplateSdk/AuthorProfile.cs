using System.Text.Json;

namespace NextNet.TemplateSdk;

/// <summary>
/// Represents the authenticated template author profile stored locally at
/// <c>~/.nextnet/author.json</c>.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="AuthorProfile"/> is used by the <see cref="TemplatePublisher"/> to
/// authenticate with the NextNet template registry. It stores the author's name,
/// email, and API token for publishing templates.
/// </para>
/// <para>
/// The profile is persisted to the user's home directory under <c>.nextnet/author.json</c>
/// and can be created or updated via the <c>nextnet template login</c> command.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Load existing profile
/// var profile = await AuthorProfile.LoadAsync();
/// if (profile is not null)
/// {
///     Console.WriteLine($"Logged in as {profile.Name}");
/// }
///
/// // Create and save a new profile
/// var newProfile = new AuthorProfile
/// {
///     Name = "Jane Doe",
///     Email = "jane@example.com",
///     ApiToken = "sk_abc123"
/// };
/// await newProfile.SaveAsync();
/// </code>
/// </example>
public sealed class AuthorProfile
{
    /// <summary>
    /// The display name of the template author.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// The email address associated with the author's registry account.
    /// </summary>
    public string Email { get; set; } = "";

    /// <summary>
    /// The API token used to authenticate with the NextNet template registry.
    /// </summary>
    public string? ApiToken { get; set; }

    private static string ProfilePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".nextnet", "author.json");

    /// <summary>
    /// Loads the author profile from disk (<c>~/.nextnet/author.json</c>).
    /// </summary>
    /// <returns>
    /// The deserialized <see cref="AuthorProfile"/>, or <c>null</c> if the file
    /// does not exist or cannot be parsed.
    /// </returns>
    public static async Task<AuthorProfile?> LoadAsync()
    {
        if (!File.Exists(ProfilePath)) return null;
        try
        {
            var json = await File.ReadAllTextAsync(ProfilePath);
            return JsonSerializer.Deserialize<AuthorProfile>(json, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            });
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Saves this profile to disk at <c>~/.nextnet/author.json</c>.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task SaveAsync()
    {
        var dir = Path.GetDirectoryName(ProfilePath)!;
        Directory.CreateDirectory(dir);
        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });
        await File.WriteAllTextAsync(ProfilePath, json);
    }
}
