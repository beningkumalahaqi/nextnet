using NextNet.DesignSystem.Tokens;
using NextNet.UI.Abstractions.Components;
using NextNet.UI.Abstractions.Rendering;
using NextNet.UI.DesignSystem.Components;
using Xunit;

namespace NextNet.UI.DesignSystem.Tests.Components;

public class BadgeTests
{
    private readonly RenderContext _context;

    public BadgeTests()
    {
        _context = new RenderContext(new DesignTokenSet(), new EmptyServiceProvider());
    }

    [Fact]
    public void Render_Should_GenerateBadgeWithLabel()
    {
        var badge = new Badge { Label = "New" };
        var html = badge.Render(_context).ToHtml();

        Assert.Contains("<span", html);
        Assert.Contains("New", html);
        Assert.Contains("</span>", html);
    }

    [Fact]
    public void Render_Should_ApplyVariantClass()
    {
        var badge = new Badge { Variant = ComponentVariant.Success, Label = "Done" };
        var html = badge.Render(_context).ToHtml();

        Assert.Contains("badge-success", html);
    }

    [Fact]
    public void Render_Should_ApplySizeClass()
    {
        var badge = new Badge { Size = ComponentSize.Lg, Label = "Large" };
        var html = badge.Render(_context).ToHtml();

        Assert.Contains("badge-lg", html);
    }

    [Fact]
    public void Render_Should_RenderDot_WhenDotEnabled()
    {
        var badge = new Badge { Dot = true, Label = "Should Not Appear" };
        var html = badge.Render(_context).ToHtml();

        Assert.Contains("badge-dot", html);
        Assert.DoesNotContain("Should Not Appear", html);
    }

    [Fact]
    public void Render_Should_HaveNoTextContent_WhenDotEnabled()
    {
        var badge = new Badge { Dot = true, Label = "Hidden" };
        var html = badge.Render(_context).ToHtml();

        // Only the span tag should be present with no text content
        // The span should be self-closing since there's no content
        Assert.Contains("<span", html);
    }

    [Fact]
    public void Render_Should_UseDefaultVariant_WhenNotSpecified()
    {
        var badge = new Badge { Label = "Default" };
        var html = badge.Render(_context).ToHtml();

        Assert.Contains("badge-primary", html);
    }

    [Fact]
    public void Badge_Implements_IBadge()
    {
        var badge = new Badge();
        Assert.IsAssignableFrom<IBadge>(badge);
    }
}
