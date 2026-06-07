namespace NextNet.TemplateEngine.Tests.Conditionals;

using NextNet.TemplateEngine.Conditionals;
using NextNet.Templates.Models;
using Xunit;

public static class EnumerableExtensions
{
    public static int IndexOf<T>(this IReadOnlyList<T> list, T item)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (EqualityComparer<T>.Default.Equals(list[i], item))
                return i;
        }
        return -1;
    }
}

public class FeatureResolverTests
{
    private readonly FeatureResolver _resolver = new();

    private static readonly TemplateFeature[] SampleFeatures = new[]
    {
        new TemplateFeature("auth", "Authentication", new[] { "identity" }),
        new TemplateFeature("identity", "Identity management"),
        new TemplateFeature("logging", "Logging"),
        new TemplateFeature("audit", "Audit trail", new[] { "logging" }),
        new TemplateFeature("legacy", "Legacy mode", null, new[] { "auth" })
    };

    [Fact]
    public void Resolve_Should_ReturnSelectedFeatures_When_NoDependencies()
    {
        var result = _resolver.Resolve(SampleFeatures, new HashSet<string> { "logging" });

        Assert.Contains("logging", result.ResolvedFeatures);
        Assert.Null(result.Errors);
        Assert.Null(result.Warnings);
    }

    [Fact]
    public void Resolve_Should_IncludeTransitiveDependencies()
    {
        var result = _resolver.Resolve(SampleFeatures, new HashSet<string> { "auth" });

        Assert.Contains("identity", result.ResolvedFeatures);
        Assert.Contains("auth", result.ResolvedFeatures);
        Assert.Null(result.Errors);
        Assert.Null(result.Warnings);
    }

    [Fact]
    public void Resolve_Should_DetectCycle()
    {
        var cyclicFeatures = new[]
        {
            new TemplateFeature("a", "A", new[] { "b" }),
            new TemplateFeature("b", "B", new[] { "c" }),
            new TemplateFeature("c", "C", new[] { "a" })
        };

        var result = _resolver.Resolve(cyclicFeatures, new HashSet<string> { "a" });

        Assert.NotNull(result.Errors);
        Assert.Contains(result.Errors, e => e.Contains("Circular dependency", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Resolve_Should_DetectConflict()
    {
        var result = _resolver.Resolve(SampleFeatures, new HashSet<string> { "auth", "legacy" });

        Assert.NotNull(result.Errors);
        Assert.Contains(result.Errors, e => e.Contains("conflict", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Resolve_Should_TopologicallySort()
    {
        var features = new[]
        {
            new TemplateFeature("c", "C", new[] { "a" }),
            new TemplateFeature("b", "B"),
            new TemplateFeature("a", "A")
        };

        var result = _resolver.Resolve(features, new HashSet<string> { "b", "c" });

        // b has no deps, a is dep of c, so a should come before c
        var resolved = result.ResolvedFeatures;
        Assert.Contains("a", resolved);
        Assert.Contains("b", resolved);
        Assert.Contains("c", resolved);

        // a must appear before c in the sorted list
        Assert.True(resolved.IndexOf("a") < resolved.IndexOf("c"),
            "Feature 'a' (dependency) should appear before 'c' (dependent) in topological sort");

        // b has no dependencies, could be anywhere
        Assert.Null(result.Errors);
        Assert.Null(result.Warnings);
    }

    [Fact]
    public void Resolve_Should_ReturnError_When_FeatureNotFound()
    {
        var result = _resolver.Resolve(SampleFeatures, new HashSet<string> { "nonexistent" });

        Assert.NotNull(result.Errors);
        Assert.Contains(result.Errors, e => e.Contains("not declared", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Resolve_Should_HandleEmptySelection()
    {
        var result = _resolver.Resolve(SampleFeatures, new HashSet<string>());

        Assert.Empty(result.ResolvedFeatures);
        Assert.Null(result.Errors);
        Assert.Null(result.Warnings);
    }

    [Fact]
    public void Resolve_Should_HandleNestedDependencies()
    {
        var nestedFeatures = new[]
        {
            new TemplateFeature("a", "A", new[] { "b" }),
            new TemplateFeature("b", "B", new[] { "c" }),
            new TemplateFeature("c", "C")
        };

        var result = _resolver.Resolve(nestedFeatures, new HashSet<string> { "a" });

        Assert.Equal(3, result.ResolvedFeatures.Count);
        Assert.Contains("a", result.ResolvedFeatures);
        Assert.Contains("b", result.ResolvedFeatures);
        Assert.Contains("c", result.ResolvedFeatures);

        // c should be before b, b before a
        Assert.True(result.ResolvedFeatures.IndexOf("c") < result.ResolvedFeatures.IndexOf("b"));
        Assert.True(result.ResolvedFeatures.IndexOf("b") < result.ResolvedFeatures.IndexOf("a"));

        Assert.Null(result.Errors);
    }

    [Fact]
    public void Resolve_Should_Throw_When_AvailableFeaturesNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _resolver.Resolve(null!, new HashSet<string>()));
    }

    [Fact]
    public void Resolve_Should_Throw_When_SelectedFeaturesNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _resolver.Resolve(Array.Empty<TemplateFeature>(), null!));
    }

    [Fact]
    public void Resolve_Should_HandleMultipleDependencies()
    {
        var features = new[]
        {
            new TemplateFeature("app", "App", new[] { "db", "logging", "auth" }),
            new TemplateFeature("db", "Database"),
            new TemplateFeature("logging", "Logging"),
            new TemplateFeature("auth", "Authentication")
        };

        var result = _resolver.Resolve(features, new HashSet<string> { "app" });

        Assert.Equal(4, result.ResolvedFeatures.Count);
        // All deps must be before app
        var appIndex = result.ResolvedFeatures.IndexOf("app");
        Assert.True(result.ResolvedFeatures.IndexOf("db") < appIndex);
        Assert.True(result.ResolvedFeatures.IndexOf("logging") < appIndex);
        Assert.True(result.ResolvedFeatures.IndexOf("auth") < appIndex);

        Assert.Null(result.Errors);
    }
}
