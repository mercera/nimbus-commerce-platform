using NimbusCommerce.Application.Catalog.Interfaces;

namespace NimbusCommerce.Application.Catalog.AttributeDefinitions.ListAttributeDefinitionOptions;

/// <summary>Backs GET /api/attribute-definitions/options — active only, unpaginated, for the "add attribute to category" picker.</summary>
public interface IListAttributeDefinitionOptionsService
{
    Task<IReadOnlyList<AttributeDefinitionOption>> ListAsync(CancellationToken cancellationToken);
}
