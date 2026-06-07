using NextNet.Data.Abstractions.Models;

namespace NextNet.Data.EntityFramework.Scaffolding.Internal;

/// <summary>
/// Builds C# property declaration strings from <see cref="ScaffoldProperty"/> lists,
/// using EF Core data annotation attributes.
/// </summary>
internal static class TemplatePropertyBuilder
{
    /// <summary>
    /// Builds property declaration lines suitable for injection into an EF Core model template.
    /// Each property includes XML doc comments, data annotations, and the property signature.
    /// Properties flagged as <see cref="ScaffoldProperty.IsKey"/> are excluded (key is generated
    /// separately in the template).
    /// </summary>
    /// <param name="properties">The list of properties to render. May be null or empty.</param>
    /// <returns>A string containing all property declarations, ready for template insertion.</returns>
    public static string BuildPropertyDeclarations(IReadOnlyList<ScaffoldProperty>? properties)
    {
        if (properties is null || properties.Count == 0)
            return string.Empty;

        var sb = new System.Text.StringBuilder();
        foreach (var prop in properties)
        {
            if (prop.IsKey)
                continue;

            sb.AppendLine();
            sb.AppendLine("    /// <summary>");
            sb.AppendLine($"    /// Gets or sets the {prop.Name}.");
            sb.AppendLine("    /// </summary>");

            if (prop.IsRequired)
                sb.AppendLine("    [Required]");
            if (prop.MaxLength.HasValue)
                sb.AppendLine($"    [MaxLength({prop.MaxLength.Value})]");

            var typeSuffix = prop.IsRequired ? string.Empty : "?";
            sb.AppendLine($"    public {prop.Type}{typeSuffix} {prop.Name} {{ get; set; }}");
        }
        return sb.ToString();
    }
}
