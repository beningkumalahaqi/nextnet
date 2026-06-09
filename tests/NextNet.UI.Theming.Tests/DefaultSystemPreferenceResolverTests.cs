using Xunit;

namespace NextNet.UI.Theming.Tests;

public class DefaultSystemPreferenceResolverTests
{
    private readonly DefaultSystemPreferenceResolver _resolver;

    public DefaultSystemPreferenceResolverTests()
    {
        _resolver = new DefaultSystemPreferenceResolver();
    }

    [Fact]
    public void IsDarkModePreferred_Should_ReturnFalse_ByDefault()
    {
        var result = _resolver.IsDarkModePreferred();
        Assert.False(result);
    }

    [Fact]
    public void DefaultSystemPreferenceResolver_Should_ImplementInterface()
    {
        Assert.IsAssignableFrom<ISystemPreferenceResolver>(_resolver);
    }
}
