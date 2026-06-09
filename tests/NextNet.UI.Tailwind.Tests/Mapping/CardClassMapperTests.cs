using NextNet.DesignSystem.Tokens;
using NextNet.UI.Abstractions.Components;
using NextNet.UI.Abstractions.Rendering;
using NextNet.UI.DesignSystem.Components;
using NextNet.UI.Tailwind.Mapping;
using Xunit;

namespace NextNet.UI.Tailwind.Tests.Mapping;

public class CardClassMapperTests
{
    private readonly RenderContext _context;
    private readonly CardClassMapper _mapper;

    public CardClassMapperTests()
    {
        _context = new RenderContext(new DesignTokenSet(), new EmptyServiceProvider());
        _mapper = new CardClassMapper();
    }

    [Fact]
    public void MapClasses_Should_IncludeBaseCardClass()
    {
        var card = new Card { Title = "Card" };
        var classes = _mapper.MapClasses(card, _context);

        Assert.Contains("card", classes);
    }

    [Fact]
    public void MapClasses_Should_IncludeRoundedAndBorderClasses()
    {
        var card = new Card { Title = "Card" };
        var classes = _mapper.MapClasses(card, _context);

        Assert.Contains("rounded-lg", classes);
        Assert.Contains("border", classes);
    }

    [Fact]
    public void MapClasses_Should_ApplyPaddingClass_ForMd()
    {
        var card = new Card { Title = "Card", Padding = ComponentSize.Md };
        var classes = _mapper.MapClasses(card, _context);

        Assert.Contains("p-4", classes);
    }

    [Fact]
    public void MapClasses_Should_ApplyPaddingClass_ForSm()
    {
        var card = new Card { Title = "Card", Padding = ComponentSize.Sm };
        var classes = _mapper.MapClasses(card, _context);

        Assert.Contains("p-3", classes);
    }

    [Fact]
    public void MapClasses_Should_ApplyPaddingClass_ForLg()
    {
        var card = new Card { Title = "Card", Padding = ComponentSize.Lg };
        var classes = _mapper.MapClasses(card, _context);

        Assert.Contains("p-6", classes);
    }

    [Fact]
    public void MapClasses_Should_ApplyPaddingClass_ForXl()
    {
        var card = new Card { Title = "Card", Padding = ComponentSize.Xl };
        var classes = _mapper.MapClasses(card, _context);

        Assert.Contains("p-8", classes);
    }

    [Fact]
    public void MapClasses_Should_ApplyShadowClass_When_Set()
    {
        var card = new Card { Title = "Card", Shadow = "md" };
        var classes = _mapper.MapClasses(card, _context);

        Assert.Contains("shadow-md", classes);
    }

    [Fact]
    public void MapClasses_Should_NotIncludeShadowClass_When_Null()
    {
        var card = new Card { Title = "Card", Shadow = null };
        var classes = _mapper.MapClasses(card, _context);

        Assert.DoesNotContain("shadow-", classes);
    }

    [Fact]
    public void MapClasses_Should_IncludeCustomClassName()
    {
        var card = new Card { Title = "Card", ClassName = "my-card" };
        var classes = _mapper.MapClasses(card, _context);

        Assert.Contains("my-card", classes);
    }

    [Fact]
    public void MapClasses_Should_ImplementIComponentClassMapper()
    {
        Assert.IsAssignableFrom<IComponentClassMapper<ICard>>(_mapper);
    }
}
