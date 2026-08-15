using NimbusCommerce.Application.Catalog.Interfaces;
using NimbusCommerce.Application.Common.Models;

namespace NimbusCommerce.Application.Catalog.AttributeDefinitions.CreateAttributeDefinition;

public interface ICreateAttributeDefinitionService
{
    Task<OperationResult<AttributeDefinitionDetail>> CreateAsync(CreateAttributeDefinitionRequest request, CancellationToken cancellationToken);
}
