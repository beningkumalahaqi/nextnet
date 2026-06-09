using NextNet.DesignSystem.Tokens;
using NextNet.UI.Abstractions.Components;
using NextNet.UI.Abstractions.Rendering;
using NextNet.UI.DesignSystem.Components;
using Xunit;

namespace NextNet.UI.DesignSystem.Tests.Components;

public class AlertTests
{
    private readonly RenderContext _context;

    public AlertTests()
    {
        _context = new RenderContext(new DesignTokenSet(), new EmptyServiceProvider());
    }

    [Fact]
    public void Render_Should_GenerateAlertStructure()
    {
        var alert = new Alert { Title = "Notice", Message = "Something happened" };
        var html = alert.Render(_context).ToHtml();

        Assert.Contains("role=\"alert\"", html);
        Assert.Contains("alert", html);
    }

    [Fact]
    public void Render_Should_IncludeTitleAndMessage()
    {
        var alert = new Alert { Title = "Warning!", Message = "Your session is about to expire." };
        var html = alert.Render(_context).ToHtml();

        Assert.Contains("Warning!", html);
        Assert.Contains("Your session is about to expire.", html);
        Assert.Contains("alert-title", html);
        Assert.Contains("alert-message", html);
    }

    [Fact]
    public void Render_Should_ApplyVariantClass()
    {
        var alert = new Alert { Variant = ComponentVariant.Danger, Title = "Error" };
        var html = alert.Render(_context).ToHtml();

        Assert.Contains("alert-danger", html);
    }

    [Fact]
    public void Render_Should_IncludeDismissButton_WhenDismissible()
    {
        var alert = new Alert { Dismissible = true, Title = "Dismiss me" };
        var html = alert.Render(_context).ToHtml();

        Assert.Contains("alert-dismiss", html);
        Assert.Contains("alert-dismissible", html);
    }

    [Fact]
    public void Render_Should_NotIncludeDismissButton_WhenNotDismissible()
    {
        var alert = new Alert { Dismissible = false, Title = "Persistent" };
        var html = alert.Render(_context).ToHtml();

        Assert.DoesNotContain("alert-dismiss", html);
    }

    [Fact]
    public void Render_Should_UseDefaultVariant_WhenNotSpecified()
    {
        var alert = new Alert { Title = "Info" };
        var html = alert.Render(_context).ToHtml();

        Assert.Contains("alert-info", html);
    }

    [Fact]
    public void Alert_Implements_IAlert()
    {
        var alert = new Alert();
        Assert.IsAssignableFrom<IAlert>(alert);
    }

    [Fact]
    public void Render_Should_NotIncludeTitle_WhenTitleIsNull()
    {
        var alert = new Alert { Message = "No title alert" };
        var html = alert.Render(_context).ToHtml();

        Assert.DoesNotContain("alert-title", html);
        Assert.Contains("alert-message", html);
    }

    [Fact]
    public void Render_Should_NotIncludeMessage_WhenMessageIsNull()
    {
        var alert = new Alert { Title = "No message alert" };
        var html = alert.Render(_context).ToHtml();

        Assert.Contains("alert-title", html);
        Assert.DoesNotContain("alert-message", html);
    }
}
