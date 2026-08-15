using NimbusCommerce.Application.Catalog.Interfaces;
using NimbusCommerce.Application.Common.Models;

namespace NimbusCommerce.Application.Catalog.AttributeDefinitions.UpdateAttributeDefinition;

public interface IUpdateAttributeDefinitionService
{
    Task<OperationResult<AttributeDefinitionDetail>> UpdateAsync(Guid attributeDefinitionId, UpdateAttributeDefinitionRequest request, CancellationToken cancellationToken);
}
