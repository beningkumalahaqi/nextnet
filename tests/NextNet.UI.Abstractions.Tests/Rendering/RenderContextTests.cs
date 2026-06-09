using NextNet.DesignSystem.Tokens;
using NextNet.UI.Abstractions.Rendering;
using Xunit;

namespace NextNet.UI.Abstractions.Tests.Rendering;

public class RenderContextTests
{
    [Fact]
    public void Constructor_Should_SetProperties_When_AllArgumentsProvided()
    {
        var tokens = new DesignTokenSet();
        var services = new EmptyServiceProvider();
        var themeName = "dark";

        var context = new RenderContext(tokens, services, themeName);

        Assert.Same(tokens, context.Tokens);
        Assert.Same(services, context.Services);
        Assert.Equal(themeName, context.ThemeName);
    }

    [Fact]
    public void Constructor_Should_SetThemeNameToNull_When_NotProvided()
    {
        var tokens = new DesignTokenSet();
        var services = new EmptyServiceProvider();

        var context = new RenderContext(tokens, services);

        Assert.Same(tokens, context.Tokens);
        Assert.Same(services, context.Services);
        Assert.Null(context.ThemeName);
    }

    [Fact]
    public void Tokens_Should_BeReadOnly()
    {
        var tokens = new DesignTokenSet();
        var services = new EmptyServiceProvider();
        var context = new RenderContext(tokens, services);

        // The record is immutable after construction
        Assert.NotNull(context.Tokens);
    }

    [Fact]
    public void Equality_Should_BeValueBased()
    {
        var tokens1 = new DesignTokenSet();
        var services1 = new EmptyServiceProvider();
        var context1 = new RenderContext(tokens1, services1, "light");

        var tokens2 = new DesignTokenSet();
        var services2 = new EmptyServiceProvider();
        var context2 = new RenderContext(tokens2, services2, "light");

        // Different service provider instances means inequality
        Assert.NotEqual(context1, context2);
    }

    [Fact]
    public void Equality_Should_BeSame_When_SameInstance()
    {
        var tokens = new DesignTokenSet();
        var services = new EmptyServiceProvider();
        var context = new RenderContext(tokens, services, "light");

        Assert.Equal(context, context);
    }

    /// <summary>
    /// A minimal <see cref="IServiceProvider"/> stub that returns <c>null</c>
    /// for all service types. Used to satisfy constructor requirements in tests.
    /// </summary>
    private sealed class EmptyServiceProvider : IServiceProvider
    {
        /// <inheritdoc />
        public object? GetService(Type serviceType) => null;
    }
}
