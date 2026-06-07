using System.Reflection;
using Xunit;

namespace NextNet.Architecture.Tests;

/// <summary>
/// Architecture invariant tests ensuring NextNet.Data.Abstractions does not reference
/// specific data provider packages.
/// </summary>
public class DataAbstractionsDependencyTests
{
    private readonly Assembly _dataAbstractionsAssembly = typeof(NextNet.Data.Abstractions.Abstractions.IDataProvider).Assembly;

    /// <summary>
    /// NextNet.Data.Abstractions must NOT reference any specific provider packages.
    /// It should only reference abstractions (DI, logging, etc.) — not EF Core, Dapper,
    /// MongoDB.Driver, Npgsql, Sqlite, or MySQL implementations.
    /// </summary>
    [Fact]
    public void DataAbstractions_Should_NotReference_ProviderPackages()
    {
        // Arrange
        var referencedAssemblies = _dataAbstractionsAssembly.GetReferencedAssemblies()
            .Select(a => a.Name)
            .Where(name => name is not null)
            .Cast<string>()
            .ToHashSet();

        var forbiddenProviders = new[]
        {
            "EntityFrameworkCore",
            "Microsoft.EntityFrameworkCore",
            "Dapper",
            "MongoDB.Driver",
            "MongoDB.Bson",
            "Npgsql",
            "Microsoft.Data.Sqlite",
            "MySql",
            "MySql.Data",
            "MySqlConnector"
        };

        // Act
        var violations = referencedAssemblies
            .Where(name => forbiddenProviders.Any(prefix =>
                name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        // Assert
        Assert.Empty(violations);
    }
}
