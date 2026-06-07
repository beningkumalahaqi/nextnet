using System.Reflection;
using NextNet.Components;
using NextNet.Data;
using Xunit;

namespace NextNet.Architecture.Tests;

/// <summary>
/// Architecture invariant tests ensuring NextNet.Core and data-layer packages
/// maintain correct dependency isolation.
/// </summary>
public class CoreDependencyTests
{
    private readonly Assembly _coreAssembly = typeof(IPage).Assembly;
    private readonly Assembly _providersAssembly = typeof(NextNetDataBuilder).Assembly;

    /// <summary>
    /// NextNet.Core must not reference any NextNet.Data.* assemblies or EntityFrameworkCore.
    /// The core kernel must remain agnostic of data-layer concerns.
    /// </summary>
    [Fact]
    public void Core_Should_NotReference_DataPackages()
    {
        // Arrange
        var referencedAssemblies = _coreAssembly.GetReferencedAssemblies()
            .Select(a => a.Name)
            .Where(name => name is not null)
            .Cast<string>()
            .ToHashSet();

        var forbiddenPrefixes = new[] { "NextNet.Data", "EntityFrameworkCore" };

        // Act
        var violations = referencedAssemblies
            .Where(name => forbiddenPrefixes.Any(prefix =>
                name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        // Assert
        Assert.Empty(violations);
    }

    /// <summary>
    /// NextNet.Data.Providers must NOT reference specific provider implementation packages.
    /// The providers package is a lightweight registration/registry layer that should only
    /// depend on abstractions — not on EF Core, Dapper, MongoDB, SQLite, or PostgreSQL implementations.
    /// </summary>
    [Fact]
    public void DataProviders_Should_NotReference_ProviderImplementations()
    {
        // Arrange
        var referencedAssemblies = _providersAssembly.GetReferencedAssemblies()
            .Select(a => a.Name)
            .Where(name => name is not null)
            .Cast<string>()
            .ToHashSet();

        var forbiddenImplementations = new[]
        {
            "NextNet.Data.EntityFramework",
            "NextNet.Data.Dapper",
            "NextNet.Data.MongoDB",
            "NextNet.Data.Sqlite",
            "NextNet.Data.PostgreSQL"
        };

        // Act
        var violations = referencedAssemblies
            .Where(name => forbiddenImplementations.Any(prefix =>
                name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        // Assert
        Assert.Empty(violations);
    }
}
