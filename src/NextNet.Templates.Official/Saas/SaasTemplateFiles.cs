namespace NextNet.Templates.Official.Saas;

using System.Text;

/// <summary>
/// Static file content for the SaaS template.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="SaasTemplateFiles"/> provides the raw byte content for all files in the
/// official SaaS template package. In a real production implementation, these would be
/// loaded from embedded resources or the file system. For simplicity and testability,
/// they are defined inline as static string constants.
/// </para>
/// <para>
/// The <see cref="GetAllFiles"/> method returns a dictionary mapping source paths
/// (relative to the template root) to UTF-8 encoded byte arrays, which are consumed
/// by <see cref="SaasTemplateProvider.GetFilesAsync"/>.
/// </para>
/// </remarks>
public static class SaasTemplateFiles
{
    /// <summary>
    /// Returns all template files as a dictionary of source path to byte content.
    /// </summary>
    /// <returns>A read-only dictionary mapping source paths to raw content bytes.</returns>
    /// <example>
    /// <code>
    /// var files = SaasTemplateFiles.GetAllFiles();
    /// var programCs = Encoding.UTF8.GetString(files["app/Program.cs"]);
    /// </code>
    /// </example>
    public static IReadOnlyDictionary<string, byte[]> GetAllFiles()
    {
        var files = new Dictionary<string, byte[]>
        {
            ["template.json"] = Encoding.UTF8.GetBytes(TemplateManifestJson),
            ["app/Program.cs"] = Encoding.UTF8.GetBytes(ProgramCs),
            ["app/appsettings.json"] = Encoding.UTF8.GetBytes(AppSettingsJson),
            ["app/Models/Organization.cs"] = Encoding.UTF8.GetBytes(OrganizationModelCs),
            ["app/Models/Membership.cs"] = Encoding.UTF8.GetBytes(MembershipModelCs),
            ["app/Models/User.cs"] = Encoding.UTF8.GetBytes(UserModelCs),
            ["app/Services/ITenantService.cs"] = Encoding.UTF8.GetBytes(ITenantServiceCs),
            ["app/Services/TenantService.cs"] = Encoding.UTF8.GetBytes(TenantServiceCs),
            ["app/Services/IUserService.cs"] = Encoding.UTF8.GetBytes(IUserServiceCs),
            ["app/Services/UserService.cs"] = Encoding.UTF8.GetBytes(UserServiceCs),
            ["app/Auth/AuthController.cs"] = Encoding.UTF8.GetBytes(AuthControllerCs),
            ["app/Billing/BillingService.cs"] = Encoding.UTF8.GetBytes(BillingServiceCs),
            ["app/Data/AppDbContext.cs"] = Encoding.UTF8.GetBytes(AppDbContextCs),
            ["app/page.cs"] = Encoding.UTF8.GetBytes(HomePageCs),
            ["app/signup/page.cs"] = Encoding.UTF8.GetBytes(SignupPageCs),
            ["app/login/page.cs"] = Encoding.UTF8.GetBytes(LoginPageCs),
            ["app/dashboard/page.cs"] = Encoding.UTF8.GetBytes(DashboardPageCs)
        };
        return files;
    }

    private const string TemplateManifestJson = """
    {
      "name": "saas",
      "version": "1.0.0",
      "nextnetVersion": ">=3.0.0",
      "author": "NextNet Team",
      "description": "Multi-tenant SaaS starter with users, organizations, and auth"
    }
    """;

    private const string ProgramCs = """
    using Microsoft.AspNetCore.Builder;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using {{namespaceName}}.App.Auth;
    using {{namespaceName}}.App.Data;
    using {{namespaceName}}.App.Services;

    var builder = WebApplication.CreateBuilder(args);
    builder.Services.AddControllers();
    builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlite("Data Source={{projectName}}.db"));
    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddScoped<ITenantService, TenantService>();
    var app = builder.Build();
    app.MapControllers();
    app.Run();
    """;

    private const string AppSettingsJson = """
    {
      "Database": { "Provider": "{{database}}" },
      "Billing": { "Enabled": {{includeBilling}}, "StripeApiKey": "REPLACE_ME" }
    }
    """;

