using System;
using Xunit;
using NextNet.ServerActions.Client;
using NextNet.ServerActions.ServerActions;

namespace NextNet.ServerActions.Tests
{
    public class ServerActionClientProxyGeneratorTests
    {
        [Fact]
        public void GenerateClientProxy_Should_GenerateValidCode_When_ActionsRegistered()
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
        public void GenerateClientProxy_Should_IncludeBaseUrl_When_CustomBaseUrlProvided()
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
        public void GenerateClientProxy_Should_UseClassName_When_CustomClassNameProvided()
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
        public void GenerateClientProxy_Should_UseNamespace_When_CustomNamespaceProvided()
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
        public void GenerateClientProxy_Should_CreateAnonymousPayload_When_MultipleParameters()
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
        public void GenerateClientProxy_Should_GenerateEmptyClass_When_NoActions()
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
        public void Constructor_Should_NotThrow_When_NullBaseUrl()
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
