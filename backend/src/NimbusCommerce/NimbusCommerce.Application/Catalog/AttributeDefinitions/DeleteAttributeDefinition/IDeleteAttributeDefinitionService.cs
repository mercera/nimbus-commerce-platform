using NimbusCommerce.Application.Common.Models;

namespace NimbusCommerce.Application.Catalog.AttributeDefinitions.DeleteAttributeDefinition;

public interface IDeleteAttributeDefinitionService
{
    Task<OperationResult> DeleteAsync(Guid attributeDefinitionId, CancellationToken cancellationToken);
}
