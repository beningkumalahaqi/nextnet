using NextNet.DesignSystem.Tokens;
using NextNet.UI.Abstractions.Components;
using NextNet.UI.Abstractions.Rendering;
using NextNet.UI.DesignSystem.Components;
using NextNet.UI.Tailwind.Mapping;
using Xunit;

namespace NextNet.UI.Tailwind.Tests.Mapping;

public class InputClassMapperTests
{
    private readonly RenderContext _context;
    private readonly InputClassMapper _mapper;

    public InputClassMapperTests()
    {
        _context = new RenderContext(new DesignTokenSet(), new EmptyServiceProvider());
        _mapper = new InputClassMapper();
    }

    [Fact]
    public void MapClasses_Should_IncludeBaseInputClass()
    {
        var input = new Input();
        var classes = _mapper.MapClasses(input, _context);

        Assert.Contains("input", classes);
    }

    [Fact]
    public void MapClasses_Should_IncludeFullWidthClass()
    {
        var input = new Input();
        var classes = _mapper.MapClasses(input, _context);

        Assert.Contains("w-full", classes);
    }

    [Fact]
    public void MapClasses_Should_IncludeErrorBorder_When_HasError()
    {
        var input = new Input { Error = "Required" };
        var classes = _mapper.MapClasses(input, _context);

        Assert.Contains("border-red-500", classes);
    }

    [Fact]
    public void MapClasses_Should_NotIncludeErrorBorder_When_NoError()
    {
        var input = new Input();
        var classes = _mapper.MapClasses(input, _context);

        Assert.DoesNotContain("border-red-500", classes);
    }

    [Fact]
    public void MapClasses_Should_IncludeDisabledClasses_When_Disabled()
    {
        var input = new Input { Disabled = true };
        var classes = _mapper.MapClasses(input, _context);

        Assert.Contains("opacity-50", classes);
        Assert.Contains("cursor-not-allowed", classes);
    }

    [Fact]
    public void MapClasses_Should_IncludePadding()
    {
        var input = new Input();
        var classes = _mapper.MapClasses(input, _context);

        Assert.Contains("px-3", classes);
        Assert.Contains("py-2", classes);
    }

    [Fact]
    public void MapClasses_Should_IncludeCustomClassName()
    {
        var input = new Input { ClassName = "my-input" };
        var classes = _mapper.MapClasses(input, _context);

        Assert.Contains("my-input", classes);
    }

    [Fact]
    public void MapGroupClasses_Should_IncludeGroupWrapper()
    {
        var input = new Input();
        var classes = _mapper.MapGroupClasses(input);

        Assert.Contains("input-group", classes);
    }

    [Fact]
    public void MapGroupClasses_Should_IncludeErrorClass_When_HasError()
    {
        var input = new Input { Error = "Invalid" };
        var classes = _mapper.MapGroupClasses(input);

        Assert.Contains("input-has-error", classes);
    }

    [Fact]
    public void MapGroupClasses_Should_NotIncludeErrorClass_When_NoError()
    {
        var input = new Input();
        var classes = _mapper.MapGroupClasses(input);

        Assert.DoesNotContain("input-has-error", classes);
    }

    [Fact]
    public void MapClasses_Should_ImplementIComponentClassMapper()
    {
        Assert.IsAssignableFrom<IComponentClassMapper<IInput>>(_mapper);
    }
}
