using NimbusCommerce.Application.Catalog.Interfaces;
using NimbusCommerce.Application.Common.Interfaces;
using NimbusCommerce.Application.Common.Models;

namespace NimbusCommerce.Application.Catalog.Categories.GetCategory;

internal sealed class GetCategoryService : IGetCategoryService
{
    private readonly ICategoryStore _categoryStore;
    private readonly ICurrentUser _currentUser;

    public GetCategoryService(ICategoryStore categoryStore, ICurrentUser currentUser)
    {
        _categoryStore = categoryStore;
        _currentUser = currentUser;
    }

    public async Task<OperationResult<CategoryDetail>> GetAsync(Guid categoryId, CancellationToken cancellationToken)
    {
        var detail = await _categoryStore.GetDetailAsync(_currentUser.RequireUserId(), categoryId, cancellationToken);

        return detail is null
            ? OperationResult<CategoryDetail>.NotFound("Category not found.")
            : OperationResult<CategoryDetail>.Success(detail);
    }
}
