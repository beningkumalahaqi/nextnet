using Microsoft.Extensions.DependencyInjection;
using NextNet.UI.Abstractions.Components;
using NextNet.UI.Tailwind.Config;
using NextNet.UI.Tailwind.Css;
using NextNet.UI.Tailwind.Extensions;
using NextNet.UI.Tailwind.Mapping;
using Xunit;

namespace NextNet.UI.Tailwind.Tests.Extensions;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddNextNetTailwind_Should_RegisterTailwindOptions()
    {
        var services = new ServiceCollection();
        services.AddNextNetTailwind();

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<TailwindOptions>();

        Assert.NotNull(options);
    }

    [Fact]
    public void AddNextNetTailwind_Should_RegisterStyleBuilder()
    {
        var services = new ServiceCollection();
        services.AddNextNetTailwind();

        var provider = services.BuildServiceProvider();
        var builder = provider.GetRequiredService<TailwindStyleBuilder>();

        Assert.NotNull(builder);
    }

    [Fact]
    public void AddNextNetTailwind_Should_RegisterClassMapperRegistry()
    {
        var services = new ServiceCollection();
        services.AddNextNetTailwind();

        var provider = services.BuildServiceProvider();
        var registry = provider.GetRequiredService<ClassMapperRegistry>();

        Assert.NotNull(registry);
    }

    [Fact]
    public void AddNextNetTailwind_Should_RegisterButtonMapper()
    {
        var services = new ServiceCollection();
        services.AddNextNetTailwind();

        var provider = services.BuildServiceProvider();
        var registry = provider.GetRequiredService<ClassMapperRegistry>();

        Assert.NotNull(registry.Resolve<IButton>());
    }

    [Fact]
    public void AddNextNetTailwind_Should_RegisterCardMapper()
    {
        var services = new ServiceCollection();
        services.AddNextNetTailwind();

        var provider = services.BuildServiceProvider();
        var registry = provider.GetRequiredService<ClassMapperRegistry>();

        Assert.NotNull(registry.Resolve<ICard>());
    }

    [Fact]
    public void AddNextNetTailwind_Should_RegisterInputMapper()
    {
        var services = new ServiceCollection();
        services.AddNextNetTailwind();

        var provider = services.BuildServiceProvider();
        var registry = provider.GetRequiredService<ClassMapperRegistry>();

        Assert.NotNull(registry.Resolve<IInput>());
    }

    [Fact]
    public void AddNextNetTailwind_Should_RegisterBadgeMapper()
    {
        var services = new ServiceCollection();
        services.AddNextNetTailwind();

        var provider = services.BuildServiceProvider();
        var registry = provider.GetRequiredService<ClassMapperRegistry>();

        Assert.NotNull(registry.Resolve<IBadge>());
    }

    [Fact]
    public void AddNextNetTailwind_Should_RegisterAlertMapper()
    {
        var services = new ServiceCollection();
        services.AddNextNetTailwind();

        var provider = services.BuildServiceProvider();
        var registry = provider.GetRequiredService<ClassMapperRegistry>();

        Assert.NotNull(registry.Resolve<IAlert>());
    }

    [Fact]
    public void AddNextNetTailwind_Should_ApplyCustomOptions()
    {
        var services = new ServiceCollection();
        services.AddNextNetTailwind(options =>
        {
            options.ContentPaths = new[] { "./Custom/**/*.cshtml" };
            options.SafelistPatterns = new[] { "custom-*" };
        });

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<TailwindOptions>();

        Assert.Contains("./Custom/**/*.cshtml", options.ContentPaths);
        Assert.Contains("custom-*", options.SafelistPatterns);
    }

    [Fact]
    public void AddNextNetTailwind_Should_Throw_When_ServicesIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            ((IServiceCollection)null!).AddNextNetTailwind());
    }

    [Fact]
    public void AddNextNetTailwind_Should_BeChainable()
    {
        var services = new ServiceCollection();

        var result = services.AddNextNetTailwind();

        Assert.Same(services, result);
    }

    [Fact]
    public void AddNextNetTailwind_Should_RegisterDefaultContentPaths()
    {
        var services = new ServiceCollection();
        services.AddNextNetTailwind();

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<TailwindOptions>();

        Assert.Contains("./**/*.{html,cshtml,razor}", options.ContentPaths);
    }

    [Fact]
    public void AddNextNetTailwind_Should_HaveEmptySafelistByDefault()
    {
        var services = new ServiceCollection();
        services.AddNextNetTailwind();

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<TailwindOptions>();

        Assert.Empty(options.SafelistPatterns);
    }

    [Fact]
    public void AddNextNetTailwind_Should_RegisterAllMappers()
    {
        var services = new ServiceCollection();
        services.AddNextNetTailwind();

        var provider = services.BuildServiceProvider();
        var registry = provider.GetRequiredService<ClassMapperRegistry>();

        // All 5 mappers should be registered
        Assert.NotNull(registry.Resolve<IButton>());
        Assert.NotNull(registry.Resolve<ICard>());
        Assert.NotNull(registry.Resolve<IInput>());
        Assert.NotNull(registry.Resolve<IBadge>());
        Assert.NotNull(registry.Resolve<IAlert>());
    }

    [Fact]
    public void AddNextNetTailwind_Should_RegisterServicesAsSingletons()
    {
        var services = new ServiceCollection();
        services.AddNextNetTailwind();

        var provider = services.BuildServiceProvider();

        var first = provider.GetRequiredService<TailwindStyleBuilder>();
        var second = provider.GetRequiredService<TailwindStyleBuilder>();

        Assert.Same(first, second);
    }

    [Theory]
    [InlineData("btn-*")]
    [InlineData("badge-*")]
    [InlineData("alert-*")]
    public void AddNextNetTailwind_Should_SupportSafelistPattern(string pattern)
    {
        var services = new ServiceCollection();
        services.AddNextNetTailwind(options =>
        {
            options.SafelistPatterns = new[] { pattern };
        });

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<TailwindOptions>();

        Assert.Contains(pattern, options.SafelistPatterns);
    }
}
