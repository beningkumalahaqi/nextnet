using NextNet.DesignSystem.Tokens;
using NextNet.UI.Abstractions.Components;
using NextNet.UI.Abstractions.Rendering;
using NextNet.UI.DesignSystem.Components;
using Xunit;

namespace NextNet.UI.DesignSystem.Tests.Components;

public class CardTests
{
    private readonly RenderContext _context;

    public CardTests()
    {
        _context = new RenderContext(new DesignTokenSet(), new EmptyServiceProvider());
    }

    [Fact]
    public void Render_Should_GenerateCardStructure()
    {
        var card = new Card { Title = "Hello", Description = "World" };
        var html = card.Render(_context).ToHtml();

        Assert.Contains("<div class=\"card", html);
        Assert.Contains("</div>", html);
    }

    [Fact]
    public void Render_Should_IncludeTitleAndDescription()
    {
        var card = new Card { Title = "My Title", Description = "My Description" };
        var html = card.Render(_context).ToHtml();

        Assert.Contains("My Title", html);
        Assert.Contains("My Description", html);
        Assert.Contains("card-header", html);
        Assert.Contains("card-body", html);
    }

    [Fact]
    public void Render_Should_ApplyPaddingClass()
    {
        var card = new Card { Padding = ComponentSize.Lg, Title = "Padded" };
        var html = card.Render(_context).ToHtml();

        Assert.Contains("card-padding-lg", html);
    }

    [Fact]
    public void Render_Should_ApplyShadow_WhenSet()
    {
        var card = new Card { Shadow = "md", Title = "Shadowed" };
        var html = card.Render(_context).ToHtml();

        Assert.Contains("card-shadow-md", html);
    }

    [Fact]
    public void Render_Should_NotIncludeHeader_WhenTitleIsNull()
    {
        var card = new Card { Description = "No title" };
        var html = card.Render(_context).ToHtml();

        Assert.DoesNotContain("card-header", html);
        Assert.Contains("card-body", html);
    }

    [Fact]
    public void Render_Should_NotIncludeBody_WhenDescriptionIsNull()
    {
        var card = new Card { Title = "No body" };
        var html = card.Render(_context).ToHtml();

        Assert.Contains("card-header", html);
        Assert.DoesNotContain("card-body", html);
    }

    [Fact]
    public void Render_Should_IncludeFooter_WhenSet()
    {
        var card = new Card { Title = "Card", Footer = new Button { Label = "Action" } };
        var html = card.Render(_context).ToHtml();

        Assert.Contains("card-footer", html);
    }

    [Fact]
    public void Card_Implements_ICard()
    {
        var card = new Card();
        Assert.IsAssignableFrom<ICard>(card);
    }

    [Fact]
    public void Render_Should_IncludeCustomClassName()
    {
        var card = new Card { Title = "X", ClassName = "custom-card" };
        var html = card.Render(_context).ToHtml();

        Assert.Contains("custom-card", html);
    }
}
