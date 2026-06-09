using System.Text.Json;
using NextNet.Isr.Endpoints;

namespace NextNet.Isr.Tests;

public class IsrRevalidationRequestTests
{
    [Fact]
    public void Deserialize_Should_ParseCorrectly_When_PathAndSecretProvided()
    {
        var json = "{\"path\":\"/blog/post-1\",\"secret\":\"my-secret\"}";
        var request = JsonSerializer.Deserialize<IsrRevalidationRequest>(json);

        Assert.NotNull(request);
        Assert.Equal("/blog/post-1", request.Path);
        Assert.Equal("my-secret", request.Secret);
        Assert.Null(request.Tags);
    }

    [Fact]
    public void Deserialize_Should_ParseCorrectly_When_TagsProvided()
    {
        var json = "{\"tags\":[\"blog\",\"news\"],\"secret\":\"abc\"}";
        var request = JsonSerializer.Deserialize<IsrRevalidationRequest>(json);

        Assert.NotNull(request);
        Assert.Null(request.Path);
        Assert.Equal(new[] { "blog", "news" }, request.Tags);
        Assert.Equal("abc", request.Secret);
    }

    [Fact]
    public void Deserialize_Should_Work_When_CamelCaseJsonPropertyNames()
    {
        var json = "{\"path\":\"/about\",\"tags\":null}";
        var request = JsonSerializer.Deserialize<IsrRevalidationRequest>(json);

        Assert.NotNull(request);
        Assert.Equal("/about", request.Path);
        Assert.Null(request.Tags);
    }

    [Fact]
    public void Serialize_Should_PreserveValues_When_RoundTrip()
    {
        var request = new IsrRevalidationRequest
        {
            Path = "/test",
            Tags = new[] { "tag1" },
            Secret = "secret123"
        };

        var json = JsonSerializer.Serialize(request);
        var deserialized = JsonSerializer.Deserialize<IsrRevalidationRequest>(json);

        Assert.NotNull(deserialized);
        Assert.Equal(request.Path, deserialized.Path);
        Assert.Equal(request.Tags, deserialized.Tags);
        Assert.Equal(request.Secret, deserialized.Secret);
    }
}
