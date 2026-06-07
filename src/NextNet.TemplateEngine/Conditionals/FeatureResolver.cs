using NextNet.Templates.Models;

namespace NextNet.TemplateEngine.Conditionals;

/// <summary>
/// Resolves feature dependencies, validates conflicts, and topologically sorts
/// selected features for a template generation session.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="FeatureResolver"/> takes the set of available features declared
/// in a template manifest and the set of features selected by the user, then:
/// <list type="number">
///   <item>Validates all selected features exist in the manifest.</item>
///   <item>Adds transitive dependencies automatically.</item>
///   <item>Detects conflicting features.</item>
///   <item>Detects circular dependencies using Kahn's algorithm.</item>
///   <item>Returns a topologically sorted feature list.</item>
/// </list>
/// </para>
/// <para>
/// Feature dependency resolution is important for ensuring that generated projects
/// have all required components. For example, if the user selects an "auth" feature
/// that depends on "identity", both features are automatically included.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var resolver = new FeatureResolver();
/// var features = new[] {
///     new TemplateFeature("auth", "Authentication", new[] { "identity" }),
///     new TemplateFeature("identity", "Identity management"),
///     new TemplateFeature("no-auth", "No authentication", null, new[] { "auth" })
/// };
/// var result = resolver.Resolve(features, new HashSet&lt;string&gt; { "auth" });
/// // result.ResolvedFeatures = ["identity", "auth"] (topologically sorted)
/// // result.Errors = null
/// </code>
/// </example>
public sealed class FeatureResolver
{
    /// <summary>
    /// Resolves the given set of selected features against the available features,
    /// producing a topologically sorted result with any errors or warnings.
    /// </summary>
    /// <param name="availableFeatures">The list of all features declared in the template manifest. Must not be null.</param>
    /// <param name="selectedFeatures">The set of feature names selected by the user. Must not be null.</param>
    /// <returns>A <see cref="FeatureResolutionResult"/> containing the resolved, sorted features and any errors or warnings.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="availableFeatures"/> or <paramref name="selectedFeatures"/> is null.</exception>
    public FeatureResolutionResult Resolve(
        IReadOnlyList<TemplateFeature> availableFeatures,
        HashSet<string> selectedFeatures)
    {
        ArgumentNullException.ThrowIfNull(availableFeatures);
        ArgumentNullException.ThrowIfNull(selectedFeatures);

        var errors = new List<string>();
        var warnings = new List<string>();

        // Build feature lookup
        var byName = availableFeatures.ToDictionary(f => f.Name, StringComparer.Ordinal);

        // Validate all selected features exist
        var resolved = new HashSet<string>(StringComparer.Ordinal);
        foreach (var feature in selectedFeatures)
        {
            if (!byName.ContainsKey(feature))
            {
                errors.Add($"Selected feature '{feature}' is not declared in the manifest.");
                continue;
            }
            resolved.Add(feature);
        }

        // Add transitive dependencies using BFS
        var toProcess = new Queue<string>(resolved);
        while (toProcess.Count > 0)
        {
            var current = toProcess.Dequeue();
            if (!byName.TryGetValue(current, out var feature))
                continue;

            if (feature.Dependencies is null)
                continue;

            foreach (var dep in feature.Dependencies)
            {
                if (!byName.ContainsKey(dep))
                {
                    errors.Add($"Feature '{current}' depends on undeclared feature '{dep}'.");
                    continue;
                }
                if (resolved.Add(dep))
                {
                    toProcess.Enqueue(dep);
                }
            }
        }

        // Detect conflicts
        foreach (var featureName in resolved.ToList())
        {
            if (!byName.TryGetValue(featureName, out var feature))
                continue;

            if (feature.Conflicts is null)
                continue;

            foreach (var conflict in feature.Conflicts)
            {
                if (resolved.Contains(conflict))
                {
                    errors.Add($"Feature '{featureName}' conflicts with feature '{conflict}'.");
                }
            }
        }

        // Detect cycles using Kahn's algorithm (topological sort)
        var inDegree = resolved.ToDictionary(f => f, _ => 0, StringComparer.Ordinal);
        var graph = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        foreach (var f in resolved)
            graph[f] = new List<string>();

        foreach (var featureName in resolved)
        {
            if (!byName.TryGetValue(featureName, out var feature))
                continue;

            if (feature.Dependencies is null)
                continue;

            foreach (var dep in feature.Dependencies)
            {
                if (resolved.Contains(dep))
                {
                    // Edge from dependency to dependent
                    graph[dep].Add(featureName);
                    inDegree[featureName]++;
                }
            }
        }

        // Topological sort using Kahn's algorithm
        var sorted = new List<string>();
        var queue = new Queue<string>(inDegree.Where(kvp => kvp.Value == 0).Select(kvp => kvp.Key));
        while (queue.Count > 0)
        {
            var node = queue.Dequeue();
            sorted.Add(node);
            foreach (var neighbor in graph[node])
            {
                inDegree[neighbor]--;
                if (inDegree[neighbor] == 0)
                    queue.Enqueue(neighbor);
            }
        }

        if (sorted.Count != resolved.Count)
        {
            var cycleNodes = inDegree.Where(kvp => kvp.Value > 0).Select(kvp => kvp.Key);
            errors.Add($"Circular dependency detected among features: {string.Join(", ", cycleNodes)}");
            // Still return partial order
        }

        return new FeatureResolutionResult(
            ResolvedFeatures: sorted,
            Errors: errors.Count > 0 ? errors : null,
            Warnings: warnings.Count > 0 ? warnings : null
        );
    }
}

/// <summary>
/// The result of resolving features against a template manifest.
/// </summary>
/// <remarks>
/// <para>
/// Contains the topologically sorted list of resolved features, and optional
/// lists of errors and warnings that occurred during resolution.
/// </para>
/// <para>
/// Even when errors are present (e.g., conflicts), the resolver returns a partial
/// result so that consumers can decide how to handle the situation.
/// </para>
/// </remarks>
/// <param name="ResolvedFeatures">The topologically sorted list of resolved feature names.</param>
/// <param name="Errors">Optional list of error messages (feature not found, conflicts, cycles).</param>
/// <param name="Warnings">Optional list of warning messages.</param>
public sealed record FeatureResolutionResult(
    IReadOnlyList<string> ResolvedFeatures,
    IReadOnlyList<string>? Errors = null,
    IReadOnlyList<string>? Warnings = null
);
