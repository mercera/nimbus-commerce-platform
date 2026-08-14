using NimbusCommerce.Application.Catalog.Interfaces;
using NimbusCommerce.Application.Common.Models;

namespace NimbusCommerce.Application.Catalog.Categories.CreateCategory;

public interface ICreateCategoryService
{
    Task<OperationResult<CategoryDetail>> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken);
}
