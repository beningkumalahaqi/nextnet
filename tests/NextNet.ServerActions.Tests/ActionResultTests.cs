using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Xunit;
using NextNet.ServerActions.Results;

namespace NextNet.ServerActions.Tests
{
    public class ActionResultTests
    {
        [Fact]
        public void ActionSuccess_Should_SetProperties_When_WithCalled()
        {
            // Act
            var result = ActionSuccess.With("test-data");

            // Assert
            Assert.True(result.IsSuccess);
            Assert.False(result.IsError);
            Assert.Equal("test-data", result.Data);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public void ActionSuccess_Should_SetMessage_When_WithCalledWithMessage()
        {
            // Act
            var result = ActionSuccess.With(42, "the answer");

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(42, result.Data);
            Assert.Equal("the answer", result.Message);
        }

        [Fact]
        public void ActionSuccess_Should_ReturnNonGenericResult_When_EmptyCalled()
        {
            // Act
            var result = ActionSuccess.Empty();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.False(result.IsError);
            Assert.IsAssignableFrom<ActionResult>(result);
        }

        [Fact]
        public void ActionSuccess_Should_SetStatusCode_When_WithStatusCalled()
        {
            // Act
            var result = ActionSuccess.WithStatus(201, "Created");

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(201, result.StatusCode);
            Assert.Equal("Created", result.Message);
        }

        [Fact]
        public void ActionError_Validation_Should_SetCorrectProperties_When_WithMessage()
        {
            // Act
            var result = ActionError.Validation("Email is required");

            // Assert
            Assert.False(result.IsSuccess);
            Assert.True(result.IsError);
            Assert.Equal("validation", result.ErrorType);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Email is required", result.Message);
        }

        [Fact]
        public void ActionError_Validation_Should_SetCorrectProperties_When_GenericWithMessage()
        {
            // Act
            var result = ActionError.Validation<string>("Name is required");

            // Assert
            Assert.False(result.IsSuccess);
            Assert.True(result.IsError);
            Assert.Equal("validation", result.ErrorType);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public void ActionError_Validation_Should_SetFieldErrors_When_WithErrors()
        {
            // Arrange
            var errors = new Dictionary<string, string[]>
            {
                { "Email", new[] { "Email is required" } }
            };

            // Act
            var result = ActionError.Validation<string>(errors);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.True(result.IsError);
            Assert.Equal("validation", result.ErrorType);
            Assert.NotNull(result.Errors);
            Assert.Contains("Email", result.Errors.Keys);
        }

        [Fact]
        public void ActionError_NotFound_Should_SetCorrectProperties_When_Called()
        {
            // Act
            var result = ActionError.NotFound("User not found");

            // Assert
            Assert.False(result.IsSuccess);
            Assert.True(result.IsError);
            Assert.Equal("notFound", result.ErrorType);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("User not found", result.Message);
        }

        [Fact]
        public void ActionError_NotFound_Should_SetCorrectProperties_When_NonGenericCalled()
        {
            // Act
            var result = ActionError.NotFound("Item not found");

            // Assert
            Assert.False(result.IsSuccess);
            Assert.True(result.IsError);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public void ActionError_Unauthorized_Should_SetCorrectProperties_When_Called()
        {
            // Act
            var result = ActionError.Unauthorized("Access denied");

            // Assert
            Assert.False(result.IsSuccess);
            Assert.True(result.IsError);
            Assert.Equal("unauthorized", result.ErrorType);
            Assert.Equal(401, result.StatusCode);
        }

        [Fact]
        public void ActionError_Unauthorized_Should_SetCorrectProperties_When_NonGenericCalled()
        {
            // Act
            var result = ActionError.Unauthorized();

            // Assert
            Assert.False(result.IsSuccess);
            Assert.True(result.IsError);
            Assert.Equal(401, result.StatusCode);
        }

        [Fact]
        public void ActionError_Error_Should_SetCorrectProperties_When_Called()
        {
            // Act
            var result = ActionError.Error("Something went wrong");

            // Assert
            Assert.False(result.IsSuccess);
            Assert.True(result.IsError);
            Assert.Equal("error", result.ErrorType);
            Assert.Equal(500, result.StatusCode);
        }

        [Fact]
        public void ActionError_Error_Should_UseCallerMessage_When_WithException()
        {
            // Act
            var result = ActionError.Error("Failed", new System.Exception("Inner details"));

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Failed", result.Message);
        }

        [Fact]
        public void ActionError_Error_Should_SetCorrectProperties_When_GenericCalled()
        {
            // Act
            var result = ActionError.Error<string>("Critical failure");

            // Assert
            Assert.False(result.IsSuccess);
            Assert.True(result.IsError);
            Assert.Equal("error", result.ErrorType);
            Assert.Equal(500, result.StatusCode);
        }

        [Fact]
        public async Task ActionResult_WriteAsync_Should_SetResponseHeaders_When_Written()
        {
            // Arrange
            var result = ActionSuccess.With("test");
            var context = new DefaultHttpContext();
            context.Response.Body = new System.IO.MemoryStream();

            // Act
            await result.WriteAsync(context);

            // Assert
            Assert.Equal(200, context.Response.StatusCode);
            Assert.Contains("application/json", context.Response.ContentType?.ToString());
        }
    }
}
