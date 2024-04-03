using Passwordless.AdminConsole.Components.Shared.Tables;
using Xunit;

namespace Passwordless.AdminConsole.Tests.Components.Shared.Table;

public class PagedTableTests
{
    [Fact]
    public void Ctor_ShouldSetCurrentIndex()
    {
        // Arrange
        var count = 10;
        var page = 1;
        var pageSize = 5;

        // Act
        var pagedTable = new PagedTable(count, page, pageSize);

        // Assert
        Assert.Equal(page, pagedTable.CurrentPage);
        Assert.False(pagedTable.HasPreviousPage);
        Assert.True(pagedTable.HasNextPage);
    }

    [Fact]
    public void Ctor_ShouldSetTotalPages()
    {
        // Arrange
        var count = 10;
        var page = 1;
        var pageSize = 5;

        // Act
        var pagedTable = new PagedTable(count, page, pageSize);

        // Assert
        Assert.Equal(2, pagedTable.TotalPages);
    }

    [Fact]
    public void Ctor_ShouldSetPages()
    {
        // Arrange
        var count = 10;
        var page = 1;
        var pageSize = 5;

        // Act
        var pagedTable = new PagedTable(count, page, pageSize);

        // Assert
        Assert.Equal(new[] { 1, 2 }, pagedTable.Pages);
    }

    [Fact]
    public void Ctor_SetsExpectedAmountOfPages_WhenLastPageContainsLessItemsThanPageSize()
    {
        // Arrange
        var count = 11;
        var page = 2;
        var pageSize = 5;

        // Act
        var pagedTable = new PagedTable(count, page, pageSize);

        // Assert
        Assert.True(pagedTable.HasPreviousPage);
        Assert.True(pagedTable.HasNextPage);
        Assert.Equal(3, pagedTable.TotalPages);
    }
}