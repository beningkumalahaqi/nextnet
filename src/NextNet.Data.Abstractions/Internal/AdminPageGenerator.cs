using System.Text;
using NextNet.Data.Abstractions.Models;

namespace NextNet.Data.Abstractions.Internal;

/// <summary>
/// Shared helper for generating admin CRUD page source code.
/// Both EF Core and Dapper admin scaffold providers use this to produce
/// <c>List.cs</c>, <c>Detail.cs</c>, <c>Create.cs</c>, <c>Edit.cs</c>,
/// and <c>Delete.cs</c> page components, plus the <c>AdminLayout.cs</c> layout.
/// </summary>
internal sealed class AdminPageGenerator
{
    private const string ToolVersion = "1.0.0";

    private readonly string _entityName;
    private readonly ScaffoldOptions _options;
    private readonly AdminScaffoldOptions _adminOptions;

    private readonly string _projectNamespace;
    private readonly string _pluralName;
    private readonly string _lowerName;
    private readonly string _lowerPlural;
    private readonly string _adminNs;
    private readonly string _adminDir;
    private readonly string _routePrefix;
    private readonly string _layoutName;
    private readonly string _adminTitle;
    private readonly string _keyType;
    private readonly string _modelNs;
    private readonly string _repoNs;
    private readonly IReadOnlyList<ScaffoldProperty> _properties;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminPageGenerator"/> class.
    /// </summary>
    public AdminPageGenerator(
        string entityName,
        ScaffoldOptions options,
        AdminScaffoldOptions adminOptions,
        string providerName)
    {
        _entityName = entityName ?? throw new ArgumentNullException(nameof(entityName));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _adminOptions = adminOptions ?? throw new ArgumentNullException(nameof(adminOptions));

        _projectNamespace = options.ProjectNamespace ?? "App";
        _pluralName = TemplateEngine.Pluralize(entityName);
        _lowerName = char.ToLowerInvariant(entityName[0]) + entityName[1..];
        _lowerPlural = char.ToLowerInvariant(_pluralName[0]) + _pluralName[1..];
        _adminNs = adminOptions.AdminNamespace ?? $"{_projectNamespace}.Admin";
        _adminDir = adminOptions.AdminDirectory ?? Path.Combine(options.OutputDirectory, "app", "admin");
        _routePrefix = adminOptions.RoutePrefix ?? "admin";
        _layoutName = adminOptions.LayoutName ?? "AdminLayout";
        _adminTitle = adminOptions.AdminTitle ?? $"{_projectNamespace} Admin";
        _keyType = DetermineKeyType(adminOptions.Properties);
        _properties = adminOptions.Properties ?? Array.Empty<ScaffoldProperty>();
        _modelNs = TemplateEngine.ResolveNamespace(options.ModelsNamespace, _projectNamespace);
        _repoNs = TemplateEngine.ResolveNamespace(options.RepositoriesNamespace, _projectNamespace);
    }

    /// <summary>
    /// Generates all admin pages and returns artifact descriptors.
    /// </summary>
    public Task<ScaffoldArtifact[]> GenerateAllAsync(CancellationToken cancellationToken = default)
    {
        var artifacts = new List<ScaffoldArtifact>();

        // Step 1: AdminLayout.cs
        artifacts.Add(GenerateLayoutFile());

        // Step 2: Entity admin pages
        artifacts.Add(GenerateListFile());
        artifacts.Add(GenerateDetailFile());
        artifacts.Add(GenerateCreateFile());
        artifacts.Add(GenerateEditFile());
        artifacts.Add(GenerateDeleteFile());

        return Task.FromResult(artifacts.ToArray());
    }

    #region File Generation Helpers

    private ScaffoldArtifact GenerateLayoutFile()
    {
        var layoutDir = Path.GetFullPath(_adminDir);
        var layoutPath = Path.Combine(layoutDir, $"{_layoutName}.cs");
        var layoutRelativePath = Path.Combine("app", "admin", $"{_layoutName}.cs");

        if (!_options.DryRun && !_options.OverwriteExisting && File.Exists(layoutPath))
        {
            var existingContent = File.ReadAllText(layoutPath);
            return new ScaffoldArtifact(FilePath: layoutPath, RelativePath: layoutRelativePath,
                ArtifactType: ScaffoldArtifactType.AdminPage, EntityName: _entityName,
                LinesOfCode: TemplateEngine.CountLines(existingContent), WasSkipped: true);
        }

        var content = BuildAdminLayoutContent();
        var lines = TemplateEngine.CountLines(content);
        var skipped = TemplateEngine.WriteOrSkip(layoutPath, content, _options.DryRun, _options.OverwriteExisting);

        return new ScaffoldArtifact(FilePath: layoutPath, RelativePath: layoutRelativePath,
            ArtifactType: ScaffoldArtifactType.AdminPage, EntityName: _entityName,
            LinesOfCode: lines, WasSkipped: skipped);
    }

