namespace NextNet.TemplateSecurity.Tests;

using System.Text;
using Xunit;

public class ChecksumVerifierTests
{
    [Fact]
    public async Task ComputeChecksumAsync_Should_ReturnConsistentHash_When_SameData()
    {
        var verifier = new ChecksumVerifier();
        var data = "Hello, World!"u8.ToArray();
        
        using var stream1 = new MemoryStream(data);
        using var stream2 = new MemoryStream(data);
        
        var hash1 = await verifier.ComputeChecksumAsync(stream1);
        var hash2 = await verifier.ComputeChecksumAsync(stream2);
        
        Assert.Equal(hash1, hash2);
        Assert.Equal(64, hash1.Length); // SHA-256 hex is 64 chars
    }

    [Fact]
    public async Task VerifyAsync_Should_ReturnTrue_When_ChecksumMatches()
    {
        var verifier = new ChecksumVerifier();
        var data = "Test data"u8.ToArray();
        
        using var computeStream = new MemoryStream(data);
        var expected = await verifier.ComputeChecksumAsync(computeStream);
        
        using var verifyStream = new MemoryStream(data);
        var isValid = await verifier.VerifyAsync(verifyStream, expected);
        
        Assert.True(isValid);
    }

    [Fact]
    public async Task VerifyAsync_Should_ReturnFalse_When_ChecksumMismatches()
    {
        var verifier = new ChecksumVerifier();
        using var stream = new MemoryStream("data"u8.ToArray());
        var isValid = await verifier.VerifyAsync(stream, "0000000000000000000000000000000000000000000000000000000000000000");
        Assert.False(isValid);
    }
}
