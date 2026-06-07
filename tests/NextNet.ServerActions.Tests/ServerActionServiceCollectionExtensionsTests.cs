using System;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using NextNet.ServerActions.ServerActions;

namespace NextNet.ServerActions.Tests
{
    public class ServerActionServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddNextNetServerActions_RegistersServices()
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
        public void AddNextNetServerActions_WithAutoDiscovery_RegistersActions()
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
        public void AddNextNetServerActions_NullServices_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                ((IServiceCollection)null!).AddNextNetServerActions());
        }

        [Fact]
        public void ServerActionOptions_DefaultValues()
        {
            // Arrange
            var options = new ServerActionOptions();

            // Assert
            Assert.Null(options.AutoDiscoverAssemblies);
        }
    }
}
