using Microsoft.Extensions.DependencyInjection;
using NextNet.Components;
using NextNet.DesignSystem.Tokens;
using NextNet.UI.Abstractions.Components;
using NextNet.UI.Abstractions.Rendering;
using NextNet.UI.Rendering.Composition;
using Xunit;

namespace NextNet.UI.Rendering.Tests.Composition;

/// <summary>
/// Tests for <see cref="ComponentTreeRenderer"/> tree rendering correctness.
/// </summary>
public class ComponentTreeRendererTests
{
    [Fact]
    public void RenderNode_Should_ReturnFallbackHtml_When_NoRendererRegistered()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var renderer = new ComponentTreeRenderer(serviceProvider);
        var context = new RenderContext(
            new DesignTokenSet(),
            serviceProvider);
        var node = new ComponentNode(new TestComponent { ClassName = "test-component" });

        // Act
        var html = renderer.RenderNode(node, context).ToHtml();

        // Assert
        Assert.Contains("test-component", html);
        Assert.Contains("TestComponent", html);
    }

    [Fact]
    public void RenderTree_Should_ReturnFragment_When_MultipleRoots()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var renderer = new ComponentTreeRenderer(serviceProvider);
        var context = new RenderContext(
            new DesignTokenSet(),
            serviceProvider);
        var roots = new[]
        {
            new ComponentNode(new TestComponent { Id = "comp1" }),
            new ComponentNode(new TestComponent { Id = "comp2" })
        };

        // Act
        var html = renderer.RenderTree(roots, context).ToHtml();

        // Assert
        Assert.Contains("comp1", html);
        Assert.Contains("comp2", html);
    }

    [Fact]
    public void RenderTree_Should_Throw_When_RootsIsNull()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var renderer = new ComponentTreeRenderer(serviceProvider);
        var context = new RenderContext(
            new DesignTokenSet(),
            serviceProvider);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => renderer.RenderTree(null!, context));
    }

    [Fact]
    public void RenderTree_Should_Throw_When_ContextIsNull()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var renderer = new ComponentTreeRenderer(serviceProvider);
        var roots = Array.Empty<ComponentNode>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => renderer.RenderTree(roots, null!));
    }

    /// <summary>
    /// A simple test component for rendering tests.
    /// </summary>
    private sealed class TestComponent : IComponent
    {
        public string? ClassName { get; init; }
        public string? Style { get; init; }
        public string? Id { get; init; }
        public IReadOnlyList<IComponent> Children { get; init; } = Array.Empty<IComponent>();
    }
}
