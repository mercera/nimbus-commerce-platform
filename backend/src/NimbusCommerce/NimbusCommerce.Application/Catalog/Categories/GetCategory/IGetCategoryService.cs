using NimbusCommerce.Application.Catalog.Interfaces;
using NimbusCommerce.Application.Common.Models;

namespace NimbusCommerce.Application.Catalog.Categories.GetCategory;

public interface IGetCategoryService
{
    Task<OperationResult<CategoryDetail>> GetAsync(Guid categoryId, CancellationToken cancellationToken);
}
