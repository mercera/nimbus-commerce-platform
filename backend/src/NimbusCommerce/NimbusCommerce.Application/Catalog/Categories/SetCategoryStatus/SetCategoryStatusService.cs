using NimbusCommerce.Application.Catalog.Interfaces;
using NimbusCommerce.Application.Common.Interfaces;
using NimbusCommerce.Application.Common.Models;

namespace NimbusCommerce.Application.Catalog.Categories.SetCategoryStatus;

internal sealed class SetCategoryStatusService : ISetCategoryStatusService
{
    private readonly ICategoryStore _categoryStore;
    private readonly ICurrentUser _currentUser;

    public SetCategoryStatusService(ICategoryStore categoryStore, ICurrentUser currentUser)
    {
        _categoryStore = categoryStore;
        _currentUser = currentUser;
    }

    public async Task<OperationResult> SetStatusAsync(Guid categoryId, bool isActive, CancellationToken cancellationToken)
    {
        var ownerUserId = _currentUser.RequireUserId();

        var category = await _categoryStore.FindByIdAsync(ownerUserId, categoryId, cancellationToken);
        if (category is null)
        {
            return OperationResult.NotFound("Category not found.");
        }

        var utcNow = DateTime.UtcNow;

        if (isActive)
        {
            category.Activate(ownerUserId, utcNow);
        }
        else
        {
            // Requires a database query, so this rule is enforced here rather than on Category
            // itself — see Category.Deactivate's doc comment.
            var activeProductCount = await _categoryStore.CountActiveProductsAsync(ownerUserId, categoryId, cancellationToken);
            if (activeProductCount > 0)
            {
                return OperationResult.RuleViolation(
                    $"Cannot deactivate this category: it has {activeProductCount} active product(s). Deactivate them first.");
            }

            category.Deactivate(ownerUserId, utcNow);
        }

        await _categoryStore.SaveChangesAsync(cancellationToken);

        return OperationResult.Success();
    }
}
