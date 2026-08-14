using NimbusCommerce.Application.Catalog.Interfaces;
using NimbusCommerce.Application.Common.Models;

namespace NimbusCommerce.Application.Catalog.Categories.ListCategories;

public interface IListCategoriesService
{
    Task<PagedResult<CategoryListItem>> ListAsync(ListCategoriesRequest request, CancellationToken cancellationToken);
}
