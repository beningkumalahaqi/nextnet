namespace NextNet.Data.Abstractions.Models;

/// <summary>
/// Admin-specific scaffold options extending <see cref="ScaffoldOptions"/>.
/// Controls admin page generation behavior including route prefix, layout,
/// and entity property metadata.
/// </summary>
/// <param name="RoutePrefix">URL prefix for admin routes. Default: "admin".</param>
/// <param name="LayoutName">The layout class name to use. Default: "AdminLayout".</param>
/// <param name="AdminNamespace">Namespace for generated admin pages.</param>
/// <param name="AdminDirectory">Output directory for admin pages.</param>
/// <param name="Properties">Entity properties for form generation.</param>
/// <param name="AdminTitle">Title shown in admin layout header.</param>
public sealed record AdminScaffoldOptions(
    string RoutePrefix = "admin",
    string LayoutName = "AdminLayout",
    string? AdminNamespace = null,
    string? AdminDirectory = null,
    IReadOnlyList<ScaffoldProperty>? Properties = null,
    string? AdminTitle = null
);
