using NimbusCommerce.Application.Common.Models;
using NimbusCommerce.Domain.Catalog;

namespace NimbusCommerce.Application.Catalog.Interfaces;

public sealed record CategoryListItem(Guid Id, string Name, string? Description, bool IsActive, DateTime CreatedAtUtc);

public sealed record CategoryDetail(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive,
    DateTime CreatedAtUtc,
    string CreatedByUserId,
    DateTime? UpdatedAtUtc,
    string? UpdatedByUserId);

public sealed record CategoryQuery(string? Search, bool? IsActive, int Page, int PageSize);

/// <summary>
/// Application-owned data-access contract for Categories, mirroring IRefreshTokenStore: plain
/// records cross the boundary for reads, the tracked Domain aggregate is returned for writes so
/// its own mutators enforce its invariants, and no EF Core type is ever visible to Application.
/// </summary>
public interface ICategoryStore
{
    Task<bool> ExistsWithNameAsync(string ownerUserId, string name, Guid? excludingCategoryId, CancellationToken cancellationToken);

    /// <summary>Tracked lookup, for use cases that call a mutator and then SaveChangesAsync.</summary>
    Task<Category?> FindByIdAsync(string ownerUserId, Guid categoryId, CancellationToken cancellationToken);

    /// <summary>Untracked read projection, for GetCategory.</summary>
    Task<CategoryDetail?> GetDetailAsync(string ownerUserId, Guid categoryId, CancellationToken cancellationToken);

    Task<PagedResult<CategoryListItem>> ListAsync(string ownerUserId, CategoryQuery query, CancellationToken cancellationToken);

    /// <summary>Backs the "cannot deactivate a category with active products" rule.</summary>
    Task<int> CountActiveProductsAsync(string ownerUserId, Guid categoryId, CancellationToken cancellationToken);

    /// <summary>Backs the "cannot delete a category with any products" rule.</summary>
    Task<int> CountProductsAsync(string ownerUserId, Guid categoryId, CancellationToken cancellationToken);

    void Add(Category category);

    void Remove(Category category);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
