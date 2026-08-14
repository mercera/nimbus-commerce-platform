using NimbusCommerce.Application.Catalog.Interfaces;
using NimbusCommerce.Application.Common.Interfaces;
using NimbusCommerce.Application.Common.Models;
using NimbusCommerce.Domain.Catalog;

namespace NimbusCommerce.Application.Catalog.Categories.CreateCategory;

internal sealed class CreateCategoryService : ICreateCategoryService
{
    private readonly ICategoryStore _categoryStore;
    private readonly ICurrentUser _currentUser;

    public CreateCategoryService(ICategoryStore categoryStore, ICurrentUser currentUser)
    {
        _categoryStore = categoryStore;
        _currentUser = currentUser;
    }

    public async Task<OperationResult<CategoryDetail>> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        var ownerUserId = _currentUser.RequireUserId();
        var name = request.Name.Trim();

        // Pre-check for a clear error message. The database's unique index on
        // (OwnerUserId, Name) is what actually guarantees correctness against a concurrent
        // duplicate create; this check-then-insert race is accepted as documented in
        // Architecture.md rather than closed with an EF-specific catch, which would pull
        // Microsoft.EntityFrameworkCore into this project.
        if (await _categoryStore.ExistsWithNameAsync(ownerUserId, name, excludingCategoryId: null, cancellationToken))
        {
            return OperationResult<CategoryDetail>.Conflict($"A category named '{name}' already exists.");
        }

        var category = Category.Create(ownerUserId, name, request.Description, ownerUserId, DateTime.UtcNow);

        _categoryStore.Add(category);
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
