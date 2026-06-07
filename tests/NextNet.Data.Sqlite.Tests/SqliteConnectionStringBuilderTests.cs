namespace NextNet.Data.Sqlite.Tests;

/// <summary>
/// Tests for <see cref="SqliteConnectionStringBuilder"/> static helper methods.
/// </summary>
public sealed class SqliteConnectionStringBuilderTests
{
    [Fact]
    public void FromFile_Should_BuildBasicString_When_SimplePath()
    {
        // Arrange
        var filePath = "app.db";

        // Act
        var result = SqliteConnectionStringBuilder.FromFile(filePath);

        // Assert
        Assert.Contains("Data Source=", result);
        Assert.Contains("app.db", result);
    }

    [Fact]
    public void FromFile_Should_IncludeCache_When_Shared()
    {
        // Arrange
        var filePath = "app.db";

        // Act
        var result = SqliteConnectionStringBuilder.FromFile(filePath, SqliteCacheMode.Shared);

        // Assert
        Assert.Contains("Data Source=", result);
        Assert.Contains("Cache=Shared", result);
    }

    [Fact]
    public void FromFile_Should_ResolveRelativePath()
    {
        // Arrange
        var filePath = "mydata.db";

        // Act
        var result = SqliteConnectionStringBuilder.FromFile(filePath);

        // Assert
        var expectedFullPath = Path.GetFullPath(filePath);
        Assert.Contains(expectedFullPath, result);
    }

    [Fact]
    public void FromFile_Should_Throw_When_PathIsEmpty()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => SqliteConnectionStringBuilder.FromFile(""));
    }

    [Fact]
    public void FromFile_Should_Throw_When_PathIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => SqliteConnectionStringBuilder.FromFile(null!));
    }

    [Fact]
    public void InMemory_Should_UseMemorySource()
    {
        // Act
        var result = SqliteConnectionStringBuilder.InMemory();

        // Assert
        Assert.Contains(":memory:", result);
    }

    [Fact]
    public void InMemory_Should_IncludeName_When_Provided()
    {
        // Arrange
        var dbName = "myapp";

        // Act
        var result = SqliteConnectionStringBuilder.InMemory(dbName);

        // Assert
        Assert.Contains(dbName, result);
        Assert.Contains("Mode=Memory", result);
        Assert.Contains("Cache=Shared", result);
    }

    [Fact]
    public void FromFile_Should_NotIncludeCache_When_Default()
    {
        // Arrange
        var filePath = "app.db";

        // Act
        var result = SqliteConnectionStringBuilder.FromFile(filePath, SqliteCacheMode.Default);

        // Assert
        Assert.DoesNotContain("Cache=", result);
    }
}
