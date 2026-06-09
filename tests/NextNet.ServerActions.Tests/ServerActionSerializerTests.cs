using System;
using System.Collections.Generic;
using Xunit;
using NextNet.ServerActions.Serialization;
using NextNet.ServerActions.Results;

namespace NextNet.ServerActions.Tests
{
    public class ServerActionSerializerTests
    {
        private readonly ServerActionSerializer _serializer = new();

        [Fact]
        public void Serialize_Should_ReturnValidJson_When_SuccessResult()
        {
            // Arrange
            var result = ActionSuccess.With(new { Name = "Test", Value = 42 }, "Success");

            // Act
            var json = _serializer.Serialize(result);

            // Assert
            Assert.Contains("\"isSuccess\":true", json);
            Assert.Contains("\"data\"", json);
            Assert.Contains("Test", json);
        }

        [Fact]
        public void Serialize_Should_ReturnJson_When_EmptyResult()
        {
            // Arrange
            var result = ActionSuccess.Empty();

            // Act
            var json = _serializer.Serialize(result);

            // Assert
            Assert.Contains("\"isSuccess\":true", json);
        }

        [Fact]
        public void Serialize_Should_ReturnErrorJson_When_ValidationError()
        {
            // Arrange
            var result = ActionError.Validation("Email is required");

            // Act
            var json = _serializer.Serialize(result);

            // Assert
            Assert.Contains("\"isSuccess\":false", json);
            Assert.Contains("\"isError\":true", json);
            Assert.Contains("\"errorType\":\"validation\"", json);
            Assert.Contains("Email is required", json);
        }

        [Fact]
        public void Serialize_Should_ReturnNotFoundJson_When_NotFoundError()
        {
            // Arrange
            var result = ActionError.NotFound("User not found");

            // Act
            var json = _serializer.Serialize(result);

            // Assert
            Assert.Contains("\"isSuccess\":false", json);
            Assert.Contains("\"errorType\":\"notFound\"", json);
            Assert.Contains("User not found", json);
        }

        [Fact]
        public void Serialize_Should_ReturnUnauthorizedJson_When_UnauthorizedError()
        {
            // Arrange
            var result = ActionError.Unauthorized("Access denied");

            // Act
            var json = _serializer.Serialize(result);

            // Assert
            Assert.Contains("\"isSuccess\":false", json);
            Assert.Contains("\"errorType\":\"unauthorized\"", json);
            Assert.Contains("Access denied", json);
        }

        [Fact]
        public void Serialize_Should_ReturnErrorJson_When_GenericError()
        {
            // Arrange
            var result = ActionError.Error("Something went wrong", new Exception("Inner error"));

            // Act
            var json = _serializer.Serialize(result);

            // Assert
            Assert.Contains("\"isSuccess\":false", json);
            Assert.Contains("\"errorType\":\"error\"", json);
            Assert.Contains("Something went wrong", json);
        }

        [Fact]
        public void Serialize_Should_IncludeErrors_When_FieldValidationErrors()
        {
            // Arrange
            var errors = new Dictionary<string, string[]>
            {
                { "Email", new[] { "Email is required" } },
                { "Name", new[] { "Name is too short" } }
            };
            var result = ActionError.Validation<string>(errors);

            // Act
            var json = _serializer.Serialize(result);

            // Assert
            Assert.Contains("\"errors\"", json);
            Assert.Contains("Email is required", json);
            Assert.Contains("Name is too short", json);
        }

        [Fact]
        public void DeserializeResult_Should_ReturnTypedResult_When_Generic()
        {
            // Arrange
            var json = @"{""isSuccess"":true,""isError"":false,""data"":""test-value""}";

            // Act
            var result = _serializer.DeserializeResult<string>(json);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsSuccess, "IsSuccess should be true. JSON: " + json);
            Assert.Equal("test-value", result.Data);
        }



        [Fact]
        public void DeserializeParameters_Should_ReturnDictionary_When_ValidJson()
        {
            // Arrange
            var json = @"{""name"":""Alice"",""age"":30}";

            // Act
            var parameters = _serializer.DeserializeParameters(json);

            // Assert
            Assert.NotNull(parameters);
            Assert.True(parameters.ContainsKey("name"));
            Assert.True(parameters.ContainsKey("age"));
        }
    }
}
