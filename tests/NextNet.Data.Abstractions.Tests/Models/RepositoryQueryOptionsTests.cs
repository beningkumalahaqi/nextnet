using NextNet.Data.Abstractions.Models;
using Xunit;

namespace NextNet.Data.Abstractions.Tests.Models;

public class RepositoryQueryOptionsTests
{
    [Fact]
    public void Constructor_Should_SetDefaultPagination()
    {
        // Arrange & Act
        var options = new RepositoryQueryOptions();

        // Assert
        Assert.Equal(1, options.Page);
        Assert.Equal(20, options.PageSize);
        Assert.Null(options.Filter);
        Assert.Null(options.SortBy);
        Assert.False(options.SortDescending);
        Assert.Null(options.Includes);
    }

    [Fact]
    public void Constructor_Should_SetAllProperties()
    {
        // Arrange
        var includes = new[] { "Orders", "Profile" };

        // Act
        var options = new RepositoryQueryOptions(
            "Age > 18",
            "LastName",
            true,
            2,
            50,
            includes);

        // Assert
        Assert.Equal("Age > 18", options.Filter);
        Assert.Equal("LastName", options.SortBy);
        Assert.True(options.SortDescending);
        Assert.Equal(2, options.Page);
        Assert.Equal(50, options.PageSize);
        Assert.Equal(includes, options.Includes);
    }

    [Fact]
    public void EffectivePageSize_Should_ClampToMax_When_PageSizeExceedsLimit()
    {
        // Arrange & Act
        var options = new RepositoryQueryOptions(PageSize: 5000);

        // Assert
        Assert.Equal(1000, options.EffectivePageSize);
    }

    [Fact]
    public void EffectivePageSize_Should_ReturnMinimum_When_PageSizeIsLessThanOne()
    {
        // Arrange & Act
        var options = new RepositoryQueryOptions(PageSize: -5);

        // Assert
        Assert.Equal(20, options.EffectivePageSize);
    }

    [Fact]
    public void EffectivePageSize_Should_ReturnPageSize_When_WithinRange()
    {
        // Arrange & Act
        var options = new RepositoryQueryOptions(PageSize: 50);

        // Assert
        Assert.Equal(50, options.EffectivePageSize);
    }

    [Fact]
    public void EffectivePage_Should_ReturnMinimum_When_PageIsLessThanOne()
    {
        // Arrange & Act
        var options = new RepositoryQueryOptions(Page: 0);

        // Assert
        Assert.Equal(1, options.EffectivePage);
    }

    [Fact]
    public void EffectivePage_Should_ReturnPage_When_WithinRange()
    {
        // Arrange & Act
        var options = new RepositoryQueryOptions(Page: 3);

        // Assert
        Assert.Equal(3, options.EffectivePage);
    }
}
