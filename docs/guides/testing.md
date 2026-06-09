---
uid: guides/testing
title: Testing
description: Unit, integration, and end to end testing for NextNet apps
---

# Testing `v1.0` `stable`

Test your NextNet applications at multiple levels — from unit tests on individual components to full integration tests against the running application.

## Testing Levels

| Level | What to Test | Speed | Tools |
|-------|-------------|-------|-------|
| **Unit** | Individual components, helpers, services | 🚀 Fast | xUnit, NUnit |
| **Integration** | Route resolution, rendering pipeline | ⚡ Medium | TestServer, xUnit |
| **End to End** | Full application flows | 🐢 Slow | Playwright, Selenium |

## Unit Testing Pages

Test page components in isolation:

```csharp
// File: tests/NextNet.Core.Tests/Pages/AboutPageTests.cs
public class AboutPageTests
{
    [Fact]
    [Category("Unit")]
    public async Task Render_Should_ReturnExpectedHtml_When_Called()
    {
        // Arrange
        var page = new AboutPage();

        // Act
        var result = await page.Render();

        // Assert
        var html = result.ToHtmlString();
        Assert.Contains("<h1>About Us</h1>", html);
    }
}
```

> [!TIP]
> Use `.ToHtmlString()` to convert `IHtmlContent` to a string for assertions.

## Testing with Dependencies

Test pages that use DI with mocked services:

```csharp
// File: tests/NextNet.Core.Tests/Pages/ProductsPageTests.cs
public class ProductsPageTests
{
    [Fact]
    [Category("Unit")]
    public async Task Render_Should_ListProducts_When_ProductsExist()
    {
        // Arrange
        var mockService = new Mock<IProductService>();
        mockService.Setup(s => s.GetAll())
            .ReturnsAsync(new List<Product>
            {
                new() { Id = 1, Name = "Widget", Price = 9.99m },
                new() { Id = 2, Name = "Gadget", Price = 19.99m }
            });

        var page = new ProductsPage(mockService.Object);

        // Act
        var result = await page.Render();
        var html = result.ToHtmlString();

        // Assert
        Assert.Contains("Widget", html);
        Assert.Contains("Gadget", html);
        Assert.Contains("$9.99", html);
    }

    [Fact]
    [Category("Unit")]
    public async Task Render_Should_ShowEmptyMessage_When_NoProducts()
    {
        // Arrange
        var mockService = new Mock<IProductService>();
        mockService.Setup(s => s.GetAll())
            .ReturnsAsync(new List<Product>());

        var page = new ProductsPage(mockService.Object);

        // Act
        var result = await page.Render();
        var html = result.ToHtmlString();

        // Assert
        Assert.Contains("No products found", html);
    }
}
```

## Testing API Routes

Test API routes with request/response verification:

```csharp
// File: tests/NextNet.Core.Tests/Routes/UsersRouteTests.cs
public class UsersRouteTests
{
    [Fact]
    [Category("Unit")]
    public async Task Get_Should_ReturnOk_With_Users()
    {
        // Arrange
        var mockRepo = new Mock<IUserRepository>();
        mockRepo.Setup(r => r.GetAll())
            .ReturnsAsync(new List<User> { new() { Id = 1, Name = "Alice" } });

        var route = new UsersRoute(mockRepo.Object);

        // Act
        var result = await route.Get();

        // Assert
        var okResult = Assert.IsType<Ok<List<User>>>(result);
        Assert.Single(okResult.Value);
        Assert.Equal("Alice", okResult.Value[0].Name);
    }

    [Fact]
    [Category("Unit")]
    public async Task Post_Should_ReturnCreated_When_ValidRequest()
    {
        // Arrange
        var mockRepo = new Mock<IUserRepository>();
        var request = new CreateUserRequest { Name = "Bob", Email = "bob@test.com" };

        mockRepo.Setup(r => r.Create(It.IsAny<CreateUserRequest>()))
            .ReturnsAsync(new User { Id = 2, Name = "Bob", Email = "bob@test.com" });

        var route = new UsersRoute(mockRepo.Object);

        // Act
        var result = await route.Post(request);

        // Assert
        var createdResult = Assert.IsType<Created<User>>(result);
        Assert.Equal(2, createdResult.Value.Id);
    }
}
```

## Integration Testing with TestServer

Use ASP.NET Core's `TestServer` for integration tests:

