using NimbusCommerce.Application.Catalog.Interfaces;
using NimbusCommerce.Application.Common.Interfaces;
using NimbusCommerce.Application.Common.Models;

namespace NimbusCommerce.Application.Catalog.Categories.AttributeConfiguration.ListCategoryAttributes;

internal sealed class ListCategoryAttributesService : IListCategoryAttributesService
{
    private readonly ICategoryStore _categoryStore;
    private readonly ICurrentUser _currentUser;

    public ListCategoryAttributesService(ICategoryStore categoryStore, ICurrentUser currentUser)
    {
        _categoryStore = categoryStore;
        _currentUser = currentUser;
    }

    public async Task<OperationResult<IReadOnlyList<CategoryAttributeItem>>> ListAsync(Guid categoryId, CancellationToken cancellationToken)
    {
        var ownerUserId = _currentUser.RequireUserId();

        // Existence/ownership check only — same FindByIdAsync SetCategoryStatusService/
        // DeleteCategoryService already use for the same purpose — so a category that isn't the
        // caller's, or doesn't exist, reports 404 rather than an empty list.
        var category = await _categoryStore.FindByIdAsync(ownerUserId, categoryId, cancellationToken);
        if (category is null)
        {
            return OperationResult<IReadOnlyList<CategoryAttributeItem>>.NotFound("Category not found.");
        }

        var items = await _categoryStore.ListAttributeConfigurationsAsync(ownerUserId, categoryId, cancellationToken);

        return OperationResult<IReadOnlyList<CategoryAttributeItem>>.Success(items);
    }
}
