using NextNet.Cli.UI;
using Xunit;

namespace NextNet.Cli.Tests.UI;

public class NextNetTreeTests
{
    [Fact]
    public void CreateTree_WithRootLabel_SetsRoot()
    {
        var tree = new NextNetTree("Routes");
        Assert.NotNull(tree);
        Assert.NotNull(tree.GetSpectreTree());
    }

    [Fact]
    public void CreateTree_PlainMode_Works()
    {
        var tree = new NextNetTree("Root", OutputMode.Plain);
        Assert.NotNull(tree);
    }

    [Fact]
    public void AddNode_ReturnsNode()
    {
        var tree = new NextNetTree("Root");
        var node = tree.AddNode("Child");
        Assert.NotNull(node);
    }

    [Fact]
    public void AddNode_WithIcon_SetsLabel()
    {
        var tree = new NextNetTree("Root");
        var node = tree.AddNode("Child", icon: "📁");
        Assert.NotNull(node);
    }

    [Fact]
    public void TreeNode_AddChild_ReturnsChild()
    {
        var tree = new NextNetTree("Root");
        var parent = tree.AddNode("Parent");
        var child = parent.AddChild("Child");
        Assert.NotNull(child);
    }

    [Fact]
    public void TreeNode_MarkSuccess_DoesNotThrow()
    {
        var tree = new NextNetTree("Build");
        var node = tree.AddNode("Route discovery");
        node.MarkSuccess("Route discovery (12ms)");
    }

    [Fact]
    public void TreeNode_MarkError_DoesNotThrow()
    {
        var tree = new NextNetTree("Build");
        var node = tree.AddNode("Compilation");
        node.MarkError("Compilation failed");
    }

    [Fact]
    public void TreeNode_MarkPending_DoesNotThrow()
    {
        var tree = new NextNetTree("Build");
        var node = tree.AddNode("Static generation");
        node.MarkPending("Static generation");
    }

    [Fact]
    public void TreeNode_MarkActive_DoesNotThrow()
    {
        var tree = new NextNetTree("Build");
        var node = tree.AddNode("Compilation");
        node.MarkActive("Compilation...");
    }

    [Fact]
    public void Tree_PlainMode_Nodes_DoNotThrow()
    {
        var tree = new NextNetTree("Root", OutputMode.Plain);
        var node = tree.AddNode("Child");
        node.MarkSuccess("Done");
        node.MarkError("Fail");
        node.MarkPending("Wait");
        node.MarkActive("Active");
    }

    [Fact]
    public void TreeNode_AddChild_WithStyle_ReturnsChild()
    {
        var tree = new NextNetTree("Root");
        var parent = tree.AddNode("Parent");
        var child = parent.AddChild("Styled", Spectre.Console.Style.Plain);
        Assert.NotNull(child);
    }
}
