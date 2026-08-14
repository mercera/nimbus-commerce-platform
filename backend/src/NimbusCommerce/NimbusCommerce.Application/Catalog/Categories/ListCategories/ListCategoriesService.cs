using NimbusCommerce.Application.Catalog.Interfaces;
using NimbusCommerce.Application.Common.Interfaces;
using NimbusCommerce.Application.Common.Models;

namespace NimbusCommerce.Application.Catalog.Categories.ListCategories;

internal sealed class ListCategoriesService : IListCategoriesService
{
    private readonly ICategoryStore _categoryStore;
    private readonly ICurrentUser _currentUser;

    public ListCategoriesService(ICategoryStore categoryStore, ICurrentUser currentUser)
    {
        _categoryStore = categoryStore;
        _currentUser = currentUser;
    }

    public Task<PagedResult<CategoryListItem>> ListAsync(ListCategoriesRequest request, CancellationToken cancellationToken)
    {
        var query = new CategoryQuery(request.Search, request.IsActive, request.Page, request.PageSize);

        return _categoryStore.ListAsync(_currentUser.RequireUserId(), query, cancellationToken);
    }
}
