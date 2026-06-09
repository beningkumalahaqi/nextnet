using System.CommandLine;

namespace NextNet.TemplateSdk.CLI;

/// <summary>
/// Implements the <c>template login</c> command — authenticates the user with the
/// NextNet template registry by creating or updating a local <see cref="AuthorProfile"/>.
/// </summary>
/// <remarks>
/// <para>
/// The <c>template login</c> command prompts the user for their name, email, and API
/// token, then saves the profile to <c>~/.nextnet/author.json</c>. This profile is
/// used by <see cref="TemplatePublisher"/> when publishing templates.
/// </para>
/// <para>
/// If a profile already exists, it will be overwritten with the new values.
/// Run this command again to update your credentials.
/// </para>
/// </remarks>
public sealed class TemplateLoginCommand : Command
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TemplateLoginCommand"/> class.
    /// </summary>
    public TemplateLoginCommand() : base("login", "Authenticate with the template registry")
    {
        this.SetHandler(HandleAsync);
    }

    private static async Task<int> HandleAsync()
    {
        try
        {
            Console.Write("Name: ");
            var name = Console.ReadLine() ?? "";

            Console.Write("Email: ");
            var email = Console.ReadLine() ?? "";

            Console.Write("API Token: ");
            var apiToken = Console.ReadLine() ?? "";

            if (string.IsNullOrWhiteSpace(name))
            {
                Console.Error.WriteLine("Error: Name is required.");
                return 1;
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                Console.Error.WriteLine("Error: Email is required.");
                return 1;
            }

            if (string.IsNullOrWhiteSpace(apiToken))
            {
                Console.Error.WriteLine("Error: API Token is required.");
                return 1;
            }

            var profile = new AuthorProfile
            {
                Name = name.Trim(),
                Email = email.Trim(),
                ApiToken = apiToken.Trim()
            };

            await profile.SaveAsync();

            Console.WriteLine($"Logged in as {profile.Name} ({profile.Email})");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error saving profile: {ex.Message}");
            return 1;
        }
    }
}
