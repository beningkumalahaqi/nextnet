using System;
using System.Linq;
using System.Reflection;
using Xunit;
using NextNet.ServerActions.ServerActions;

namespace NextNet.ServerActions.Tests
{
    public class ServerActionRegistryTests
    {
        [Fact]
        public void RegisterFromAssembly_WithActionAttributes_RegistersActions()
        {
            // Arrange
            var registry = new ServerActionRegistry();

            // Act
            var count = registry.RegisterFromAssembly(typeof(TestActions).Assembly);

            // Assert
            Assert.NotEqual(0, count);
        }

        [Fact]
        public void RegisterFromType_WithAttributeOnMethod_RegistersAction()
        {
            // Arrange
            var registry = new ServerActionRegistry();

            // Act
            var count = registry.RegisterFromType(typeof(TestActions));

            // Assert
            Assert.True(count > 0);
            Assert.True(registry.TryGetAction("CreateUser", out var descriptor));
            Assert.NotNull(descriptor);
            Assert.Equal("CreateUser", descriptor.ActionName);
        }

        [Fact]
        public void RegisterFromType_WithNamedAttribute_UsesCustomName()
        {
            // Arrange
            var registry = new ServerActionRegistry();

            // Act
            registry.RegisterFromType(typeof(TestActions));

            // Assert
            Assert.True(registry.TryGetAction("named-action", out var descriptor));
            Assert.NotNull(descriptor);
            Assert.Equal("named-action", descriptor.ActionName);
            Assert.Equal("/_actions/named-action", descriptor.Route);
        }

        [Fact]
        public void TryGetAction_UnknownName_ReturnsFalse()
        {
            // Arrange
            var registry = new ServerActionRegistry();

            // Act
            var found = registry.TryGetAction("nonexistent", out var descriptor);

            // Assert
            Assert.False(found);
            Assert.Null(descriptor);
        }

        [Fact]
        public void GetAllActions_ReturnsAllRegisteredActions()
        {
            // Arrange
            var registry = new ServerActionRegistry();
            registry.RegisterFromType(typeof(TestActions));

            // Act
            var allActions = registry.GetAllActions();

            // Assert
            Assert.NotEmpty(allActions);
        }

        [Fact]
        public void Count_ReturnsCorrectCount()
        {
            // Arrange
            var registry = new ServerActionRegistry();
            registry.RegisterFromType(typeof(TestActions));

            // Act
            var count = registry.Count;

            // Assert
            Assert.True(count > 0);
        }

        [Fact]
        public void RegisterFromType_WithInstanceMethod_RegistersAction()
        {
            // Arrange
            var registry = new ServerActionRegistry();

            // Act
            registry.RegisterFromType(typeof(TestInstanceActions));

            // Assert
            Assert.True(registry.TryGetAction("UpdateProfile", out var descriptor));
            Assert.NotNull(descriptor);
            Assert.False(descriptor.IsStatic);
        }

        [Fact]
        public void RegisterFromType_WithRequireAuth_SetsProperty()
        {
            // Arrange
            var registry = new ServerActionRegistry();
            registry.RegisterFromType(typeof(TestActions));
            registry.RegisterFromType(typeof(TestAuthActions));

            // Act
            Assert.True(registry.TryGetAction("AuthAction", out var descriptor));

            // Assert
            Assert.NotNull(descriptor);
            Assert.True(descriptor.RequireAuth);
        }

        [Fact]
        public void RegisterFromType_NullType_ThrowsArgumentNullException()
        {
            // Arrange
            var registry = new ServerActionRegistry();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => registry.RegisterFromType(null!));
        }

        [Fact]
        public void RegisterFromAssembly_NullAssembly_ThrowsArgumentNullException()
        {
            // Arrange
            var registry = new ServerActionRegistry();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => registry.RegisterFromAssembly(null!));
        }

        [Fact]
        public void Clear_RemovesAllActions()
        {
            // Arrange
            var registry = new ServerActionRegistry();
            registry.RegisterFromType(typeof(TestActions));
            Assert.True(registry.Count > 0);

            // Act
            registry.Clear();

            // Assert
            Assert.Equal(0, registry.Count);
        }

        [Fact]
        public void RegisterFromAssembly_WithAttributeOnClass_RegistersAllMethods()
        {
            // Arrange
            var registry = new ServerActionRegistry();

            // Act
            registry.RegisterFromType(typeof(TestActionClass));

            // Assert
            Assert.True(registry.TryGetAction("ClassAction1", out var desc1));
            Assert.NotNull(desc1);
            Assert.True(registry.TryGetAction("ClassAction2", out var desc2));
            Assert.NotNull(desc2);
        }
    }

    // --- Test types used by registry tests ---

    public static class TestActions
    {
        [ServerAction]
        public static object? CreateUser(string name, string email)
        {
            return null;
        }

        [ServerAction(Name = "named-action")]
        public static object? DeleteUser(int id)
        {
            return null;
        }

        [ServerAction(RequireAuth = false)]
        public static object? PublicAction()
        {
            return null;
        }
    }

    public class TestInstanceActions
    {
        [ServerAction]
        public object? UpdateProfile(string name)
        {
            return null;
        }
    }

    [ServerAction]
    public static class TestActionClass
    {
        public static object? ClassAction1() => null;
        public static object? ClassAction2() => null;
    }

    public static class TestAuthActions
    {
        [ServerAction(RequireAuth = true)]
        public static object? AuthAction() => null;
    }
}
