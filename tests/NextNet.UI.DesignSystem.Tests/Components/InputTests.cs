using NextNet.DesignSystem.Tokens;
using NextNet.UI.Abstractions.Components;
using NextNet.UI.Abstractions.Rendering;
using NextNet.UI.DesignSystem.Components;
using Xunit;

namespace NextNet.UI.DesignSystem.Tests.Components;

public class InputTests
{
    private readonly RenderContext _context;

    public InputTests()
    {
        _context = new RenderContext(new DesignTokenSet(), new EmptyServiceProvider());
    }

    [Fact]
    public void Render_Should_GenerateInputElement()
    {
        var input = new Input { Type = "email" };
        var html = input.Render(_context).ToHtml();

        Assert.Contains("<input", html);
        Assert.Contains("type=\"email\"", html);
    }

    [Fact]
    public void Render_Should_SetPlaceholder()
    {
        var input = new Input { Placeholder = "Enter your name" };
        var html = input.Render(_context).ToHtml();

        Assert.Contains("placeholder=\"Enter your name\"", html);
    }

    [Fact]
    public void Render_Should_IncludeLabel_WhenSet()
    {
        var input = new Input { Label = "Email Address" };
        var html = input.Render(_context).ToHtml();

        Assert.Contains("Email Address", html);
        Assert.Contains("input-group-label", html);
    }

    [Fact]
    public void Render_Should_ShowError()
    {
        var input = new Input { Error = "This field is required" };
        var html = input.Render(_context).ToHtml();

        Assert.Contains("This field is required", html);
        Assert.Contains("input-error", html);
        Assert.Contains("input-has-error", html);
    }

    [Fact]
    public void Render_Should_SetRequired_WhenRequired()
    {
        var input = new Input { Required = true };
        var html = input.Render(_context).ToHtml();

        Assert.Contains("required=\"required\"", html);
    }

    [Fact]
    public void Render_Should_SetDisabled_WhenDisabled()
    {
        var input = new Input { Disabled = true };
        var html = input.Render(_context).ToHtml();

        Assert.Contains("disabled=\"disabled\"", html);
    }

    [Fact]
    public void Render_Should_SetCustomType()
    {
        var input = new Input { Type = "email" };
        var html = input.Render(_context).ToHtml();

        Assert.Contains("type=\"email\"", html);
    }

    [Fact]
    public void Render_Should_SetValue()
    {
        var input = new Input { Value = "test@example.com" };
        var html = input.Render(_context).ToHtml();

        Assert.Contains("value=\"test@example.com\"", html);
    }

    [Fact]
    public void Input_Implements_IInput()
    {
        var input = new Input();
        Assert.IsAssignableFrom<IInput>(input);
    }

    [Fact]
    public void Render_Should_UseDefaultType_WhenNotSet()
    {
        // Type can be null via init, but defaults to "text"
        var input = new Input { Type = null };
        var html = input.Render(_context).ToHtml();

        Assert.Contains("type=\"text\"", html);
    }
}
