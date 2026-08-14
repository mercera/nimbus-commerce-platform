namespace NimbusCommerce.Application.Common.Models;

/// <summary>
/// Reusable server-side pagination envelope. Intended for Products, Orders, Customers and
/// Inventory alike — not catalogue-specific despite being introduced alongside it.
/// </summary>
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    public int TotalPages => TotalCount == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    public static PagedResult<T> Create(IReadOnlyList<T> items, int page, int pageSize, int totalCount) =>
        new(items, page, pageSize, totalCount);
}
