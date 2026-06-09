using System;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using NextNet.ServerActions.ServerActions;

namespace NextNet.ServerActions.Tests
{
    public class ServerActionServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddNextNetServerActions_Should_RegisterServices_When_Called()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddNextNetServerActions();
            var provider = services.BuildServiceProvider();

            // Assert
            Assert.NotNull(provider.GetService<ServerActionRegistry>());
            Assert.NotNull(provider.GetService<ServerActionInvoker>());
            Assert.NotNull(provider.GetService<ServerActionExecutor>());
        }

        [Fact]
        public void AddNextNetServerActions_Should_RegisterActions_When_AutoDiscoveryEnabled()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddNextNetServerActions(options =>
            {
                options.AutoDiscoverAssemblies = new[] { typeof(ServerActionServiceCollectionExtensionsTests).Assembly };
            });

            // Assert - Should not throw
            var provider = services.BuildServiceProvider();
            var registry = provider.GetRequiredService<ServerActionRegistry>();
            Assert.NotNull(registry);
        }

        [Fact]
        public void AddNextNetServerActions_Should_ThrowArgumentNullException_When_ServicesIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                ((IServiceCollection)null!).AddNextNetServerActions());
        }

        [Fact]
        public void ServerActionOptions_Should_HaveNullDefault_When_AutoDiscoverAssembliesNotSet()
        {
            // Arrange
            var options = new ServerActionOptions();

            // Assert
            Assert.Null(options.AutoDiscoverAssemblies);
        }
    }
}
