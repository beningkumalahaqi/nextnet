using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using NextNet.ServerActions.ServerActions;
using NextNet.ServerActions.Results;

namespace NextNet.ServerActions.Tests.Integration
{
    /// <summary>
    /// Integration tests covering full server action round-trips:
    /// client → HTTP → middleware → invoker → serializer → response
    /// </summary>
    public class ServerActionIntegrationTests
    {
        [Fact]
        public async Task FullRoundTrip_SimpleAction_ReturnsSuccess()
        {
            // Arrange
            using var server = CreateIntegrationServer();
            var client = server.CreateClient();

            // Act
            var response = await client.PostAsync("/_actions/Greet",
                new StringContent(
                    JsonSerializer.Serialize(new { name = "Alice" }),
                    Encoding.UTF8,
                    "application/json"));

            // Assert
            var body = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = JsonSerializer.Deserialize<ActionResult<GreetingResponse>>(body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal("Hello, Alice!", result.Data.Greeting);
        }

        [Fact]
        public async Task FullRoundTrip_ValidationError_Returns400()
        {
            // Arrange
            using var server = CreateIntegrationServer();
            var client = server.CreateClient();

            // Act
            var response = await client.PostAsync("/_actions/Greet",
                new StringContent(
                    JsonSerializer.Serialize(new { name = "" }),
                    Encoding.UTF8,
                    "application/json"));

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            Assert.Contains("\"isSuccess\":false", body);
            Assert.Contains("validation", body);
        }

        [Fact]
        public async Task FullRoundTrip_WithServiceInjection_ResolvesService()
        {
            // Arrange
            using var server = CreateIntegrationServer();
            var client = server.CreateClient();

            // Act
            var response = await client.PostAsync("/_actions/GetTimestamp",
                new StringContent("{}", Encoding.UTF8, "application/json"));

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ActionResult<string>>(body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.False(string.IsNullOrEmpty(result.Data));
        }

        [Fact]
        public async Task FullRoundTrip_MultipleParameters_WorksCorrectly()
        {
            // Arrange
            using var server = CreateIntegrationServer();
            var client = server.CreateClient();

            // Act
            var response = await client.PostAsync("/_actions/Add",
                new StringContent(
                    JsonSerializer.Serialize(new { a = 5, b = 3 }),
                    Encoding.UTF8,
                    "application/json"));

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ActionResult<int>>(body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.Equal(8, result.Data);
        }

        [Fact]
        public async Task FullRoundTrip_EmptyPayload_ReturnsSuccess()
        {
            // Arrange
            using var server = CreateIntegrationServer();
            var client = server.CreateClient();

            // Act
            var response = await client.PostAsync("/_actions/Ping",
                new StringContent("{}", Encoding.UTF8, "application/json"));

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            Assert.Contains("\"isSuccess\":true", body);
        }

        [Fact]
        public async Task NonExistentAction_Returns404()
        {
            // Arrange
            using var server = CreateIntegrationServer();
            var client = server.CreateClient();

            // Act
            var response = await client.PostAsync("/_actions/DoesNotExist",
                new StringContent("{}", Encoding.UTF8, "application/json"));

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task ClientProxy_GeneratedClient_WorksCorrectly()
        {
            // Arrange
            using var server = CreateIntegrationServer();
            var client = server.CreateClient();

            // Simulate generated client proxy calling the action
            var jsonPayload = System.Text.Json.JsonSerializer.Serialize(new { name = "Bob" });
            var response = await client.PostAsync("/_actions/Greet",
                new StringContent(jsonPayload, Encoding.UTF8, "application/json"));

            var body = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = System.Text.Json.JsonSerializer.Deserialize<ActionResult<GreetingResponse>>(
                body, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.Equal("Hello, Bob!", result.Data?.Greeting);
        }

        private static TestServer CreateIntegrationServer()
        {
            var builder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddNextNetServerActions();
                    services.AddSingleton<ITimeService, SystemTimeService>();
                })
                .Configure(app =>
                {
                    var registry = app.ApplicationServices.GetRequiredService<ServerActionRegistry>();
                    registry.RegisterFromType(typeof(IntegrationTestActions));

                    app.UseNextNetServerActions();

                    app.Run(async context =>
                    {
                        await context.Response.WriteAsync("not-found");
                    });
                });

            return new TestServer(builder);
        }
    }

    // --- Integration test action types ---

    public static class IntegrationTestActions
    {
        [ServerAction]
        public static Task<ActionResult<GreetingResponse>> Greet(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Task.FromResult(ActionError.Validation<GreetingResponse>("Name is required"));

            return Task.FromResult(
                ActionSuccess.With(new GreetingResponse($"Hello, {name}!")));
        }

        [ServerAction]
        public static Task<ActionResult<string>> GetTimestamp(ITimeService timeService)
        {
            return Task.FromResult(
                ActionSuccess.With(timeService.GetCurrentTimestamp()));
        }

        [ServerAction]
        public static Task<ActionResult<int>> Add(int a, int b)
        {
            return Task.FromResult(ActionSuccess.With(a + b));
        }

        [ServerAction]
        public static Task<ActionResult> Ping()
        {
            return Task.FromResult(ActionSuccess.Empty());
        }
    }

    public class GreetingResponse
    {
        public string Greeting { get; set; } = string.Empty;

        public GreetingResponse() { }

        public GreetingResponse(string greeting)
        {
            Greeting = greeting;
        }
    }

    public interface ITimeService
    {
        string GetCurrentTimestamp();
    }

    public class SystemTimeService : ITimeService
    {
        public string GetCurrentTimestamp() => DateTime.UtcNow.ToString("O");
    }
}
