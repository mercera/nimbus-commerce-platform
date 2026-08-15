using NimbusCommerce.Application.Catalog.Interfaces;
using NimbusCommerce.Application.Common.Models;

namespace NimbusCommerce.Application.Catalog.Categories.AttributeConfiguration.SetCategoryAttributeRequired;

public interface ISetCategoryAttributeRequiredService
{
    Task<OperationResult<CategoryAttributeItem>> SetRequiredAsync(Guid categoryId, Guid attributeDefinitionId, SetCategoryAttributeRequiredRequest request, CancellationToken cancellationToken);
}