    private const string OrganizationModelCs = """
    namespace {{namespaceName}}.App.Models;

    public sealed class Organization
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Slug { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<Membership> Memberships { get; set; } = new List<Membership>();
    }
    """;

    private const string MembershipModelCs = """
    namespace {{namespaceName}}.App.Models;

    public sealed class Membership
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public int OrganizationId { get; set; }
        public Organization? Organization { get; set; }
        public string Role { get; set; } = "member";
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }
    """;

    private const string UserModelCs = """
    namespace {{namespaceName}}.App.Models;

    public sealed class User
    {
        public int Id { get; set; }
        public string Email { get; set; } = "";
        public string PasswordHash { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<Membership> Memberships { get; set; } = new List<Membership>();
    }
    """;

    private const string ITenantServiceCs = """
    namespace {{namespaceName}}.App.Services;

    using {{namespaceName}}.App.Models;

    public interface ITenantService
    {
        Task<Organization> CreateOrganizationAsync(string name, int ownerUserId, CancellationToken ct = default);
        Task<Organization?> GetOrganizationAsync(int id, CancellationToken ct = default);
        Task<Organization?> GetOrganizationBySlugAsync(string slug, CancellationToken ct = default);
        Task AddMemberAsync(int orgId, int userId, string role, CancellationToken ct = default);
    }
    """;

    private const string TenantServiceCs = """
    namespace {{namespaceName}}.App.Services;

    using {{namespaceName}}.App.Data;
    using {{namespaceName}}.App.Models;
    using Microsoft.EntityFrameworkCore;

    public class TenantService : ITenantService
    {
        private readonly AppDbContext _db;
        public TenantService(AppDbContext db) => _db = db;

        public async Task<Organization> CreateOrganizationAsync(string name, int ownerUserId, CancellationToken ct = default)
        {
            var org = new Organization
            {
                Name = name,
                Slug = name.ToLowerInvariant().Replace(' ', '-')
            };
            _db.Organizations.Add(org);
            await _db.SaveChangesAsync(ct);

            _db.Memberships.Add(new Membership { OrganizationId = org.Id, UserId = ownerUserId, Role = "owner" });
            await _db.SaveChangesAsync(ct);

            return org;
        }

        public Task<Organization?> GetOrganizationAsync(int id, CancellationToken ct = default)
            => _db.Organizations.Include(o => o.Memberships).FirstOrDefaultAsync(o => o.Id == id, ct);

        public Task<Organization?> GetOrganizationBySlugAsync(string slug, CancellationToken ct = default)
            => _db.Organizations.Include(o => o.Memberships).FirstOrDefaultAsync(o => o.Slug == slug, ct);

        public async Task AddMemberAsync(int orgId, int userId, string role, CancellationToken ct = default)
        {
            _db.Memberships.Add(new Membership { OrganizationId = orgId, UserId = userId, Role = role });
            await _db.SaveChangesAsync(ct);
        }
    }
    """;

    private const string IUserServiceCs = """
    namespace {{namespaceName}}.App.Services;

    using {{namespaceName}}.App.Models;

    public interface IUserService
    {
        Task<User> RegisterAsync(string email, string password, string displayName, CancellationToken ct = default);
        Task<User?> AuthenticateAsync(string email, string password, CancellationToken ct = default);
        Task<User?> GetByIdAsync(int id, CancellationToken ct = default);
    }
    """;

    private const string UserServiceCs = """
    namespace {{namespaceName}}.App.Services;

    using {{namespaceName}}.App.Data;
    using {{namespaceName}}.App.Models;
    using Microsoft.EntityFrameworkCore;

    public class UserService : IUserService
    {
        private readonly AppDbContext _db;
        public UserService(AppDbContext db) => _db = db;

        public async Task<User> RegisterAsync(string email, string password, string displayName, CancellationToken ct = default)
        {
            var user = new User
            {
                Email = email,
                PasswordHash = HashPassword(password),
                DisplayName = displayName
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync(ct);
            return user;
        }

        public Task<User?> AuthenticateAsync(string email, string password, CancellationToken ct = default)
        {
            var hash = HashPassword(password);
            return _db.Users.FirstOrDefaultAsync(u => u.Email == email && u.PasswordHash == hash, ct);
        }

        public Task<User?> GetByIdAsync(int id, CancellationToken ct = default)
            => _db.Users.Include(u => u.Memberships).FirstOrDefaultAsync(u => u.Id == id, ct);

        private static string HashPassword(string password)
        {
            // NOTE: Replace with proper password hashing (BCrypt, Argon2) in production
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password));
        }
    }
    """;

