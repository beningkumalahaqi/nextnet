namespace NextNet.Templates.Official.Api;

using System.Text;

/// <summary>
/// Static file content for the API template.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="ApiTemplateFiles"/> provides the raw byte content for all files in the
/// official REST API template package. For simplicity and testability, they are defined
/// inline as static string constants.
/// </para>
/// <para>
/// The <see cref="GetAllFiles"/> method returns a dictionary mapping source paths
/// (relative to the template root) to UTF-8 encoded byte arrays, which are consumed
/// by <see cref="ApiTemplateProvider.GetFilesAsync"/>.
/// </para>
/// </remarks>
public static class ApiTemplateFiles
{
    /// <summary>
    /// Returns all template files as a dictionary of source path to byte content.
    /// </summary>
    /// <returns>A read-only dictionary mapping source paths to raw content bytes.</returns>
    /// <example>
    /// <code>
    /// var files = ApiTemplateFiles.GetAllFiles();
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
            ["app/Controllers/TodosController.cs"] = Encoding.UTF8.GetBytes(TodosControllerCs),
            ["app/Controllers/HealthController.cs"] = Encoding.UTF8.GetBytes(HealthControllerCs),
            ["app/Models/Todo.cs"] = Encoding.UTF8.GetBytes(TodoModelCs),
            ["app/Models/Dto/CreateTodoRequest.cs"] = Encoding.UTF8.GetBytes(CreateTodoRequestCs),
            ["app/Models/Dto/UpdateTodoRequest.cs"] = Encoding.UTF8.GetBytes(UpdateTodoRequestCs),
            ["app/Services/ITodoService.cs"] = Encoding.UTF8.GetBytes(ITodoServiceCs),
            ["app/Services/TodoService.cs"] = Encoding.UTF8.GetBytes(TodoServiceCs),
            ["app/Auth/AuthController.cs"] = Encoding.UTF8.GetBytes(AuthControllerCs),
            ["app/Auth/JwtTokenGenerator.cs"] = Encoding.UTF8.GetBytes(JwtTokenGeneratorCs),
            ["app/Data/AppDbContext.cs"] = Encoding.UTF8.GetBytes(AppDbContextCs),
            ["app/Middleware/ErrorHandlingMiddleware.cs"] = Encoding.UTF8.GetBytes(ErrorHandlingMiddlewareCs)
        };
        return files;
    }

    private const string ManifestJson = """
    {
      "name": "api",
      "version": "1.0.0",
      "nextnetVersion": ">=3.0.0",
      "author": "NextNet Team",
      "description": "Production-ready REST API with OpenAPI, Swagger, health checks"
    }
    """;

    private const string ProgramCs = """
    using Microsoft.AspNetCore.Builder;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using {{namespaceName}}.App.Auth;
    using {{namespaceName}}.App.Data;
    using {{namespaceName}}.App.Middleware;
    using {{namespaceName}}.App.Services;

    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // Database
    var dbProvider = builder.Configuration["Database:Provider"] ?? "sqlite";
    if (dbProvider == "sqlite")
    {
        builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlite("Data Source={{projectName}}.db"));
    }
    else if (dbProvider == "postgres")
    {
        builder.Services.AddDbContext<AppDbContext>(opt => opt.UseNpgsql(builder.Configuration.GetConnectionString("Default")));
    }

    builder.Services.AddScoped<ITodoService, TodoService>();

    // Auth (conditional)
    var includeAuth = builder.Configuration.GetValue<bool>("Auth:Enabled");
    if (includeAuth) { builder.Services.AddSingleton<JwtTokenGenerator>(); }

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseMiddleware<ErrorHandlingMiddleware>();
    app.MapControllers();

    app.Run();
    """;

    private const string AppSettingsJson = """
    {
      "Logging": { "LogLevel": { "Default": "Information" } },
      "Database": { "Provider": "sqlite" },
      "Auth": { "Enabled": false, "Secret": "REPLACE_WITH_SECRET", "Issuer": "{{projectName}}" }
    }
    """;

    private const string TodosControllerCs = """
    namespace {{namespaceName}}.App.Controllers;

    using Microsoft.AspNetCore.Mvc;
    using {{namespaceName}}.App.Models;
    using {{namespaceName}}.App.Models.Dto;
    using {{namespaceName}}.App.Services;

    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public sealed class TodosController : ControllerBase
    {
        private readonly ITodoService _service;
        public TodosController(ITodoService service) => _service = service;

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Todo>), 200)]
        public async Task<ActionResult<IEnumerable<Todo>>> GetAll()
            => Ok(await _service.GetAllAsync());

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(Todo), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<Todo>> GetById(int id)
        {
            var todo = await _service.GetByIdAsync(id);
            return todo is null ? NotFound() : Ok(todo);
        }

        [HttpPost]
        [ProducesResponseType(typeof(Todo), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<Todo>> Create(CreateTodoRequest request)
        {
            var todo = await _service.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = todo.Id }, todo);
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(Todo), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<Todo>> Update(int id, UpdateTodoRequest request)
        {
            var todo = await _service.UpdateAsync(id, request);
            return todo is null ? NotFound() : Ok(todo);
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(int id)
            => await _service.DeleteAsync(id) ? NoContent() : NotFound();
    }
    """;

    private const string HealthControllerCs = """
    namespace {{namespaceName}}.App.Controllers;

    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    public sealed class HealthController : ControllerBase
    {
        [HttpGet("/health")]
        public IActionResult Check() => Ok(new { status = "healthy" });

        [HttpGet("/health/ready")]
        public IActionResult Readiness() => Ok(new { status = "ready" });

        [HttpGet("/health/live")]
        public IActionResult Liveness() => Ok(new { status = "live" });
    }
    """;

    private const string TodoModelCs = """
    namespace {{namespaceName}}.App.Models;

    public sealed class Todo
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string? Description { get; set; }
        public bool IsComplete { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
    """;

    private const string CreateTodoRequestCs = """
    namespace {{namespaceName}}.App.Models.Dto;

    using System.ComponentModel.DataAnnotations;

    public sealed class CreateTodoRequest
    {
        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Title { get; set; } = "";

        [StringLength(2000)]
        public string? Description { get; set; }
    }
    """;

    private const string UpdateTodoRequestCs = """
    namespace {{namespaceName}}.App.Models.Dto;

    using System.ComponentModel.DataAnnotations;

    public sealed class UpdateTodoRequest
    {
        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Title { get; set; } = "";

        [StringLength(2000)]
        public string? Description { get; set; }

        public bool IsComplete { get; set; }
    }
    """;

    private const string ITodoServiceCs = """
    namespace {{namespaceName}}.App.Services;

    using {{namespaceName}}.App.Models;
    using {{namespaceName}}.App.Models.Dto;

    public interface ITodoService
    {
        Task<IEnumerable<Todo>> GetAllAsync(CancellationToken ct = default);
        Task<Todo?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<Todo> CreateAsync(CreateTodoRequest request, CancellationToken ct = default);
        Task<Todo?> UpdateAsync(int id, UpdateTodoRequest request, CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    }
    """;

    private const string TodoServiceCs = """
    namespace {{namespaceName}}.App.Services;

    using {{namespaceName}}.App.Models;
    using {{namespaceName}}.App.Models.Dto;

    public sealed class TodoService : ITodoService
    {
        private readonly List<Todo> _todos = new();
        private int _nextId = 1;

        public Task<IEnumerable<Todo>> GetAllAsync(CancellationToken ct = default)
            => Task.FromResult<IEnumerable<Todo>>(_todos.ToList());

        public Task<Todo?> GetByIdAsync(int id, CancellationToken ct = default)
            => Task.FromResult(_todos.FirstOrDefault(t => t.Id == id));

        public Task<Todo> CreateAsync(CreateTodoRequest request, CancellationToken ct = default)
        {
            var todo = new Todo
            {
                Id = _nextId++,
                Title = request.Title,
                Description = request.Description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _todos.Add(todo);
            return Task.FromResult(todo);
        }

        public Task<Todo?> UpdateAsync(int id, UpdateTodoRequest request, CancellationToken ct = default)
        {
            var todo = _todos.FirstOrDefault(t => t.Id == id);
            if (todo is null) return Task.FromResult<Todo?>(null);
            todo.Title = request.Title;
            todo.Description = request.Description;
            todo.IsComplete = request.IsComplete;
            todo.UpdatedAt = DateTime.UtcNow;
            return Task.FromResult<Todo?>(todo);
        }

        public Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var todo = _todos.FirstOrDefault(t => t.Id == id);
            if (todo is null) return Task.FromResult(false);
            _todos.Remove(todo);
            return Task.FromResult(true);
        }
    }
    """;

    private const string AuthControllerCs = """
    namespace {{namespaceName}}.App.Auth;

    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("api/auth")]
    public sealed class AuthController : ControllerBase
    {
        private readonly JwtTokenGenerator _jwt;
        public AuthController(JwtTokenGenerator jwt) => _jwt = jwt;

        [HttpPost("login")]
        [ProducesResponseType(typeof(TokenResponse), 200)]
        [ProducesResponseType(401)]
        public ActionResult<TokenResponse> Login(LoginRequest request)
        {
            // Demo: accept any non-empty credentials
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                return Unauthorized();
            var token = _jwt.GenerateToken(request.Username);
            return Ok(new TokenResponse(token, "Bearer"));
        }
    }

    public sealed record LoginRequest(string Username, string Password);
    public sealed record TokenResponse(string AccessToken, string TokenType);
    """;

    private const string JwtTokenGeneratorCs = """
    namespace {{namespaceName}}.App.Auth;

    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Text;
    using Microsoft.Extensions.Configuration;
    using Microsoft.IdentityModel.Tokens;

    public sealed class JwtTokenGenerator
    {
        private readonly string _secret;
        private readonly string _issuer;

        public JwtTokenGenerator(IConfiguration config)
        {
            _secret = config["Auth:Secret"] ?? "REPLACE_WITH_SECRET_MIN_32_CHARS_LONG_FOR_HS256";
            _issuer = config["Auth:Issuer"] ?? "nextnet";
        }

        public string GenerateToken(string username)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var claims = new[] { new Claim(ClaimTypes.Name, username) };
            var token = new JwtSecurityToken(issuer: _issuer, claims: claims, expires: DateTime.UtcNow.AddHours(1), signingCredentials: creds);
            return new JwtSecurityTokenHandler().WriteToken(token);
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

        public DbSet<Todo> Todos => Set<Todo>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Todo>(e =>
            {
                e.HasKey(t => t.Id);
                e.Property(t => t.Title).IsRequired().HasMaxLength(200);
            });
        }
    }
    """;

    private const string ErrorHandlingMiddlewareCs = """
    namespace {{namespaceName}}.App.Middleware;

    using Microsoft.AspNetCore.Http;
    using System.Net;
    using System.Text.Json;

    public sealed class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        public ErrorHandlingMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            try { await _next(context); }
            catch (Exception ex)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/problem+json";
                var problem = new
                {
                    type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                    title = "Internal Server Error",
                    status = 500,
                    detail = ex.Message,
                    traceId = context.TraceIdentifier
                };
                await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
            }
        }
    }
    """;
}
