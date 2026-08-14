using NimbusCommerce.Application.Common.Models;

namespace NimbusCommerce.UnitTests.Common;

public class PagedResultTests
{
    [Fact]
    public void TotalPages_ExactMultiple_ReturnsExactQuotient()
    {
        var result = PagedResult<int>.Create([], page: 1, pageSize: 20, totalCount: 100);

        Assert.Equal(5, result.TotalPages);
    }

    [Fact]
    public void TotalPages_WithRemainder_RoundsUp()
    {
        var result = PagedResult<int>.Create([], page: 1, pageSize: 20, totalCount: 101);

        Assert.Equal(6, result.TotalPages);
    }

    [Fact]
    public void TotalPages_ZeroTotalCount_ReturnsZero()
    {
        var result = PagedResult<int>.Create([], page: 1, pageSize: 20, totalCount: 0);

        Assert.Equal(0, result.TotalPages);
    }

    [Fact]
    public void TotalPages_SingleItem_ReturnsOne()
    {
        var result = PagedResult<int>.Create([1], page: 1, pageSize: 20, totalCount: 1);

        Assert.Equal(1, result.TotalPages);
    }
}
