using System.Security.Cryptography;
using System.Text;
using Moq;
using NextNet.Isr.Revalidation;

namespace NextNet.Isr.Tests;

public class WebhookRevalidatorTests
{
    private readonly IsrGlobalOptions _options;
    private readonly Mock<IIsrRevalidationManager> _manager;

    public WebhookRevalidatorTests()
    {
        _options = new IsrGlobalOptions { WebhookSecret = "whsec_test" };
        _manager = new Mock<IIsrRevalidationManager>(MockBehavior.Strict);
    }

    [Fact]
    public void VerifySignature_Should_ReturnTrue_When_SignatureIsValid()
    {
        var body = Encoding.UTF8.GetBytes("{\"event\":\"update\"}");
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes("whsec_test"));
        var hash = hmac.ComputeHash(body);
        var signature = "sha256=" + Convert.ToHexString(hash).ToLowerInvariant();

        var revalidator = new WebhookRevalidator(_manager.Object, _options);
        Assert.True(revalidator.VerifySignature(body, signature));
    }

    [Fact]
    public void VerifySignature_Should_ReturnFalse_When_SignatureIsInvalid()
    {
        var body = Encoding.UTF8.GetBytes("{\"event\":\"update\"}");

        var revalidator = new WebhookRevalidator(_manager.Object, _options);
        Assert.False(revalidator.VerifySignature(body, "sha256=invalid"));
    }

    [Fact]
    public void VerifySignature_Should_ReturnTrue_When_NoSecretConfigured()
    {
        var options = new IsrGlobalOptions { WebhookSecret = null };
        var body = Encoding.UTF8.GetBytes("test");

        var revalidator = new WebhookRevalidator(_manager.Object, options);
        Assert.True(revalidator.VerifySignature(body, null));
    }

    [Fact]
    public void VerifySignature_Should_ReturnFalse_When_SignatureIsNull()
    {
        var body = Encoding.UTF8.GetBytes("test");

        var revalidator = new WebhookRevalidator(_manager.Object, _options);
        Assert.False(revalidator.VerifySignature(body, null));
    }

    [Fact]
    public void VerifySignature_Should_ReturnFalse_When_SignatureIsEmpty()
    {
        var body = Encoding.UTF8.GetBytes("test");

        var revalidator = new WebhookRevalidator(_manager.Object, _options);
        Assert.False(revalidator.VerifySignature(body, ""));
    }

    [Fact]
    public void VerifySignature_Should_ReturnFalse_When_BodyIsNull()
    {
        var revalidator = new WebhookRevalidator(_manager.Object, _options);
        Assert.False(revalidator.VerifySignature(null!, "sha256=abc"));
    }

    [Fact]
    public void VerifySignature_Should_ReturnFalse_When_BodyIsEmpty()
    {
        var revalidator = new WebhookRevalidator(_manager.Object, _options);
        Assert.False(revalidator.VerifySignature(Array.Empty<byte>(), "sha256=abc"));
    }

    [Fact]
    public async Task ProcessWebhookAsync_Should_RevalidateRoutes_When_SignatureIsValid()
    {
        var body = Encoding.UTF8.GetBytes("{\"event\":\"update\"}");
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes("whsec_test"));
        var hash = hmac.ComputeHash(body);
        var signature = "sha256=" + Convert.ToHexString(hash).ToLowerInvariant();

        _manager.Setup(m => m.RevalidateAsync("/blog/post-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(RevalidationResult.Ok("/blog/post-1"));
        _manager.Setup(m => m.RevalidateAsync("/blog/post-2", It.IsAny<CancellationToken>()))
            .ReturnsAsync(RevalidationResult.Ok("/blog/post-2"));

        var revalidator = new WebhookRevalidator(_manager.Object, _options);
        var result = await revalidator.ProcessWebhookAsync(
            body, signature,
            routes: new[] { "/blog/post-1", "/blog/post-2" },
            tags: null);

        Assert.True(result.Success);
        Assert.Equal(2, result.RevalidatedCount);
    }

    [Fact]
    public async Task ProcessWebhookAsync_Should_ReturnFailure_When_SignatureIsInvalid()
    {
        var body = Encoding.UTF8.GetBytes("test");

        var revalidator = new WebhookRevalidator(_manager.Object, _options);
        var result = await revalidator.ProcessWebhookAsync(
            body, "sha256=invalid",
            routes: new[] { "/test" },
            tags: null);

        Assert.False(result.Success);
        Assert.Equal("[DS-318] Invalid webhook signature.", result.ErrorMessage);
    }

    [Fact]
    public async Task ProcessWebhookAsync_Should_InvalidateByTags_When_TagsProvided()
    {
        var body = Encoding.UTF8.GetBytes("{}");
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes("whsec_test"));
        var hash = hmac.ComputeHash(body);
        var signature = "sha256=" + Convert.ToHexString(hash).ToLowerInvariant();

        _manager.Setup(m => m.InvalidateByTagsAsync(
                new[] { "blog" }, It.IsAny<CancellationToken>()))
            .ReturnsAsync(RevalidationResult.Ok(new[] { "/blog/post-1" }));

        var revalidator = new WebhookRevalidator(_manager.Object, _options);
        var result = await revalidator.ProcessWebhookAsync(
            body, signature,
            routes: null,
            tags: new[] { "blog" });

        Assert.True(result.Success);
        Assert.Equal(1, result.RevalidatedCount);
    }
}
