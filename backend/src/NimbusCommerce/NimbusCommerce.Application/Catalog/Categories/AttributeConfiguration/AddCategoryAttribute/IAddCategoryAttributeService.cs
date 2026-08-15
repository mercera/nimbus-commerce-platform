using NimbusCommerce.Application.Catalog.Interfaces;
using NimbusCommerce.Application.Common.Models;

namespace NimbusCommerce.Application.Catalog.Categories.AttributeConfiguration.AddCategoryAttribute;

public interface IAddCategoryAttributeService
{
    Task<OperationResult<CategoryAttributeItem>> AddAsync(Guid categoryId, AddCategoryAttributeRequest request, CancellationToken cancellationToken);
}
