using Microsoft.EntityFrameworkCore;
using NimbusCommerce.Application.Catalog.Interfaces;
using NimbusCommerce.Application.Common.Models;
using NimbusCommerce.Domain.Catalog;
using NimbusCommerce.Infrastructure.Persistence;

namespace NimbusCommerce.Infrastructure.Catalog;

internal sealed class CategoryStore : ICategoryStore
{
    private readonly ApplicationDbContext _dbContext;

    public CategoryStore(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> ExistsWithNameAsync(string ownerUserId, string name, Guid? excludingCategoryId, CancellationToken cancellationToken)
    {
        var query = _dbContext.Categories
            .AsNoTracking()
            .Where(c => c.OwnerUserId == ownerUserId && c.Name == name);

        if (excludingCategoryId is Guid excluded)
        {
            query = query.Where(c => c.Id != excluded);
        }

        return query.AnyAsync(cancellationToken);
    }

    public Task<Category?> FindByIdAsync(string ownerUserId, Guid categoryId, CancellationToken cancellationToken) =>
        _dbContext.Categories
            .Where(c => c.OwnerUserId == ownerUserId && c.Id == categoryId)
            .SingleOrDefaultAsync(cancellationToken);

    public Task<CategoryDetail?> GetDetailAsync(string ownerUserId, Guid categoryId, CancellationToken cancellationToken) =>
        _dbContext.Categories
            .AsNoTracking()
            .Where(c => c.OwnerUserId == ownerUserId && c.Id == categoryId)
            .Select(c => new CategoryDetail(
                c.Id,
                c.Name,
                c.Description,
                c.IsActive,
                c.CreatedAtUtc,
                c.CreatedByUserId,
                c.UpdatedAtUtc,
                c.UpdatedByUserId))
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<PagedResult<CategoryListItem>> ListAsync(string ownerUserId, CategoryQuery query, CancellationToken cancellationToken)
    {
        var categories = _dbContext.Categories
            .AsNoTracking()
            .Where(c => c.OwnerUserId == ownerUserId);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            categories = categories.Where(c => c.Name.Contains(search));
        }

        if (query.IsActive is bool isActive)
        {
            categories = categories.Where(c => c.IsActive == isActive);
        }

        // Count before paging, per the locked "filter and sort before paging, at the database
        // level" rule.
        var totalCount = await categories.CountAsync(cancellationToken);

        var items = await categories
            .OrderBy(c => c.Name)
            .ThenBy(c => c.Id) // stable tiebreak across pages
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(c => new CategoryListItem(c.Id, c.Name, c.Description, c.IsActive, c.CreatedAtUtc))
            .ToListAsync(cancellationToken);

        return PagedResult<CategoryListItem>.Create(items, query.Page, query.PageSize, totalCount);
    }

    public Task<int> CountActiveProductsAsync(string ownerUserId, Guid categoryId, CancellationToken cancellationToken) =>
        _dbContext.Products
            .Where(p => p.OwnerUserId == ownerUserId && p.CategoryId == categoryId && p.IsActive)
            .CountAsync(cancellationToken);

    public Task<int> CountProductsAsync(string ownerUserId, Guid categoryId, CancellationToken cancellationToken) =>
        _dbContext.Products
            .Where(p => p.OwnerUserId == ownerUserId && p.CategoryId == categoryId)
            .CountAsync(cancellationToken);

    public void Add(Category category) => _dbContext.Categories.Add(category);

    public void Remove(Category category) => _dbContext.Categories.Remove(category);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => _dbContext.SaveChangesAsync(cancellationToken);
}