    private ScaffoldArtifact GenerateListFile()
    {
        var content = BuildListContent();
        return WritePageFile("List", content);
    }

    private ScaffoldArtifact GenerateDetailFile()
    {
        var content = BuildDetailContent();
        return WritePageFile("Detail", content);
    }

    private ScaffoldArtifact GenerateCreateFile()
    {
        var content = BuildCreateContent();
        return WritePageFile("Create", content);
    }

    private ScaffoldArtifact GenerateEditFile()
    {
        var content = BuildEditContent();
        return WritePageFile("Edit", content);
    }

    private ScaffoldArtifact GenerateDeleteFile()
    {
        var content = BuildDeleteContent();
        return WritePageFile("Delete", content);
    }

    #endregion

    #region Content Builders

    private string BuildAdminLayoutContent()
    {
        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated />");
        sb.AppendLine($"// Generated by NextNet Admin Scaffolding v{ToolVersion} on {DateTime.Now:yyyy-MM-dd}");
        sb.AppendLine("// This file is generated once. Modify to customize the admin chrome.");
        sb.AppendLine();
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using System.Text.Encodings.Web;");
        sb.AppendLine("using System.Text;");
        sb.AppendLine("using System.Threading.Tasks;");
        sb.AppendLine("using NextNet.Components;");
        sb.AppendLine();
        sb.AppendLine($"namespace {_adminNs};");
        sb.AppendLine();

        // Class declaration
        sb.AppendLine($"/// <summary>");
        sb.AppendLine($"/// Admin layout providing sidebar navigation and shared chrome");
        sb.AppendLine($"/// for all admin CRUD pages.");
        sb.AppendLine($"/// </summary>");
        sb.AppendLine($"public class {_layoutName} : ILayout");
        sb.AppendLine("{");
        sb.AppendLine("    public string? PageTitle { get; set; }");
        sb.AppendLine("    public IReadOnlyList<AdminNavItemModel>? Navigation { get; set; }");
        sb.AppendLine();
        sb.AppendLine("    public async Task<IHtmlContent> Render(IHtmlContent children)");
        sb.AppendLine("    {");
        sb.AppendLine("        var navHtml = BuildNavigationHtml();");
        sb.AppendLine("        var breadcrumbHtml = BuildBreadcrumbHtml();");
        sb.AppendLine();
        sb.AppendLine("        var html = $@\"<div class=\"\"admin-shell\"\">");
        sb.AppendLine("    <aside class=\"\"admin-sidebar\"\">");
        sb.AppendLine($"        <div class=\"\"admin-brand\"\">{_adminTitle}</div>");
        sb.AppendLine("        <nav>{navHtml.ToHtml()}</nav>");
        sb.AppendLine("    </aside>");
        sb.AppendLine("    <div class=\"\"admin-content-area\"\">");
        sb.AppendLine("        <header class=\"\"admin-topbar\"\">{breadcrumbHtml.ToHtml()}</header>");
        sb.AppendLine("        <main class=\"\"admin-page\"\">{children.ToHtml()}</main>");
        sb.AppendLine("    </div>");
        sb.AppendLine("</div>\";");
        sb.AppendLine("        return HtmlHelper.Raw(html);");
        sb.AppendLine("    }");
        sb.AppendLine();

        // BuildNavigationHtml
        sb.AppendLine("    private IHtmlContent BuildNavigationHtml()");
        sb.AppendLine("    {");
        sb.AppendLine("        var nav = Navigation ?? Array.Empty<AdminNavItemModel>();");
        sb.AppendLine("        var sb = new StringBuilder();");
        sb.AppendLine("        sb.Append(\"<ul class=\\\"admin-nav\\\">\");");
        sb.AppendLine("        sb.Append(\"<li><a href=\\\"/\\\">Back to Site</a></li>\");");
        sb.AppendLine("        sb.Append(\"<li class=\\\"admin-nav-divider\\\"></li>\");");
        sb.AppendLine("        foreach (var item in nav)");
        sb.AppendLine("        {");
        sb.AppendLine("            sb.Append(\"<li><a href=\\\"\");");
        sb.AppendLine("            sb.Append(HtmlEncoder.Default.Encode(item.Route));");
        sb.AppendLine("            sb.Append(\"\\\">\");");
        sb.AppendLine("            sb.Append(HtmlEncoder.Default.Encode(item.Label));");
        sb.AppendLine("            sb.Append(\"</a></li>\");");
        sb.AppendLine("        }");
        sb.AppendLine("        sb.Append(\"</ul>\");");
        sb.AppendLine("        return HtmlHelper.Raw(sb.ToString());");
        sb.AppendLine("    }");
        sb.AppendLine();

        // BuildBreadcrumbHtml
        sb.AppendLine("    private IHtmlContent BuildBreadcrumbHtml()");
        sb.AppendLine("    {");
        sb.AppendLine("        var html = $\"<nav class=\\\"admin-breadcrumbs\\\">");
        sb.AppendLine($"            <a href=\\\"/{_routePrefix}\\\">Dashboard</a>");
        sb.AppendLine("            <span class=\\\"sep\\\">/</span>");
        sb.AppendLine("            <span>{HtmlEncoder.Default.Encode(PageTitle ?? \"\")}</span>");
        sb.AppendLine("        </nav>\";");
        sb.AppendLine("        return HtmlHelper.Raw(html);");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        sb.AppendLine();

        // AdminNavItemModel record
        sb.AppendLine("public sealed record AdminNavItemModel(");
        sb.AppendLine("    string Label,");
        sb.AppendLine("    string Route,");
        sb.AppendLine("    string? Icon = null,");
        sb.AppendLine("    string? Badge = null");
        sb.AppendLine(");");

        return sb.ToString();
    }

