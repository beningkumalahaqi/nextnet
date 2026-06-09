namespace NextNet.Layouts;

/// <summary>
/// Carries data down through the layout chain. Each layout can read data set by
/// outer layouts and provide its own data for inner layouts and pages.
/// Child data overrides parent data when keys collide.
/// </summary>
/// <example>
/// <code>
/// var rootData = new LayoutData(new Dictionary&lt;string, object?&gt;
/// {
///     ["title"] = "My Site",
///     ["layout"] = "root"
/// });
/// var childData = new LayoutData(new Dictionary&lt;string, object?&gt;
/// {
///     ["title"] = "About Us"
/// });
/// var merged = childData.Merge(rootData);
/// Console.WriteLine(merged.Data["title"]);   // "About Us" (child wins)
/// Console.WriteLine(merged.Data["layout"]);  // "root" (inherited from parent)
/// </code>
/// </example>
public sealed class LayoutData
{
    /// <summary>
    /// Gets the underlying key-value data dictionary.
    /// </summary>
    public Dictionary<string, object?> Data { get; init; }

    /// <summary>
    /// Initializes a new instance of <see cref="LayoutData"/> with an empty data dictionary.
    /// </summary>
    public LayoutData()
    {
        Data = new Dictionary<string, object?>();
    }

    /// <summary>
    /// Initializes a new instance of <see cref="LayoutData"/> with the specified data.
    /// </summary>
    /// <param name="data">The initial data dictionary. If <c>null</c>, an empty dictionary is used.</param>
    public LayoutData(Dictionary<string, object?>? data)
    {
        Data = data ?? new Dictionary<string, object?>();
    }

    /// <summary>
    /// Merges data from a parent layout with this layout's data.
    /// Values from this layout override values from the parent when keys collide.
    /// Returns a new <see cref="LayoutData"/> instance; the originals are unchanged.
    /// </summary>
    /// <param name="parent">The parent layout's data to merge from.</param>
    /// <returns>A new <see cref="LayoutData"/> with the merged results.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="parent"/> is <c>null</c>.</exception>
    public LayoutData Merge(LayoutData parent)
    {
        if (parent == null) throw new ArgumentNullException(nameof(parent));

        var merged = new Dictionary<string, object?>(parent.Data);
        foreach (var (key, value) in Data)
        {
            merged[key] = value;
        }
        return new LayoutData(merged);
    }

    /// <summary>
    /// Tries to get a value from the data dictionary.
    /// </summary>
    /// <typeparam name="T">The expected type of the value.</typeparam>
    /// <param name="key">The data key.</param>
    /// <param name="value">When this method returns, contains the value if found and castable; otherwise, default.</param>
    /// <returns><c>true</c> if the key exists and the value can be cast to <typeparamref name="T"/>; otherwise, <c>false</c>.</returns>
    public bool TryGetValue<T>(string key, out T? value)
    {
        if (Data.TryGetValue(key, out var raw) && raw is T typed)
        {
            value = typed;
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Sets a value in the data dictionary.
    /// </summary>
    /// <param name="key">The data key.</param>
    /// <param name="value">The value to set.</param>
    public void SetValue(string key, object? value)
    {
        Data[key] = value;
    }
}
