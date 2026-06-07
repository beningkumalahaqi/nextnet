using Moq;
using NextNet.Isr.Revalidation;

namespace NextNet.Isr.Tests;

public class OnDemandRevalidatorTests
{
    [Fact]
    public void ValidateSecret_WithMatchingSecret_ReturnsTrue()
    {
        var options = new IsrGlobalOptions { RevalidationSecret = "my-secret" };
        var manager = new Mock<IIsrRevalidationManager>(MockBehavior.Strict);
        var revalidator = new OnDemandRevalidator(manager.Object, options);

        Assert.True(revalidator.ValidateSecret("my-secret"));
    }

    [Fact]
    public void ValidateSecret_WithNonMatchingSecret_ReturnsFalse()
    {
        var options = new IsrGlobalOptions { RevalidationSecret = "my-secret" };
        var manager = new Mock<IIsrRevalidationManager>(MockBehavior.Strict);
        var revalidator = new OnDemandRevalidator(manager.Object, options);

        Assert.False(revalidator.ValidateSecret("wrong-secret"));
    }

    [Fact]
    public void ValidateSecret_WithNoConfiguredSecret_ReturnsTrue()
    {
        var options = new IsrGlobalOptions { RevalidationSecret = null };
        var manager = new Mock<IIsrRevalidationManager>(MockBehavior.Strict);
        var revalidator = new OnDemandRevalidator(manager.Object, options);

        Assert.True(revalidator.ValidateSecret("anything"));
    }

    [Fact]
    public void ValidateSecret_WithNullProvidedSecret_ReturnsFalse()
    {
        var options = new IsrGlobalOptions { RevalidationSecret = "secret" };
        var manager = new Mock<IIsrRevalidationManager>(MockBehavior.Strict);
        var revalidator = new OnDemandRevalidator(manager.Object, options);

        Assert.False(revalidator.ValidateSecret(null));
    }

    [Fact]
    public void ValidateSecret_WithEmptyProvidedSecret_ReturnsFalse()
    {
        var options = new IsrGlobalOptions { RevalidationSecret = "secret" };
        var manager = new Mock<IIsrRevalidationManager>(MockBehavior.Strict);
        var revalidator = new OnDemandRevalidator(manager.Object, options);

        Assert.False(revalidator.ValidateSecret(""));
    }

    [Fact]
    public void ValidateSecret_WithEmptyConfiguredSecret_ReturnsTrue()
    {
        var options = new IsrGlobalOptions { RevalidationSecret = "" };
        var manager = new Mock<IIsrRevalidationManager>(MockBehavior.Strict);
        var revalidator = new OnDemandRevalidator(manager.Object, options);

        Assert.True(revalidator.ValidateSecret("anything"));
    }

    [Fact]
    public async Task RevalidateRouteAsync_DelegatesToManager()
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
    public async Task RevalidateByTagsAsync_DelegatesToManager()
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
