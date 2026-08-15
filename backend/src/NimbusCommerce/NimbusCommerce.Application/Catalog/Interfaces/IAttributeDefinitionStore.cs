using NimbusCommerce.Application.Common.Models;
using NimbusCommerce.Domain.Catalog;

namespace NimbusCommerce.Application.Catalog.Interfaces;

public sealed record AttributeDefinitionListItem(Guid Id, string Name, AttributeDataType DataType, bool IsActive, DateTime CreatedAtUtc);

public sealed record AttributeDefinitionDetail(
    Guid Id,
    string Name,
    AttributeDataType DataType,
    bool IsActive,
    DateTime CreatedAtUtc,
    string CreatedByUserId,
    DateTime? UpdatedAtUtc,
    string? UpdatedByUserId);

/// <summary>Lightweight shape for the "add attribute to category" picker — GET /api/attribute-definitions/options.</summary>
public sealed record AttributeDefinitionOption(Guid Id, string Name, AttributeDataType DataType);

public sealed record AttributeDefinitionQuery(string? Search, bool? IsActive, int Page, int PageSize);

/// <summary>
/// Application-owned data-access contract for AttributeDefinitions, mirroring ICategoryStore:
/// plain records cross the boundary for reads, the tracked Domain aggregate is returned for
/// writes so its own mutators enforce its invariants, and no EF Core type is ever visible to
/// Application.
/// </summary>
public interface IAttributeDefinitionStore
{
    Task<bool> ExistsWithNameAsync(string ownerUserId, string name, Guid? excludingAttributeDefinitionId, CancellationToken cancellationToken);

    /// <summary>Tracked lookup, for use cases that call a mutator and then SaveChangesAsync.</summary>
    Task<AttributeDefinition?> FindByIdAsync(string ownerUserId, Guid attributeDefinitionId, CancellationToken cancellationToken);

    /// <summary>Untracked read projection, for GetAttributeDefinition and for validating an association target.</summary>
    Task<AttributeDefinitionDetail?> GetDetailAsync(string ownerUserId, Guid attributeDefinitionId, CancellationToken cancellationToken);

    Task<PagedResult<AttributeDefinitionListItem>> ListAsync(string ownerUserId, AttributeDefinitionQuery query, CancellationToken cancellationToken);

    /// <summary>Active-only, unpaginated, ordered by name — backs the picker endpoint.</summary>
    Task<IReadOnlyList<AttributeDefinitionOption>> ListActiveOptionsAsync(string ownerUserId, CancellationToken cancellationToken);

    /// <summary>Backs the "cannot delete a definition associated with any category" rule.</summary>
    Task<int> CountCategoryAssociationsAsync(string ownerUserId, Guid attributeDefinitionId, CancellationToken cancellationToken);

    void Add(AttributeDefinition attributeDefinition);

    void Remove(AttributeDefinition attributeDefinition);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
