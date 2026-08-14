using NimbusCommerce.Application.Catalog.Interfaces;
using NimbusCommerce.Application.Common.Models;

namespace NimbusCommerce.Application.Catalog.Categories.UpdateCategory;

public interface IUpdateCategoryService
{
    Task<OperationResult<CategoryDetail>> UpdateAsync(Guid categoryId, UpdateCategoryRequest request, CancellationToken cancellationToken);
}
