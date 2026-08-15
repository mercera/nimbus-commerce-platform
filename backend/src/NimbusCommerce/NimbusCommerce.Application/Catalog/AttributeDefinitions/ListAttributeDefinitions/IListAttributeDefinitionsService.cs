using NimbusCommerce.Application.Catalog.Interfaces;
using NimbusCommerce.Application.Common.Models;

namespace NimbusCommerce.Application.Catalog.AttributeDefinitions.ListAttributeDefinitions;

public interface IListAttributeDefinitionsService
{
    Task<PagedResult<AttributeDefinitionListItem>> ListAsync(ListAttributeDefinitionsRequest request, CancellationToken cancellationToken);
}
