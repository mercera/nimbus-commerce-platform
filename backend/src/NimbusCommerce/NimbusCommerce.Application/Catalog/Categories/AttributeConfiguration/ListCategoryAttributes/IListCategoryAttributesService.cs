using NimbusCommerce.Application.Catalog.Interfaces;
using NimbusCommerce.Application.Common.Models;

namespace NimbusCommerce.Application.Catalog.Categories.AttributeConfiguration.ListCategoryAttributes;

public interface IListCategoryAttributesService
{
    Task<OperationResult<IReadOnlyList<CategoryAttributeItem>>> ListAsync(Guid categoryId, CancellationToken cancellationToken);
}
