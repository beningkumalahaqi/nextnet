using NextNet.DevTools.UI;
using Xunit;

namespace NextNet.DevTools.Tests;

public class DevToolsTreeTests
{
    [Fact]
    public void Render_Should_ShowLabel_When_GivenRootLabel()
    {
        var tree = new DevToolsTree("Root");
        var output = tree.Render();
        Assert.Contains("Root", output);
    }

    [Fact]
    public void Render_Should_ShowHierarchy_When_NodesAdded()
    {
        var tree = new DevToolsTree("Root");
        var child = tree.AddNode("Child1", icon: "📄");
        child.AddChild("Grandchild", color: ConsoleColor.Blue);
        tree.AddNode("Child2");

        var output = tree.Render();
        Assert.Contains("Child1", output);
        Assert.Contains("Child2", output);
        Assert.Contains("Grandchild", output);
        Assert.Contains("📄", output);
    }

    [Fact]
    public void RenderToConsole_Should_WriteToConsole_When_Called()
    {
        var tree = new DevToolsTree("Root");
        tree.AddNode("Node1");

        var output = CaptureConsoleOutput(() => tree.RenderToConsole());
        Assert.Contains("Root", output);
        Assert.Contains("Node1", output);
    }

    [Fact]
    public void TreeNode_Should_ReturnChild_When_AddChildCalled()
    {
        var node = new DevToolsTreeNode("Parent");
        var child = node.AddChild("Child");
        Assert.Contains(child, node.Children);
        Assert.Equal("Child", child.Label);
    }

    [Fact]
    public void TreeNode_Should_HaveDefaults_When_Initialized()
    {
        var node = new DevToolsTreeNode("Test", icon: "🔧", color: ConsoleColor.Green);
        Assert.Equal("Test", node.Label);
        Assert.Equal("🔧", node.Icon);
        Assert.Equal(ConsoleColor.Green, node.Color);
    }

    private static string CaptureConsoleOutput(Action action)
    {
        var original = System.Console.Out;
        try
        {
            using var writer = new StringWriter();
            System.Console.SetOut(writer);
            action();
            return writer.ToString();
        }
        finally
        {
            System.Console.SetOut(original);
        }
    }
}
