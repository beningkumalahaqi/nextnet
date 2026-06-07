using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using Xunit;

namespace NextNet.Architecture.Tests;

/// <summary>
/// Architecture invariant tests ensuring all public types have XML documentation comments.
/// </summary>
public class XmlDocumentationTests
{
    /// <summary>
    /// All public types in NextNet.Data.Abstractions must have XML documentation.
    /// This test loads the XML doc file and verifies every public type has a summary element.
    /// </summary>
    [Fact]
    public void AllPublicTypes_Should_HaveXmlDocumentation()
    {
        // Arrange
        var assembly = typeof(NextNet.Data.Abstractions.Abstractions.IDataProvider).Assembly;
        var xmlDocPath = Path.ChangeExtension(assembly.Location, ".xml");

        Assert.True(File.Exists(xmlDocPath),
            $"XML documentation file not found at: {xmlDocPath}");

        var doc = XDocument.Load(xmlDocPath);
        var members = doc.Root?.Element("members");
        Assert.NotNull(members);

        var documentedMembers = members.Elements("member")
            .Select(m => m.Attribute("name")?.Value)
            .Where(name => name != null)
            .ToHashSet();

        // Get all public types in the assembly
        var publicTypes = assembly.GetExportedTypes()
            .Where(t => t.IsPublic)
            .ToList();

        var missingDocTypes = new List<string>();

        foreach (var type in publicTypes)
        {
            var docKey = $"T:{type.FullName}";
            if (!documentedMembers.Contains(docKey))
            {
                missingDocTypes.Add(docKey);
            }
        }

        // Assert
        Assert.Empty(missingDocTypes);
    }
}
