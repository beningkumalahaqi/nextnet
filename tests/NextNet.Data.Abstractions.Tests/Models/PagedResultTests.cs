using NextNet.Data.Abstractions.Models;
using Xunit;

namespace NextNet.Data.Abstractions.Tests.Models;

public class PagedResultTests
{
    [Fact]
    public void Constructor_Should_SetAllProperties()
    {
        // Arrange
        var items = new List<string> { "a", "b", "c" };

        // Act
        var result = new PagedResult<string>(items, 100, 1, 10);

        // Assert
        Assert.Same(items, result.Items);
        Assert.Equal(100, result.TotalCount);
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
    }

    [Fact]
    public void Constructor_Should_ComputeHasNextPage_When_MorePagesExist()
    {
        // Arrange & Act
        var result = new PagedResult<string>(new List<string> { "a" }, 50, 1, 10);

        // Assert
        Assert.True(result.HasNextPage);
    }

    [Fact]
    public void Constructor_Should_SetHasNextPageFalse_When_OnLastPage()
    {
        // Arrange & Act
        var result = new PagedResult<string>(new List<string> { "a" }, 10, 1, 10);

        // Assert
        Assert.False(result.HasNextPage);
    }

    [Fact]
    public void Constructor_Should_SetHasNextPageFalse_When_TotalCountEqualsLastPageBoundary()
    {
        // page 2 of 2, with page size 10, total 20 = last page
        var result = new PagedResult<string>(new List<string>(), 20, 2, 10);

        Assert.False(result.HasNextPage);
    }

    [Fact]
    public void Constructor_Should_SetHasPreviousPageTrue_When_PageGreaterThanOne()
    {
        // Arrange & Act
        var result = new PagedResult<string>(new List<string> { "a" }, 100, 2, 10);

        // Assert
        Assert.True(result.HasPreviousPage);
    }

    [Fact]
    public void Constructor_Should_SetHasPreviousPageFalse_When_OnFirstPage()
    {
        // Arrange & Act
        var result = new PagedResult<string>(new List<string> { "a" }, 100, 1, 10);

        // Assert
        Assert.False(result.HasPreviousPage);
    }

    [Fact]
    public void Constructor_Should_HandleEmptyItems()
    {
        // Arrange & Act
        var result = new PagedResult<string>(new List<string>(), 0, 1, 10);

        // Assert
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.False(result.HasNextPage);
        Assert.False(result.HasPreviousPage);
    }
}
