namespace NextNet.Templates.Official.Dashboard;

using System.Text;

/// <summary>
/// Static file content for the Dashboard template.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="DashboardTemplateFiles"/> provides the raw byte content for all files in the
/// official Admin Dashboard template package. For simplicity and testability, they are defined
/// inline as static string constants.
/// </para>
/// <para>
/// The <see cref="GetAllFiles"/> method returns a dictionary mapping source paths
/// (relative to the template root) to UTF-8 encoded byte arrays, which are consumed
/// by <see cref="DashboardTemplateProvider.GetFilesAsync"/>.
/// </para>
/// </remarks>
public static class DashboardTemplateFiles
{
    /// <summary>
    /// Returns all template files as a dictionary of source path to byte content.
    /// </summary>
    /// <returns>A read-only dictionary mapping source paths to raw content bytes.</returns>
    /// <example>
    /// <code>
    /// var files = DashboardTemplateFiles.GetAllFiles();
    /// var programCs = Encoding.UTF8.GetString(files["app/Program.cs"]);
    /// </code>
    /// </example>
    public static IReadOnlyDictionary<string, byte[]> GetAllFiles()
    {
        var files = new Dictionary<string, byte[]>
        {
            ["template.json"] = Encoding.UTF8.GetBytes(ManifestJson),
            ["app/Program.cs"] = Encoding.UTF8.GetBytes(ProgramCs),
            ["app/appsettings.json"] = Encoding.UTF8.GetBytes(AppSettingsJson),
            ["app/dashboard/page.cs"] = Encoding.UTF8.GetBytes(DashboardIndexCs),
            ["app/login/page.cs"] = Encoding.UTF8.GetBytes(LoginPageCs),
            ["app/logout/page.cs"] = Encoding.UTF8.GetBytes(LogoutPageCs),
            ["app/layouts/dashboard.cs"] = Encoding.UTF8.GetBytes(DashboardLayoutCs),
            ["app/Services/AuthService.cs"] = Encoding.UTF8.GetBytes(AuthServiceCs),
            ["app/Services/IAuthService.cs"] = Encoding.UTF8.GetBytes(IAuthServiceCs),
            ["app/Models/User.cs"] = Encoding.UTF8.GetBytes(UserModelCs),
            ["app/wwwroot/css/dashboard.css"] = Encoding.UTF8.GetBytes(DashboardCss)
        };
        return files;
    }

    private const string ManifestJson = """
    {
      "name": "dashboard",
      "version": "1.0.0",
      "nextnetVersion": ">=3.0.0",
      "author": "NextNet Team",
      "description": "Admin dashboard with auth, navigation, and layout"
    }
    """;

    private const string ProgramCs = """
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using {{namespaceName}}.App.Services;

    var builder = WebApplication.CreateBuilder(args);
    builder.Services.AddSingleton<IAuthService, AuthService>();
    var app = builder.Build();
    app.Run();
    """;

    private const string AppSettingsJson = """
    {
      "AppTitle": "{{appTitle}}",
      "Theme": { "PrimaryColor": "{{primaryColor}}" }
    }
    """;

    private const string DashboardIndexCs = """"
    namespace {{namespaceName}}.App.Pages.Dashboard;

    public class IndexPage
    {
        public string Render(string userName) => $$"""
            <!DOCTYPE html>
            <html>
            <head><title>Dashboard</title></head>
            <body>
                <h1>Welcome, {{userName}}!</h1>
                <p>This is your admin dashboard.</p>
            </body>
            </html>
            """;
    }
    """";

    private const string LoginPageCs = """"
    namespace {{namespaceName}}.App.Pages;

    public class LoginPage
    {
        public string Render() => """
            <!DOCTYPE html>
            <html>
            <head><title>Login</title></head>
            <body>
                <form method="post" action="/login">
                    <input name="username" placeholder="Username" required />
                    <input name="password" type="password" placeholder="Password" required />
                    <button type="submit">Sign In</button>
                </form>
            </body>
            </html>
            """;
    }
    """";

    private const string LogoutPageCs = """
    namespace {{namespaceName}}.App.Pages;

    public class LogoutPage
    {
        public string Render() => "<p>You have been logged out.</p>";
    }
    """;

    private const string DashboardLayoutCs = """"
    namespace {{namespaceName}}.App.Layouts;

    public class DashboardLayout
    {
        public string Wrap(string title, string body) => $$"""
            <!DOCTYPE html>
            <html>
            <head>
                <title>{{title}}</title>
                <link rel="stylesheet" href="/css/dashboard.css" />
            </head>
            <body>
                <nav>
                    <a href="/dashboard">Home</a>
                    <a href="/users">Users</a>
                    <a href="/settings">Settings</a>
                    <a href="/logout">Logout</a>
                </nav>
                <main>{{body}}</main>
            </body>
            </html>
            """;
    }
    """";

    private const string IAuthServiceCs = """
    namespace {{namespaceName}}.App.Services;

    using {{namespaceName}}.App.Models;

    public interface IAuthService
    {
        Task<User?> AuthenticateAsync(string username, string password, CancellationToken ct = default);
        Task<User?> GetUserByIdAsync(int id, CancellationToken ct = default);
    }
    """;

    private const string AuthServiceCs = """
    namespace {{namespaceName}}.App.Services;

    using {{namespaceName}}.App.Models;

    public class AuthService : IAuthService
    {
        private readonly Dictionary<string, (User user, string passwordHash)> _users = new();
        
        public AuthService()
        {
            // Demo user
            _users["admin"] = (new User { Id = 1, Username = "admin", Email = "admin@example.com", Role = "admin" }, "admin");
        }

        public Task<User?> AuthenticateAsync(string username, string password, CancellationToken ct = default)
        {
            if (_users.TryGetValue(username, out var entry) && entry.passwordHash == password)
                return Task.FromResult<User?>(entry.user);
            return Task.FromResult<User?>(null);
        }

        public Task<User?> GetUserByIdAsync(int id, CancellationToken ct = default)
            => Task.FromResult<User?>(_users.Values.Select(v => v.user).FirstOrDefault(u => u.Id == id));
    }
    """;

    private const string UserModelCs = """
    namespace {{namespaceName}}.App.Models;

    public sealed class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = "";
        public string Email { get; set; } = "";
        public string Role { get; set; } = "user";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
    """;

    private const string DashboardCss = """
    :root { --primary: #007bff; }
    body { font-family: -apple-system, sans-serif; margin: 0; }
    nav { background: var(--primary); color: white; padding: 1rem; }
    nav a { color: white; margin-right: 1rem; text-decoration: none; }
    main { padding: 2rem; }
    h1 { color: var(--primary); }
    """;
}
