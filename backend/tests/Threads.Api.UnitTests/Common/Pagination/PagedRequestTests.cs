using FluentAssertions;
using Threads.Api.Common.Pagination;

namespace Threads.Api.UnitTests.Common.Pagination;

public class PagedRequestTests
{
    [Fact]
    public void Constructor_ShouldUseDefaultPageSize_WhenPageSizeIsOmitted()
    {
        // Arrange

        // Act
        var request = new PagedRequest(cursor: null);

        // Assert
        request.PageSize.Should().Be(20);
    }

    [Theory]
    [InlineData(-100)]
    [InlineData(0)]
    public void Constructor_ShouldClampPageSizeToOne_WhenPageSizeIsBelowMinimum(int pageSize)
    {
        // Arrange

        // Act
        var request = new PagedRequest(cursor: null, pageSize: pageSize);

        // Assert
        request.PageSize.Should().Be(1);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(20)]
    [InlineData(100)]
    public void Constructor_ShouldPreservePageSize_WhenPageSizeIsWithinRange(int pageSize)
    {
        // Arrange

        // Act
        var request = new PagedRequest(cursor: null, pageSize: pageSize);

        // Assert
        request.PageSize.Should().Be(pageSize);
    }

    [Theory]
    [InlineData(101)]
    [InlineData(int.MaxValue)]
    public void Constructor_ShouldClampPageSizeToOneHundred_WhenPageSizeIsAboveMaximum(int pageSize)
    {
        // Arrange

        // Act
        var request = new PagedRequest(cursor: null, pageSize: pageSize);

        // Assert
        request.PageSize.Should().Be(100);
    }

    [Fact]
    public void Constructor_ShouldPreserveCursor_WhenCursorIsProvided()
    {
        // Arrange
        const string cursor = "encoded-cursor";

        // Act
        var request = new PagedRequest(cursor: cursor);

        // Assert
        request.Cursor.Should().Be(cursor);
    }
}
