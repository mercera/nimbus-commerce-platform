using NimbusCommerce.Application.Catalog.Interfaces;
using NimbusCommerce.Application.Common.Models;

namespace NimbusCommerce.Application.Catalog.AttributeDefinitions.GetAttributeDefinition;

public interface IGetAttributeDefinitionService
{
    Task<OperationResult<AttributeDefinitionDetail>> GetAsync(Guid attributeDefinitionId, CancellationToken cancellationToken);
}