    private string BuildListContent()
    {
        var sb = new StringBuilder();
        AppendHeader(sb, "List", "list page");
        AppendRouteComment(sb, "List", $"GET /{_routePrefix}/{_lowerPlural}");

        sb.AppendLine($"public class List : IPage");
        sb.AppendLine("{");
        sb.AppendLine($"    private readonly IRepository<{_entityName}> _repository;");
        sb.AppendLine();
        sb.AppendLine("    public int Page { get; set; } = 1;");
        sb.AppendLine("    public int PageSize { get; set; } = 20;");
        sb.AppendLine("    public string? Search { get; set; }");
        sb.AppendLine("    public string? SortBy { get; set; }");
        sb.AppendLine("    public bool SortDescending { get; set; }");
        sb.AppendLine();
        sb.AppendLine("    public IReadOnlyDictionary<string, object> Props { get; }");
        sb.AppendLine("        = new Dictionary<string, object>();");
        sb.AppendLine();
        sb.AppendLine($"    public List(IRepository<{_entityName}> repository)");
        sb.AppendLine("    {");
        sb.AppendLine("        _repository = repository ?? throw new ArgumentNullException(nameof(repository));");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    public async Task<IHtmlContent> Render()");
        sb.AppendLine("    {");
        sb.AppendLine("        var options = new RepositoryQueryOptions(");
        sb.AppendLine("            Filter: Search,");
        sb.AppendLine("            SortBy: SortBy,");
        sb.AppendLine("            SortDescending: SortDescending,");
        sb.AppendLine("            Page: Page,");
        sb.AppendLine("            PageSize: PageSize");
        sb.AppendLine("        );");
        sb.AppendLine();
        sb.AppendLine("        var result = await _repository.GetAllAsync(options);");
        sb.AppendLine();
        sb.AppendLine("        var html = $@\"<div class=\"\"admin-list-page\"\">");
        sb.AppendLine("    <div class=\"\"admin-list-toolbar\"\">");
        sb.AppendLine("        <form method=\"\"get\"\" class=\"\"admin-search\"\">");
        sb.AppendLine("            <input type=\"\"search\"\" name=\"\"search\"\" value=\"\"{HtmlEncoder.Default.Encode(Search ?? \"\")}\"\"");
        sb.AppendLine($"                   placeholder=\"\"Search {_pluralName}...\"\" />");
        sb.AppendLine("            <button type=\"\"submit\"\">Search</button>");
        sb.AppendLine("        </form>");
        sb.AppendLine($"        <a href=\"\"/{_routePrefix}/{_lowerPlural}/create\"\" class=\"\"btn btn-primary\"\">+ New {_entityName}</a>");
        sb.AppendLine("    </div>");
        sb.AppendLine("    <table class=\"\"admin-table\"\">");
        sb.AppendLine("        <thead><tr>");
        sb.AppendLine($"            {BuildColumnsHeaderHtml()}");
        sb.AppendLine("            <th>Actions</th>");
        sb.AppendLine("        </tr></thead>");
        sb.AppendLine("        <tbody>");
        sb.AppendLine("            {{#Items}}");
        sb.AppendLine("            <tr>");
        sb.AppendLine($"                {BuildColumnValuesHtml()}");
        sb.AppendLine("                <td class=\"\"admin-actions\"\">");
        sb.AppendLine($"                    <a href=\"\"/{_routePrefix}/{_lowerPlural}/{{{{id}}}}\"\">View</a>");
        sb.AppendLine($"                    <a href=\"\"/{_routePrefix}/{_lowerPlural}/{{{{id}}}}/edit\"\">Edit</a>");
        sb.AppendLine($"                    <a href=\"\"/{_routePrefix}/{_lowerPlural}/{{{{id}}}}/delete\"\" class=\"\"danger\"\">Delete</a>");
        sb.AppendLine("                </td>");
        sb.AppendLine("            </tr>");
        sb.AppendLine("            {{/Items}}");
        sb.AppendLine("        </tbody>");
        sb.AppendLine("    </table>");
        sb.AppendLine("    <div class=\"\"admin-pagination\"\">");
        sb.AppendLine("        <span>Page {result.Page} of {Math.Max(1, (int)Math.Ceiling((double)result.TotalCount / result.PageSize))}</span>");
        sb.AppendLine("        <div class=\"\"admin-pagination-links\"\">");
        sb.AppendLine("            {{(result.HasPreviousPage ? $\"<a href=\\\"?page={result.Page - 1}\\\">&laquo; Previous</a>\" : \"\")}}");
        sb.AppendLine("            {{(result.HasNextPage ? $\"<a href=\\\"?page={result.Page + 1}\\\">Next &raquo;</a>\" : \"\")}}");
        sb.AppendLine("        </div>");
        sb.AppendLine("    </div>");
        sb.AppendLine("</div>\";");
        sb.AppendLine("        return HtmlHelper.Raw(html);");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private string BuildDetailContent()
    {
        var sb = new StringBuilder();
        AppendHeader(sb, "Detail", "detail page");
        AppendRouteComment(sb, "Detail", $"GET /{_routePrefix}/{_lowerPlural}/{{id}}");

        sb.AppendLine($"public class Detail : IPage");
        sb.AppendLine("{");
        sb.AppendLine($"    private readonly IRepository<{_entityName}> _repository;");
        sb.AppendLine();
        sb.AppendLine($"    public {_keyType} Id {{ get; set; }}");
        sb.AppendLine();
        sb.AppendLine("    public IReadOnlyDictionary<string, object> Props { get; }");
        sb.AppendLine("        = new Dictionary<string, object>();");
        sb.AppendLine();
        sb.AppendLine($"    public Detail(IRepository<{_entityName}> repository)");
        sb.AppendLine("    {");
        sb.AppendLine("        _repository = repository ?? throw new ArgumentNullException(nameof(repository));");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    public async Task<IHtmlContent> Render()");
        sb.AppendLine("    {");
        sb.AppendLine("        var entity = await _repository.GetByIdAsync(Id);");
        sb.AppendLine("        if (entity is null)");
        sb.AppendLine("        {");
        sb.AppendLine($"            return HtmlHelper.Raw(\"<div class=\\\"admin-not-found\\\"><h2>Not Found</h2><p>The requested {_entityName} was not found.</p><a href=\\\"/{_routePrefix}/{_lowerPlural}\\\">Back to {_pluralName}</a></div>\");");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        var html = $@\"<div class=\"\"admin-detail-page\"\">");
        sb.AppendLine($"    <h2>{{_entityName}} Details</h2>");
        sb.AppendLine("    <dl class=\"\"admin-detail-grid\"\">");
        sb.AppendLine("        <div class=\"\"admin-detail-row\"\"><dt>Id</dt><dd>{entity.Id}</dd></div>");
        sb.AppendLine("    </dl>");
        sb.AppendLine("    <div class=\"\"admin-detail-actions\"\">");
        sb.AppendLine($"        <a href=\"\"/{_routePrefix}/{_lowerPlural}/{{{{Id}}}}/edit\"\" class=\"\"btn\"\">Edit</a>");
        sb.AppendLine($"        <a href=\"\"/{_routePrefix}/{_lowerPlural}/{{{{Id}}}}/delete\"\" class=\"\"btn btn-danger\"\">Delete</a>");
        sb.AppendLine($"        <a href=\"\"/{_routePrefix}/{_lowerPlural}\"\" class=\"\"btn\"\">Back to List</a>");
        sb.AppendLine("    </div>");
        sb.AppendLine("</div>\";");
        sb.AppendLine("        return HtmlHelper.Raw(html);");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private string BuildCreateContent()
    {
        var sb = new StringBuilder();
        AppendHeader(sb, "Create", "create page");
        AppendRouteComment(sb, "Create", $"GET+POST /{_routePrefix}/{_lowerPlural}/create");

        sb.AppendLine($"public class Create : IPage");
        sb.AppendLine("{");
        sb.AppendLine($"    private readonly IRepository<{_entityName}> _repository;");
        sb.AppendLine();
        sb.AppendLine("    public string Method { get; set; } = \"GET\";");
        sb.AppendLine($"    public {_entityName}? Input {{ get; set; }}");
        sb.AppendLine("    public Dictionary<string, string>? Errors { get; set; }");
        sb.AppendLine();
        sb.AppendLine("    public IReadOnlyDictionary<string, object> Props { get; }");
        sb.AppendLine("        = new Dictionary<string, object>();");
        sb.AppendLine();
        sb.AppendLine($"    public Create(IRepository<{_entityName}> repository)");
        sb.AppendLine("    {");
        sb.AppendLine("        _repository = repository ?? throw new ArgumentNullException(nameof(repository));");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    public async Task<IHtmlContent> Render()");
        sb.AppendLine("    {");
        sb.AppendLine("        if (string.Equals(Method, \"POST\", StringComparison.OrdinalIgnoreCase) && Input is not null && Errors is null)");
        sb.AppendLine("        {");
        sb.AppendLine("            await _repository.InsertAsync(Input);");
        sb.AppendLine("            return HtmlHelper.Raw($@\"<div class=\"\"admin-success\"\">");
        sb.AppendLine($"    <h2>{{_entityName}} Created</h2>");
        sb.AppendLine($"    <p>The {_lowerName} has been created successfully.</p>");
        sb.AppendLine($"    <a href=\"\"/{_routePrefix}/{_lowerPlural}\"\">Back to {_pluralName}</a>");
        sb.AppendLine("</div>\");");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        var formFields = new System.Text.StringBuilder();");
        sb.AppendLine("        var html = $@\"<div class=\"\"admin-form-page\"\">");
        sb.AppendLine($"    <form method=\"\"post\"\" action=\"\"/{_routePrefix}/{_lowerPlural}/create\"\" class=\"\"admin-form\"\">");
        sb.AppendLine($"        {BuildFormFieldsHtml()}");
        sb.AppendLine("        {{(Errors is not null ? BuildErrorsHtml() : \"\")}}");
        sb.AppendLine("        <div class=\"\"admin-form-actions\"\">");
        sb.AppendLine($"            <button type=\"\"submit\"\" class=\"\"btn btn-primary\"\">Create {_entityName}</button>");
        sb.AppendLine($"            <a href=\"\"/{_routePrefix}/{_lowerPlural}\"\" class=\"\"btn\"\">Cancel</a>");
        sb.AppendLine("        </div>");
        sb.AppendLine("    </form>");
        sb.AppendLine("</div>\";");
        sb.AppendLine("        return HtmlHelper.Raw(html);");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    private string BuildErrorsHtml()");
        sb.AppendLine("    {");
        sb.AppendLine("        var sb = new System.Text.StringBuilder();");
        sb.AppendLine("        sb.Append(\"<div class=\\\"admin-errors\\\"><ul>\");");
        sb.AppendLine("        foreach (var err in Errors!)");
        sb.AppendLine("            sb.Append($\"<li>{HtmlEncoder.Default.Encode(err.Value)}</li>\");");
        sb.AppendLine("        sb.Append(\"</ul></div>\");");
        sb.AppendLine("        return sb.ToString();");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private string BuildEditContent()
    {
        var sb = new StringBuilder();
        AppendHeader(sb, "Edit", "edit page");
        AppendRouteComment(sb, "Edit", $"GET+POST /{_routePrefix}/{_lowerPlural}/{{id}}/edit");

        sb.AppendLine($"public class Edit : IPage");
        sb.AppendLine("{");
        sb.AppendLine($"    private readonly IRepository<{_entityName}> _repository;");
        sb.AppendLine();
        sb.AppendLine($"    public {_keyType} Id {{ get; set; }}");
        sb.AppendLine("    public string Method { get; set; } = \"GET\";");
        sb.AppendLine($"    public {_entityName}? Input {{ get; set; }}");
        sb.AppendLine("    public Dictionary<string, string>? Errors { get; set; }");
        sb.AppendLine();
        sb.AppendLine("    public IReadOnlyDictionary<string, object> Props { get; }");
        sb.AppendLine("        = new Dictionary<string, object>();");
        sb.AppendLine();
        sb.AppendLine($"    public Edit(IRepository<{_entityName}> repository)");
        sb.AppendLine("    {");
        sb.AppendLine("        _repository = repository ?? throw new ArgumentNullException(nameof(repository));");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    public async Task<IHtmlContent> Render()");
        sb.AppendLine("    {");
        sb.AppendLine("        if (string.Equals(Method, \"POST\", StringComparison.OrdinalIgnoreCase) && Input is not null && Errors is null)");
        sb.AppendLine("        {");
        sb.AppendLine("            await _repository.UpdateAsync(Input);");
        sb.AppendLine("            return HtmlHelper.Raw($@\"<div class=\"\"admin-success\"\">");
        sb.AppendLine($"    <h2>{{_entityName}} Updated</h2>");
        sb.AppendLine($"    <p>The {_lowerName} has been updated successfully.</p>");
        sb.AppendLine($"    <a href=\"\"/{_routePrefix}/{_lowerPlural}\"\">Back to {_pluralName}</a>");
        sb.AppendLine("</div>\");");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        var entity = await _repository.GetByIdAsync(Id);");
        sb.AppendLine("        if (entity is null)");
        sb.AppendLine("        {");
        sb.AppendLine($"            return HtmlHelper.Raw(\"<div class=\\\"admin-not-found\\\"><h2>Not Found</h2><p>The requested {_entityName} was not found.</p><a href=\\\"/{_routePrefix}/{_lowerPlural}\\\">Back to {_pluralName}</a></div>\");");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        var html = $@\"<div class=\"\"admin-form-page\"\">");
        sb.AppendLine($"    <form method=\"\"post\"\" action=\"\"/{_routePrefix}/{_lowerPlural}/{{{{Id}}}}/edit\"\" class=\"\"admin-form\"\">");
        sb.AppendLine($"        {BuildFormFieldsHtml()}");
        sb.AppendLine("        {{(Errors is not null ? BuildErrorsHtml() : \"\")}}");
        sb.AppendLine("        <div class=\"\"admin-form-actions\"\">");
        sb.AppendLine($"            <button type=\"\"submit\"\" class=\"\"btn btn-primary\"\">Save {_entityName}</button>");
        sb.AppendLine($"            <a href=\"\"/{_routePrefix}/{_lowerPlural}\"\" class=\"\"btn\"\">Cancel</a>");
        sb.AppendLine("        </div>");
        sb.AppendLine("    </form>");
        sb.AppendLine("</div>\";");
        sb.AppendLine("        return HtmlHelper.Raw(html);");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    private string BuildErrorsHtml()");
        sb.AppendLine("    {");
        sb.AppendLine("        var sb = new System.Text.StringBuilder();");
        sb.AppendLine("        sb.Append(\"<div class=\\\"admin-errors\\\"><ul>\");");
        sb.AppendLine("        foreach (var err in Errors!)");
        sb.AppendLine("            sb.Append($\"<li>{HtmlEncoder.Default.Encode(err.Value)}</li>\");");
        sb.AppendLine("        sb.Append(\"</ul></div>\");");
        sb.AppendLine("        return sb.ToString();");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private string BuildDeleteContent()
    {
        var sb = new StringBuilder();
        AppendHeader(sb, "Delete", "delete confirmation page");
        AppendRouteComment(sb, "Delete", $"GET+POST /{_routePrefix}/{_lowerPlural}/{{id}}/delete");

        sb.AppendLine($"public class Delete : IPage");
        sb.AppendLine("{");
        sb.AppendLine($"    private readonly IRepository<{_entityName}> _repository;");
        sb.AppendLine();
        sb.AppendLine($"    public {_keyType} Id {{ get; set; }}");
        sb.AppendLine("    public string Method { get; set; } = \"GET\";");
        sb.AppendLine();
        sb.AppendLine("    public IReadOnlyDictionary<string, object> Props { get; }");
        sb.AppendLine("        = new Dictionary<string, object>();");
        sb.AppendLine();
        sb.AppendLine($"    public Delete(IRepository<{_entityName}> repository)");
        sb.AppendLine("    {");
        sb.AppendLine("        _repository = repository ?? throw new ArgumentNullException(nameof(repository));");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    public async Task<IHtmlContent> Render()");
        sb.AppendLine("    {");
        sb.AppendLine("        var entity = await _repository.GetByIdAsync(Id);");
        sb.AppendLine("        if (entity is null)");
        sb.AppendLine("        {");
        sb.AppendLine($"            return HtmlHelper.Raw(\"<div class=\\\"admin-not-found\\\"><h2>Not Found</h2><p>The requested {_entityName} was not found.</p><a href=\\\"/{_routePrefix}/{_lowerPlural}\\\">Back to {_pluralName}</a></div>\");");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        if (string.Equals(Method, \"POST\", StringComparison.OrdinalIgnoreCase))");
        sb.AppendLine("        {");
        sb.AppendLine("            await _repository.DeleteAsync(Id);");
        sb.AppendLine("            return HtmlHelper.Raw($@\"<div class=\"\"admin-success\"\">");
        sb.AppendLine($"    <h2>{{_entityName}} Deleted</h2>");
        sb.AppendLine($"    <p>The {_lowerName} has been deleted successfully.</p>");
        sb.AppendLine($"    <a href=\"\"/{_routePrefix}/{_lowerPlural}\"\">Back to {_pluralName}</a>");
        sb.AppendLine("</div>\");");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        return HtmlHelper.Raw($@\"<div class=\"\"admin-delete-page\"\">");
        sb.AppendLine("    <div class=\"\"admin-delete-confirm\"\">");
        sb.AppendLine($"        <h2>Delete {_entityName}</h2>");
        sb.AppendLine($"        <p>Are you sure you want to delete this {_lowerName}?</p>");
        sb.AppendLine($"        <p class=\"\"admin-delete-warning\"\">This action cannot be undone.</p>");
        sb.AppendLine($"        <form method=\"\"post\"\" action=\"\"/{_routePrefix}/{_lowerPlural}/{{{{Id}}}}/delete\"\">");
        sb.AppendLine("            <div class=\"\"admin-form-actions\"\">");
        sb.AppendLine($"                <button type=\"\"submit\"\" class=\"\"btn btn-danger\"\">Delete {_entityName}</button>");
        sb.AppendLine($"                <a href=\"\"/{_routePrefix}/{_lowerPlural}\"\" class=\"\"btn\"\">Cancel</a>");
        sb.AppendLine("            </div>");
        sb.AppendLine("        </form>");
        sb.AppendLine("    </div>");
        sb.AppendLine("</div>\");");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    #endregion

    #region Utility Methods

    private void AppendHeader(StringBuilder sb, string pageType, string description)
    {
        sb.AppendLine("// <auto-generated />");
        sb.AppendLine($"// Generated by NextNet Admin Scaffolding v{ToolVersion} on {DateTime.Now:yyyy-MM-dd}");
        sb.AppendLine($"// Entity: {_entityName}");
        sb.AppendLine();
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using System.Threading.Tasks;");
        sb.AppendLine("using System.Text.Encodings.Web;");
        sb.AppendLine("using NextNet.Components;");
        sb.AppendLine("using NextNet.Data.Abstractions.Abstractions;");
        sb.AppendLine("using NextNet.Data.Abstractions.Models;");
        sb.AppendLine($"using {_modelNs};");
        sb.AppendLine();
        sb.AppendLine($"namespace {_adminNs}.{_entityName};");
        sb.AppendLine();
        sb.AppendLine($"/// <summary>");
        sb.AppendLine($"/// Admin {description} for {_entityName}.");
        sb.AppendLine($"/// </summary>");
    }

    private void AppendRouteComment(StringBuilder sb, string pageType, string route)
    {
        sb.AppendLine($"/// <remarks>Route: {route}</remarks>");
    }

    private ScaffoldArtifact WritePageFile(string pageType, string content)
    {
        var entityDir = Path.GetFullPath(Path.Combine(_adminDir, _entityName));
        var entityRelativeDir = Path.Combine("app", "admin", _entityName);
        var filePath = Path.Combine(entityDir, $"{pageType}.cs");
        var relativePath = Path.Combine(entityRelativeDir, $"{pageType}.cs");
        var lines = TemplateEngine.CountLines(content);
        var skipped = TemplateEngine.WriteOrSkip(filePath, content, _options.DryRun, _options.OverwriteExisting);

        return new ScaffoldArtifact(FilePath: filePath, RelativePath: relativePath,
            ArtifactType: ScaffoldArtifactType.AdminPage, EntityName: _entityName,
            LinesOfCode: lines, WasSkipped: skipped);
    }

    private string BuildColumnsHeaderHtml()
    {
        if (_properties.Count == 0)
            return "<th>Id</th><th>Name</th>";

        var sb = new StringBuilder();
        foreach (var prop in _properties)
        {
            if (prop.IsKey) continue;
            sb.Append($"<th><a href=\"?sortBy={prop.Name}&sortDesc={{{{SortDescending ? \"false\" : \"true\"}}}}\">{SplitPascalCase(prop.Name)}</a></th>");
        }
        return sb.ToString();
    }

    private string BuildColumnValuesHtml()
    {
        if (_properties.Count == 0)
            return "<td>{{entity.Id}}</td><td>{{entity.Name}}</td>";

        var sb = new StringBuilder();
        foreach (var prop in _properties)
        {
            if (prop.IsKey) continue;
            sb.Append($"<td>{{{{HtmlEncoder.Default.Encode(entity.{prop.Name}?.ToString() ?? \"\")}}}}</td>");
        }
        return sb.ToString();
    }

    private string BuildFormFieldsHtml()
    {
        if (_properties.Count == 0)
        {
            return "<div class=\"admin-form-field\"><label for=\"Name\">Name</label><input type=\"text\" id=\"Name\" name=\"Name\" value=\"{{Input?.Name}}\" class=\"admin-input\" /></div>";
        }

        var sb = new StringBuilder();
        foreach (var prop in _properties)
        {
            if (prop.IsKey) continue;
            var displayName = SplitPascalCase(prop.Name);
            var inputType = GetInputType(prop.Type);
            sb.Append($"<div class=\"admin-form-field\">");
            sb.Append($"<label for=\"{prop.Name}\">{displayName}</label>");
            sb.Append($"<input type=\"{inputType}\" id=\"{prop.Name}\" name=\"{prop.Name}\" value=\"{{{{Input?.{prop.Name}}}}}\" class=\"admin-input\" />");
            sb.Append("</div>");
        }
        return sb.ToString();
    }

    private static string DetermineKeyType(IReadOnlyList<ScaffoldProperty>? properties)
    {
        if (properties is not null)
            foreach (var prop in properties)
                if (prop.IsKey) return prop.Type;
        return "int";
    }

    private static string SplitPascalCase(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        var sb = new StringBuilder();
        for (int i = 0; i < name.Length; i++)
        {
            if (i > 0 && char.IsUpper(name[i])) sb.Append(' ');
            sb.Append(name[i]);
        }
        return sb.ToString();
    }

    private static string GetInputType(string propertyType)
    {
        return propertyType.ToLowerInvariant() switch
        {
            "int" or "long" or "decimal" or "double" or "float" => "number",
            "bool" => "checkbox",
            "datetime" or "datetimeoffset" => "datetime-local",
            "email" => "email",
            "uri" => "url",
            _ => "text"
        };
    }

    #endregion
}
