using NimbusCommerce.Application.Catalog.Interfaces;
using NimbusCommerce.Application.Common.Interfaces;
using NimbusCommerce.Application.Common.Models;

namespace NimbusCommerce.Application.Catalog.Categories.UpdateCategory;

internal sealed class UpdateCategoryService : IUpdateCategoryService
{
    private readonly ICategoryStore _categoryStore;
    private readonly ICurrentUser _currentUser;

    public UpdateCategoryService(ICategoryStore categoryStore, ICurrentUser currentUser)
    {
        _categoryStore = categoryStore;
        _currentUser = currentUser;
    }

    public async Task<OperationResult<CategoryDetail>> UpdateAsync(Guid categoryId, UpdateCategoryRequest request, CancellationToken cancellationToken)
    {
        var ownerUserId = _currentUser.RequireUserId();

        var category = await _categoryStore.FindByIdAsync(ownerUserId, categoryId, cancellationToken);
        if (category is null)
        {
            return OperationResult<CategoryDetail>.NotFound("Category not found.");
        }

        var name = request.Name.Trim();

        if (!string.Equals(name, category.Name, StringComparison.Ordinal) &&
            await _categoryStore.ExistsWithNameAsync(ownerUserId, name, categoryId, cancellationToken))
        {
            return OperationResult<CategoryDetail>.Conflict($"A category named '{name}' already exists.");
        }

        var utcNow = DateTime.UtcNow;
        category.Rename(name, ownerUserId, utcNow);
        category.UpdateDescription(request.Description, ownerUserId, utcNow);

        await _categoryStore.SaveChangesAsync(cancellationToken);

        return OperationResult<CategoryDetail>.Success(new CategoryDetail(
            category.Id,
            category.Name,
            category.Description,
            category.IsActive,
            category.CreatedAtUtc,
            category.CreatedByUserId,
            category.UpdatedAtUtc,
            category.UpdatedByUserId));
    }
}
