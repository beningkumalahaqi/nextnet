using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using NextNet.ServerActions.ServerActions;
using NextNet.ServerActions.Results;

namespace NextNet.ServerActions.Tests
{
    public class ServerActionInvokerTests
    {
        [Fact]
        public async Task InvokeAsync_Should_ReturnResult_When_StaticMethod()
        {
            // Arrange
            var registry = new ServerActionRegistry();
            registry.RegisterFromType(typeof(TestInvokerActions));
            var invoker = new ServerActionInvoker();

            Assert.True(registry.TryGetAction("Hello", out var descriptor));

            var parameters = new Dictionary<string, object?>
            {
                { "name", "World" }
            };

            var services = new TestServiceProvider();

            // Act
            var result = await invoker.InvokeAsync(descriptor!, parameters, services);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ActionResult<string>>(result);
            var actionResult = (ActionResult<string>)result;
            Assert.True(actionResult.IsSuccess);
            Assert.Equal("Hello, World!", actionResult.Data);
        }

        [Fact]
        public async Task InvokeAsync_Should_ResolveService_When_ServiceInjection()
        {
            // Arrange
            var registry = new ServerActionRegistry();
            registry.RegisterFromType(typeof(TestInvokerActions));
            var invoker = new ServerActionInvoker();

            Assert.True(registry.TryGetAction("GetUserCount", out var descriptor));

            var parameters = new Dictionary<string, object?>();
            var services = new TestServiceProvider();
            services.AddService<IUserService>(new TestUserService { Count = 42 });

            // Act
            var result = await invoker.InvokeAsync(descriptor!, parameters, services);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ActionResult<int>>(result);
            var actionResult = (ActionResult<int>)result;
            Assert.Equal(42, actionResult.Data);
        }

        [Fact]
        public async Task InvokeAsync_Should_PassToken_When_CancellationTokenProvided()
        {
            // Arrange
            var registry = new ServerActionRegistry();
            registry.RegisterFromType(typeof(TestInvokerActions));
            var invoker = new ServerActionInvoker();

            Assert.True(registry.TryGetAction("CheckCancel", out var descriptor));

            var parameters = new Dictionary<string, object?>();
            var services = new TestServiceProvider();
            var cts = new CancellationTokenSource();

            // Act
            var result = await invoker.InvokeAsync(descriptor!, parameters, services, cts.Token);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ActionResult<bool>>(result);
            var actionResult = (ActionResult<bool>)result;
            Assert.False(actionResult.Data); // Token was not cancelled
        }

        [Fact]
        public async Task InvokeAsync_Should_CreateInstanceFromDi_When_InstanceMethod()
        {
            // Arrange
            var registry = new ServerActionRegistry();
            registry.RegisterFromType(typeof(TestInvokerInstanceActions));
            var invoker = new ServerActionInvoker();

            Assert.True(registry.TryGetAction("InstanceMethod", out var descriptor));

            var parameters = new Dictionary<string, object?>
            {
                { "value", 99 }
            };
            var services = new TestServiceProvider();

            // Act
            var result = await invoker.InvokeAsync(descriptor!, parameters, services);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ActionResult<int>>(result);
            var actionResult = (ActionResult<int>)result;
        }

        [Fact]
        public async Task InvokeAsync_Should_ThrowArgumentNullException_When_DescriptorIsNull()
        {
            // Arrange
            var invoker = new ServerActionInvoker();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                invoker.InvokeAsync(null!, new Dictionary<string, object?>(), new TestServiceProvider()));
        }

        [Fact]
        public async Task InvokeAsync_Should_ThrowArgumentNullException_When_ParametersAreNull()
        {
            // Arrange
            var registry = new ServerActionRegistry();
            registry.RegisterFromType(typeof(TestInvokerActions));
            var invoker = new ServerActionInvoker();
            Assert.True(registry.TryGetAction("Hello", out var descriptor));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                invoker.InvokeAsync(descriptor!, null!, new TestServiceProvider()));
        }

        [Fact]
        public async Task InvokeAsync_Should_ThrowArgumentNullException_When_ServiceProviderIsNull()
        {
            // Arrange
            var registry = new ServerActionRegistry();
            registry.RegisterFromType(typeof(TestInvokerActions));
            var invoker = new ServerActionInvoker();
            Assert.True(registry.TryGetAction("Hello", out var descriptor));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                invoker.InvokeAsync(descriptor!, new Dictionary<string, object?>(), null!));
        }
    }

    // --- Test helpers ---

    public static class TestInvokerActions
    {
        [ServerAction]
        public static Task<ActionResult<string>> Hello(string name)
        {
            return Task.FromResult(ActionSuccess.With($"Hello, {name}!"));
        }

        [ServerAction]
        public static Task<ActionResult<int>> GetUserCount(IUserService userService)
        {
            return Task.FromResult(ActionSuccess.With(userService.GetCount()));
        }

        [ServerAction]
        public static Task<ActionResult<bool>> CheckCancel(CancellationToken cancellationToken)
        {
            return Task.FromResult(ActionSuccess.With(cancellationToken.IsCancellationRequested));
        }
    }

    public class TestInvokerInstanceActions
    {
        [ServerAction]
        public Task<ActionResult<int>> InstanceMethod(int value)
        {
            return Task.FromResult(ActionSuccess.With(value));
        }
    }

    public interface IUserService
    {
        int GetCount();
    }

    public class TestUserService : IUserService
    {
        public int Count { get; set; }
        public int GetCount() => Count;
    }

    public class TestServiceProvider : IServiceProvider
    {
        private readonly Dictionary<Type, object> _services = new();

        public void AddService<T>(T service)
        {
            _services[typeof(T)] = service!;
        }

        public object? GetService(Type serviceType)
        {
            if (_services.TryGetValue(serviceType, out var service))
                return service;
            if (serviceType == typeof(IUserService))
                return new TestUserService { Count = 0 };
            return null;
        }
    }
}
