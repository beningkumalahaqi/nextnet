using Xunit;

namespace NextNet.Layouts.Tests;

public class LayoutDataTests
{
    [Fact]
    public void DefaultConstructor_Should_CreateEmptyData_When_Invoked()
    {
        var data = new LayoutData();
        Assert.NotNull(data.Data);
        Assert.Empty(data.Data);
    }

    [Fact]
    public void ParameterizedConstructor_Should_CreateEmptyData_When_NullPassed()
    {
        var data = new LayoutData(null);
        Assert.NotNull(data.Data);
        Assert.Empty(data.Data);
    }

    [Fact]
    public void ParameterizedConstructor_Should_StoreData_When_DictionaryProvided()
    {
        var dict = new Dictionary<string, object?>
        {
            ["title"] = "My Page",
            ["count"] = 42
        };
        var data = new LayoutData(dict);

        Assert.Equal(2, data.Data.Count);
        Assert.Equal("My Page", data.Data["title"]);
        Assert.Equal(42, data.Data["count"]);
    }

    [Fact]
    public void Merge_Should_MergeParentAndChildData_When_Called()
    {
        var parent = new LayoutData(new Dictionary<string, object?>
        {
            ["title"] = "Parent Title",
            ["layout"] = "root"
        });
        var child = new LayoutData(new Dictionary<string, object?>
        {
            ["title"] = "Child Title", // overrides parent
            ["page"] = "about"
        });

        var merged = child.Merge(parent);

        Assert.Equal("Child Title", merged.Data["title"]);   // child overrides
        Assert.Equal("root", merged.Data["layout"]);         // from parent
        Assert.Equal("about", merged.Data["page"]);          // from child
    }

    [Fact]
    public void Merge_Should_UseChildValue_When_KeyCollides()
    {
        var parent = new LayoutData(new Dictionary<string, object?> { ["key"] = "parent" });
        var child = new LayoutData(new Dictionary<string, object?> { ["key"] = "child" });

        var merged = child.Merge(parent);

        Assert.Equal("child", merged.Data["key"]);
    }

    [Fact]
    public void Merge_Should_ThrowArgumentNullException_When_ParentIsNull()
    {
        var child = new LayoutData();

        Assert.Throws<ArgumentNullException>(() => child.Merge(null!));
    }

    [Fact]
    public void Merge_Should_NotMutateOriginals_When_Called()
    {
        var parent = new LayoutData(new Dictionary<string, object?> { ["key"] = "parent" });
        var child = new LayoutData(new Dictionary<string, object?> { ["key"] = "child" });

        var merged = child.Merge(parent);

        Assert.Equal("parent", parent.Data["key"]);
        Assert.Equal("child", child.Data["key"]);
        Assert.Equal("child", merged.Data["key"]);
    }

    [Fact]
    public void TryGetValue_Should_ReturnTrue_When_KeyExists()
    {
        var data = new LayoutData(new Dictionary<string, object?> { ["name"] = "Test" });

        var found = data.TryGetValue<string>("name", out var value);

        Assert.True(found);
        Assert.Equal("Test", value);
    }

    [Fact]
    public void TryGetValue_Should_ReturnFalse_When_KeyMissing()
    {
        var data = new LayoutData();

        var found = data.TryGetValue<string>("missing", out var value);

        Assert.False(found);
        Assert.Null(value);
    }

    [Fact]
    public void TryGetValue_Should_ReturnFalse_When_TypeMismatch()
    {
        var data = new LayoutData(new Dictionary<string, object?> { ["count"] = 42 });

        var found = data.TryGetValue<string>("count", out var value);

        Assert.False(found);
        Assert.Null(value);
    }

    [Fact]
    public void SetValue_Should_StoreAndRetrieve_When_KeyIsNew()
    {
        var data = new LayoutData();
        data.SetValue("key", "value");

        Assert.True(data.Data.ContainsKey("key"));
        Assert.Equal("value", data.Data["key"]);
    }

    [Fact]
    public void SetValue_Should_OverwriteExisting_When_KeyAlreadyExists()
    {
        var data = new LayoutData(new Dictionary<string, object?> { ["key"] = "old" });
        data.SetValue("key", "new");

        Assert.Equal("new", data.Data["key"]);
    }
}
