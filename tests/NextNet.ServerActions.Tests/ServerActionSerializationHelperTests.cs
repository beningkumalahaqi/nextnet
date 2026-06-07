using System.Text.Json;
using Xunit;

namespace NextNet.ServerActions.Tests
{
    public class ServerActionSerializationHelperTests
    {
        [Fact]
        public void DefaultOptions_PropertyNameCaseInsensitive_IsTrue()
        {
            // Assert
            Assert.True(ServerActionsSerialization.DefaultOptions.PropertyNameCaseInsensitive);
        }

        [Fact]
        public void DefaultOptions_PropertyNamingPolicy_IsCamelCase()
        {
            // Assert
            Assert.Same(JsonNamingPolicy.CamelCase, ServerActionsSerialization.DefaultOptions.PropertyNamingPolicy);
        }

        [Fact]
        public async System.Threading.Tasks.Task SerializeAsync_WritesToStream()
        {
            // Arrange
            using var stream = new System.IO.MemoryStream();
            var value = new { Test = "value" };

            // Act
            await ServerActionsSerialization.SerializeAsync(stream, value);

            // Assert
            Assert.True(stream.Length > 0);
            stream.Position = 0;
            var json = new System.IO.StreamReader(stream).ReadToEnd();
            Assert.Contains("test", json);
        }
    }
}
