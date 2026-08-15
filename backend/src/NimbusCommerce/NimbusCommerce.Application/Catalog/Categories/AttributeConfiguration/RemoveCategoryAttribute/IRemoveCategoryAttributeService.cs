using NimbusCommerce.Application.Common.Models;

namespace NimbusCommerce.Application.Catalog.Categories.AttributeConfiguration.RemoveCategoryAttribute;

public interface IRemoveCategoryAttributeService
{
    Task<OperationResult> RemoveAsync(Guid categoryId, Guid attributeDefinitionId, CancellationToken cancellationToken);
}
