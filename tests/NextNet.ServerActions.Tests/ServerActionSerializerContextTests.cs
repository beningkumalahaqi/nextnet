using Xunit;
using NextNet.ServerActions.Serialization;

namespace NextNet.ServerActions.Tests
{
    public class ServerActionSerializerContextTests
    {
        [Fact]
        public void Default_Instance_IsAccessible()
        {
            // Act
            var context = ServerActionSerializerContext.Default;

            // Assert
            Assert.NotNull(context);
        }

        [Fact]
        public void GetTypeInfo_ActionResult_ReturnsNotNull()
        {
            // Act
            var typeInfo = ServerActionSerializerContext.Default.GetTypeInfo(typeof(Results.ActionResult));

            // Assert
            Assert.NotNull(typeInfo);
        }

        [Fact]
        public void GetTypeInfo_String_ReturnsNotNull()
        {
            // Act
            var typeInfo = ServerActionSerializerContext.Default.GetTypeInfo(typeof(string));

            // Assert
            Assert.NotNull(typeInfo);
        }

        [Fact]
        public void GetTypeInfo_Int32_ReturnsNotNull()
        {
            // Act
            var typeInfo = ServerActionSerializerContext.Default.GetTypeInfo(typeof(int));

            // Assert
            Assert.NotNull(typeInfo);
        }

        [Fact]
        public void GetTypeInfo_Boolean_ReturnsNotNull()
        {
            // Act
            var typeInfo = ServerActionSerializerContext.Default.GetTypeInfo(typeof(bool));

            // Assert
            Assert.NotNull(typeInfo);
        }
    }
}
