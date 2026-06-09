using NextNet.DesignSystem.Tokens;
using NextNet.UI.Abstractions.Components;
using NextNet.UI.Abstractions.Rendering;
using NextNet.UI.DesignSystem.Components;
using Xunit;

namespace NextNet.UI.DesignSystem.Tests.Components;

public class ButtonTests
{
    private readonly RenderContext _context;

    public ButtonTests()
    {
        _context = new RenderContext(new DesignTokenSet(), new EmptyServiceProvider());
    }

    [Fact]
    public void Render_Should_GenerateButtonElementWithLabel()
    {
        var button = new Button { Label = "Click me" };
        var html = button.Render(_context).ToHtml();

        Assert.Contains("<button", html);
        Assert.Contains("Click me", html);
        Assert.Contains("</button>", html);
    }

    [Fact]
    public void Render_Should_ApplyVariantClass()
    {
        var button = new Button { Variant = ComponentVariant.Danger, Label = "Delete" };
        var html = button.Render(_context).ToHtml();

        Assert.Contains("class=\"btn btn-danger btn-md\"", html);
    }

    [Fact]
    public void Render_Should_ApplySizeClass()
    {
        var button = new Button { Size = ComponentSize.Lg, Label = "Large" };
        var html = button.Render(_context).ToHtml();

        Assert.Contains("btn-lg", html);
    }

    [Fact]
    public void Render_Should_AddDisabledAttribute_WhenDisabled()
    {
        var button = new Button { Disabled = true, Label = "Disabled" };
        var html = button.Render(_context).ToHtml();

        Assert.Contains("disabled=\"disabled\"", html);
        Assert.Contains("btn-disabled", html);
    }

    [Fact]
    public void Render_Should_AddDefaultVariant_WhenNotSpecified()
    {
        var button = new Button { Label = "Default" };
        var html = button.Render(_context).ToHtml();

        Assert.Contains("btn-primary", html);
    }

    [Fact]
    public void Render_Should_IncludeCustomClassName()
    {
        var button = new Button { Label = "Custom", ClassName = "my-custom-class" };
        var html = button.Render(_context).ToHtml();

        Assert.Contains("my-custom-class", html);
    }

    [Fact]
    public void Render_Should_IncludeIdAttribute()
    {
        var button = new Button { Label = "ID'd", Id = "submit-btn" };
        var html = button.Render(_context).ToHtml();

        Assert.Contains("id=\"submit-btn\"", html);
    }

    [Fact]
    public void Render_Should_IncludeStyleAttribute()
    {
        var button = new Button { Label = "Styled", Style = "margin: 8px;" };
        var html = button.Render(_context).ToHtml();

        Assert.Contains("style=\"margin: 8px;\"", html);
    }

    [Fact]
    public void Render_Should_HandleNullLabel()
    {
        var button = new Button { Label = null };
        var html = button.Render(_context).ToHtml();

        Assert.Contains("<button", html);
        Assert.Contains("</button>", html);
    }

    [Fact]
    public void Button_Implements_IButton()
    {
        var button = new Button();
        Assert.IsAssignableFrom<IButton>(button);
        Assert.IsAssignableFrom<IComponent>(button);
    }
}
