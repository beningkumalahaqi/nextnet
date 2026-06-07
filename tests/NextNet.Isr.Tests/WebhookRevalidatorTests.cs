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
    public void VerifySignature_WithValidSignature_ReturnsTrue()
    {
        var body = Encoding.UTF8.GetBytes("{\"event\":\"update\"}");
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes("whsec_test"));
        var hash = hmac.ComputeHash(body);
        var signature = "sha256=" + Convert.ToHexString(hash).ToLowerInvariant();

        var revalidator = new WebhookRevalidator(_manager.Object, _options);
        Assert.True(revalidator.VerifySignature(body, signature));
    }

    [Fact]
    public void VerifySignature_WithInvalidSignature_ReturnsFalse()
    {
        var body = Encoding.UTF8.GetBytes("{\"event\":\"update\"}");

        var revalidator = new WebhookRevalidator(_manager.Object, _options);
        Assert.False(revalidator.VerifySignature(body, "sha256=invalid"));
    }

    [Fact]
    public void VerifySignature_WithNoConfiguredSecret_ReturnsTrue()
    {
        var options = new IsrGlobalOptions { WebhookSecret = null };
        var body = Encoding.UTF8.GetBytes("test");

        var revalidator = new WebhookRevalidator(_manager.Object, options);
        Assert.True(revalidator.VerifySignature(body, null));
    }

    [Fact]
    public void VerifySignature_WithNullSignature_ReturnsFalse()
    {
        var body = Encoding.UTF8.GetBytes("test");

        var revalidator = new WebhookRevalidator(_manager.Object, _options);
        Assert.False(revalidator.VerifySignature(body, null));
    }

    [Fact]
    public void VerifySignature_WithEmptySignature_ReturnsFalse()
    {
        var body = Encoding.UTF8.GetBytes("test");

        var revalidator = new WebhookRevalidator(_manager.Object, _options);
        Assert.False(revalidator.VerifySignature(body, ""));
    }

    [Fact]
    public void VerifySignature_WithNullBody_ReturnsFalse()
    {
        var revalidator = new WebhookRevalidator(_manager.Object, _options);
        Assert.False(revalidator.VerifySignature(null!, "sha256=abc"));
    }

    [Fact]
    public void VerifySignature_WithEmptyBody_ReturnsFalse()
    {
        var revalidator = new WebhookRevalidator(_manager.Object, _options);
        Assert.False(revalidator.VerifySignature(Array.Empty<byte>(), "sha256=abc"));
    }

    [Fact]
    public async Task ProcessWebhookAsync_WithValidSignature_RevalidatesRoutes()
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
    public async Task ProcessWebhookAsync_WithInvalidSignature_ReturnsFailure()
    {
        var body = Encoding.UTF8.GetBytes("test");

        var revalidator = new WebhookRevalidator(_manager.Object, _options);
        var result = await revalidator.ProcessWebhookAsync(
            body, "sha256=invalid",
            routes: new[] { "/test" },
            tags: null);

        Assert.False(result.Success);
        Assert.Equal("Invalid webhook signature.", result.ErrorMessage);
    }

    [Fact]
    public async Task ProcessWebhookAsync_WithTags_InvalidatesByTags()
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