```csharp
// File: tests/NextNet.Core.Tests/Integration/RouteResolutionTests.cs
public class RouteResolutionTests : IClassFixture<NextNetTestFixture>
{
    private readonly HttpClient _client;

    public RouteResolutionTests(NextNetTestFixture fixture)
    {
        _client = fixture.CreateClient();
    }

    [Fact]
    [Category("Integration")]
    public async Task Get_RootUrl_Should_Return200()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("<h1", html);
    }

    [Fact]
    [Category("Integration")]
    public async Task Get_KnownRoute_Should_ReturnCorrectContent()
    {
        // Act
        var response = await _client.GetAsync("/about");
        var html = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Contains("About Us", html);
    }

    [Fact]
    [Category("Integration")]
    public async Task Get_UnknownRoute_Should_Return404()
    {
        // Act
        var response = await _client.GetAsync("/nonexistent");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
```

### Test Fixture

```csharp
// File: tests/NextNet.Core.Tests/NextNetTestFixture.cs
public class NextNetTestFixture : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            // Replace real services with test doubles
            services.AddScoped<IUserRepository, MockUserRepository>();
        });
    }
}
```

## Testing Middleware

Test middleware in isolation:

```csharp
[Fact]
[Category("Unit")]
public async Task AuthMiddleware_Should_Return401_When_NoToken()
{
    // Arrange
    var middleware = new AuthMiddleware();
    var context = new DefaultHttpContext();
    context.Request.Headers["Authorization"] = "";

    // Act
    await middleware.InvokeAsync(context, _ => Task.CompletedTask);

    // Assert
    Assert.Equal(401, context.Response.StatusCode);
}
```

## Testing Server Actions

```csharp
[Fact]
[Category("Unit")]
public async Task CreateUser_Should_ReturnUser_When_ValidRequest()
{
    // Arrange
    var request = new CreateUserRequest
    {
        Name = "Alice",
        Email = "alice@example.com"
    };

    // Act
    var result = await UserActions.CreateUser(request);

    // Assert
    Assert.NotNull(result);
    Assert.Equal("Alice", result.Name);
}
```

## Snapshot Testing

Use snapshot testing for HTML output verification:

```csharp
[Fact]
[Category("Unit")]
public async Task HomePage_Render_Should_MatchSnapshot()
{
    // Arrange
    var page = new HomePage();

    // Act
    var html = (await page.Render()).ToHtmlString();

    // Assert
    await Verify(html);
}
```

> [!TIP]
> Snapshot testing tools like `Verify` (by VerifyTests) automatically manage
> `.verified.cs` files containing expected HTML output.

## End to End Testing

Use Playwright for browser level testing:

```csharp
// File: tests/NextNet.E2ETests/HomePageTests.cs
public class HomePageTests
{
    [Fact]
    [Category("E2E")]
    public async Task HomePage_Should_DisplayCorrectTitle()
    {
        // Arrange
        using var playwright = await Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync();
        var page = await browser.NewPageAsync();

        // Act
        await page.GotoAsync("http://localhost:3000");
        var title = await page.TitleAsync();

        // Assert
        Assert.Equal("Welcome to NextNet", title);
    }
}
```

## Test Configuration

Create a test configuration file:

```json
// File: nextnet.testing.json
{
  "appDir": "test-app",
  "devPort": 0,
  "ssr": true,
  "rendering": {
    "prettyPrint": true
  }
}
```

Load it in your test fixture:

```csharp
builder.ConfigureAppConfiguration((context, config) =>
{
    config.AddJsonFile("nextnet.testing.json");
});
```

## Test Naming Convention

Follow this pattern for test methods:

```
{MethodName}_Should_{ExpectedBehavior}_When_{Condition}
```

Examples:
- `Render_Should_ReturnHeading_When_Called`
- `Get_Should_Return404_When_UserNotFound`
- `Post_Should_Return400_When_InvalidRequest`

## Running Tests

```bash
# Run all tests
dotnet test

# Run unit tests only
dotnet test --filter "Category=Unit"

# Run integration tests
dotnet test --filter "Category=Integration"

# Run tests for a specific area
dotnet test --filter "FullyQualifiedName~Routing"

# Run with code coverage
dotnet test --collect:"XPlat Code Coverage"
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coverage"
```

## Related

- **Contributing**: [Development Setup](../contributing/development-setup.md)
- **Reference**: [API Reference](../reference/api-reference.md)
- **Concept**: [Architecture](../contributing/architecture.md)
