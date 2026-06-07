using System;
using Xunit;
using NextNet.ServerActions.Client;
using NextNet.ServerActions.ServerActions;

namespace NextNet.ServerActions.Tests
{
    public class ServerActionClientProxyGeneratorTests
    {
        [Fact]
        public void GenerateClientProxy_WithActions_GeneratesValidCode()
        {
            // Arrange
            var registry = new ServerActionRegistry();
            registry.RegisterFromType(typeof(ProxyTestActions));
            var generator = new ServerActionClientProxyGenerator();

            // Act
            var code = generator.GenerateClientProxy(registry);

            // Assert
            Assert.Contains("class ServerActionsClient", code);
            Assert.Contains("HelloWorld", code);
            Assert.Contains("ActionResult<string>", code);
            Assert.Contains("PostAsJsonAsync", code);
        }

        [Fact]
        public void GenerateClientProxy_WithCustomBaseUrl_IncludesBaseUrl()
        {
            // Arrange
            var registry = new ServerActionRegistry();
            registry.RegisterFromType(typeof(ProxyTestActions));
            var generator = new ServerActionClientProxyGenerator("https://example.com");

            // Act
            var code = generator.GenerateClientProxy(registry);

            // Assert
            Assert.Contains("https://example.com", code);
        }

        [Fact]
        public void GenerateClientProxy_WithCustomClassName_UsesClassName()
        {
            // Arrange
            var registry = new ServerActionRegistry();
            registry.RegisterFromType(typeof(ProxyTestActions));
            var generator = new ServerActionClientProxyGenerator();

            // Act
            var code = generator.GenerateClientProxy(registry, "MyCustomClient");

            // Assert
            Assert.Contains("class MyCustomClient", code);
        }

        [Fact]
        public void GenerateClientProxy_WithCustomNamespace_UsesNamespace()
        {
            // Arrange
            var registry = new ServerActionRegistry();
            registry.RegisterFromType(typeof(ProxyTestActions));
            var generator = new ServerActionClientProxyGenerator();

            // Act
            var code = generator.GenerateClientProxy(registry, "ActionsClient", "MyApp.Client");

            // Assert
            Assert.Contains("namespace MyApp.Client", code);
        }

        [Fact]
        public void GenerateClientProxy_WithMultipleParameters_CreatesAnonymousPayload()
        {
            // Arrange
            var registry = new ServerActionRegistry();
            registry.RegisterFromType(typeof(ProxyTestActionsWithMultipleParams));
            var generator = new ServerActionClientProxyGenerator();

            // Act
            var code = generator.GenerateClientProxy(registry);

            // Assert
            Assert.Contains("new { ", code);
            Assert.Contains("firstName", code);
            Assert.Contains("lastName", code);
        }

        [Fact]
        public void GenerateClientProxy_WithNoActions_GeneratesEmptyClass()
        {
            // Arrange
            var registry = new ServerActionRegistry();
            var generator = new ServerActionClientProxyGenerator();

            // Act
            var code = generator.GenerateClientProxy(registry);

            // Assert
            Assert.Contains("class ServerActionsClient", code);
        }

        [Fact]
        public void Constructor_NullBaseUrl_DoesNotThrow()
        {
            // Act
            var generator = new ServerActionClientProxyGenerator(null!);

            // Assert
            Assert.NotNull(generator);
        }
    }

    public static class ProxyTestActions
    {
        [ServerAction]
        public static string HelloWorld(string name)
        {
            return $"Hello, {name}!";
        }
    }

    public static class ProxyTestActionsWithMultipleParams
    {
        [ServerAction]
        public static string FullName(string firstName, string lastName)
        {
            return $"{firstName} {lastName}";
        }
    }
}
