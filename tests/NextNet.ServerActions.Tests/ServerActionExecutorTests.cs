using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using NextNet.ServerActions.ServerActions;
using NextNet.ServerActions.Serialization;
using NextNet.ServerActions.Results;

namespace NextNet.ServerActions.Tests
{
    public class ServerActionExecutorTests
    {
        private static DefaultHttpContext CreateContext(string path = "/_actions/TestAction", string body = "{}")
        {
            var services = new ServiceCollection();
            services.AddLogging();
            var serviceProvider = services.BuildServiceProvider();
            var context = new DefaultHttpContext();
            context.RequestServices = serviceProvider;
            context.Request.Method = "POST";
            context.Request.Path = path;
            context.Request.Body = new MemoryStream(
                System.Text.Encoding.UTF8.GetBytes(body));
            context.Response.Body = new MemoryStream();
            return context;
        }
        [Fact]
        public async Task ExecuteAsync_WithActionResult_WritesResponse()
        {
            // Arrange
            var registry = new ServerActionRegistry();
            registry.RegisterFromType(typeof(ExecutorTestActions));
            var invoker = new ServerActionInvoker();
            var serializer = new ServerActionSerializer();
            var executor = new ServerActionExecutor(registry, invoker, serializer);

            var context = CreateContext();

            // Act
            await executor.ExecuteAsync(context, "TestAction");

            // Assert
            context.Response.Body.Position = 0;
            var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
            Assert.Equal(200, context.Response.StatusCode);
            Assert.False(string.IsNullOrEmpty(body), "Response body was empty. Status: " + context.Response.StatusCode);
            Assert.Contains("\"isSuccess\":true", body);
        }

        [Fact]
        public async Task ExecuteAsync_UnknownAction_Returns404()
        {
            // Arrange
            var registry = new ServerActionRegistry();
            var invoker = new ServerActionInvoker();
            var serializer = new ServerActionSerializer();
            var executor = new ServerActionExecutor(registry, invoker, serializer);

            var context = new DefaultHttpContext();
            context.Request.Method = "POST";
            context.Request.Path = "/_actions/Unknown";
            context.Request.Body = new MemoryStream(
                System.Text.Encoding.UTF8.GetBytes("{}"));

            // Act
            await executor.ExecuteAsync(context, "Unknown");

            // Assert
            Assert.Equal(404, context.Response.StatusCode);
        }

        [Fact]
        public async Task ExecuteAsync_NullContext_ThrowsArgumentNullException()
        {
            // Arrange
            var registry = new ServerActionRegistry();
            var invoker = new ServerActionInvoker();
            var serializer = new ServerActionSerializer();
            var executor = new ServerActionExecutor(registry, invoker, serializer);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                executor.ExecuteAsync(null!, "test"));
        }

        [Fact]
        public async Task ExecuteAsync_NullActionName_ThrowsArgumentException()
        {
            // Arrange
            var registry = new ServerActionRegistry();
            var invoker = new ServerActionInvoker();
            var serializer = new ServerActionSerializer();
            var executor = new ServerActionExecutor(registry, invoker, serializer);

            var context = CreateContext();
            context.Request.Method = "POST";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                executor.ExecuteAsync(context, null!));
        }

        [Fact]
        public async Task ExecuteAsync_EmptyActionName_ThrowsArgumentException()
        {
            // Arrange
            var registry = new ServerActionRegistry();
            var invoker = new ServerActionInvoker();
            var serializer = new ServerActionSerializer();
            var executor = new ServerActionExecutor(registry, invoker, serializer);

            var context = CreateContext();
            context.Request.Method = "POST";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                executor.ExecuteAsync(context, ""));
        }

        [Fact]
        public async Task ExecuteAsync_WithNonActionResult_WrapsInSuccess()
        {
            // Arrange
            var registry = new ServerActionRegistry();
            registry.RegisterFromType(typeof(ExecutorTestActions));
            var invoker = new ServerActionInvoker();
            var serializer = new ServerActionSerializer();
            var executor = new ServerActionExecutor(registry, invoker, serializer);

            var context = CreateContext("/_actions/ReturnString", @"{""value"":""hello""}");

            // Act
            await executor.ExecuteAsync(context, "ReturnString");

            // Assert
            context.Response.Body.Position = 0;
            var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
            Assert.Contains("\"isSuccess\":true", body);
        }

        [Fact]
        public async Task ExecuteAsync_ActionThrows_Returns500()
        {
            // Arrange
            var registry = new ServerActionRegistry();
            registry.RegisterFromType(typeof(ExecutorTestActions));
            var invoker = new ServerActionInvoker();
            var serializer = new ServerActionSerializer();
            var executor = new ServerActionExecutor(registry, invoker, serializer);

            var context = CreateContext("/_actions/ThrowException");

            // Act
            await executor.ExecuteAsync(context, "ThrowException");

            // Assert
            Assert.Equal(500, context.Response.StatusCode);
        }

        [Fact]
        public async Task ExecuteAsync_WithRequireAuth_Unauthenticated_Returns401()
        {
            // Arrange
            var registry = new ServerActionRegistry();
            registry.RegisterFromType(typeof(ExecutorTestAuthActions));
            var invoker = new ServerActionInvoker();
            var serializer = new ServerActionSerializer();
            var executor = new ServerActionExecutor(registry, invoker, serializer);

            var context = CreateContext("/_actions/SecuredAction");

            // Act
            await executor.ExecuteAsync(context, "SecuredAction");

            // Assert
            Assert.Equal(401, context.Response.StatusCode);
        }

        [Fact]
        public async Task ExecuteAsync_WithActionResultReturningIResult_UsesIResult()
        {
            // Arrange
            var registry = new ServerActionRegistry();
            registry.RegisterFromType(typeof(ExecutorTestActions));
            var invoker = new ServerActionInvoker();
            var serializer = new ServerActionSerializer();
            var executor = new ServerActionExecutor(registry, invoker, serializer);

            var context = CreateContext("/_actions/ReturnIResult");

            // Act
            await executor.ExecuteAsync(context, "ReturnIResult");

            // Assert
            Assert.Equal(200, context.Response.StatusCode);
        }

        [Fact]
        public async Task ExecuteAsync_WithFormData_ParsesCorrectly()
        {
            // Arrange
            var registry = new ServerActionRegistry();
            registry.RegisterFromType(typeof(ExecutorTestActions));
            var invoker = new ServerActionInvoker();
            var serializer = new ServerActionSerializer();
            var executor = new ServerActionExecutor(registry, invoker, serializer);

            var context = CreateContext("/_actions/TestAction");
            context.Request.ContentType = "application/json";

            // Act
            await executor.ExecuteAsync(context, "TestAction");

            // Assert
            Assert.Equal(200, context.Response.StatusCode);
        }
    }

    public static class ExecutorTestActions
    {
        [ServerAction]
        public static Task<ActionResult> TestAction()
        {
            return Task.FromResult(ActionSuccess.Empty());
        }

        [ServerAction]
        public static string ReturnString(string value)
        {
            return value;
        }

        [ServerAction]
        public static Task<ActionResult> ThrowException()
        {
            throw new InvalidOperationException("Test exception");
        }

        [ServerAction]
        public static Microsoft.AspNetCore.Http.IResult ReturnIResult()
        {
            return Microsoft.AspNetCore.Http.Results.Ok();
        }
    }

    public static class ExecutorTestAuthActions
    {
        [ServerAction(RequireAuth = true)]
        public static Task<ActionResult> SecuredAction()
        {
            return Task.FromResult(ActionSuccess.Empty());
        }
    }
}
