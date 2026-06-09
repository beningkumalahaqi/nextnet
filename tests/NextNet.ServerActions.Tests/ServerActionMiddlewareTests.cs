using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using NextNet.ServerActions.Middleware;
using NextNet.ServerActions.ServerActions;
using NextNet.ServerActions.Results;

namespace NextNet.ServerActions.Tests
{
    public class ServerActionMiddlewareTests
    {
        [Fact]
        public async Task InvokeAsync_Should_ReturnSuccess_When_ValidAction()
        {
            // Arrange
            using var server = CreateTestServer();
            var client = server.CreateClient();

            // Act
            var response = await client.PostAsync("/_actions/Echo",
                new StringContent(
                    JsonSerializer.Serialize(new { message = "Hello" }),
                    Encoding.UTF8,
                    "application/json"));

            // Assert
            var body = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("\"isSuccess\":true", body);
        }

        [Fact]
        public async Task InvokeAsync_Should_Return404_When_UnknownAction()
        {
            // Arrange
            using var server = CreateTestServer();
            var client = server.CreateClient();

            // Act
            var response = await client.PostAsync("/_actions/UnknownAction",
                new StringContent("{}", Encoding.UTF8, "application/json"));

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            Assert.Contains("\"isSuccess\":false", body);
            Assert.Contains("notFound", body);
        }

        [Fact]
        public async Task InvokeAsync_Should_PassThrough_When_GetRequest()
        {
            // Arrange
            using var server = CreateTestServer();
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync("/_actions/Echo");

            // Assert
            // GET requests to /_actions/ should pass through (not handled by middleware)
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task InvokeAsync_Should_PassThrough_When_NonActionPath()
        {
            // Arrange
            using var server = CreateTestServer();
            var client = server.CreateClient();

            // Act
            var response = await client.PostAsync("/api/test",
                new StringContent("{}", Encoding.UTF8, "application/json"));

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            Assert.Equal("pass-through", body);
        }

        [Fact]
        public async Task InvokeAsync_Should_Return400_When_WithoutActionName()
        {
            // Arrange
            using var server = CreateTestServer();
            var client = server.CreateClient();

            // Act
            var response = await client.PostAsync("/_actions/",
                new StringContent("{}", Encoding.UTF8, "application/json"));

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task InvokeAsync_Should_ReturnStatusCode_When_ActionResult()
        {
            // Arrange
            using var server = CreateTestServer();
            var client = server.CreateClient();

            // Act
            var response = await client.PostAsync("/_actions/Fail",
                new StringContent("{}", Encoding.UTF8, "application/json"));

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            Assert.Contains("validation", body);
        }

        private static TestServer CreateTestServer()
        {
            var builder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddNextNetServerActions();
                })
                .Configure(app =>
                {
                    // Register test actions
                    var registry = app.ApplicationServices.GetRequiredService<ServerActionRegistry>();
                    registry.RegisterFromType(typeof(TestMiddlewareActions));

                    app.UseNextNetServerActions();

                    // Pass-through endpoint for testing
                    app.Run(async context =>
                    {
                        await context.Response.WriteAsync("pass-through");
                    });
                });

            return new TestServer(builder);
        }
    }

    public static class TestMiddlewareActions
    {
        [ServerAction]
        public static Task<ActionResult<object>> Echo(string message)
        {
            return Task.FromResult(ActionSuccess.With<object>(new { message }, "Echoed"));
        }

        [ServerAction]
        public static Task<ActionResult> Fail()
        {
            return Task.FromResult(ActionError.Validation("Intentional failure"));
        }
    }
}
