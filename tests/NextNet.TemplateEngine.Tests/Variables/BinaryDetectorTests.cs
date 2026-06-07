namespace NextNet.TemplateEngine.Tests.Variables;

using NextNet.TemplateEngine.Variables;
using Xunit;

public class BinaryDetectorTests
{
    [Fact]
    public void IsBinary_Should_ReturnTrue_When_NullBytePresent()
    {
        var content = new byte[] { 0x48, 0x00, 0x65, 0x6C, 0x6C, 0x6F }; // "H\0ello"
        Assert.True(BinaryDetector.IsBinary(content));
    }

    [Fact]
    public void IsBinary_Should_ReturnTrue_When_HighEntropy()
    {
        var random = new Random(42);
        var content = new byte[1024];
        random.NextBytes(content);
        Assert.True(BinaryDetector.IsBinary(content));
    }

    [Fact]
    public void IsBinary_Should_ReturnFalse_When_PlainText()
    {
        var content = System.Text.Encoding.UTF8.GetBytes("Hello, World! This is plain text.");
        Assert.False(BinaryDetector.IsBinary(content));
    }

    [Fact]
    public void IsBinary_Should_ReturnFalse_When_Utf8Text()
    {
        // Use a long text with mostly ASCII and a few accented characters
        // so the non-printable ratio stays below the 30% threshold
        var text = "Hello, World! This is a long text with some accented characters like " +
                   "Café résumé naïve and über cool österreichische Schönheit. " +
                   "Most of this text is plain ASCII which should keep the ratio low. " +
                   "We need enough ASCII padding to ensure the binary detector " +
                   "does not incorrectly classify it as binary.";
        var content = System.Text.Encoding.UTF8.GetBytes(text);
        Assert.False(BinaryDetector.IsBinary(content));
    }

    [Fact]
    public void IsBinary_Should_ReturnFalse_When_Empty()
    {
        Assert.False(BinaryDetector.IsBinary(Array.Empty<byte>()));
    }

    [Fact]
    public void IsKnownBinaryExtension_Should_ReturnTrue_ForPng()
    {
        Assert.True(BinaryDetector.IsKnownBinaryExtension(".png"));
    }

    [Fact]
    public void IsKnownBinaryExtension_Should_ReturnTrue_WithoutLeadingDot()
    {
        Assert.True(BinaryDetector.IsKnownBinaryExtension("jpg"));
    }

    [Fact]
    public void IsKnownBinaryExtension_Should_ReturnFalse_ForCs()
    {
        Assert.False(BinaryDetector.IsKnownBinaryExtension(".cs"));
    }

    [Fact]
    public void IsBinary_Should_Throw_When_ContentIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => BinaryDetector.IsBinary(null!));
    }
}
