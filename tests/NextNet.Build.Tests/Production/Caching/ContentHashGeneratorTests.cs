using NextNet.Build.Production.Caching;
using Xunit;

namespace NextNet.Build.Tests.Production.Caching;

public class ContentHashGeneratorTests
{
    private readonly ContentHashGenerator _generator = new();

    [Fact]
    public void GenerateHash_SameContent_ReturnsSameHash()
    {
        var content = "Hello, World!";
        var hash1 = _generator.GenerateHash(content);
        var hash2 = _generator.GenerateHash(content);
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void GenerateHash_DifferentContent_ReturnsDifferentHash()
    {
        var hash1 = _generator.GenerateHash("Hello, World!");
        var hash2 = _generator.GenerateHash("Goodbye, World!");
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void GenerateHash_EmptyContent_ReturnsEmpty()
    {
        Assert.Equal(string.Empty, _generator.GenerateHash(""));
    }

    [Fact]
    public void GenerateHash_NullContent_ReturnsEmpty()
    {
        Assert.Equal(string.Empty, _generator.GenerateHash((string)null!));
    }

    [Fact]
    public void GenerateHash_ReturnsCorrectLength()
    {
        var hash = _generator.GenerateHash("test content");
        Assert.Equal(8, hash.Length);
    }

    [Fact]
    public void GenerateHash_CustomLength_ReturnsCorrectLength()
    {
        var generator = new ContentHashGenerator(16);
        var hash = generator.GenerateHash("test content");
        Assert.Equal(16, hash.Length);
    }

    [Fact]
    public void GenerateETag_ReturnsQuotedHash()
    {
        var etag = _generator.GenerateETag("test content");
        Assert.StartsWith("\"", etag);
        Assert.EndsWith("\"", etag);
    }

    [Fact]
    public void HashFileName_AddsHashToFilename()
    {
        var content = System.Text.Encoding.UTF8.GetBytes("file content");
        var hashed = _generator.HashFileName("styles.css", content);
        Assert.Matches(@"^styles\.[a-f0-9]+\.css$", hashed);
    }

    [Fact]
    public void HashFileName_SameContent_SameHashedName()
    {
        var content = System.Text.Encoding.UTF8.GetBytes("same content");
        var name1 = _generator.HashFileName("app.js", content);
        var name2 = _generator.HashFileName("app.js", content);
        Assert.Equal(name1, name2);
    }

    [Fact]
    public void HashFileName_DifferentContent_DifferentHashedName()
    {
        var content1 = System.Text.Encoding.UTF8.GetBytes("content one");
        var content2 = System.Text.Encoding.UTF8.GetBytes("content two");
        var name1 = _generator.HashFileName("app.js", content1);
        var name2 = _generator.HashFileName("app.js", content2);
        Assert.NotEqual(name1, name2);
    }
}
