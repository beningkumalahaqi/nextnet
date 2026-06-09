using NextNet.DesignSystem.Tokens;
using NextNet.UI.Abstractions.Components;
using NextNet.UI.Abstractions.Rendering;
using NextNet.UI.DesignSystem.Components;
using NextNet.UI.Tailwind.Mapping;
using Xunit;

namespace NextNet.UI.Tailwind.Tests.Mapping;

public class ButtonClassMapperTests
{
    private readonly RenderContext _context;
    private readonly ButtonClassMapper _mapper;

    public ButtonClassMapperTests()
    {
        _context = new RenderContext(new DesignTokenSet(), new EmptyServiceProvider());
        _mapper = new ButtonClassMapper();
    }

    [Fact]
    public void MapClasses_Should_IncludeBaseButtonClass()
    {
        var button = new Button { Label = "Click" };
        var classes = _mapper.MapClasses(button, _context);

        Assert.Contains("btn", classes);
    }

    [Fact]
    public void MapClasses_Should_ApplyVariantClass()
    {
        var button = new Button { Label = "Delete", Variant = ComponentVariant.Danger };
        var classes = _mapper.MapClasses(button, _context);

        Assert.Contains("btn-danger", classes);
    }

    [Fact]
    public void MapClasses_Should_UseDefaultVariant_When_NotSpecified()
    {
        var button = new Button { Label = "Default" };
        var classes = _mapper.MapClasses(button, _context);

        Assert.Contains("btn-primary", classes);
    }

    [Fact]
    public void MapClasses_Should_ApplySizeClass()
    {
        var button = new Button { Label = "Large", Size = ComponentSize.Lg };
        var classes = _mapper.MapClasses(button, _context);

        Assert.Contains("btn-lg", classes);
    }

    [Fact]
    public void MapClasses_Should_UseDefaultSize_When_NotSpecified()
    {
        var button = new Button { Label = "Default Size" };
        var classes = _mapper.MapClasses(button, _context);

        Assert.Contains("btn-md", classes);
    }

    [Fact]
    public void MapClasses_Should_IncludeDisabledClasses_When_Disabled()
    {
        var button = new Button { Label = "Disabled", Disabled = true };
        var classes = _mapper.MapClasses(button, _context);

        Assert.Contains("opacity-50", classes);
        Assert.Contains("cursor-not-allowed", classes);
    }

    [Fact]
    public void MapClasses_Should_NotIncludeDisabledClasses_When_NotDisabled()
    {
        var button = new Button { Label = "Enabled" };
        var classes = _mapper.MapClasses(button, _context);

        Assert.DoesNotContain("opacity-50", classes);
        Assert.DoesNotContain("cursor-not-allowed", classes);
    }

    [Fact]
    public void MapClasses_Should_IncludeCustomClassName()
    {
        var button = new Button { Label = "Custom", ClassName = "my-custom-class" };
        var classes = _mapper.MapClasses(button, _context);

        Assert.Contains("my-custom-class", classes);
    }

    [Fact]
    public void MapClasses_Should_CombineAllClasses()
    {
        var button = new Button
        {
            Label = "All",
            Variant = ComponentVariant.Success,
            Size = ComponentSize.Sm,
            Disabled = true,
            ClassName = "extra"
        };
        var classes = _mapper.MapClasses(button, _context);

        Assert.Contains("btn", classes);
        Assert.Contains("btn-success", classes);
        Assert.Contains("btn-sm", classes);
        Assert.Contains("opacity-50", classes);
        Assert.Contains("cursor-not-allowed", classes);
        Assert.Contains("extra", classes);
    }

    [Fact]
    public void MapClasses_Should_ImplementIComponentClassMapper()
    {
        Assert.IsAssignableFrom<IComponentClassMapper<IButton>>(_mapper);
    }
}
