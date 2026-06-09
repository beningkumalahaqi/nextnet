using Moq;
using NextNet.Isr.Revalidation;

namespace NextNet.Isr.Tests;

public class OnDemandRevalidatorTests
{
    [Fact]
    public void ValidateSecret_Should_ReturnTrue_When_SecretMatches()
    {
        var options = new IsrGlobalOptions { RevalidationSecret = "my-secret" };
        var manager = new Mock<IIsrRevalidationManager>(MockBehavior.Strict);
        var revalidator = new OnDemandRevalidator(manager.Object, options);

        Assert.True(revalidator.ValidateSecret("my-secret"));
    }

    [Fact]
    public void ValidateSecret_Should_ReturnFalse_When_SecretDoesNotMatch()
    {
        var options = new IsrGlobalOptions { RevalidationSecret = "my-secret" };
        var manager = new Mock<IIsrRevalidationManager>(MockBehavior.Strict);
        var revalidator = new OnDemandRevalidator(manager.Object, options);

        Assert.False(revalidator.ValidateSecret("wrong-secret"));
    }

    [Fact]
    public void ValidateSecret_Should_ReturnTrue_When_NoSecretConfigured()
    {
        var options = new IsrGlobalOptions { RevalidationSecret = null };
        var manager = new Mock<IIsrRevalidationManager>(MockBehavior.Strict);
        var revalidator = new OnDemandRevalidator(manager.Object, options);

        Assert.True(revalidator.ValidateSecret("anything"));
    }

    [Fact]
    public void ValidateSecret_Should_ReturnFalse_When_ProvidedSecretIsNull()
    {
        var options = new IsrGlobalOptions { RevalidationSecret = "secret" };
        var manager = new Mock<IIsrRevalidationManager>(MockBehavior.Strict);
        var revalidator = new OnDemandRevalidator(manager.Object, options);

        Assert.False(revalidator.ValidateSecret(null));
    }

    [Fact]
    public void ValidateSecret_Should_ReturnFalse_When_ProvidedSecretIsEmpty()
    {
        var options = new IsrGlobalOptions { RevalidationSecret = "secret" };
        var manager = new Mock<IIsrRevalidationManager>(MockBehavior.Strict);
        var revalidator = new OnDemandRevalidator(manager.Object, options);

        Assert.False(revalidator.ValidateSecret(""));
    }

    [Fact]
    public void ValidateSecret_Should_ReturnTrue_When_ConfiguredSecretIsEmpty()
    {
        var options = new IsrGlobalOptions { RevalidationSecret = "" };
        var manager = new Mock<IIsrRevalidationManager>(MockBehavior.Strict);
        var revalidator = new OnDemandRevalidator(manager.Object, options);

        Assert.True(revalidator.ValidateSecret("anything"));
    }

    [Fact]
    public async Task RevalidateRouteAsync_Should_DelegateToManager_When_Called()
    {
        var options = new IsrGlobalOptions();
        var manager = new Mock<IIsrRevalidationManager>(MockBehavior.Strict);
        manager.Setup(m => m.RevalidateAsync("/test", It.IsAny<CancellationToken>()))
            .ReturnsAsync(RevalidationResult.Ok("/test"));

        var revalidator = new OnDemandRevalidator(manager.Object, options);
        var result = await revalidator.RevalidateRouteAsync("/test");

        Assert.True(result.Success);
        Assert.Equal("/test", result.Route);
    }

    [Fact]
    public async Task RevalidateByTagsAsync_Should_DelegateToManager_When_Called()
    {
        var options = new IsrGlobalOptions();
        var manager = new Mock<IIsrRevalidationManager>(MockBehavior.Strict);
        manager.Setup(m => m.InvalidateByTagsAsync(
                new[] { "blog" }, It.IsAny<CancellationToken>()))
            .ReturnsAsync(RevalidationResult.Ok(new[] { "/blog/post-1" }));

        var revalidator = new OnDemandRevalidator(manager.Object, options);
        var result = await revalidator.RevalidateByTagsAsync(new[] { "blog" });

        Assert.True(result.Success);
        Assert.Single(result.Routes!);
    }
}
