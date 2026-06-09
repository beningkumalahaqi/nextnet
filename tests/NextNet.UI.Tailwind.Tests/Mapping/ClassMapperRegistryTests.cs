using NextNet.DesignSystem.Tokens;
using NextNet.UI.Abstractions.Components;
using NextNet.UI.Abstractions.Rendering;
using NextNet.UI.DesignSystem.Components;
using NextNet.UI.Tailwind.Mapping;
using Xunit;

namespace NextNet.UI.Tailwind.Tests.Mapping;

public class ClassMapperRegistryTests
{
    private readonly RenderContext _context;
    private readonly ClassMapperRegistry _registry;

    public ClassMapperRegistryTests()
    {
        _context = new RenderContext(new DesignTokenSet(), new EmptyServiceProvider());
        _registry = new ClassMapperRegistry();
    }

    [Fact]
    public void Register_Should_StoreMapper()
    {
        _registry.Register<IButton>(new ButtonClassMapper());

        var resolved = _registry.Resolve<IButton>();
        Assert.NotNull(resolved);
    }

    [Fact]
    public void Resolve_Should_ReturnCorrectMapperType()
    {
        _registry.Register<IButton>(new ButtonClassMapper());

        var resolved = _registry.Resolve<IButton>();
        Assert.IsType<ButtonClassMapper>(resolved);
    }

    [Fact]
    public void Resolve_Should_Throw_When_NoMapperRegistered()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            _registry.Resolve<IButton>());

        Assert.Contains("DS-400", ex.Message);
    }

    [Fact]
    public void TryResolve_Should_ReturnTrue_When_MapperExists()
    {
        _registry.Register<IButton>(new ButtonClassMapper());

        var found = _registry.TryResolve<IButton>(out var mapper);

        Assert.True(found);
        Assert.NotNull(mapper);
    }

    [Fact]
    public void TryResolve_Should_ReturnFalse_When_NoMapper()
    {
        var found = _registry.TryResolve<IButton>(out var mapper);

        Assert.False(found);
        Assert.Null(mapper);
    }

    [Fact]
    public void Register_Should_ReplaceExistingMapper()
    {
        _registry.Register<IButton>(new ButtonClassMapper());
        _registry.Register<IButton>(new ButtonClassMapper()); // Replace

        var resolved = _registry.Resolve<IButton>();
        Assert.NotNull(resolved);
    }

    [Fact]
    public void MapClasses_Should_DelegateToRegisteredMapper()
    {
        _registry.Register<IButton>(new ButtonClassMapper());
        var button = new Button { Label = "Test", Variant = ComponentVariant.Danger };

        var classes = _registry.MapClasses<IButton>(button, _context);

        Assert.Contains("btn-danger", classes);
    }

    [Fact]
    public void Count_Should_ReflectRegisteredMappers()
    {
        Assert.Equal(0, _registry.Count);

        _registry.Register<IButton>(new ButtonClassMapper());
        Assert.Equal(1, _registry.Count);

        _registry.Register<ICard>(new CardClassMapper());
        Assert.Equal(2, _registry.Count);
    }

    [Fact]
    public void Register_Should_Throw_When_MapperIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _registry.Register<IButton>(null!));
    }

    [Fact]
    public void MapClasses_Should_Work_ForMultipleTypes()
    {
        _registry.Register<IButton>(new ButtonClassMapper());
        _registry.Register<ICard>(new CardClassMapper());

        var button = new Button { Label = "Click" };
        var card = new Card { Title = "Card" };

        var buttonClasses = _registry.MapClasses<IButton>(button, _context);
        var cardClasses = _registry.MapClasses<ICard>(card, _context);

        Assert.Contains("btn", buttonClasses);
        Assert.Contains("card", cardClasses);
    }
}
