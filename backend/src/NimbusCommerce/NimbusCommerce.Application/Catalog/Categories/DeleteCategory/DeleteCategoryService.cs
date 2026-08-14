using NimbusCommerce.Application.Catalog.Interfaces;
using NimbusCommerce.Application.Common.Interfaces;
using NimbusCommerce.Application.Common.Models;

namespace NimbusCommerce.Application.Catalog.Categories.DeleteCategory;

internal sealed class DeleteCategoryService : IDeleteCategoryService
{
    private readonly ICategoryStore _categoryStore;
    private readonly ICurrentUser _currentUser;

    public DeleteCategoryService(ICategoryStore categoryStore, ICurrentUser currentUser)
    {
        _categoryStore = categoryStore;
        _currentUser = currentUser;
    }

    public async Task<OperationResult> DeleteAsync(Guid categoryId, CancellationToken cancellationToken)
    {
        var ownerUserId = _currentUser.RequireUserId();

        var category = await _categoryStore.FindByIdAsync(ownerUserId, categoryId, cancellationToken);
        if (category is null)
        {
            return OperationResult.NotFound("Category not found.");
        }

        // Allowed only when the category holds zero products of any status — a category with
        // only Inactive products still cannot be deleted, only deactivated.
        var productCount = await _categoryStore.CountProductsAsync(ownerUserId, categoryId, cancellationToken);
        if (productCount > 0)
        {
            return OperationResult.RuleViolation(
                $"Cannot delete this category: it has {productCount} product(s). Deactivate the category instead.");
        }

        _categoryStore.Remove(category);
        await _categoryStore.SaveChangesAsync(cancellationToken);

        return OperationResult.Success();
    }
}
