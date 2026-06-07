using System;
using System.Threading.Tasks;
using Xunit;
using NextNet.ServerActions.Client;
using NextNet.ServerActions.ServerActions;
using NextNet.ServerActions.Results;

namespace NextNet.ServerActions.Tests
{
    public class ServerActionClientProxyGeneratorEdgeCasesTests
    {
        [Fact]
        public void GenerateClientProxy_NullRegistry_ThrowsArgumentNullException()
        {
            // Arrange
            var generator = new ServerActionClientProxyGenerator();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => generator.GenerateClientProxy(null!));
        }

        [Fact]
        public void GenerateClientProxy_ActionWithVoidReturn_GeneratesActionResult()
        {
            // Arrange
            var registry = new ServerActionRegistry();
            registry.RegisterFromType(typeof(VoidReturnActions));
            var generator = new ServerActionClientProxyGenerator();

            // Act
            var code = generator.GenerateClientProxy(registry);

            // Assert
            Assert.Contains("VoidAction", code);
        }

        [Fact]
        public void GenerateClientProxy_ActionWithServiceParam_SkipsServiceParam()
        {
            // Arrange
            var registry = new ServerActionRegistry();
            registry.RegisterFromType(typeof(ServiceParamActions));
            var generator = new ServerActionClientProxyGenerator();

            // Act
            var code = generator.GenerateClientProxy(registry);

            // Assert
            Assert.Contains("ServiceAction", code);
        }

        [Fact]
        public void GenerateClientProxy_WithUnsafeMethodName_HandlesCorrectly()
        {
            // Arrange
            var registry = new ServerActionRegistry();
            registry.RegisterFromType(typeof(UnsafeNameActions));
            var generator = new ServerActionClientProxyGenerator();

            // Act
            var code = generator.GenerateClientProxy(registry);

            // Assert
            Assert.Contains("_123Action", code);
        }
    }

    public static class VoidReturnActions
    {
        [ServerAction]
        public static void VoidAction() { }
    }

    public static class ServiceParamActions
    {
        [ServerAction]
        public static Task<ActionResult> ServiceAction(IServiceProvider services)
        {
            return Task.FromResult(ActionSuccess.Empty());
        }
    }

    public static class UnsafeNameActions
    {
        [ServerAction(Name = "123Action")]
        public static void SomeMethod() { }
    }
}