    private const string AuthControllerCs = """
    namespace {{namespaceName}}.App.Auth;

    using Microsoft.AspNetCore.Mvc;
    using {{namespaceName}}.App.Services;

    [ApiController]
    [Route("api/auth")]
    public sealed class AuthController : ControllerBase
    {
        private readonly IUserService _users;
        public AuthController(IUserService users) => _users = users;

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var user = await _users.RegisterAsync(request.Email, request.Password, request.DisplayName ?? "");
            return Ok(new { user.Id, user.Email, user.DisplayName });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _users.AuthenticateAsync(request.Email, request.Password);
            if (user is null) return Unauthorized();
            return Ok(new { user.Id, user.Email, user.DisplayName });
        }
    }

    public sealed record RegisterRequest(string Email, string Password, string? DisplayName);
    public sealed record LoginRequest(string Email, string Password);
    """;

    private const string BillingServiceCs = """
    namespace {{namespaceName}}.App.Billing;

    public class BillingService
    {
        private readonly string _apiKey;
        public BillingService(IConfiguration config) => _apiKey = config["Billing:StripeApiKey"] ?? "";

        public Task<string> CreateCustomerAsync(string email)
        {
            // TODO: Integrate with Stripe API
            return Task.FromResult($"cust_demo_{Guid.NewGuid():N}");
        }

        public Task<string> CreateSubscriptionAsync(string customerId, string planId)
        {
            // TODO: Integrate with Stripe API
            return Task.FromResult($"sub_demo_{Guid.NewGuid():N}");
        }
    }
    """;

    private const string AppDbContextCs = """
    namespace {{namespaceName}}.App.Data;

    using Microsoft.EntityFrameworkCore;
    using {{namespaceName}}.App.Models;

    public sealed class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Organization> Organizations => Set<Organization>();
        public DbSet<Membership> Memberships => Set<Membership>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(e =>
            {
                e.HasKey(u => u.Id);
                e.HasIndex(u => u.Email).IsUnique();
            });
            modelBuilder.Entity<Organization>(e =>
            {
                e.HasKey(o => o.Id);
                e.HasIndex(o => o.Slug).IsUnique();
            });
            modelBuilder.Entity<Membership>(e =>
            {
                e.HasKey(m => m.Id);
                e.HasOne(m => m.User).WithMany(u => u.Memberships).HasForeignKey(m => m.UserId);
                e.HasOne(m => m.Organization).WithMany(o => o.Memberships).HasForeignKey(m => m.OrganizationId);
            });
        }
    }
    """;

    private const string HomePageCs = """
    namespace {{namespaceName}}.App.Pages;

    public class IndexPage
    {
        public string Render() => "<h1>{{projectName}}</h1><p>Multi-tenant SaaS starter</p><a href='/signup'>Sign up</a> | <a href='/login'>Log in</a>";
    }
    """;

    private const string SignupPageCs = """""
    namespace {{namespaceName}}.App.Pages;

    public class SignupPage
    {
        public string Render() => """
            <form method="post" action="/api/auth/register">
                <input name="email" type="email" placeholder="Email" required />
                <input name="password" type="password" placeholder="Password" required />
                <input name="displayName" placeholder="Display Name" />
                <button type="submit">Create Account</button>
            </form>
            """;
    }
    """"";

    private const string LoginPageCs = """""
    namespace {{namespaceName}}.App.Pages;

    public class LoginPage
    {
        public string Render() => """
            <form method="post" action="/api/auth/login">
                <input name="email" type="email" placeholder="Email" required />
                <input name="password" type="password" placeholder="Password" required />
                <button type="submit">Sign In</button>
            </form>
            """;
    }
    """"";

    private const string DashboardPageCs = """
    namespace {{namespaceName}}.App.Pages.Dashboard;

    public class IndexPage
    {
        public string Render(string orgName) => $"<h1>{orgName} Dashboard</h1><p>Welcome to your organization.</p>";
    }
    """;
}
