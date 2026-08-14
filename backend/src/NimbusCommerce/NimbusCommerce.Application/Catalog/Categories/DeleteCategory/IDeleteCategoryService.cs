using NimbusCommerce.Application.Common.Models;

namespace NimbusCommerce.Application.Catalog.Categories.DeleteCategory;

public interface IDeleteCategoryService
{
    Task<OperationResult> DeleteAsync(Guid categoryId, CancellationToken cancellationToken);
}
