using System.Text.Encodings.Web;

namespace NextNet.UI.DesignSystem.Rendering;

/// <summary>
/// Provides a fluent API for accumulating HTML attributes that can later be
/// consumed by <see cref="HtmlContentBuilder"/> or rendered directly.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="HtmlAttributeBuilder"/> maintains an ordered dictionary of attribute
/// key-value pairs. It supports conditional attribute addition via <c>AddIf</c>
/// to keep rendering logic clean and declarative.
/// </para>
/// <para>
/// All attribute values are HTML-encoded when written to the output via
/// <see cref="Build"/>. Keys are not encoded as they are expected to be
/// well-known HTML attribute names.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var attrs = new HtmlAttributeBuilder()
///     .Add("class", "btn btn-primary")
///     .AddIf("disabled", "disabled", isDisabled)
///     .Add("id", "submit-btn")
///     .Build();
/// </code>
/// </example>
public sealed class HtmlAttributeBuilder
{
    private readonly Dictionary<string, string> _attributes = new();

    /// <summary>
    /// Adds or replaces an HTML attribute with the specified name and value.
    /// </summary>
    /// <param name="name">The attribute name (e.g., "class", "id", "disabled"). Must not be null or empty.</param>
    /// <param name="value">The attribute value. Must not be null.</param>
    /// <returns>This builder instance for chaining.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="name"/> is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public HtmlAttributeBuilder Add(string name, string value)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Attribute name cannot be null or empty.", nameof(name));
        ArgumentNullException.ThrowIfNull(value);

        _attributes[name] = value;
        return this;
    }

    /// <summary>
    /// Adds an HTML attribute only if the specified condition is <c>true</c>.
    /// </summary>
    /// <param name="name">The attribute name. Must not be null or empty.</param>
    /// <param name="value">The attribute value. Must not be null.</param>
    /// <param name="condition">The condition that must be <c>true</c> for the attribute to be added.</param>
    /// <returns>This builder instance for chaining.</returns>
    public HtmlAttributeBuilder AddIf(string name, string value, bool condition)
        => condition ? Add(name, value) : this;

    /// <summary>
    /// Removes an attribute with the specified name, if present.
    /// </summary>
    /// <param name="name">The attribute name to remove.</param>
    /// <returns>This builder instance for chaining.</returns>
    public HtmlAttributeBuilder Remove(string name)
    {
        _attributes.Remove(name);
        return this;
    }

    /// <summary>
    /// Returns a read-only view of the accumulated attributes.
    /// </summary>
    /// <returns>A read-only dictionary of attribute key-value pairs.</returns>
    public IReadOnlyDictionary<string, string> Build()
        => _attributes;

    /// <summary>
    /// Returns the number of attributes accumulated.
    /// </summary>
    public int Count => _attributes.Count;

    /// <summary>
    /// Clears all accumulated attributes.
    /// </summary>
    /// <returns>This builder instance for chaining.</returns>
    public HtmlAttributeBuilder Clear()
    {
        _attributes.Clear();
        return this;
    }

    /// <summary>
    /// Renders the accumulated attributes as an HTML attribute string.
    /// For example: <c>class="btn" id="submit"</c>.
    /// Returns an empty string if no attributes have been added.
    /// </summary>
    /// <returns>A string containing the rendered HTML attributes.</returns>
    public string Render()
    {
        if (_attributes.Count == 0) return string.Empty;

        var parts = new List<string>(_attributes.Count);
        foreach (var (name, value) in _attributes)
        {
            parts.Add($"{name}=\"{HtmlEncoder.Default.Encode(value)}\"");
        }

        return string.Join(" ", parts);
    }
}
