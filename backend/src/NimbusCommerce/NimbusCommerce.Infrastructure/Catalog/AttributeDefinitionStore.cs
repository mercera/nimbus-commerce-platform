using Microsoft.EntityFrameworkCore;
using NimbusCommerce.Application.Catalog.Interfaces;
using NimbusCommerce.Application.Common.Models;
using NimbusCommerce.Domain.Catalog;
using NimbusCommerce.Infrastructure.Persistence;

namespace NimbusCommerce.Infrastructure.Catalog;

internal sealed class AttributeDefinitionStore : IAttributeDefinitionStore
{
    private readonly ApplicationDbContext _dbContext;

    public AttributeDefinitionStore(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> ExistsWithNameAsync(string ownerUserId, string name, Guid? excludingAttributeDefinitionId, CancellationToken cancellationToken)
    {
        var query = _dbContext.AttributeDefinitions
            .AsNoTracking()
            .Where(a => a.OwnerUserId == ownerUserId && a.Name == name);

        if (excludingAttributeDefinitionId is Guid excluded)
        {
            query = query.Where(a => a.Id != excluded);
        }

        return query.AnyAsync(cancellationToken);
    }

    public Task<AttributeDefinition?> FindByIdAsync(string ownerUserId, Guid attributeDefinitionId, CancellationToken cancellationToken) =>
        _dbContext.AttributeDefinitions
            .Where(a => a.OwnerUserId == ownerUserId && a.Id == attributeDefinitionId)
            .SingleOrDefaultAsync(cancellationToken);

    public Task<AttributeDefinitionDetail?> GetDetailAsync(string ownerUserId, Guid attributeDefinitionId, CancellationToken cancellationToken) =>
        _dbContext.AttributeDefinitions
            .AsNoTracking()
            .Where(a => a.OwnerUserId == ownerUserId && a.Id == attributeDefinitionId)
            .Select(a => new AttributeDefinitionDetail(
                a.Id,
                a.Name,
                a.DataType,
                a.IsActive,
                a.CreatedAtUtc,
                a.CreatedByUserId,
                a.UpdatedAtUtc,
                a.UpdatedByUserId))
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<PagedResult<AttributeDefinitionListItem>> ListAsync(string ownerUserId, AttributeDefinitionQuery query, CancellationToken cancellationToken)
    {
        var attributeDefinitions = _dbContext.AttributeDefinitions
            .AsNoTracking()
            .Where(a => a.OwnerUserId == ownerUserId);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            attributeDefinitions = attributeDefinitions.Where(a => a.Name.Contains(search));
        }

        if (query.IsActive is bool isActive)
        {
            attributeDefinitions = attributeDefinitions.Where(a => a.IsActive == isActive);
        }

        // Count before paging, per the locked "filter and sort before paging, at the database
        // level" rule (same as CategoryStore.ListAsync).
        var totalCount = await attributeDefinitions.CountAsync(cancellationToken);

        var items = await attributeDefinitions
            .OrderBy(a => a.Name)
            .ThenBy(a => a.Id) // stable tiebreak across pages
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(a => new AttributeDefinitionListItem(a.Id, a.Name, a.DataType, a.IsActive, a.CreatedAtUtc))
            .ToListAsync(cancellationToken);

        return PagedResult<AttributeDefinitionListItem>.Create(items, query.Page, query.PageSize, totalCount);
    }

    public async Task<IReadOnlyList<AttributeDefinitionOption>> ListActiveOptionsAsync(string ownerUserId, CancellationToken cancellationToken) =>
        await _dbContext.AttributeDefinitions
            .AsNoTracking()
            .Where(a => a.OwnerUserId == ownerUserId && a.IsActive)
            .OrderBy(a => a.Name)
            .Select(a => new AttributeDefinitionOption(a.Id, a.Name, a.DataType))
            .ToListAsync(cancellationToken);

    // Filters by AttributeDefinitionId alone (no OwnerUserId re-join): the id being counted was
    // already proven to belong to the caller by the FindByIdAsync/GetDetailAsync lookup that ran
    // earlier in the calling use case, and "how many categories anywhere reference this specific,
    // already-owned definition" doesn't need a second ownership filter to stay tenant-safe.
    public Task<int> CountCategoryAssociationsAsync(string ownerUserId, Guid attributeDefinitionId, CancellationToken cancellationToken) =>
        _dbContext.CategoryAttributeDefinitions
            .Where(cad => cad.AttributeDefinitionId == attributeDefinitionId)
            .CountAsync(cancellationToken);

    public void Add(AttributeDefinition attributeDefinition) => _dbContext.AttributeDefinitions.Add(attributeDefinition);

    public void Remove(AttributeDefinition attributeDefinition) => _dbContext.AttributeDefinitions.Remove(attributeDefinition);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => _dbContext.SaveChangesAsync(cancellationToken);
}
