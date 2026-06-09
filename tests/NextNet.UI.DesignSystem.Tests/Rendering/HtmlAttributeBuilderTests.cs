using NextNet.UI.DesignSystem.Rendering;
using Xunit;

namespace NextNet.UI.DesignSystem.Tests.Rendering;

public class HtmlAttributeBuilderTests
{
    [Fact]
    public void Add_Should_StoreAttribute()
    {
        var attrs = new HtmlAttributeBuilder()
            .Add("class", "btn")
            .Build();

        Assert.Single(attrs);
        Assert.Equal("btn", attrs["class"]);
    }

    [Fact]
    public void AddIf_Should_Add_WhenConditionTrue()
    {
        var attrs = new HtmlAttributeBuilder()
            .AddIf("disabled", "disabled", true)
            .Build();

        Assert.Contains("disabled", attrs.Keys);
    }

    [Fact]
    public void AddIf_Should_NotAdd_WhenConditionFalse()
    {
        var attrs = new HtmlAttributeBuilder()
            .AddIf("disabled", "disabled", false)
            .Build();

        Assert.Empty(attrs);
    }

    [Fact]
    public void MultipleAttributes_Should_AllBeStored()
    {
        var attrs = new HtmlAttributeBuilder()
            .Add("class", "btn btn-primary")
            .Add("id", "submit-btn")
            .Add("disabled", "disabled")
            .Build();

        Assert.Equal(3, attrs.Count);
        Assert.Equal("btn btn-primary", attrs["class"]);
        Assert.Equal("submit-btn", attrs["id"]);
        Assert.Equal("disabled", attrs["disabled"]);
    }

    [Fact]
    public void Remove_Should_RemoveExistingAttribute()
    {
        var attrs = new HtmlAttributeBuilder()
            .Add("class", "btn")
            .Add("id", "test")
            .Remove("id")
            .Build();

        Assert.Single(attrs);
        Assert.DoesNotContain("id", attrs.Keys);
    }

    [Fact]
    public void Clear_Should_RemoveAllAttributes()
    {
        var builder = new HtmlAttributeBuilder()
            .Add("class", "btn")
            .Add("id", "test");

        Assert.Equal(2, builder.Count);

        builder.Clear();
        Assert.Equal(0, builder.Count);
        Assert.Empty(builder.Build());
    }

    [Fact]
    public void Render_Should_ProduceAttributeString()
    {
        var result = new HtmlAttributeBuilder()
            .Add("class", "btn")
            .Add("id", "submit")
            .Render();

        Assert.Contains("class=\"btn\"", result);
        Assert.Contains("id=\"submit\"", result);
    }

    [Fact]
    public void Render_Should_ReturnEmpty_WhenNoAttributes()
    {
        var result = new HtmlAttributeBuilder().Render();
        Assert.Equal("", result);
    }

    [Fact]
    public void Render_Should_EncodeAttributeValues()
    {
        var result = new HtmlAttributeBuilder()
            .Add("data-label", "Hello & Welcome")
            .Render();

        Assert.Contains("Hello &amp; Welcome", result);
    }

    [Fact]
    public void Add_Should_Throw_WhenNameIsEmpty()
    {
        var builder = new HtmlAttributeBuilder();
        Assert.Throws<ArgumentException>(() => builder.Add("", "value"));
    }

    [Fact]
    public void Add_Should_Throw_WhenValueIsNull()
    {
        var builder = new HtmlAttributeBuilder();
        Assert.Throws<ArgumentNullException>(() => builder.Add("class", null!));
    }

    [Fact]
    public void Chaining_Should_ProduceCorrectCount()
    {
        var builder = new HtmlAttributeBuilder()
            .Add("a", "1")
            .Add("b", "2")
            .Add("c", "3");

        Assert.Equal(3, builder.Count);
    }
}
