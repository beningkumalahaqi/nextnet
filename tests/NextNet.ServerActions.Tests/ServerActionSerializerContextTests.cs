using Xunit;
using NextNet.ServerActions.Serialization;

namespace NextNet.ServerActions.Tests
{
    public class ServerActionSerializerContextTests
    {
        [Fact]
        public void Default_Should_BeAccessible_When_Accessed()
        {
            // Act
            var context = ServerActionSerializerContext.Default;

            // Assert
            Assert.NotNull(context);
        }

        [Fact]
        public void GetTypeInfo_Should_ReturnNotNull_When_ActionResult()
        {
            // Act
            var typeInfo = ServerActionSerializerContext.Default.GetTypeInfo(typeof(Results.ActionResult));

            // Assert
            Assert.NotNull(typeInfo);
        }

        [Fact]
        public void GetTypeInfo_Should_ReturnNotNull_When_String()
        {
            // Act
            var typeInfo = ServerActionSerializerContext.Default.GetTypeInfo(typeof(string));

            // Assert
            Assert.NotNull(typeInfo);
        }

        [Fact]
        public void GetTypeInfo_Should_ReturnNotNull_When_Int32()
        {
            // Act
            var typeInfo = ServerActionSerializerContext.Default.GetTypeInfo(typeof(int));

            // Assert
            Assert.NotNull(typeInfo);
        }

        [Fact]
        public void GetTypeInfo_Should_ReturnNotNull_When_Boolean()
        {
            // Act
            var typeInfo = ServerActionSerializerContext.Default.GetTypeInfo(typeof(bool));

            // Assert
            Assert.NotNull(typeInfo);
        }
    }
}
